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
    public class WaveformViewControl : Control
    {
        public WaveformViewControl() 
        {
            ColorProperties = new ColorProperties();


            CyclePropertyItemsSource = new List<CycleProperties>()
            {
                new CycleProperties(10), new CycleProperties(20),
            };

            PinPropertyItemsSource = new List<PinProperties>()
            {
                new PinProperties("pin0", 1, CyclePropertyItemsSource.Count, 3.3, -1.2),
            };

            WaveformLinePropertyItemsSource = new List<WaveformLineProperties>()
            {
                new WaveformLineProperties("line1", Colors.Blue),
            };

            var pin0 = PinPropertyItemsSource[0];

            for(int cycleIndex = 0; cycleIndex < CyclePropertyItemsSource.Count; cycleIndex++)
            {
                int pointSize = CyclePropertyItemsSource[cycleIndex].PointSize;
                for (int pointIndex = 0; pointIndex < pointSize; pointIndex++) 
                {
                    pin0[0, cycleIndex][pointIndex] = (new Random(pointIndex + DateTime.Now.GetHashCode())).NextDouble() * 3.3;
                }
            }
        }

        private bool _mouseEnter = false;

        private bool _mouseLeftBtnDown = false;

        private bool _mouseRightBtnDown = false;

        private Point _mouseLocation;

        private double hornizontalValue;
        public double HornizontalValue 
        {
            get => hornizontalValue;
            set
            {
                hornizontalValue = value;
                this.InvalidateVisual();
            }
        }

        private double verticalValue;
        public double VerticalValue 
        {
            get => verticalValue;
            set
            {
                verticalValue = value;
                this.InvalidateVisual();
            }
        }

        public ColorProperties ColorProperties { get; set; }

        public List<CycleProperties> CyclePropertyItemsSource { get; set; }

        public List<PinProperties> PinPropertyItemsSource { get; set; }

        public List<WaveformLineProperties> WaveformLinePropertyItemsSource { get; set; }

        internal Point ActualTopLeft { get; } = new Point(10, 10);

        internal double ActualMW => this.ActualWidth - 20.0d;
        internal double ActualMH => this.ActualHeight - 20.0d;

        internal double LegendHeight => 40.0d;

        internal double CH => 20.0d;

        internal double TH => 20.0d;

        internal double WH => 80d * VerticalScale;

        internal double PixelPerPoint => 4 * HornizontalScale;

        internal double PNW { get; set; } = 50d;
        internal double VBW { get; set; } = 50d;
        internal double WW => ActualWidth - PNW - VBW;

        internal double WFTop => ActualTopLeft.Y + LegendHeight + CH;

        internal double WFBottom => WFTop + ActualMH - LegendHeight - CH - TH;

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
            ColorProperties.Update();
            var gridThickness = 1.0d;
            var gridPen = new Pen(ColorProperties.GridBrush, gridThickness);
            gridPen.Freeze();
            //dc.DrawLine(new Pen(Brushes.Black, 1.0d), new Point(0, 0), new Point(this.ActualWidth, this.ActualHeight));

            // background
            dc.DrawRectangle(ColorProperties.BackgroundBrush, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));


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



        public void Update()
        {
            this.InvalidateVisual();
        }

        
    }
}
