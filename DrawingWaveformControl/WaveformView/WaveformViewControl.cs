using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WaveformView
{

    internal delegate void OnUpdatedHandler();

    public enum ETimeUnit
    {
        S,
        ms,
        us,
        ns,
        ps,
        fs,
        Auto,
    }

    public enum EVoltageUnit
    {
        V,
        mV,
        uV,
        nV,
        Auto,
    }

    public class WaveformViewControl : Control
    {
        public WaveformViewControl() 
        {
            ColorProperties = new ColorProperties();
        }

        internal event OnUpdatedHandler OnUpdated = null;

        private bool _mouseEnter = false;

        private bool _mouseLeftBtnDown = false;

        private bool _mouseRightBtnDown = false;

        private Point _mouseLocation;

        public ETimeUnit TimeUnit { get; set; } = ETimeUnit.Auto;

        public EVoltageUnit VoltageUnit { get; set; } = EVoltageUnit.Auto;

        public double TimeResolution { get; set; } = 0.000000001; //Sec


        private double hornizontalValue;
        public double HornizontalScrollValue 
        {
            get => hornizontalValue;
            set
            {
                hornizontalValue = value;
                this.InvalidateVisual();
            }
        }

        private double verticalValue;
        public double VerticalScrollValue 
        {
            get => verticalValue;
            set
            {
                verticalValue = value;
                this.InvalidateVisual();
            }
        }

        internal double MaxHornizontalScrollValue { get; private set; }

        internal double MaxVerticalScrollValue { get; private set; }

        internal IEnumerable<PinProperties> ShowingPinPropertyItemsSource
        {
            get
            {
                if (ShowdowPinPropertyItemsSource is null) yield break;

                int fromIndex = (int)(VerticalScrollValue / WH);
                int toIndex = (int)((VerticalScrollValue + TWH) / WH);

                for(int i = fromIndex; i <= toIndex && i < ShowdowPinPropertyItemsSource.Count; i++)
                {
                    yield return ShowdowPinPropertyItemsSource[i];
                }

                yield break;
            }
        }

        internal IEnumerable<CycleProperties> ShowingCyclePropertyItemsSource
        {
            get 
            {
                if(CyclePropertyItemsSource is null) yield break;

                int fromIndex = CyclePropertyItemsSource.FirstOrDefault(item => (item.PointAccumulator + item.PointSize) * PixelPerPoint >= HornizontalScrollValue)?.Index ?? -1;
                int toIndex = CyclePropertyItemsSource.LastOrDefault(item => (item.PointAccumulator + item.PointSize) * PixelPerPoint > HornizontalScrollValue + WW)?.Index ?? -1;

                if(fromIndex == -1) 
                    yield break;

                for (int i = fromIndex; (i <= toIndex || toIndex == -1) && i < CyclePropertyItemsSource.Count; i++)
                {
                    yield return CyclePropertyItemsSource[i];
                }

                yield break;
            }
        }

        public ColorProperties ColorProperties { get; set; }

        public IReadOnlyList<CycleProperties> CyclePropertyItemsSource { get; internal set; }

        public IReadOnlyList<PinProperties> PinPropertyItemsSource { get; internal set; }

        internal IReadOnlyList<PinProperties> ShowdowPinPropertyItemsSource { get; set; }

        public IReadOnlyList<WaveformLineProperties> WaveformLinePropertyItemsSource { get; internal set; }


        public void Setup(IReadOnlyList<CycleProperties> CyclePropertyItemsSource, IReadOnlyList<PinProperties> PinPropertyItemsSource, IReadOnlyList<WaveformLineProperties> WaveformLinePropertyItemsSource)
        {
            this.CyclePropertyItemsSource = CyclePropertyItemsSource;
            this.PinPropertyItemsSource = PinPropertyItemsSource;
            this.WaveformLinePropertyItemsSource = WaveformLinePropertyItemsSource;

            var pointAccumulator = 0;

            for (int i = 0; i < CyclePropertyItemsSource.Count; i++)
            {
                var cycleProp = CyclePropertyItemsSource[i];
                cycleProp.Index = i;
                cycleProp.PointAccumulator = pointAccumulator;
                pointAccumulator += cycleProp.PointSize;
            }

            for (int i = 0; i < PinPropertyItemsSource.Count; i++)
            {
                var pin = PinPropertyItemsSource[i];

                pin.Index = i;
                
                for(int lineIndex = 0; lineIndex < pin.LineSize; lineIndex++)
                {
                    for (int j = 0; j < pin.CycleSize; j++)
                    {
                        pin[lineIndex, j].Index = j;
                    }
                }
            }

            TimeUnit = ETimeUnit.Auto;

            VoltageUnit = EVoltageUnit.Auto;
        }

        internal Point ActualTopLeft { get; } = new Point(10, 10);

        internal double ActualMW => this.ActualWidth - 20.0d;
        internal double ActualMH => this.ActualHeight - 20.0d;

        internal double LegendHeight => 40.0d;

        internal double CH => 20.0d;

        internal double TH => 20.0d;

        internal double WH => 80d * VerticalScale;

        internal double MaxMinVoltageScalePadding => 5.0d;

        internal double PixelPerPoint => 4 * HornizontalScale;

        internal double PNW { get; set; } = 50d;
        internal double VBW { get; set; } = 50d;
        internal double WW => ActualWidth - PNW - VBW;

        internal double WFTop => ActualTopLeft.Y + LegendHeight + CH;

        internal double WFBottom => WFTop + ActualMH - LegendHeight - CH - TH;

        internal double TWH => WFBottom - WFTop;

        internal double WFLeft => ActualTopLeft.X + PNW + VBW;

        internal double WFRight => ActualTopLeft.X + ActualMW;

        public double HornizontalScale { get; set; } = 1.0d;

        public double VerticalScale { get; set; } = 1.0d;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            this.InvalidateVisual();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _mouseEnter = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _mouseEnter = false;
        }


        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            _mouseRightBtnDown = false;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            _mouseRightBtnDown = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            _mouseLeftBtnDown = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            _mouseLeftBtnDown = false;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            _mouseLocation = e.GetPosition(this);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);
            _mouseRightBtnDown = true;
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);
            _mouseRightBtnDown = false;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            this.InvalidateVisual();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var cycleProperties = ShowingCyclePropertyItemsSource.ToArray();

            var pinProperties = ShowingPinPropertyItemsSource.ToArray();

            if (cycleProperties.Length == 0) { return; }
            if(pinProperties.Length == 0) { return; }


            var gridThickness = 1.0d;
            var gridPen = new Pen(ColorProperties.GridBrush, gridThickness);
            gridPen.Freeze();
            //dc.DrawLine(new Pen(Brushes.Black, 1.0d), new Point(0, 0), new Point(this.ActualWidth, this.ActualHeight));

            // background
            dc.DrawRectangle(ColorProperties.BackgroundBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));


            OnRender(dc, pinProperties);

            OnRender(dc, cycleProperties);

            // Legend line
            var legendHeightTop = ActualTopLeft.Y + LegendHeight;
            dc.DrawLine(gridPen, new Point(ActualTopLeft.X, legendHeightTop), new Point(WFRight, legendHeightTop));

            // WF Top
            dc.DrawLine(gridPen, new Point(ActualTopLeft.X, WFTop), new Point(WFRight, WFTop));

            // WF Bottom
            dc.DrawLine(gridPen, new Point(ActualTopLeft.X, WFBottom), new Point(WFRight, WFBottom));

            // Voltage Bar Width
            dc.DrawLine(gridPen, new Point(WFLeft, WFTop), new Point(WFLeft, WFBottom));

            // Pin Name Width Line
            dc.DrawLine(gridPen, new Point(ActualTopLeft.X + PNW, WFTop), new Point(ActualTopLeft.X + PNW, WFBottom));
            
            // 最外圍方框
            dc.DrawRectangle(null, gridPen, new Rect(ActualTopLeft, new Size(ActualMW, ActualMH)));
            
        }

        private void OnRender(DrawingContext dc, CycleProperties[] cycleProperties)
        {
            foreach(var cycleProp in cycleProperties)
            {
                var cycleIndex = cycleProp.Index;

                var cycleLine_PosiX = (cycleProp.PointAccumulator + cycleProp.PointSize) * PixelPerPoint - HornizontalScrollValue + WFLeft;

                if (cycleLine_PosiX < WFLeft || cycleLine_PosiX > WFRight) continue;

                dc.DrawLine(ColorProperties.GridPen, new Point(cycleLine_PosiX, WFTop), new Point(cycleLine_PosiX, WFBottom));
                

            }
        }

        private void OnRender(DrawingContext dc, PinProperties[] pinProperties)
        {
            var showdowPinPropItems = ShowdowPinPropertyItemsSource.ToList();

            var WH_Half = WH / 2.0d;

            foreach (var pinProp in pinProperties)
            {
                var pinIndex = showdowPinPropItems.IndexOf(pinProp);

                var pinTop = WFTop + pinIndex * WH - VerticalScrollValue;
                var pinBottom = pinTop + WH;

                // pin name drawing

                var formatText = pinProp.FormattedText;

                var textPosiX = ActualTopLeft.X + 2.5d;

                var textPosiY = pinTop + WH_Half - formatText.Height / 2.0d;

                if(textPosiY > WFTop && textPosiY + formatText.Height < WFBottom)
                {
                    dc.DrawText(pinProp.FormattedText, new Point(textPosiX, textPosiY));
                }

                // max/min voltage drawing

                var maxVoltage = pinProp.MaxScopeVoltage;
                var minVoltage = pinProp.MinScopeVoltage;

                var maxVoltPosiY = pinTop + MaxMinVoltageScalePadding;
                var minVoltPosiY = pinTop + WH - MaxMinVoltageScalePadding;

                //drawing max min voltage text...
                if(maxVoltPosiY > WFTop && maxVoltPosiY < WFBottom)
                {
                    dc.DrawLine(ColorProperties.MaxMinVoltLinePen, new Point(WFLeft, maxVoltPosiY), new Point(WFRight, maxVoltPosiY));
                }

                if (minVoltPosiY > WFTop && minVoltPosiY < WFBottom)
                {
                    dc.DrawLine(ColorProperties.MaxMinVoltLinePen, new Point(WFLeft, minVoltPosiY), new Point(WFRight, minVoltPosiY));

                }


                // pin bottom line drawing

                if (pinBottom > WFTop && pinBottom < WFBottom)
                {
                    dc.DrawLine(ColorProperties.GridPen, new Point(ActualTopLeft.X, pinBottom), new Point(WFRight, pinBottom));
                }
            }
        }

        public void Update()
        {
            ColorProperties.Update();

            ShowdowPinPropertyItemsSource = PinPropertyItemsSource.Where(item => item.IsVisible).OrderBy(item => item.Index).ToList();

            var lastCycleProp = CyclePropertyItemsSource?.LastOrDefault();

            MaxHornizontalScrollValue = lastCycleProp is null ? 1 : (lastCycleProp.PointAccumulator + lastCycleProp.PointSize) * PixelPerPoint;

            MaxVerticalScrollValue = ShowdowPinPropertyItemsSource.Count * WH;

            //found max pin name width
            var maxTextWidth = 10d;
            for(int pinIndex = 0; pinIndex < ShowdowPinPropertyItemsSource.Count; pinIndex++) 
            {
                var pinProp = ShowdowPinPropertyItemsSource[pinIndex];
                if(pinProp.FormattedText is null)
                {
                    pinProp.FormattedText = new FormattedText(
                    pinProp.Name, // 文字內容
                    System.Globalization.CultureInfo.CurrentCulture, // 使用當前文化信息
                    FlowDirection.LeftToRight, // 文字流向
                    new Typeface("Verdana"), // 字體
                    DefaultProperties.TextThickness, // 字號
                    ColorProperties.TextBrush, // 文字顏色
                    VisualTreeHelper.GetDpi(this).PixelsPerDip); //Render在不同DPI的顯示器上能夠自動調整
                }
                else
                {
                    pinProp.FormattedText.SetForegroundBrush(ColorProperties.TextBrush);
                }

                if(maxTextWidth < pinProp.FormattedText.WidthIncludingTrailingWhitespace)
                {
                    maxTextWidth = pinProp.FormattedText.WidthIncludingTrailingWhitespace;
                }
            }

            PNW = maxTextWidth + 10d;

            RaiseOnUpdated();

            this.InvalidateVisual();
        }

        internal void RaiseOnUpdated()
        {
            OnUpdated?.Invoke();
        }
    }
}
