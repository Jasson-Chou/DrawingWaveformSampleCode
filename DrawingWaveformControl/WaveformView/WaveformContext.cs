using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaveformView
{
    internal delegate void OnScrollValueChangedHandler();
    internal delegate void OnScrollMaxValueChangedHandler();
    internal delegate void OnZoomInOutChangedHandler();

    internal struct DrawingVoltageLevel
    {
        public string Name { get; set; }

        public FormattedText Formatted { get; set; }

        public Point LinePosiLeft { get; set; }

        public Point ScaleLinePosiLeft { get; set; }

        public Point TextPosiLeft { get; set; }
    }

    public class WaveformContext
    {
        public enum ERenderDirect
        {
            Vertical,
            Hornizontal,
            ZoomInOut,
            MouseMoving,
            ShowPins,
            All,
        }

        public WaveformContext(Image image)
        {
            SpacingProperties = new SpacingProperties();
            ColorProperties = new ColorProperties();
            this.waveformImage = image;

#if DEBUG
            Stopwatches = new Dictionary<string, Stopwatch>();
#endif
        }

        internal event OnScrollMaxValueChangedHandler OnScrollMaxValueChanged;
        internal event OnScrollValueChangedHandler OnScrollValueChanged;
        internal event OnZoomInOutChangedHandler OnZoomInOutChanged;

        internal void RaiseOnScrollMaxValueChanged()
        {
            OnScrollMaxValueChanged?.Invoke();
        }

        internal void RaiseOnScrollValueChanged()
        {
            OnScrollValueChanged?.Invoke();
        }

        internal void RaiseOnZoomInOutChanged()
        {
            OnZoomInOutChanged?.Invoke();
        }

        private DrawingVisual A, B, C, D, M, T;

#if DEBUG
        private Dictionary<string, Stopwatch> Stopwatches;
#endif

        private RenderTargetBitmap NewBitmap
        {
            get
            {
                if (CurrBitmapTmp is null || CurrMW != SpacingProperties.MW || CurrMH != SpacingProperties.MH)
                    CurrBitmapTmp = new RenderTargetBitmap((int)SpacingProperties.MW, (int)SpacingProperties.MH, 96.0d, 96.0d, PixelFormats.Pbgra32);
                return CurrBitmapTmp;
            }
        }

        private double CurrMW { get; set; }
        private double CurrMH { get; set; }

        private RenderTargetBitmap CurrBitmapTmp { get; set; }

        public ColorProperties ColorProperties { get; private set; }

        public IReadOnlyList<CycleProperties> CycleProperties { get; private set; }

        public IReadOnlyList<PinProperties> PinProperties { get; private set; }

        public IReadOnlyList<WaveformLineProperties> WaveformLineProperties { get; private set; }

        internal List<TimingCursor> _TimingCursors { get; set; }

        public IReadOnlyList<TimingCursor> TimingCursors => _TimingCursors;

        public SpacingProperties SpacingProperties { get; }

        public double MaxScrollHornizontal { get; private set; }

        public double MaxScrollVertical { get; private set; }

        public double ScrollHornizontalValue { get; set; } = 0.0d;

        public double ScrollVerticalValue { get; set; } = 0.0d;

        public Image waveformImage { get; }

        internal IReadOnlyList<PinProperties> ShowingPinProperties { get; set; }

        public void Setup(ColorProperties colorProperties)
        {
            ColorProperties = colorProperties;
        }

        public void Setup(IReadOnlyList<CycleProperties> cycleProperties, IReadOnlyList<PinProperties> pinProperties, IReadOnlyList<WaveformLineProperties> waveformLineProperties)
        {
            _TimingCursors = new List<TimingCursor>();
            CycleProperties = cycleProperties;
            PinProperties = pinProperties;
            InitCycleProperties();
            InitScrollValues();
            Setup(waveformLineProperties);
        }

        public void Setup(IReadOnlyList<PinProperties> pinProperties)
        {
            PinProperties = pinProperties;
            InitScrollValues();
        }

        public void Setup(IReadOnlyList<CycleProperties> cycleProperties)
        {
            _TimingCursors = new List<TimingCursor>();
            CycleProperties = cycleProperties;
            InitCycleProperties();
            InitScrollValues();
        }

        public void Setup(IReadOnlyList<WaveformLineProperties> waveformLineProperties)
        {
            this.WaveformLineProperties = waveformLineProperties;
        }


        public void Render(ERenderDirect direct)
        {

            var bmp = NewBitmap;
            bmp.Clear();
            if (CycleProperties is null || PinProperties is null)
            {

            }
            else
            {
                SpacingProperties.Update();
                if (direct == ERenderDirect.All)
                {
                    InitPinProperties();
                    InitCycleProperties();
                    InitScrollValues();
                    InitPinBarWidth();
                    InitVoltageBarWidth();
                    InitDynamicSpacingProperties();
                    RenderD();
                    RenderC();
                    RenderB();
                    RenderA();
                    MouseMove(null);
                    RenderT();
                }
                else if (direct == ERenderDirect.Hornizontal)
                {
                    RenderD();
                    RenderB();
                    RenderT();
                }
                else if (direct == ERenderDirect.Vertical)
                {
                    InitPinBarWidth();
                    InitVoltageBarWidth();
                    InitDynamicSpacingProperties();
                    RenderD();
                    RenderC();
                    RenderB();
                    RenderA();
                    RenderT();
                }
                else if (direct == ERenderDirect.ZoomInOut)
                {
                    InitCycleProperties();
                    InitScrollValues();
                    InitPinBarWidth();
                    InitVoltageBarWidth();
                    InitDynamicSpacingProperties();
                    RenderD();
                    RenderC();
                    RenderB();
                    RenderA();
                    RenderT();
                }
                else if (direct == ERenderDirect.ShowPins)
                {
                    InitPinProperties();
                    InitPinBarWidth();
                    InitVoltageBarWidth();
                    InitDynamicSpacingProperties();
                    RenderD();
                    RenderC();
                    RenderT();
                }
                else if (direct == ERenderDirect.MouseMoving)
                {
                    RenderT();
                }
                else
                {
                    throw new Exception($"Undefine {direct.ToString()}");
                }
            }


            if (!(A is null)) bmp.Render(A);
            if (!(D is null)) bmp.Render(D);
            if (!(C is null)) bmp.Render(C);
            if (!(B is null)) bmp.Render(B);
            if (!(M is null)) bmp.Render(M);
            if (!(T is null)) bmp.Render(T);
            waveformImage.Source = bmp;
        }
#if DEBUG
        private void StartWatch(string name)
        {
            var watchName = name;
            Stopwatch watchInstance = null;
            if (!Stopwatches.ContainsKey(watchName)) Stopwatches.Add(watchName, new Stopwatch());
            watchInstance = Stopwatches[watchName];
            watchInstance.Restart();
        }

        private void StopWatch(string name)
        {
            var watchName = name;
            var watch = Stopwatches[name];
            watch.Stop();
            Debug.WriteLine($"{name}: {watch.ElapsedMilliseconds} ms");
        }
#endif
        /// <summary>
        /// Fixed Lines, Pin Header Lines
        /// </summary>
        private void RenderA()
        {
#if DEBUG
            StartWatch(nameof(RenderA));
#endif

            if (A is null) A = new DrawingVisual();
            var dc = A.RenderOpen();
            var rectBackground = new SolidColorBrush(ColorProperties.Background);
            var pen = new Pen(new SolidColorBrush(ColorProperties.Line), SpacingProperties.LineWidth);

            dc.DrawRectangle(rectBackground, null,
                new Rect(0, 0, SpacingProperties.MW, SpacingProperties.MH));

            // Actual MW and MH
            dc.DrawRectangle(rectBackground, pen,
                new Rect(SpacingProperties.TopLeft,
                new Vector(SpacingProperties.ActualMW, SpacingProperties.ActualMH)));

            // Error Bar
            dc.DrawRectangle(rectBackground, pen,
                new Rect(SpacingProperties.TopLeft,
                new Vector(SpacingProperties.ActualMW, SpacingProperties.EBH)));

            // Timing Bar
            dc.DrawRectangle(rectBackground, pen,
                new Rect(SpacingProperties.TopLeft.X, SpacingProperties.TopLeft.Y + SpacingProperties.ActualMH - SpacingProperties.TBH,
                SpacingProperties.ActualMW, SpacingProperties.TBH));

            var wLengendPoint = new Point(SpacingProperties.TopLeft.X, 0);

            var halfLengendHeight = SpacingProperties.LegendHeight / 2.0d;
            var wLengendP1 = new Point(0, halfLengendHeight);
            var wLengendP2 = new Point(0, halfLengendHeight);
            Color waveformLineColor;
            var wTextBrush = new SolidColorBrush(ColorProperties.InformationText);
            wTextBrush.Freeze();
            for (int index = 0; index < WaveformLineProperties.Count; index++)
            {
                var wLine = WaveformLineProperties[index];
                waveformLineColor = wLine.LineColor ?? ColorProperties.DefaultWaveformLine;
                var lineBrush = new SolidColorBrush(waveformLineColor);
                lineBrush.Freeze();
                var wPen = new Pen(lineBrush, wLine.Thickness);
                wPen.Freeze();
                var wText = GetDrawingFormattedText(wLine.Name, wTextBrush, SpacingProperties.LegendTextSize);
                wLengendP1.X = wLengendPoint.X + wText.WidthIncludingTrailingWhitespace + 5.0d;
                wLengendP2.X = wLengendP1.X + wText.WidthIncludingTrailingWhitespace;
                wLengendPoint.Y = halfLengendHeight - wText.Height / 2.0d;
                dc.DrawText(wText, wLengendPoint);
                dc.DrawLine(wPen, wLengendP1, wLengendP2);
                wLengendPoint.Offset(wLengendP2.X - wLengendPoint.X + 5.0d, 0.0d);
            }

            var PinBarVerticalLineX = SpacingProperties.WaveformLeftLineX - SpacingProperties.VBW;

            var PinBarVerticalLineTopPoint = new Point(PinBarVerticalLineX, SpacingProperties.WaveformTopLineY);

            var PinBarVerticalLineBottomPoint = new Point(PinBarVerticalLineX, SpacingProperties.WaveformBottomLineY);

            dc.DrawLine(pen, PinBarVerticalLineTopPoint, PinBarVerticalLineBottomPoint);

            PinBarVerticalLineTopPoint.Offset(SpacingProperties.VBW, 0);

            PinBarVerticalLineBottomPoint.Offset(SpacingProperties.VBW, 0);

            dc.DrawLine(pen, PinBarVerticalLineTopPoint, PinBarVerticalLineBottomPoint);

            dc.Close();

#if DEBUG
            StopWatch(nameof(RenderA));
#endif
        }

        /// <summary>
        /// Cycle Dash Line, Error Bar, Time Bar, Cursor
        /// </summary>
        private void RenderB()
        {
#if DEBUG
            StartWatch(nameof(RenderB));
#endif
            if (B is null) B = new DrawingVisual();
            var dc = B.RenderOpen();
            var cycleIndexes = GetDrawingCycleIndexes()?.ToList() ?? new List<int>();
            var pinIndexes = GetDrawingPinIndexes();

            var dashPen = new Pen(new SolidColorBrush(ColorProperties.Line), SpacingProperties.LineWidth);
            dashPen.DashStyle = DashStyles.Dash;
            dashPen.Freeze();
            var failCircleRadius = SpacingProperties.ErrorBarFailCircleRadius;
            //var minX = SpacingProperties.TopLeft.X + SpacingProperties.PBW + SpacingProperties.VBW;
            var failCircleBrush = new SolidColorBrush(ColorProperties.FailCircle);
            failCircleBrush.Freeze();
            var timingTextBrush = new SolidColorBrush(ColorProperties.TimingText);
            timingTextBrush.Freeze();
            var ignoreLastPointBrush = new SolidColorBrush(ColorProperties.IgnoreLastPoint);
            ignoreLastPointBrush.Freeze();
            var DrawingTextPoint = new Point();
            var cycleTopPoint = new Point(0, SpacingProperties.WaveformTopLineY);
            var cycleBottomPoint = new Point(0, SpacingProperties.WaveformBottomLineY);
            var lastCycTextPosiX = SpacingProperties.WaveformLeftLineX;
            foreach (var cycleIdx in cycleIndexes)
            {
                var drawingCycle = CycleProperties[cycleIdx];
                var isLastCycle = cycleIdx == CycleProperties.Last().Index;
                var isFail = drawingCycle.IsFail;
                var pointSize = drawingCycle.PointsSize;

                var cyclePosiX = SpacingProperties.WaveformLeftLineX + drawingCycle.DrawingXPosition - ScrollHornizontalValue;
                var cycleMinPosiX = SpacingProperties.WaveformLeftLineX + (drawingCycle.DrawingXPosition + drawingCycle.LastPointsSum * SpacingProperties.ActualPixelPerPoint) / 2.0d - ScrollHornizontalValue;

                //drawing cycle Info (Offset, Cycle Index)
                if (cycleMinPosiX < SpacingProperties.WaveformRightLineX && cycleMinPosiX > SpacingProperties.WaveformLeftLineX)
                {
                    var formatText = GetDrawingFormattedText($"[{drawingCycle.Offset},{drawingCycle.Index}]", timingTextBrush, SpacingProperties.TimeBarTextSize);
                    DrawingTextPoint.X = cycleMinPosiX - formatText.WidthIncludingTrailingWhitespace / 2.0d;
                    DrawingTextPoint.Y = SpacingProperties.WaveformBottomLineY + (SpacingProperties.TBH - formatText.Height) / 2.0d;
                    if (DrawingTextPoint.X > lastCycTextPosiX &&
                        DrawingTextPoint.X + formatText.WidthIncludingTrailingWhitespace < SpacingProperties.WaveformRightLineX)
                    {
                        dc.DrawText(formatText, DrawingTextPoint);
                        lastCycTextPosiX = cycleMinPosiX + formatText.WidthIncludingTrailingWhitespace / 2.0d;
                    }
                }

                //drawing period text
                if (cyclePosiX < SpacingProperties.WaveformRightLineX && cyclePosiX > SpacingProperties.WaveformLeftLineX)
                {
                    cycleTopPoint.X = cyclePosiX;
                    cycleBottomPoint.X = cyclePosiX;
                    dc.DrawLine(dashPen, cycleTopPoint, cycleBottomPoint);

                    var SumPointSize = drawingCycle.LastPointsSum + drawingCycle.PointsSize;
                    string timing = SpacingProperties.ValueToTimingStr(SumPointSize * SpacingProperties.TimingResolution);
                    var formatText = GetDrawingFormattedText(timing, timingTextBrush, SpacingProperties.TimeBarTextSize);
                    DrawingTextPoint.X = cyclePosiX - formatText.WidthIncludingTrailingWhitespace / 2.0d;
                    DrawingTextPoint.Y = SpacingProperties.WaveformBottomLineY + (SpacingProperties.TBH - formatText.Height) / 2.0d;
                    if (DrawingTextPoint.X > lastCycTextPosiX &&
                        DrawingTextPoint.X + formatText.WidthIncludingTrailingWhitespace < SpacingProperties.WaveformRightLineX)
                    {
                        dc.DrawText(formatText, DrawingTextPoint);
                        lastCycTextPosiX = cyclePosiX + formatText.WidthIncludingTrailingWhitespace / 2.0d;
                    }
                }



                if (isFail) // Drawing Fail Circle
                {
                    var showCycleIndex = cycleIdx - 1;
                    var preDrawingLen = showCycleIndex >= 0 ? CycleProperties[showCycleIndex].DrawingXPosition : 0;
                    var failCirclePointX = SpacingProperties.WaveformLeftLineX + (preDrawingLen + drawingCycle.DrawingXPosition) / 2.0d - ScrollHornizontalValue;
                    var failCirclePoint = new Point(failCirclePointX, SpacingProperties.WaveformTopLineY - failCircleRadius);
                    if (failCirclePointX - SpacingProperties.ErrorBarFailCircleRadius > SpacingProperties.WaveformLeftLineX &&
                        failCirclePointX < SpacingProperties.WaveformRightLineX - SpacingProperties.ErrorBarFailCircleRadius)
                        dc.DrawEllipse(failCircleBrush, null, failCirclePoint, failCircleRadius, failCircleRadius);
                }

                if (isLastCycle)
                {
                    var rectTopLeftX = SpacingProperties.WaveformLeftLineX + drawingCycle.DrawingXPosition - ScrollHornizontalValue - SpacingProperties.ActualPixelPerPoint + 0.1;
                    var rectTopLeftY = SpacingProperties.WaveformTopLineY + 0.1;
                    var sizeWidth = SpacingProperties.ActualPixelPerPoint - 0.1;
                    var sizeHeight = SpacingProperties.N_WH - 0.1;
                    if (sizeWidth > 0.0d && sizeHeight > 0.0d)
                    {
                        DrawingRect(dc, new Point(rectTopLeftX, rectTopLeftY), new Size(sizeWidth, sizeHeight),
                        new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformTopLineY), new Size(SpacingProperties.WW, SpacingProperties.N_WH),
                        null, ignoreLastPointBrush);
                    }
                }
            }

            var firstFailCirclePoint = new Point(SpacingProperties.WaveformLeftLineX + SpacingProperties.ErrorBarFailCircleRadius,
                SpacingProperties.WaveformTopLineY - SpacingProperties.ErrorBarFailCircleRadius);

            var lastFailCirclePoint = new Point(SpacingProperties.WaveformRightLineX - SpacingProperties.ErrorBarFailCircleRadius,
                SpacingProperties.WaveformTopLineY - SpacingProperties.ErrorBarFailCircleRadius);

            var failCirclePen = new Pen(failCircleBrush, SpacingProperties.LineWidth);

            if (cycleIndexes.Count() > 0)
            {
                var min = cycleIndexes.Min();
                var IsOutSideAndFailOnMinCycle = false;
                var minCycleProp = CycleProperties[min];
                if (minCycleProp.IsFail)
                {
                    var preCyclePropDrawingLength = min - 1 == -1 ? 0 : CycleProperties[min - 1].DrawingXPosition;
                    IsOutSideAndFailOnMinCycle = ScrollHornizontalValue >= (preCyclePropDrawingLength + minCycleProp.DrawingXPosition) / 2.0d - SpacingProperties.ErrorBarFailCircleRadius;
                }

                var drawingFailCircleOnFirst = IsOutSideAndFailOnMinCycle || CycleProperties.Any(item => item.Index < min && item.IsFail);

                if (drawingFailCircleOnFirst)
                {
                    dc.DrawEllipse(null, failCirclePen, firstFailCirclePoint, SpacingProperties.ErrorBarFailCircleRadius, SpacingProperties.ErrorBarFailCircleRadius);
                }


                var max = cycleIndexes.Max();
                var IsOutSideAndFailOnMaxCycle = false;
                var maxCycleProp = CycleProperties[max];
                if (maxCycleProp.IsFail)
                {
                    var preCyclePropDrawingLength = max - 1 == -1 ? 0 : CycleProperties[max - 1].DrawingXPosition;
                    IsOutSideAndFailOnMaxCycle = ScrollHornizontalValue + SpacingProperties.WW <= (preCyclePropDrawingLength + maxCycleProp.DrawingXPosition) / 2.0d + SpacingProperties.ErrorBarFailCircleRadius;
                }

                var drawingFailCircleOnLast = IsOutSideAndFailOnMaxCycle || CycleProperties.Any(item => item.Index > max && item.IsFail);

                if (drawingFailCircleOnLast)
                {
                    dc.DrawEllipse(null, failCirclePen, lastFailCirclePoint, SpacingProperties.ErrorBarFailCircleRadius, SpacingProperties.ErrorBarFailCircleRadius);
                }

            }

            dc.Close();

#if DEBUG
            StopWatch(nameof(RenderB));
#endif
        }

        /// <summary>
        /// Pin Name, Voltage Bar
        /// </summary>
        private void RenderC()
        {

#if DEBUG
            StartWatch(nameof(RenderC));
#endif
            if (C is null) C = new DrawingVisual();
            var dc = C.RenderOpen();

            try
            {
                var pinIndexes = GetDrawingPinIndexes()?.ToList();
                var baseY = SpacingProperties.WaveformTopLineY - ScrollVerticalValue;
                var LeftX = SpacingProperties.TopLeft.X;
                var RightX = SpacingProperties.TopLeft.X + SpacingProperties.ActualMW;
                var leftPoint = new Point(LeftX, 0.0d);
                var rightPoint = new Point(RightX, 0.0d);

                var lineSolidColorBrush = new SolidColorBrush(ColorProperties.Line);
                lineSolidColorBrush.Freeze();
                var pen = new Pen(lineSolidColorBrush, SpacingProperties.LineWidth);
                pen.Freeze();
                var MaxMinVoltageLineBrush = new SolidColorBrush(ColorProperties.MaxMinVoltageLine);
                var maxminVoltPen = new Pen(MaxMinVoltageLineBrush, SpacingProperties.LineWidth);
                maxminVoltPen.DashStyle = DashStyles.Dash;
                maxminVoltPen.Freeze();

                Dictionary<int, Pen> voltageLevelPens = new Dictionary<int, Pen>(ColorProperties.VoltageLevels?.Length ?? 0 + 1);

                var defaultVoltageLevelPen = new Pen(new SolidColorBrush(ColorProperties.DefaultVoltageLevel), SpacingProperties.LineWidth);
                defaultVoltageLevelPen.DashStyle = DashStyles.Dash;
                defaultVoltageLevelPen.Freeze();
                voltageLevelPens.Add(DefaultValues.NullValue, defaultVoltageLevelPen);

                for (int colorIdx = 0; colorIdx < (ColorProperties.VoltageLevels?.Length ?? 0); colorIdx++)
                {
                    var color = ColorProperties.VoltageLevels[colorIdx];
                    var _pen = new Pen(new SolidColorBrush(color), SpacingProperties.LineWidth);
                    _pen.DashStyle = DashStyles.Dash;
                    _pen.Freeze();
                    voltageLevelPens.Add(colorIdx, _pen);
                }

                var pinNameTextBrush = new SolidColorBrush(ColorProperties.PinName);
                pinNameTextBrush.Freeze();
                var voltageBarTextBrush = new SolidColorBrush(ColorProperties.VoltageText);
                voltageBarTextBrush.Freeze();
                var failCircleBrush = new SolidColorBrush(ColorProperties.FailCircle);
                failCircleBrush.Freeze();

                var pinTopLabelRectBrush = new SolidColorBrush(ColorProperties.PinTopLabelRect);
                pinTopLabelRectBrush.Freeze();
                var pinTopLabelTextBrush = new SolidColorBrush(ColorProperties.PinTopLabelText);
                pinTopLabelTextBrush.Freeze();

                var halfActualWH = SpacingProperties.ActualWaveformHeight / 2.0d;
                if (pinIndexes is null) return;
                int firstShowPinIndex = pinIndexes.First();
                int lastShowPinIndex = pinIndexes.Last();

                var pinBarFailCircleRadiusWithWH = halfActualWH / 2.0d / 2.0d;
                var pinBarFailCircleRadiusWithPBW = SpacingProperties.PBW / 4.0d / 2.0d;
                var pinBarFailCircleRadius = pinBarFailCircleRadiusWithPBW > pinBarFailCircleRadiusWithWH ? pinBarFailCircleRadiusWithWH : pinBarFailCircleRadiusWithPBW;
                var failCircleX = SpacingProperties.TopLeft.X + SpacingProperties.PBW - pinBarFailCircleRadius;
                var failCirclePoint = new Point(failCircleX, 0);


                var voltageScalePointX1 = SpacingProperties.WaveformLeftLineX - SpacingProperties.VoltageBarScaleWidth / 2.0d;
                var voltageScalePointX2 = voltageScalePointX1 + SpacingProperties.VoltageBarScaleWidth;

                var p1tmp = new Point();
                var p2tmp = new Point();

                foreach (var pinIdx in pinIndexes)
                {
                    var pinProp = ShowingPinProperties[pinIdx];

                    var pinName = pinProp.Name;
                    var hasFail = pinProp.HasFail;
                    var drawingY = (pinIdx + 1) * SpacingProperties.ActualWaveformHeight + baseY;
                    var rectPosiTopY = drawingY - SpacingProperties.ActualWaveformHeight;
                    var pinVoltLevels = GetDrawingVoltageLevels(pinIdx);

                    leftPoint.Y = drawingY;
                    rightPoint.Y = drawingY;
                    // drawing waveform bottom line
                    if (drawingY < SpacingProperties.WaveformBottomLineY &&
                        drawingY > SpacingProperties.WaveformTopLineY)
                    {
                        dc.DrawLine(pen, leftPoint, rightPoint);
                    }

                    //drawing top mark per per bar.
                    if (!string.IsNullOrEmpty(pinProp.TopLabel))
                    {
                        var labelTopY = drawingY - SpacingProperties.ActualWaveformHeight;
                        var labelFormatText = GetDrawingFormattedText(pinProp.TopLabel, pinTopLabelTextBrush, SpacingProperties.PinTopLabelTextSize);

                        var labelHeight = halfActualWH - SpacingProperties.ErrorBarFailCircleRadius * 2;
                        //123
                        if (labelHeight > 0)
                        {
                            DrawingRect(dc, new Point(SpacingProperties.TopLeft.X + 0.1d, labelTopY + 0.1d), new Size(SpacingProperties.PBW - 0.1d, labelHeight - 0.1d),
                                new Point(SpacingProperties.TopLeft.X, SpacingProperties.WaveformTopLineY), new Size(SpacingProperties.PBW, SpacingProperties.N_WH),
                                null, pinTopLabelRectBrush);
                        }

                        var labelTextX = SpacingProperties.TopLeft.X + SpacingProperties.PBW / 2.0d - labelFormatText.WidthIncludingTrailingWhitespace / 2.0d;
                        var labelTextY = labelTopY + labelHeight / 2.0d - labelFormatText.Height / 2.0d;

                        if (labelTextX > SpacingProperties.TopLeft.X && labelTextY > labelTopY &&
                            labelTextY > SpacingProperties.WaveformTopLineY && labelTextY + labelFormatText.Height < SpacingProperties.WaveformBottomLineY)
                        {
                            dc.DrawText(labelFormatText, new Point(labelTextX, labelTextY));
                        }
                    }


                    // drawing fail circle
                    var failCirclePointY = drawingY - halfActualWH - pinBarFailCircleRadius;
                    if (hasFail && failCirclePointY - pinBarFailCircleRadius >= SpacingProperties.WaveformTopLineY &&
                        failCirclePointY + pinBarFailCircleRadius <= SpacingProperties.WaveformBottomLineY)
                    {
                        failCirclePoint.Y = drawingY - halfActualWH - pinBarFailCircleRadius;
                        dc.DrawEllipse(failCircleBrush, null, failCirclePoint, pinBarFailCircleRadius, pinBarFailCircleRadius);
                    }

                    // drawing pin name
                    var formatText = GetDrawingFormattedText(pinName, pinNameTextBrush, SpacingProperties.PinNameTextSize);
                    var drawingTextY = drawingY - halfActualWH + (halfActualWH - formatText.Height) / 2.0;
                    var drawingTextX = SpacingProperties.TopLeft.X + SpacingProperties.PBW / 2.0d - formatText.Width / 2.0d;
                    if ((drawingTextY >= SpacingProperties.WaveformTopLineY) &&
                        ((drawingTextY + formatText.Height) < SpacingProperties.WaveformBottomLineY))
                    {
                        dc.DrawText(formatText, new Point(drawingTextX, drawingTextY));
                    }

                    var voltRange = pinProp.VoltageRange;
                    var maxVolt = voltRange.MaxVolt;
                    var minVolt = voltRange.MinVolt;
                    var voltLevels = voltRange.VoltageLevels?.ToList();

                    var PaddingSpaceLen = (SpacingProperties.VoltageBarTextTopBottomPadding * SpacingProperties.ActualWaveformHeight) / 2.0d;
                    var topBottomPadding = SpacingProperties.VoltageBarTextTopBottomPadding;

                    var maxVoltLevel = pinVoltLevels[DefaultValues.MaxVoltageName];
                    var minVoltLevel = pinVoltLevels[DefaultValues.MinVoltageName];

                    // Drawing Max Volt

                    if (voltRange.ShowMaxVolt)
                    {
                        if (maxVoltLevel.TextPosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                        maxVoltLevel.TextPosiLeft.Y + maxVoltLevel.Formatted.Height < SpacingProperties.WaveformBottomLineY)
                        {
                            dc.DrawText(maxVoltLevel.Formatted, maxVoltLevel.TextPosiLeft);
                        }

                        if (maxVoltLevel.ScaleLinePosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                            maxVoltLevel.ScaleLinePosiLeft.Y < SpacingProperties.WaveformBottomLineY)
                        {
                            p1tmp = maxVoltLevel.ScaleLinePosiLeft;
                            p2tmp = p1tmp;
                            p2tmp.Offset(SpacingProperties.VoltageBarScaleWidth, 0.0d);
                            dc.DrawLine(pen, p1tmp, p2tmp);

                            p1tmp = maxVoltLevel.LinePosiLeft;
                            p2tmp = p1tmp;
                            p2tmp.X = SpacingProperties.WaveformRightLineX;
                            dc.DrawLine(maxminVoltPen, p1tmp, p2tmp);
                        }
                    }

                    // Drawing Min Volt
                    if (voltRange.ShowMinVolt)
                    {
                        if (minVoltLevel.TextPosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                        minVoltLevel.TextPosiLeft.Y + minVoltLevel.Formatted.Height < SpacingProperties.WaveformBottomLineY)
                        {
                            dc.DrawText(minVoltLevel.Formatted, minVoltLevel.TextPosiLeft);
                        }

                        if (minVoltLevel.ScaleLinePosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                            minVoltLevel.ScaleLinePosiLeft.Y < SpacingProperties.WaveformBottomLineY)
                        {
                            p1tmp = minVoltLevel.ScaleLinePosiLeft;
                            p2tmp = p1tmp;
                            p2tmp.Offset(SpacingProperties.VoltageBarScaleWidth, 0.0d);
                            dc.DrawLine(pen, p1tmp, p2tmp);

                            p1tmp = minVoltLevel.LinePosiLeft;
                            p2tmp = p1tmp;
                            p2tmp.X = SpacingProperties.WaveformRightLineX;
                            dc.DrawLine(maxminVoltPen, p1tmp, p2tmp);
                        }
                    }

                    // Drawing Marks
                    if (!(voltLevels is null))
                    {
                        foreach (var voltMark in voltLevels)
                        {

                            var voltLevel = pinVoltLevels[voltMark.Name];
                            var voltLevelIndex = voltLevels.IndexOf(voltMark);
                            var linePen = voltageLevelPens.ContainsKey(voltLevelIndex) ? voltageLevelPens[voltLevelIndex] : voltageLevelPens[DefaultValues.NullValue];
                            if (!voltMark.Show) continue;

                            if (voltLevel.TextPosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                        voltLevel.TextPosiLeft.Y + voltLevel.Formatted.Height < SpacingProperties.WaveformBottomLineY)
                            {
                                dc.DrawText(voltLevel.Formatted, voltLevel.TextPosiLeft);
                            }

                            if (voltLevel.ScaleLinePosiLeft.Y > SpacingProperties.WaveformTopLineY &&
                                voltLevel.ScaleLinePosiLeft.Y < SpacingProperties.WaveformBottomLineY)
                            {
                                p1tmp = voltLevel.ScaleLinePosiLeft;
                                p2tmp = p1tmp;
                                p2tmp.Offset(SpacingProperties.VoltageBarScaleWidth, 0.0d);
                                dc.DrawLine(pen, p1tmp, p2tmp);

                                p1tmp = voltLevel.LinePosiLeft;
                                p2tmp = p1tmp;
                                p2tmp.X = SpacingProperties.WaveformRightLineX;
                                dc.DrawLine(linePen, p1tmp, p2tmp);
                            }
                        }
                    }

                }
            }
            finally
            {
                dc.Close();

#if DEBUG
                StopWatch(nameof(RenderC));
#endif
            }

        }

        /// <summary>
        /// Waveform Scope
        /// </summary>
        private void RenderD()
        {

#if DEBUG
            StartWatch(nameof(RenderD));
#endif
            if (D is null) D = new DrawingVisual();
            var dc = D.RenderOpen();
            try
            {
                var pinIndexes = GetDrawingPinIndexes()?.ToList();
                var cycleIndexes = GetDrawingCycleIndexes()?.ToList();

                if (pinIndexes is null || pinIndexes.Count == 0) return;
                if (cycleIndexes is null || cycleIndexes.Count == 0) return;
                var voltageBarTextBrush = new SolidColorBrush(ColorProperties.VoltageText);
                voltageBarTextBrush.Freeze();
                var waveformLinePens = new Dictionary<int, Pen>();
                var waveformlineDefaultBrush = new SolidColorBrush(ColorProperties.DefaultWaveformLine);
                waveformlineDefaultBrush.Freeze();
                var pen = new Pen(waveformlineDefaultBrush, SpacingProperties.LineWidth);
                pen.Freeze();
                waveformLinePens.Add(DefaultValues.NullValue, pen);
                var count = this.WaveformLineProperties.Count;
                for (int lineIdx = 0; lineIdx < count; lineIdx++)
                {
                    var lineColor = WaveformLineProperties[lineIdx].LineColor;
                    var waveformlineBrush = new SolidColorBrush(lineColor.HasValue ? lineColor.Value : ColorProperties.DefaultWaveformLine);
                    waveformlineBrush.Freeze();
                    pen = new Pen(waveformlineBrush, WaveformLineProperties[lineIdx].Thickness);
                    pen.Freeze();
                    waveformLinePens.Add(lineIdx, pen);
                }

                var naNWaveformLineBrush = new SolidColorBrush(ColorProperties.DefaultNaNWaveformLine);
                naNWaveformLineBrush.Freeze();
                var nanPosiPen = new Pen(naNWaveformLineBrush, SpacingProperties.LineWidth) { DashStyle = DashStyles.Dash };
                nanPosiPen.Freeze();

                var naNWaveformPosiFormat = GetDrawingFormattedText("!", naNWaveformLineBrush, SpacingProperties.NaNWaveformTextSize);

                var totalLineCnt = 0;
                var maxLineCntOfCycle = 0;
                var firstCycleIndx = cycleIndexes.First();
                var lastCycleIndx = cycleIndexes.Last();
                var totalPointCnt = lastCycleIndx == firstCycleIndx ? CycleProperties[firstCycleIndx].PointsSize : CycleProperties[lastCycleIndx].LastPointsSum + CycleProperties[lastCycleIndx].PointsSize - CycleProperties[firstCycleIndx].LastPointsSum;
                var firstPinIndex = pinIndexes.First();
                var firstCycleIndex = cycleIndexes.First();
                var firstCycleProp = CycleProperties[firstCycleIndex];
                var MarkValueWaveformLeftPointPerPin = new Dictionary<int, MarkValue[]>();
                var waveformLeftX = SpacingProperties.WaveformLeftLineX;
                var waveformRightX = SpacingProperties.WaveformRightLineX;
                var waveformTopY = SpacingProperties.WaveformTopLineY;
                var waveformBottomY = SpacingProperties.WaveformBottomLineY;

                var VoltageLevels = new Dictionary<int, Dictionary<string, DrawingVoltageLevel>>();
                //var totalFailCycles = 0;
                foreach (var pinIdx in pinIndexes)
                {
                    var pinProp = ShowingPinProperties[pinIdx];
                    //var maxVolt = pinProp.VoltageRange.MaxVolt;
                    //var minVolt = pinProp.VoltageRange.MinVolt;
                    //var drawingX = waveformLeftX - ScrollHornizontalValue;
                    //var drawingY = waveformTopY + pinIdx * SpacingProperties.ActualWaveformHeight - ScrollVerticalValue;

                    var voltageLevel = GetDrawingVoltageLevels(pinIdx);
                    VoltageLevels.Add(pinIdx, voltageLevel);

                    var maxVoltLevel = voltageLevel[DefaultValues.MaxVoltageName];
                    var minVoltLevel = voltageLevel[DefaultValues.MinVoltageName];
                    var maxVoltFormatHeight = maxVoltLevel.Formatted.Height;
                    var minVoltFormatHeight = minVoltLevel.Formatted.Height;
                    var maxVoltHalfHeight = maxVoltFormatHeight / 2.0d;
                    var minVoltHalfHeight = minVoltFormatHeight / 2.0d;
                    var maxVoltToMinVoltHeight = minVoltLevel.LinePosiLeft.Y - maxVoltLevel.LinePosiLeft.Y;
                    var drawingWaveformTopY = maxVoltLevel.LinePosiLeft.Y;
                    var markValues = pinProp.VoltageRange.VoltageLevels;
                    var markValuesCount = markValues?.Count ?? 0;
                    var showingPinIdx = pinIdx - firstPinIndex;


                    var drawingMarkValues = new MarkValue[2 + markValuesCount];
                    MarkValueWaveformLeftPointPerPin.Add(showingPinIdx, drawingMarkValues);
                    int maxVoltIndx = markValuesCount + 1;
                    drawingMarkValues[maxVoltIndx] = new MarkValue();
                    drawingMarkValues[maxVoltIndx].Name = DefaultValues.MaxVoltageName;
                    drawingMarkValues[maxVoltIndx].Voltage = pinProp.VoltageRange.MaxVolt;
                    drawingMarkValues[maxVoltIndx].LeftPoint = maxVoltLevel.LinePosiLeft;

                    int minVoltIdx = markValuesCount;
                    drawingMarkValues[minVoltIdx] = new MarkValue();
                    drawingMarkValues[minVoltIdx].Name = DefaultValues.MinVoltageName;
                    drawingMarkValues[minVoltIdx].Voltage = pinProp.VoltageRange.MinVolt;
                    drawingMarkValues[minVoltIdx].LeftPoint = minVoltLevel.LinePosiLeft;

                    for (int markValIdx = 0; markValIdx < markValuesCount; markValIdx++)
                    {
                        var markValue = markValues[markValIdx];
                        drawingMarkValues[markValIdx] = new MarkValue();
                        drawingMarkValues[markValIdx].Name = markValue.Name;
                        drawingMarkValues[markValIdx].Voltage = markValue.Voltage;
                        var len = SpacingProperties.VoltageToPointY(markValue.Voltage, pinProp.VoltageRange.MaxVolt, pinProp.VoltageRange.MinVolt, maxVoltToMinVoltHeight);
                        drawingMarkValues[markValIdx].LeftPoint = voltageLevel[markValue.Name].LinePosiLeft;
                    }

                    foreach (var cycleIdx in cycleIndexes)
                    {
                        var drawingCycle = pinProp.DrawingCycles[firstCycleIndex];
                        totalLineCnt += drawingCycle.LineCount;
                        if (drawingCycle.LineCount > maxLineCntOfCycle)
                        {
                            maxLineCntOfCycle = drawingCycle.LineCount;
                        }

                    }
                }
                var waveformTopLeftPoint = new Point();
                var waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding = new Point();
                var lastWaveformPoints = new Point[maxLineCntOfCycle];
                var currWaveformPoints = new Point[maxLineCntOfCycle];




                var comparePoint1 = new Point();
                var comparePoint2 = new Point();
                var failCycleRectBrush = new SolidColorBrush(ColorProperties.FailCycle);
                failCycleRectBrush.Freeze();
                var compareLineBrush = new SolidColorBrush(ColorProperties.CompareLine);
                compareLineBrush.Freeze();
                var compareLinePen = new Pen(compareLineBrush, SpacingProperties.LineWidth);
                compareLinePen.Freeze();
                var compareWindowBrush = new SolidColorBrush(ColorProperties.CompareWindow);
                compareWindowBrush.Freeze();
                var topLabelBrush = new SolidColorBrush(ColorProperties.CycleTopLabel);
                topLabelBrush.Freeze();

                for (int pointIdx = 0; pointIdx < maxLineCntOfCycle; pointIdx++)
                {
                    lastWaveformPoints[pointIdx] = new Point();
                    currWaveformPoints[pointIdx] = new Point();
                }

                const double CalculateError = 0.00005;

                foreach (var pinIdx in pinIndexes)
                {
                    var pinProp = ShowingPinProperties[pinIdx];
                    var pinLevels = GetDrawingVoltageLevels(pinIdx);
                    var drawingX = waveformLeftX - ScrollHornizontalValue;
                    var drawingY = waveformTopY + pinIdx * SpacingProperties.ActualWaveformHeight - ScrollVerticalValue;

                    waveformTopLeftPoint.X = drawingX;
                    waveformTopLeftPoint.Y = drawingY;

                    waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X = drawingX;
                    waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y = drawingY;



                    var maxVolt = pinProp.VoltageRange.MaxVolt;
                    var minVolt = pinProp.VoltageRange.MinVolt;
                    var maxVoltLevel = pinLevels[DefaultValues.MaxVoltageName];
                    var minVoltLevel = pinLevels[DefaultValues.MinVoltageName];

                    var maxVoltHeight = maxVoltLevel.Formatted.Height;
                    var minVoltHeight = minVoltLevel.Formatted.Height;
                    var maxVoltHalfHeight = maxVoltHeight / 2.0d;
                    var minVoltHalfHeight = minVoltHeight / 2.0d;
                    var maxVoltToMinVoltHeight = minVoltLevel.LinePosiLeft.Y - maxVoltLevel.LinePosiLeft.Y;

                    waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Offset(0, SpacingProperties.VoltageBarTextTopBottomPadding + maxVoltHalfHeight);


                    foreach (var cycleIdx in cycleIndexes)
                    {
                        var cycleProp = CycleProperties[cycleIdx];
                        var drawingCycle = pinProp.DrawingCycles[cycleIdx];
                        var preCycleIndx = cycleIdx - 1;
                        var currDrawingXPosi = preCycleIndx < 0 ? 0 : CycleProperties[preCycleIndx].DrawingXPosition;
                        waveformTopLeftPoint.X = currDrawingXPosi + waveformLeftX - ScrollHornizontalValue;

                        if (drawingCycle.IsFail)
                        {
                            DrawingRect(dc, waveformTopLeftPoint,
                                new Size(cycleProp.PointsSize * SpacingProperties.ActualPixelPerPoint, SpacingProperties.ActualWaveformHeight),
                                new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformTopLineY),
                                new Size(SpacingProperties.WW, SpacingProperties.N_WH), null, failCycleRectBrush);
                        }

                        for (int lineIdx = 0; lineIdx < drawingCycle.LineCount; lineIdx++)
                        {
                            if (!WaveformLineProperties[lineIdx].Show) continue;

                            int pointIdx = 0;
                            var voltage = drawingCycle[lineIdx, pointIdx];
                            var voltagePointX = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X + currDrawingXPosi + pointIdx * SpacingProperties.ActualPixelPerPoint;
                            var voltagePointY = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y + SpacingProperties.VoltageToPointY(voltage, maxVolt, minVolt, maxVoltToMinVoltHeight);
                            var waveformLinePen = waveformLinePens.ContainsKey(lineIdx) ? waveformLinePens[lineIdx] : waveformLinePens[DefaultValues.NullValue];
                            if ((cycleIdx - firstCycleIndex) > 0)
                            {
                                currWaveformPoints[lineIdx].X = voltagePointX;

                                if (!double.IsNaN(voltage))
                                {
                                    currWaveformPoints[lineIdx].Y = voltagePointY;
                                }
                                else
                                {
                                    currWaveformPoints[lineIdx].Y = lastWaveformPoints[lineIdx].Y;
                                    voltagePointY = currWaveformPoints[lineIdx].Y;
                                }

                                bool currPointCondition =
                                    currWaveformPoints[lineIdx].X >= waveformLeftX &&
                                    currWaveformPoints[lineIdx].X <= waveformRightX &&
                                    currWaveformPoints[lineIdx].Y >= waveformTopY &&
                                    currWaveformPoints[lineIdx].Y <= waveformBottomY &&
                                    currWaveformPoints[lineIdx].Y >= maxVoltLevel.LinePosiLeft.Y &&
                                    currWaveformPoints[lineIdx].Y <= minVoltLevel.LinePosiLeft.Y;

                                bool lastPointCondition =
                                    lastWaveformPoints[lineIdx].X >= waveformLeftX &&
                                    lastWaveformPoints[lineIdx].X <= waveformRightX &&
                                    lastWaveformPoints[lineIdx].Y >= waveformTopY &&
                                    lastWaveformPoints[lineIdx].Y <= waveformBottomY &&
                                    lastWaveformPoints[lineIdx].Y >= maxVoltLevel.LinePosiLeft.Y &&
                                    lastWaveformPoints[lineIdx].Y <= minVoltLevel.LinePosiLeft.Y;

                                if (currPointCondition && lastPointCondition)
                                {
                                    if (double.IsNaN(voltage))
                                    {
                                        dc.DrawLine(nanPosiPen, lastWaveformPoints[lineIdx], currWaveformPoints[lineIdx]);
                                        var formatPosi = currWaveformPoints[lineIdx];
                                        formatPosi.Offset(-1 * naNWaveformPosiFormat.WidthIncludingTrailingWhitespace / 2.0d, -1 * naNWaveformPosiFormat.Height / 2.0d);
                                        dc.DrawText(naNWaveformPosiFormat, formatPosi);
                                    }
                                    else
                                    {
                                        dc.DrawLine(waveformLinePen, lastWaveformPoints[lineIdx], currWaveformPoints[lineIdx]);
                                    }
                                }
                                else
                                {
                                    var needDrawing = true;
                                    Point p1 = new Point(), p2 = new Point();
                                    var TopLineY = (maxVoltLevel.LinePosiLeft.Y < waveformTopY) ? waveformTopY : maxVoltLevel.LinePosiLeft.Y;
                                    var BottomLineY = (minVoltLevel.LinePosiLeft.Y < waveformBottomY) ? minVoltLevel.LinePosiLeft.Y : waveformBottomY;
                                    var currPoint = currWaveformPoints[lineIdx];
                                    var lastPoint = lastWaveformPoints[lineIdx];
                                    if (currPoint.Y < TopLineY && lastPoint.Y < TopLineY)
                                    {
                                        var currPointDiff = Math.Abs(currPoint.Y - TopLineY);
                                        var lastPointDiff = Math.Abs(lastPoint.Y - TopLineY);
                                        if (currPointDiff > CalculateError || lastPointDiff > CalculateError)
                                        {
                                            needDrawing = false;
                                        }
                                        else
                                        {
                                            currPoint.Y = TopLineY;
                                            lastPoint.Y = TopLineY;
                                        }
                                    }
                                    else if (currPoint.Y > BottomLineY && lastPoint.Y > BottomLineY)
                                    {
                                        var currPointDiff = Math.Abs(currPoint.Y - BottomLineY);
                                        var lastPointDiff = Math.Abs(lastPoint.Y - BottomLineY);
                                        if (currPointDiff > CalculateError || lastPointDiff > CalculateError)
                                        {
                                            needDrawing = false;
                                        }
                                        else
                                        {
                                            currPoint.Y = BottomLineY;
                                            lastPoint.Y = BottomLineY;
                                        }
                                    }
                                    else if (lastPoint.Y < TopLineY && currPoint.Y >= TopLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = TopLineY;
                                        p2.Y = TopLineY;
                                        lastPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y >= TopLineY && currPoint.Y < TopLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = TopLineY;
                                        p2.Y = TopLineY;
                                        currPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y > BottomLineY && currPoint.Y <= BottomLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = BottomLineY;
                                        p2.Y = BottomLineY;
                                        lastPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y <= BottomLineY && currPoint.Y > BottomLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = BottomLineY;
                                        p2.Y = BottomLineY;
                                        currPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }

                                    // out of waveform left x and right x
                                    if (currPoint.X <= waveformLeftX || lastPoint.X >= waveformRightX) needDrawing = false;

                                    if (needDrawing)
                                    {
                                        if (lastPoint.X < waveformLeftX)
                                        {
                                            p1.X = waveformLeftX;
                                            p2.X = waveformLeftX;
                                            p1.Y = TopLineY;
                                            p2.Y = BottomLineY;
                                            lastPoint = CrossPoint(p1, p2, lastPoint, currPoint).Value;
                                        }
                                        else if (currPoint.X > waveformRightX)
                                        {
                                            p1.X = waveformRightX;
                                            p2.X = waveformRightX;
                                            p1.Y = TopLineY;
                                            p2.Y = BottomLineY;
                                            currPoint = CrossPoint(p1, p2, lastPoint, currPoint).Value;
                                        }

                                        dc.DrawLine(waveformLinePen, lastPoint, currPoint);
                                    }
                                }

                            }

                            lastWaveformPoints[lineIdx].X = voltagePointX;
                            lastWaveformPoints[lineIdx].Y = voltagePointY;

                            if ((cycleIdx - firstCycleIndex) == 0 && pointIdx == 0 && double.IsNaN(voltage))
                            {
                                var formatPosi = lastWaveformPoints[lineIdx];
                                formatPosi.Offset(-1 * naNWaveformPosiFormat.WidthIncludingTrailingWhitespace / 2.0d, -1 * naNWaveformPosiFormat.Height / 2.0d);
                                if (formatPosi.X >= waveformLeftX - naNWaveformPosiFormat.WidthIncludingTrailingWhitespace / 2.0d) dc.DrawText(naNWaveformPosiFormat, formatPosi);
                            }


                            for (pointIdx = 1; pointIdx < drawingCycle.PointSize; pointIdx++)
                            {
                                voltage = drawingCycle[lineIdx, pointIdx];
                                voltagePointX = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X + currDrawingXPosi + pointIdx * SpacingProperties.ActualPixelPerPoint;
                                voltagePointY = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y + SpacingProperties.VoltageToPointY(voltage, maxVolt, minVolt, maxVoltToMinVoltHeight);

                                currWaveformPoints[lineIdx].X = voltagePointX;

                                if (!double.IsNaN(voltage))
                                {
                                    currWaveformPoints[lineIdx].Y = voltagePointY;
                                }
                                else
                                {
                                    currWaveformPoints[lineIdx].Y = lastWaveformPoints[lineIdx].Y;
                                    voltagePointY = currWaveformPoints[lineIdx].Y;
                                }

                                bool currPointCondition =
                                    currWaveformPoints[lineIdx].X >= waveformLeftX &&
                                    currWaveformPoints[lineIdx].X <= waveformRightX &&
                                    currWaveformPoints[lineIdx].Y >= waveformTopY &&
                                    currWaveformPoints[lineIdx].Y <= waveformBottomY &&
                                    currWaveformPoints[lineIdx].Y >= maxVoltLevel.LinePosiLeft.Y &&
                                    currWaveformPoints[lineIdx].Y <= minVoltLevel.LinePosiLeft.Y;

                                bool lastPointCondition =
                                    lastWaveformPoints[lineIdx].X >= waveformLeftX &&
                                    lastWaveformPoints[lineIdx].X <= waveformRightX &&
                                    lastWaveformPoints[lineIdx].Y >= waveformTopY &&
                                    lastWaveformPoints[lineIdx].Y <= waveformBottomY &&
                                    lastWaveformPoints[lineIdx].Y >= maxVoltLevel.LinePosiLeft.Y &&
                                    lastWaveformPoints[lineIdx].Y <= minVoltLevel.LinePosiLeft.Y;

                                if (currPointCondition && lastPointCondition)
                                {
                                    if (double.IsNaN(voltage))
                                    {
                                        dc.DrawLine(nanPosiPen, lastWaveformPoints[lineIdx], currWaveformPoints[lineIdx]);
                                        var formatPosi = currWaveformPoints[lineIdx];
                                        formatPosi.Offset(-1 * naNWaveformPosiFormat.WidthIncludingTrailingWhitespace / 2.0d, -1 * naNWaveformPosiFormat.Height / 2.0d);
                                        dc.DrawText(naNWaveformPosiFormat, formatPosi);
                                    }
                                    else
                                    {
                                        dc.DrawLine(waveformLinePen, lastWaveformPoints[lineIdx], currWaveformPoints[lineIdx]);
                                    }
                                }
                                else
                                {
                                    var needDrawing = true;
                                    Point p1 = new Point(), p2 = new Point();
                                    var TopLineY = (maxVoltLevel.LinePosiLeft.Y < waveformTopY) ? waveformTopY : maxVoltLevel.LinePosiLeft.Y;
                                    var BottomLineY = (minVoltLevel.LinePosiLeft.Y < waveformBottomY) ? minVoltLevel.LinePosiLeft.Y : waveformBottomY;
                                    var currPoint = currWaveformPoints[lineIdx];
                                    var lastPoint = lastWaveformPoints[lineIdx];
                                    if (currPoint.Y < TopLineY && lastPoint.Y < TopLineY)
                                    {
                                        var currPointDiff = Math.Abs(currPoint.Y - TopLineY);
                                        var lastPointDiff = Math.Abs(lastPoint.Y - TopLineY);
                                        if (currPointDiff > CalculateError || lastPointDiff > CalculateError)
                                        {
                                            needDrawing = false;
                                        }
                                        else
                                        {
                                            currPoint.Y = TopLineY;
                                            lastPoint.Y = TopLineY;
                                        }
                                    }
                                    else if (currPoint.Y > BottomLineY && lastPoint.Y > BottomLineY)
                                    {
                                        var currPointDiff = Math.Abs(currPoint.Y - BottomLineY);
                                        var lastPointDiff = Math.Abs(lastPoint.Y - BottomLineY);
                                        if (currPointDiff > CalculateError || lastPointDiff > CalculateError)
                                        {
                                            needDrawing = false;
                                        }
                                        else
                                        {
                                            currPoint.Y = BottomLineY;
                                            lastPoint.Y = BottomLineY;
                                        }
                                    }
                                    else if (lastPoint.Y < TopLineY && currPoint.Y >= TopLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = TopLineY;
                                        p2.Y = TopLineY;
                                        lastPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y >= TopLineY && currPoint.Y < TopLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = TopLineY;
                                        p2.Y = TopLineY;
                                        currPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y > BottomLineY && currPoint.Y <= BottomLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = BottomLineY;
                                        p2.Y = BottomLineY;
                                        lastPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }
                                    else if (lastPoint.Y <= BottomLineY && currPoint.Y > BottomLineY)
                                    {
                                        p1.X = waveformLeftX;
                                        p2.X = waveformRightX;
                                        p1.Y = BottomLineY;
                                        p2.Y = BottomLineY;
                                        currPoint = CrossPoint(p1, p2, currPoint, lastPoint).Value;
                                    }

                                    // out of waveform left x and right x
                                    if (currPoint.X <= waveformLeftX || lastPoint.X >= waveformRightX) needDrawing = false;

                                    if (needDrawing)
                                    {
                                        if (lastPoint.X < waveformLeftX)
                                        {
                                            p1.X = waveformLeftX;
                                            p2.X = waveformLeftX;
                                            p1.Y = TopLineY;
                                            p2.Y = BottomLineY;
                                            lastPoint = CrossPoint(p1, p2, lastPoint, currPoint).Value;
                                        }
                                        else if (currPoint.X > waveformRightX)
                                        {
                                            p1.X = waveformRightX;
                                            p2.X = waveformRightX;
                                            p1.Y = TopLineY;
                                            p2.Y = BottomLineY;
                                            currPoint = CrossPoint(p1, p2, lastPoint, currPoint).Value;
                                        }

                                        dc.DrawLine(waveformLinePen, lastPoint, currPoint);
                                    }
                                }

                                lastWaveformPoints[lineIdx] = currWaveformPoints[lineIdx];
                            }
                        }



                        if (!string.IsNullOrEmpty(drawingCycle.TopLabel))
                        {
                            var labelTextFormat = GetDrawingFormattedText(drawingCycle.TopLabel, topLabelBrush, SpacingProperties.CycleTopLabelTextSize);
                            var pointX = waveformTopLeftPoint.X + (cycleProp.PointsSize * SpacingProperties.ActualPixelPerPoint) / 2.0d - labelTextFormat.WidthIncludingTrailingWhitespace / 2.0d;
                            var halfLabelHeight = labelTextFormat.Height / 2.0d;
                            var pointY = waveformTopLeftPoint.Y + halfLabelHeight;
                            if (pointX > waveformTopLeftPoint.X && SpacingProperties.ActualWaveformHeight > halfLabelHeight &&
                                SpacingProperties.WaveformLeftLineX < pointX && SpacingProperties.WaveformRightLineX > pointX + labelTextFormat.WidthIncludingTrailingWhitespace &&
                                SpacingProperties.WaveformTopLineY < pointY && SpacingProperties.WaveformBottomLineY > pointY + labelTextFormat.Height)
                                dc.DrawText(labelTextFormat, new Point(pointX, pointY));
                        }

                        if (!(drawingCycle.DrawingCompares is null))
                        {
                            var drawingMarkValues = MarkValueWaveformLeftPointPerPin[pinIdx - firstPinIndex].ToDictionary(item => item.Name);
                            foreach (var drawingCompare in drawingCycle.DrawingCompares)
                            {
                                var compareInterface = drawingCompare.CompareInterface;
                                var markName = drawingCompare.MarkValueName;

                                if (compareInterface == ECompareInterface.Strobe && drawingCompare.Compare != ECompare.NullValue)
                                {
                                    var compareStrobe = drawingCompare as DrawingCompareStrobe;
                                    var drawingMarkValue = drawingMarkValues[markName];
                                    comparePoint1.X = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X + currDrawingXPosi + compareStrobe.PointIndex * SpacingProperties.ActualPixelPerPoint;
                                    comparePoint1.Y = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y + (drawingCompare.Compare == ECompare.High ? maxVoltToMinVoltHeight : 0.0d);
                                    DrawingCompareArrow(dc, comparePoint1, drawingCompare.Compare, Math.Abs(comparePoint1.Y - drawingMarkValue.LeftPoint.Y), compareLinePen);
                                }
                                else if (compareInterface == ECompareInterface.Window && drawingCompare.Compare != ECompare.NullValue)
                                {
                                    var compareWindow = drawingCompare as DrawingCompareWindow;
                                    var drawingMarkValue = drawingMarkValues[markName];
                                    comparePoint1.X = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X + currDrawingXPosi + compareWindow.PointIndex1 * SpacingProperties.ActualPixelPerPoint;
                                    comparePoint1.Y = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y + (drawingCompare.Compare == ECompare.High ? maxVoltToMinVoltHeight : 0.0d);
                                    var absLen = Math.Abs(comparePoint1.Y - drawingMarkValue.LeftPoint.Y);
                                    DrawingCompareArrow(dc, comparePoint1, drawingCompare.Compare, absLen, compareLinePen);

                                    comparePoint2.X = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.X + currDrawingXPosi + compareWindow.PointIndex2 * SpacingProperties.ActualPixelPerPoint;
                                    comparePoint2.Y = waveformTopLeftPoint_IncludeMaxVoltHalfHeight_And_TextPadding.Y + (drawingCompare.Compare == ECompare.High ? maxVoltToMinVoltHeight : 0.0d);
                                    DrawingCompareArrow(dc, comparePoint2, drawingCompare.Compare, absLen, compareLinePen);

                                    var rectWidth = comparePoint2.X - comparePoint1.X;
                                    var rectHeight = absLen;

                                    DrawingCompareWindow(dc,
                                        new Point(comparePoint1.X,
                                        (drawingCompare.Compare == ECompare.High ? (comparePoint1.Y - absLen) : (comparePoint1.Y))),
                                        new Size(rectWidth, rectHeight), compareWindowBrush);
                                }

                            }
                        }


                    }

                }
            }
            finally
            {
                dc.Close();

#if DEBUG
                StopWatch(nameof(RenderD));
#endif
            }


        }

        public void MouseMove(Nullable<Point> point)
        {


            string mousePointText = $"Mouse Point: {(point is null ? "Leave" : $"{Math.Round(point.Value.X, 2)}, {Math.Round(point.Value.Y, 2)}")}";
#if DEBUG
            Debug.WriteLine(mousePointText);
            StartWatch(nameof(MouseMove));
#endif
            if (M is null) M = new DrawingVisual();
            var dc = M.RenderOpen();

            var mousePointTextBrush = new SolidColorBrush(ColorProperties.MousePointText);
            mousePointTextBrush.Freeze();

            var mouseCursorLineBrush = new SolidColorBrush(ColorProperties.MouseCursorLine);
            mouseCursorLineBrush.Freeze();

            var mouseCursorTextBrush = new SolidColorBrush(ColorProperties.MouseCursorText);
            mouseCursorTextBrush.Freeze();

            var mouseCursorLinePen = new Pen(mouseCursorLineBrush, SpacingProperties.MouseCursorThickness);
            mouseCursorLinePen.DashStyle = DashStyles.Dot;
            mouseCursorLinePen.Freeze();

            var informationBrush = new SolidColorBrush(ColorProperties.InformationText);
            informationBrush.Freeze();

            var mousePointPen = new Pen(mousePointTextBrush, SpacingProperties.LineWidth);
            mousePointPen.Freeze();

            var infoPen = new Pen(informationBrush, SpacingProperties.LineWidth);
            infoPen.Freeze();

            var voltageBarTextBrush = new SolidColorBrush(ColorProperties.VoltageText);
            voltageBarTextBrush.Freeze();

            var mousePointFormatText = GetDrawingFormattedText(mousePointText, mousePointTextBrush, SpacingProperties.LegendTextSize);

            var pointX = SpacingProperties.TopLeft.X + SpacingProperties.ActualMW - mousePointFormatText.WidthIncludingTrailingWhitespace;

            var pointY = SpacingProperties.TopLeft.Y + SpacingProperties.ActualMH + SpacingProperties.LegendHeight / 2.0d - mousePointFormatText.Height / 2.0d;

            dc.DrawText(mousePointFormatText, new Point(pointX, pointY));

            var infoTextXOffset = SpacingProperties.TopLeft.X;

            var intoLeftPoint = new Point(infoTextXOffset, SpacingProperties.TopLeft.Y + SpacingProperties.ActualMH + SpacingProperties.LegendHeight / 2.0d);

            var topLineY = SpacingProperties.WaveformTopLineY;
            var bottomLineY = SpacingProperties.WaveformBottomLineY;
            var leftLineX = SpacingProperties.WaveformLeftLineX;
            var rightLineX = SpacingProperties.WaveformRightLineX;

            var waveformAreaRect = new Rect(new Point(leftLineX, topLineY), new Point(rightLineX, bottomLineY));
            var isInnerWaveformArea = point.HasValue && waveformAreaRect.Contains(point.Value);

            if (isInnerWaveformArea)
            {
                var currPoint = point.Value;
                //pin name, cycle offset, timing, voltage
                var pinIndx = (int)((ScrollVerticalValue + currPoint.Y - SpacingProperties.TopLeft.Y - SpacingProperties.EBH) / SpacingProperties.ActualWaveformHeight);
                var pointIndexOfWaveform = (int)((ScrollHornizontalValue + currPoint.X - SpacingProperties.WaveformLeftLineX) / SpacingProperties.ActualPixelPerPoint);
                var cycleIndx = SearchCycleIndex(pointIndexOfWaveform); //CycleProperties.FirstOrDefault(item => item.DrawingXPosition >= currPoint.X - leftLineX + ScrollHornizontalValue)?.Index ?? -1;

                var showPinInfo = ShowingPinProperties.Count > pinIndx;
                var inCycleRanges = cycleIndx >= 0;

                var pointIndxOfCycle = 0;
                if (inCycleRanges)
                {
                    pointIndxOfCycle = (int)((currPoint.X + ScrollHornizontalValue - leftLineX - CycleProperties[cycleIndx].LastPointsSum * SpacingProperties.ActualPixelPerPoint) / SpacingProperties.ActualPixelPerPoint);
                }


                if (showPinInfo && inCycleRanges)
                {
                    var pinProp = ShowingPinProperties[pinIndx];
                    var pinCycle = pinProp.DrawingCycles[cycleIndx];
                    var voltages = new double[pinCycle.LineCount];
                    var voltInfoStr = string.Empty;
                    for (int lineIndx = 0; lineIndx < pinCycle.LineCount; lineIndx++)
                    {
                        var lineProp = WaveformLineProperties[lineIndx];
                        if (!lineProp.Show) continue;
                        voltages[lineIndx] = pinCycle.Voltages[lineIndx, pointIndxOfCycle];
                        var voltStr = SpacingProperties.ValueToVoltageStr(voltages[lineIndx]);
                        voltInfoStr += $"{lineProp.Name}({voltStr}), ";
                    }

                    var timeValue = (CycleProperties[cycleIndx].LastPointsSum + pointIndxOfCycle) * SpacingProperties.TimingResolution;
                    var timeStr = SpacingProperties.ValueToTimingStr(timeValue);
                    var currText = $"Pin Name: {pinProp.Name}, Offset: {CycleProperties[cycleIndx].Offset}, Index:{cycleIndx}, Volt: {voltInfoStr} @{timeStr}";
                    var currTextFormat = GetDrawingFormattedText(currText, informationBrush, SpacingProperties.LegendTextSize);
                    var p = intoLeftPoint;
                    p.Offset(SpacingProperties.InformationTextPadding, -1 * currTextFormat.Height / 2.0d);
                    dc.DrawText(currTextFormat, p);


                    var timingCursor = TimingCursors.FirstOrDefault(item => item.Moving);
                    if (timingCursor is null)
                    {
                        //mouse point mode
                        var pointTopOfCycle = new Point(SpacingProperties.WaveformLeftLineX + pointIndexOfWaveform * SpacingProperties.ActualPixelPerPoint - ScrollHornizontalValue,
                            SpacingProperties.WaveformTopLineY + pinIndx * SpacingProperties.ActualWaveformHeight - ScrollVerticalValue);
                        var topPoint = pointTopOfCycle;
                        var bottomPoint = pointTopOfCycle;
                        topPoint.Y = SpacingProperties.WaveformTopLineY;
                        bottomPoint.Y = SpacingProperties.WaveformBottomLineY;
                        dc.DrawLine(mouseCursorLinePen, topPoint, bottomPoint);

                        var mouseTimingTextPoint = bottomPoint;
                        var mouseTimingTextFormat = GetDrawingFormattedText(timeStr, mouseCursorTextBrush, SpacingProperties.MouseCursorTextSize);
                        mouseTimingTextPoint.Offset(mouseTimingTextFormat.WidthIncludingTrailingWhitespace / 2.0d * -1.0d, mouseTimingTextFormat.Height / 2.0d);
                        dc.DrawText(mouseTimingTextFormat, mouseTimingTextPoint);

                        var maxVolt = pinProp.VoltageRange.MaxVolt;
                        var minVolt = pinProp.VoltageRange.MinVolt;
                        var maxVoltHeight = GetDrawingFormattedText(SpacingProperties.ValueToVoltageStr(maxVolt), voltageBarTextBrush, SpacingProperties.VoltageBarTextSize).Height;
                        var minVoltHeight = GetDrawingFormattedText(SpacingProperties.ValueToVoltageStr(minVolt), voltageBarTextBrush, SpacingProperties.VoltageBarTextSize).Height;
                        var maxVoltHalfHeight = maxVoltHeight / 2.0d;
                        var minVoltHalfHeight = minVoltHeight / 2.0d;
                        var maxVoltToMinVoltHeight = SpacingProperties.ActualWaveformHeight - (2 * SpacingProperties.VoltageBarTextTopBottomPadding) - maxVoltHalfHeight - minVoltHalfHeight;
                        var voltageBarTopRight = topPoint;
                        var pinIndexVoltageTopY = SpacingProperties.WaveformTopLineY + pinIndx * SpacingProperties.ActualWaveformHeight - ScrollVerticalValue + maxVoltHalfHeight;
                        voltageBarTopRight.X = SpacingProperties.WaveformLeftLineX;

                        voltageBarTopRight.Offset(0, SpacingProperties.VoltageBarTextTopBottomPadding + maxVoltHalfHeight);

                        for (int lineIndx = 0; lineIndx < pinCycle.LineCount; lineIndx++)
                        {
                            var lineProp = WaveformLineProperties[lineIndx];
                            if (!lineProp.Show) continue;

                            var voltage = voltages[lineIndx];
                            var voltagePosiY = pinIndexVoltageTopY + SpacingProperties.VoltageToPointY(voltage, maxVolt, minVolt, maxVoltToMinVoltHeight);
                            var voltLeftPoint = new Point(voltageBarTopRight.X, voltagePosiY);
                            dc.DrawLine(mouseCursorLinePen, voltLeftPoint, new Point(topPoint.X, voltagePosiY));

                            var voltStr = SpacingProperties.ValueToVoltageStr(voltage);
                            var voltTextFormat = GetDrawingFormattedText(voltStr, mouseCursorTextBrush, SpacingProperties.MouseCursorTextSize);
                            voltLeftPoint.Offset(-1 * voltTextFormat.WidthIncludingTrailingWhitespace, voltTextFormat.Height / 2.0d * -1.0d);
                            dc.DrawText(voltTextFormat, voltLeftPoint);

                        }

                    }
                    else
                    {
                        timingCursor.CycleIndx = cycleIndx;
                        timingCursor.PointIndx = pointIndxOfCycle;
                        timingCursor.Time = (CycleProperties[cycleIndx].LastPointsSum + timingCursor.PointIndx) * SpacingProperties.TimingResolution;
                    }
                }
            }

            dc.Close();

#if DEBUG
            StopWatch(nameof(MouseMove));
#endif
        }

        /// <summary>
        /// Timing Cursor
        /// </summary>
        private void RenderT()
        {

#if DEBUG
            StartWatch(nameof(RenderT));
#endif
            if (T is null) T = new DrawingVisual();
            var dc = T.RenderOpen();

            var cycleRange = GetDrawingCycleIndexes()?.ToList();

            Dictionary<int, Pen> timingCursorPens = new Dictionary<int, Pen>(ColorProperties.TimingCursorMeasurements?.Length ?? 0 + 1);

            var defaultTimingCursorBrush = new SolidColorBrush(ColorProperties.DefaultTimingCursorMeasurement);
            defaultTimingCursorBrush.Freeze();
            var defaultTimingCursorPen = new Pen(defaultTimingCursorBrush, SpacingProperties.LineWidth);
            defaultTimingCursorPen.DashStyle = DashStyles.DashDot;
            defaultTimingCursorPen.Freeze();
            timingCursorPens.Add(DefaultValues.NullValue, defaultTimingCursorPen);
            for (int colIndx = 0; colIndx < (ColorProperties.TimingCursorMeasurements?.Length ?? 0); colIndx++)
            {
                var brush = new SolidColorBrush(ColorProperties.TimingCursorMeasurements[colIndx]);
                brush.Freeze();
                var _pen = new Pen(brush, SpacingProperties.LineWidth);
                _pen.DashStyle = DashStyles.DashDot;
                _pen.Freeze();
                timingCursorPens.Add(colIndx, _pen);
            }

            var topPoint = new Point(0, SpacingProperties.WaveformTopLineY);
            var bottomPoint = new Point(0, SpacingProperties.WaveformBottomLineY);
            var cursorNameTextPoint = new Point();
            var timingValTextPoint = new Point();

            for (int cursorIndx = 0; cursorIndx < TimingCursors.Count; cursorIndx++)
            {
                var timingCursor = TimingCursors[cursorIndx];

                var cycleIndex = timingCursor.CycleIndx;

                var pointIndex = timingCursor.PointIndx;

                if (!cycleRange.Contains(timingCursor.CycleIndx)) continue;

                var _pen = timingCursorPens.ContainsKey(cursorIndx) ? timingCursorPens[cursorIndx] : timingCursorPens[DefaultValues.NullValue];

                var xPosi = (CycleProperties[cycleIndex].LastPointsSum + pointIndex) * SpacingProperties.ActualPixelPerPoint - ScrollHornizontalValue + SpacingProperties.WaveformLeftLineX;

                var cursorName = timingCursor.Name;

                var cursorNameTextFormat = GetDrawingFormattedText(cursorName, _pen.Brush, SpacingProperties.TimingCursorTextSize);

                var timingValStr = SpacingProperties.ValueToTimingStr(timingCursor.Time);

                var timingValTextFormat = GetDrawingFormattedText(timingValStr, _pen.Brush, SpacingProperties.TimingCursorTextSize);

                topPoint.X = xPosi;
                bottomPoint.X = xPosi;

                cursorNameTextPoint.X = xPosi - cursorNameTextFormat.WidthIncludingTrailingWhitespace / 2.0d;
                cursorNameTextPoint.Y = topPoint.Y - cursorNameTextFormat.Height;
                timingValTextPoint.X = xPosi - timingValTextFormat.WidthIncludingTrailingWhitespace / 2.0d;
                timingValTextPoint.Y = bottomPoint.Y;

                if (topPoint.X < SpacingProperties.WaveformRightLineX && topPoint.X > SpacingProperties.WaveformLeftLineX)
                    dc.DrawLine(_pen, topPoint, bottomPoint);

                if (cursorNameTextPoint.X > SpacingProperties.WaveformLeftLineX && cursorNameTextPoint.X + cursorNameTextFormat.WidthIncludingTrailingWhitespace < SpacingProperties.WaveformRightLineX)
                    dc.DrawText(cursorNameTextFormat, cursorNameTextPoint);
                if (timingValTextPoint.X > SpacingProperties.WaveformLeftLineX && timingValTextPoint.X + timingValTextFormat.WidthIncludingTrailingWhitespace < SpacingProperties.WaveformRightLineX)
                    dc.DrawText(timingValTextFormat, timingValTextPoint);
            }

            var tCursor1 = TimingCursors.FirstOrDefault(item => item.Name == SpacingProperties.TimingMeasurement.CursorName1);
            var tCursor2 = TimingCursors.FirstOrDefault(item => item.Name == SpacingProperties.TimingMeasurement.CursorName2);
            if (tCursor1 is null || tCursor2 is null) { }
            else
            {
                var diffValue = Math.Abs(tCursor2.Time - tCursor1.Time);
                var diffTime = SpacingProperties.ValueToTimingStr(diffValue);
                var text = $"|{tCursor1.Name} - {tCursor2.Name}| = {diffTime}";
                var diffTimeTextFormat = GetDrawingFormattedText(text, timingCursorPens[DefaultValues.NullValue].Brush, SpacingProperties.LegendTextSize);
                var pointX = SpacingProperties.WaveformRightLineX - diffTimeTextFormat.WidthIncludingTrailingWhitespace;
                var pointY = SpacingProperties.TopLeft.Y - SpacingProperties.LegendHeight / 2 - diffTimeTextFormat.Height;

                dc.DrawText(diffTimeTextFormat, new Point(pointX, pointY));
            }

            dc.Close();
#if DEBUG
            StopWatch(nameof(RenderT));
#endif
        }


        public void AddTimingCursor(string name = null)
        {
            if (_TimingCursors.Any(item => item.Moving)) return;

            var searches = _TimingCursors.Where(item => item.Name.Length == 2 && item.Name[0] == 'X' && int.TryParse(item.Name[1].ToString(), out int num)).ToList();
            var max = searches.Count > 0 ? searches.Max(item => int.Parse(item.Name[1].ToString())) + 1 : 0;
            var timingCursor = new TimingCursor()
            {
                Name = name ?? $"X{max}",
                CycleIndx = DefaultValues.NullValue,
                PointIndx = DefaultValues.NullValue,
                Time = double.NaN,
                Moving = true,
            };
            _TimingCursors.Add(timingCursor);
        }

        public bool HasNextFail()
        {
            return HasFail(true, out int cycleIndex, out int pinIndex);
        }

        public bool HasPreviousFail()
        {
            return HasFail(false, out int cycleIndex, out int pinIndex);
        }

        private bool HasFail(bool searchForward, out int cycleIndex, out int pinIndex)
        {
            cycleIndex = -1;
            pinIndex = -1;

            var currPinIndex = (int)(ScrollVerticalValue / SpacingProperties.ActualWaveformHeight);
            var tempCurrCycleIndex = SearchCycleIndex(SpacingProperties.WaveformLeftLineX);

            if (tempCurrCycleIndex == -1) return false;

            int cycleStart = tempCurrCycleIndex;
            int cycleEnd = searchForward ? CycleProperties.Count : -1;
            int cycleStep = searchForward ? 1 : -1;

            int pinEnd = searchForward ? ShowingPinProperties.Count : -1;
            int pinStep = searchForward ? 1 : -1;

            for (cycleIndex = cycleStart; searchForward ? cycleIndex < cycleEnd : cycleIndex >= 0; cycleIndex += cycleStep)
            {
                if (!CycleProperties[cycleIndex].IsFail) continue;

                int pinStart = (tempCurrCycleIndex == cycleIndex ? (searchForward ? currPinIndex + 1 : currPinIndex - 1) : (searchForward ? 0 : ShowingPinProperties.Count - 1));

                for (pinIndex = pinStart; searchForward ? pinIndex < pinEnd : pinIndex >= 0; pinIndex += pinStep)
                {
                    if (!ShowingPinProperties[pinIndex].HasFail) break;
                    if (ShowingPinProperties[pinIndex].DrawingCycles[cycleIndex].IsFail)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void JumpToNextFail()
        {
            if (!HasFail(true, out int cycleIndex, out int pinIndex))
            {
                return;
            }

            var scrollHornizontalTemp = CycleProperties[cycleIndex].LastPointsSum * SpacingProperties.ActualPixelPerPoint;
            var scrollVerticalTemp = pinIndex * SpacingProperties.ActualWaveformHeight;
            ScrollHornizontalValue = scrollHornizontalTemp > MaxScrollHornizontal ? MaxScrollHornizontal : scrollHornizontalTemp;
            ScrollVerticalValue = scrollVerticalTemp > MaxScrollVertical ? MaxScrollVertical : scrollVerticalTemp;

            RaiseOnScrollValueChanged();
        }

        public void JumpToPreviousFail()
        {
            if (!HasFail(false, out int cycleIndex, out int pinIndex))
            {
                return;
            }

            var scrollHornizontalTemp = CycleProperties[cycleIndex].LastPointsSum * SpacingProperties.ActualPixelPerPoint;
            var scrollVerticalTemp = pinIndex * SpacingProperties.ActualWaveformHeight;

            ScrollHornizontalValue = scrollHornizontalTemp > MaxScrollHornizontal ? MaxScrollHornizontal : scrollHornizontalTemp;
            ScrollVerticalValue = scrollVerticalTemp > MaxScrollVertical ? MaxScrollVertical : scrollVerticalTemp;

            RaiseOnScrollValueChanged();
        }

        public bool CanDoJump(int patternOffset, int cycleIndex, out string errorMessage)
        {
            return CanDoJump(patternOffset, cycleIndex, out double value, out errorMessage);
        }

        internal bool CanDoJump(int patternOffset, int cycleIndex, out double hornizontalValue, out string errorMessage)
        {
            errorMessage = string.Empty;
            hornizontalValue = double.NaN;
            var firstCycleProp = CycleProperties.FirstOrDefault();
            var lastCycleProp = CycleProperties.LastOrDefault();
            if (firstCycleProp is null)
            {
                errorMessage = "Not Found Any Cycles";
                return false;
            }

            var targetOffset = CycleProperties.FirstOrDefault(item => item.Offset == patternOffset);

            if (targetOffset is null)
            {
                errorMessage = $"Pattern jump offset out of the range{patternOffset}";
                return false;
            }

            var offsetCycleIndex = CycleProperties.FirstOrDefault(item => item.Offset == patternOffset).Index;

            var targetCycleIndex = offsetCycleIndex + cycleIndex;

            if (targetCycleIndex < 0 || lastCycleProp.Index < targetCycleIndex)
            {
                errorMessage = $"Pattern jumps out of the range(Offset: {patternOffset}, CycleIndex: {cycleIndex}){Environment.NewLine}" +
                    $"Patter Range[{firstCycleProp.Offset} ~ {lastCycleProp.Offset}], Total Cycle Count: {CycleProperties.Count}";
                return false;
            }

            hornizontalValue = CycleProperties[targetCycleIndex].LastPointsSum * SpacingProperties.ActualPixelPerPoint;

            return true;
        }

        internal void MouseZoomInOut(Point mousePosi, Vector vector)
        {
            if (mousePosi.X < SpacingProperties.WaveformLeftLineX || mousePosi.X > SpacingProperties.WaveformRightLineX ||
                mousePosi.Y < SpacingProperties.WaveformTopLineY || mousePosi.Y > SpacingProperties.WaveformBottomLineY)
                return;

            var zoomCenterPosi = mousePosi + (vector / 2.0d);

            if (zoomCenterPosi.X < SpacingProperties.WaveformLeftLineX || zoomCenterPosi.X > SpacingProperties.WaveformRightLineX ||
                zoomCenterPosi.Y < SpacingProperties.WaveformTopLineY || zoomCenterPosi.Y > SpacingProperties.WaveformBottomLineY)
                return;

            var waveformRectCenterPosi = new Point((SpacingProperties.WaveformLeftLineX + SpacingProperties.WaveformRightLineX) / 2.0d,
                (SpacingProperties.WaveformTopLineY + SpacingProperties.WaveformBottomLineY) / 2.0d);

            //moving to center

            var recordCyclePointCount = (int)((zoomCenterPosi.X - SpacingProperties.WaveformLeftLineX + ScrollHornizontalValue) / SpacingProperties.ActualPixelPerPoint);
            var recordPinIndex = (int)((zoomCenterPosi.Y - SpacingProperties.WaveformTopLineY + ScrollVerticalValue) / SpacingProperties.ActualWaveformHeight);

            //zoom in/out
            var waveformXLen = SpacingProperties.WaveformRightLineX - SpacingProperties.WaveformLeftLineX;
            var waveformYLen = SpacingProperties.WaveformBottomLineY - SpacingProperties.WaveformTopLineY;
            var scopeXScaleVal = SpacingProperties.ScopeXScaleValue;
            var scopeYScaleVal = SpacingProperties.ScopeYScaleValue;
            if (vector.X > 0.0d)
            {
                scopeXScaleVal += 0.5;
            }
            else
            {
                scopeXScaleVal -= 0.5;
            }

            if (vector.Y > 0.0d)
            {
                scopeYScaleVal += 0.5;
            }
            else
            {
                scopeYScaleVal -= 0.5;
            }

            if (scopeXScaleVal > SpacingProperties.MaxScopeXScaleValue) scopeXScaleVal = SpacingProperties.MaxScopeXScaleValue;
            if (scopeXScaleVal < SpacingProperties.MinScopeXScaleValue) scopeXScaleVal = SpacingProperties.MinScopeXScaleValue;

            if (scopeYScaleVal > SpacingProperties.MaxScopeYScaleValue) scopeYScaleVal = SpacingProperties.MaxScopeYScaleValue;
            if (scopeYScaleVal < SpacingProperties.MinScopeYScaleValue) scopeYScaleVal = SpacingProperties.MinScopeYScaleValue;

            SpacingProperties.ScopeXScaleValue = scopeXScaleVal;
            SpacingProperties.ScopeYScaleValue = scopeYScaleVal;

            Render(ERenderDirect.ZoomInOut);

            var scrollHornizontalTemp = recordCyclePointCount * SpacingProperties.ActualPixelPerPoint - (SpacingProperties.WaveformRightLineX - SpacingProperties.WaveformLeftLineX) / 2.0d;

            var ScrollVerticalTemp = recordPinIndex * SpacingProperties.ActualWaveformHeight + (SpacingProperties.WaveformBottomLineY - SpacingProperties.WaveformTopLineY) / 2.0d - SpacingProperties.ActualWaveformHeight / 2.0;

            ScrollHornizontalValue = scrollHornizontalTemp > MaxScrollHornizontal ? MaxScrollHornizontal : scrollHornizontalTemp;

            ScrollVerticalValue = ScrollVerticalTemp > MaxScrollVertical ? MaxScrollVertical : ScrollVerticalTemp;

            RaiseOnScrollValueChanged();
        }

        internal void MouseZoomInOut(Point mousePosi, bool isZoomIn)
        {
            MouseZoomInOut(mousePosi, isZoomIn ? new Vector(0.1d, 0.1d) : new Vector(-0.1d, -0.1d));
        }


        public void JumpPatternOffset(int patternOffset, int cycleIndex = 0)
        {
            if (!CanDoJump(patternOffset, cycleIndex, out double value, out string errorMsg))
            {
                throw new InvalidOperationException(errorMsg);
            }

            ScrollHornizontalValue = value;

            RaiseOnScrollValueChanged();
        }

        public bool JumpPinName(string pinName, out string errorMsg)
        {
            errorMsg = string.Empty;
            var pinProp = ShowingPinProperties.FirstOrDefault(item => item.Name == pinName);
            var _pinPropTmp = PinProperties.FirstOrDefault(item => item.Name == pinName);
            if (pinProp is null && _pinPropTmp is null) { errorMsg = $"\"{pinName}\" pin is not yet defined"; return false; }
            else if (!_pinPropTmp.Show) { errorMsg = $"\"{pinName}\" pin is not yet Showing"; return false; }

            var showPinProps = ShowingPinProperties.ToList();
            showPinProps.Sort((item1, item2) => item1.Index.CompareTo(item2.Index));

            var targetIndex = showPinProps.IndexOf(pinProp);
            var scrollVerticalValue = targetIndex * SpacingProperties.ActualWaveformHeight;
            ScrollVerticalValue = scrollVerticalValue > MaxScrollVertical ? MaxScrollVertical : scrollVerticalValue;

            RaiseOnScrollValueChanged();

            return true;
        }

        public int GetMaxOffset()
        {
            if (CycleProperties is null) throw new NullReferenceException("Not Found Cycle Properties");

            return CycleProperties.LastOrDefault()?.Offset ?? throw new NullReferenceException("Not Found Cycle Properties");
        }

        public int GetMinOffset()
        {
            if (CycleProperties is null) throw new NullReferenceException("Not Found Cycle Properties");

            return CycleProperties.FirstOrDefault()?.Offset ?? throw new NullReferenceException("Not Found Cycle Properties");
        }

        public void RemoveTimingCursor(TimingCursor cursor)
        {
            _TimingCursors.Remove(cursor);
        }

        internal void MovingTimingCursor(TimingCursor cursor)
        {
            if (_TimingCursors.Any(item => item.Moving)) return;
            cursor.Moving = true;
        }

        private readonly HitTestNormal HitTestNormalInstance = new HitTestNormal();
        private readonly HitTestTimingMeasurement HitTestTimingMeasurementInstance = new HitTestTimingMeasurement(null);

        public IDrawingWaveformHitTestInfo HitTest(Point point)
        {
            try
            {
#if DEBUG
                StartWatch(nameof(HitTest));
#endif
                if (CycleProperties is null) return null;

                if (point.X < SpacingProperties.WaveformLeftLineX || point.X > SpacingProperties.WaveformRightLineX ||
                    point.Y < SpacingProperties.WaveformTopLineY || point.Y > SpacingProperties.WaveformBottomLineY) return null;

                var currX = ScrollHornizontalValue + point.X - SpacingProperties.WaveformLeftLineX;
                var cycleIndx = SearchCycleIndex(point.X);
                var inCycleRanges = cycleIndx != -1;
                if (!inCycleRanges) return null;
                var pointIndx = (int)((currX - CycleProperties[cycleIndx].LastPointsSum * SpacingProperties.ActualPixelPerPoint) / SpacingProperties.ActualPixelPerPoint);

                var cursor = _TimingCursors.FirstOrDefault(item => item.CycleIndx == cycleIndx && item.PointIndx == pointIndx);
                HitTestTimingMeasurementInstance.TimingCursor = cursor;
                return cursor is null ? HitTestNormalInstance as IDrawingWaveformHitTestInfo : HitTestTimingMeasurementInstance as IDrawingWaveformHitTestInfo;
            }
            finally
            {
#if DEBUG
                StopWatch(nameof(HitTest));
#endif
            }

        }

        internal void AddTimingCursorDone()
        {
            if (_TimingCursors is null || !_TimingCursors.Any(item => item.Moving)) return;
            var cursor = _TimingCursors.FirstOrDefault(item => item.Moving);
            if (cursor is null) return;
            cursor.Moving = false;
        }

        internal void DrawingCompareArrow(DrawingContext dc, Point bottomPoint, ECompare compare, double height, Pen pen)
        {
            var arrowTopPoint = bottomPoint;
            var innerLen = height * SpacingProperties.CompareArrowInnerScale;
            var arrowHalfBottomLen = innerLen / Math.Tan(SpacingProperties.CompareArrowAngle); // innerLen / bottom = tan
            var arrowRightPoint = arrowTopPoint;
            var arrowLeftPoint = arrowTopPoint;

            arrowTopPoint.Offset(0, (compare == ECompare.High ? -1 : 1) * height);
            //var topLineY = SpacingProperties.TopLeft.Y + SpacingProperties.EBH;
            //var bottomLineY = SpacingProperties.TopLeft.Y + SpacingProperties.ActualMH - SpacingProperties.TBH;
            //var leftLineX = SpacingProperties.TopLeft.X + SpacingProperties.PBW + SpacingProperties.VBW;
            //var rightLineX = SpacingProperties.TopLeft.X + SpacingProperties.ActualMW;

            var drawingArrowLine = true;
            var drawingArrow = true;

            if (arrowTopPoint.X < SpacingProperties.WaveformLeftLineX)
            {
                drawingArrowLine = false;
            }
            if (arrowTopPoint.X > SpacingProperties.WaveformRightLineX)
            {
                drawingArrowLine = false;
            }
            if (compare == ECompare.Low)
            {
                if (arrowTopPoint.Y <= SpacingProperties.WaveformTopLineY)
                {
                    return;
                }
                if (arrowTopPoint.Y >= SpacingProperties.WaveformBottomLineY)
                {
                    arrowTopPoint.Y = SpacingProperties.WaveformBottomLineY;
                    drawingArrow = false;
                }

                if (bottomPoint.Y < SpacingProperties.WaveformTopLineY)
                {
                    bottomPoint.Y = SpacingProperties.WaveformTopLineY;
                }

            }
            else if (compare == ECompare.High)
            {

                if (arrowTopPoint.Y < SpacingProperties.WaveformTopLineY)
                {
                    arrowTopPoint.Y = SpacingProperties.WaveformTopLineY;
                    drawingArrow = false;
                }

                if (arrowTopPoint.Y >= SpacingProperties.WaveformBottomLineY)
                {
                    return;
                }

                if (bottomPoint.Y <= SpacingProperties.WaveformTopLineY)
                {
                    return;
                }
                if (bottomPoint.Y >= SpacingProperties.WaveformBottomLineY)
                {
                    bottomPoint.Y = SpacingProperties.WaveformBottomLineY;
                }
            }
            if (drawingArrowLine)
            {
                dc.DrawLine(pen, bottomPoint, arrowTopPoint);
            }

            if (drawingArrow == false) return;

            arrowRightPoint = arrowTopPoint;
            arrowLeftPoint = arrowTopPoint;

            arrowLeftPoint.Offset(-1 * arrowHalfBottomLen, (compare == ECompare.High ? 1 : -1) * innerLen); // left

            arrowRightPoint.Offset(arrowHalfBottomLen, (compare == ECompare.High ? 1 : -1) * innerLen); // right

            //Arrow Left Handle
            if (arrowLeftPoint.X > SpacingProperties.WaveformLeftLineX && arrowTopPoint.X < SpacingProperties.WaveformRightLineX)
            {
                dc.DrawLine(pen, arrowTopPoint, arrowLeftPoint);
            }
            else if (arrowLeftPoint.X < SpacingProperties.WaveformLeftLineX && arrowTopPoint.X > SpacingProperties.WaveformLeftLineX)
            {
                var newPoint = CrossPoint(arrowLeftPoint, arrowTopPoint, new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformTopLineY),
                    new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformBottomLineY));
                if (newPoint.HasValue) arrowLeftPoint = newPoint.Value;
                dc.DrawLine(pen, arrowTopPoint, arrowLeftPoint);
            }
            else if (arrowTopPoint.X > SpacingProperties.WaveformRightLineX && arrowLeftPoint.X < SpacingProperties.WaveformRightLineX)
            {
                var newPoint = CrossPoint(arrowTopPoint, arrowLeftPoint, new Point(SpacingProperties.WaveformRightLineX, SpacingProperties.WaveformTopLineY), new Point(SpacingProperties.WaveformRightLineX, SpacingProperties.WaveformBottomLineY));
                if (newPoint.HasValue) dc.DrawLine(pen, newPoint.Value, arrowLeftPoint);
            }

            //Arrow Right Handle
            if (arrowTopPoint.X > SpacingProperties.WaveformLeftLineX && arrowRightPoint.X < SpacingProperties.WaveformRightLineX)
            {
                dc.DrawLine(pen, arrowTopPoint, arrowRightPoint);
            }
            else if (arrowTopPoint.X < SpacingProperties.WaveformLeftLineX && arrowRightPoint.X > SpacingProperties.WaveformLeftLineX)
            {
                var newPoint = CrossPoint(arrowTopPoint, arrowRightPoint, new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformTopLineY), new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformBottomLineY));
                if (newPoint.HasValue) dc.DrawLine(pen, newPoint.Value, arrowRightPoint);
            }
            else if (arrowTopPoint.X < SpacingProperties.WaveformRightLineX && arrowRightPoint.X > SpacingProperties.WaveformRightLineX)
            {
                var newPoint = CrossPoint(arrowTopPoint, arrowRightPoint, new Point(SpacingProperties.WaveformRightLineX, SpacingProperties.WaveformTopLineY), new Point(SpacingProperties.WaveformRightLineX, SpacingProperties.WaveformBottomLineY));
                if (newPoint.HasValue) dc.DrawLine(pen, newPoint.Value, arrowTopPoint);
            }

        }

        internal void DrawingCompareWindow(DrawingContext dc, Point topLeft, Size size, Brush brush)
        {
            DrawingRect(dc, topLeft, size,
                new Point(SpacingProperties.WaveformLeftLineX, SpacingProperties.WaveformTopLineY),
                new Size(SpacingProperties.WW, SpacingProperties.N_WH), null, brush);
        }

        internal void DrawingRect(DrawingContext dc, Point topleft, Size size, Point limitTopleft, Size limitSize, Pen pen = null, Brush brush = null)
        {
            var rectTopLeft = topleft;
            var leftLineX = limitTopleft.X;
            var rightLineX = leftLineX + limitSize.Width;
            var topLineY = limitTopleft.Y;
            var bottomLineY = topLineY + limitSize.Height;

            if (rectTopLeft.X + size.Width < leftLineX)
            {
                return;
            }
            if (rectTopLeft.X < leftLineX)
            {
                size.Width -= (leftLineX - rectTopLeft.X);
                rectTopLeft.X = leftLineX;
            }

            if (rectTopLeft.X > rightLineX)
            {
                return;
            }
            if (rectTopLeft.X + size.Width > rightLineX)
            {
                var diff = rightLineX - rectTopLeft.X;
                if (diff <= 0) return;
                size.Width = diff;
            }

            if (rectTopLeft.Y + size.Height < topLineY)
            {
                return;
            }
            if (rectTopLeft.Y < topLineY)
            {
                size.Height -= (topLineY - rectTopLeft.Y);
                rectTopLeft.Y = topLineY;
            }

            if (rectTopLeft.Y > bottomLineY)
            {
                return;
            }
            if (rectTopLeft.Y + size.Height > bottomLineY)
            {
                var diff = bottomLineY - rectTopLeft.Y;
                if (diff <= 0) return;
                size.Height = diff;
            }

            dc.DrawRectangle(brush, pen, new Rect(rectTopLeft, size));
        }

        internal Dictionary<string, DrawingVoltageLevel> GetDrawingVoltageLevels(int showPinIndex)
        {
            var pinProperties = ShowingPinProperties[showPinIndex];
            var voltageBarTextBrush = new SolidColorBrush(ColorProperties.VoltageText);
            voltageBarTextBrush.Freeze();
            var dic = new Dictionary<string, DrawingVoltageLevel>();
            var voltScaleHalfWidth = SpacingProperties.VoltageBarScaleWidth / 2.0d;
            var voltScalePosiX = SpacingProperties.WaveformLeftLineX - voltScaleHalfWidth;
            var waveformRectTop = SpacingProperties.WaveformTopLineY + showPinIndex * SpacingProperties.ActualWaveformHeight - ScrollVerticalValue;

            var maxVolt = pinProperties.VoltageRange.MaxVolt;
            var maxVoltStr = SpacingProperties.ValueToVoltageStr(maxVolt);
            var maxVoltFormat = GetDrawingFormattedText(maxVoltStr, voltageBarTextBrush, SpacingProperties.VoltageBarTextSize);
            var maxVoltTextHeight = maxVoltFormat.Height;
            var maxVoltTextHalfHeight = maxVoltTextHeight / 2.0d;
            var maxVoltScalePointY = waveformRectTop + SpacingProperties.VoltageBarTextTopBottomPadding + maxVoltTextHalfHeight;

            var maxVoltTextPointX = SpacingProperties.WaveformLeftLineX - voltScaleHalfWidth - maxVoltFormat.WidthIncludingTrailingWhitespace;
            var maxVoltTextPointY = maxVoltScalePointY - maxVoltTextHalfHeight;

            dic.Add(DefaultValues.MaxVoltageName, new DrawingVoltageLevel()
            {
                Name = DefaultValues.MaxVoltageName,
                Formatted = maxVoltFormat,
                LinePosiLeft = new Point(SpacingProperties.WaveformLeftLineX, maxVoltScalePointY),
                ScaleLinePosiLeft = new Point(voltScalePosiX, maxVoltScalePointY),
                TextPosiLeft = new Point(maxVoltTextPointX, maxVoltTextPointY),
            });

            var minVolt = pinProperties.VoltageRange.MinVolt;
            var minVoltStr = SpacingProperties.ValueToVoltageStr(minVolt);
            var minVoltFormat = GetDrawingFormattedText(minVoltStr, voltageBarTextBrush, SpacingProperties.VoltageBarTextSize);
            var minVoltTextHeight = minVoltFormat.Height;
            var minVoltTextHalfHeight = minVoltTextHeight / 2.0d;
            var minVoltScalePointY = waveformRectTop + SpacingProperties.ActualWaveformHeight - SpacingProperties.VoltageBarTextTopBottomPadding - minVoltTextHalfHeight;

            var minVoltTextPointX = SpacingProperties.WaveformLeftLineX - voltScaleHalfWidth - minVoltFormat.WidthIncludingTrailingWhitespace;
            var minVoltTextPointY = minVoltScalePointY - minVoltTextHalfHeight;

            dic.Add(DefaultValues.MinVoltageName, new DrawingVoltageLevel()
            {
                Name = DefaultValues.MaxVoltageName,
                Formatted = minVoltFormat,
                LinePosiLeft = new Point(SpacingProperties.WaveformLeftLineX, minVoltScalePointY),
                ScaleLinePosiLeft = new Point(voltScalePosiX, minVoltScalePointY),
                TextPosiLeft = new Point(minVoltTextPointX, minVoltTextPointY),
            });

            var voltLevels = pinProperties.VoltageRange?.VoltageLevels;
            if (!(voltLevels is null))
            {
                for (int levelIndex = 0; levelIndex < voltLevels.Count; ++levelIndex)
                {
                    var voltLevel = voltLevels[levelIndex];
                    var levelVoltStr = SpacingProperties.ValueToVoltageStr(voltLevel.Voltage);
                    var levelFormat = GetDrawingFormattedText(levelVoltStr, voltageBarTextBrush, SpacingProperties.VoltageBarTextSize);
                    var levelTextHeight = levelFormat.Height;
                    var levelTextHalfHeight = levelTextHeight / 2.0d;
                    var levelPosiHeight = SpacingProperties.VoltageToPointY(voltLevel.Voltage, maxVolt, minVolt, minVoltScalePointY - maxVoltScalePointY);
                    var levelPosiY = levelPosiHeight + maxVoltScalePointY;

                    var levelTextPointX = SpacingProperties.WaveformLeftLineX - voltScaleHalfWidth - levelFormat.WidthIncludingTrailingWhitespace;
                    var levelTextPointY = levelPosiY - levelTextHalfHeight;

                    dic.Add(voltLevel.Name, new DrawingVoltageLevel()
                    {
                        Name = voltLevel.Name,
                        Formatted = levelFormat,
                        LinePosiLeft = new Point(SpacingProperties.WaveformLeftLineX, levelPosiY),
                        ScaleLinePosiLeft = new Point(voltScalePosiX, levelPosiY),
                        TextPosiLeft = new Point(levelTextPointX, levelTextPointY),
                    });
                }
            }

            return dic;
        }



        private static Nullable<Point> CrossPoint(Point line1Point1, Point line1Point2, Point line2Point1, Point line2Point2)
        {
            /**
             *兩條線求直線方程式
             * y = ax + b
             * * L1P1.Y = a1 * L1P1.X + b1
             * * L1P2.Y = a1 * L1P2.X + b1
             * * (L1P1.Y - L1P2.Y) = (L1P1.X - L1P2.X) * a1  ==> a1 = (L1P1.Y - L1P2.Y) / (L1P1.X - L1P2.X),  L1P1.Y = (L1P1.Y - L1P2.Y)  * L1P1.X / (L1P1.X - L1P2.X) + b1 ==> b1 =  L1P1.Y - (L1P1.Y - L1P2.Y)  * L1P1.X / (L1P1.X - L1P2.X)
             * *
             * 兩條線交叉點
             * y = a1 x + b1
             * y = a2 x + b2
             * 0 = (a1 - a2) x + (b1 - b2) ==> x = -(b1 - b2) / (a1 - a2) = x,,, y = a1 (- (b1 - b2) / (a1 - a2)) + b1
             */
            var px = double.NaN;
            var py = double.NaN;
            var L1XDiff = (line1Point1.X - line1Point2.X);
            var L1YDiff = (line1Point1.Y - line1Point2.Y);
            var L2XDiff = (line2Point1.X - line2Point2.X);
            var L2YDiff = (line2Point1.Y - line2Point2.Y);

            if (L1XDiff == L2XDiff && L1YDiff == L2YDiff) // 平行線
            {
                return null;
            }

            if (L1XDiff == 0.0d) // 直線
            {
                px = line1Point1.X;
            }
            if (L2XDiff == 0.0d)
            {
                px = line2Point1.X;
            }
            if (L1YDiff == 0.0d)
            {
                py = line1Point1.Y;
            }
            if (L2YDiff == 0.0d)
            {
                py = line2Point1.Y;
            }

            if (double.IsNaN(px) && double.IsNaN(py))
            {
                var a1 = (line1Point1.Y - line1Point2.Y) / (line1Point1.X - line1Point2.X);
                var b1 = line1Point1.Y - a1 * line1Point1.X;

                var a2 = (line2Point1.Y - line2Point2.Y) / (line2Point1.X - line2Point2.X);
                var b2 = line2Point1.Y - a2 * line2Point1.X;

                px = -(b1 - b2) / (a1 - a2);
                py = a1 * px + b1;
            }
            else if (!double.IsNaN(px) && double.IsNaN(py))
            {
                //py = a * px + b
                if (L1XDiff == 0.0d)
                {
                    var a2 = (line2Point1.Y - line2Point2.Y) / (line2Point1.X - line2Point2.X);
                    //var b2 = line2Point1.Y - a2 * line1Point1.X; ==> error fixed
                    var b2 = line2Point1.Y - a2 * line2Point1.X;

                    py = a2 * px + b2;
                }
                else
                {
                    var a1 = (line1Point1.Y - line1Point2.Y) / (line1Point1.X - line1Point2.X);
                    var b1 = line1Point1.Y - a1 * line1Point1.X;

                    py = a1 * px + b1;
                }
            }
            else if (double.IsNaN(px) && !double.IsNaN(py))
            {
                if (L1YDiff == 0.0d)
                {
                    var a2 = (line2Point1.Y - line2Point2.Y) / (line2Point1.X - line2Point2.X);
                    //fixed line1Point1.X ==> line2Point1.X
                    var b2 = line2Point1.Y - a2 * line2Point1.X;

                    px = (py - b2) / a2;
                }
                else
                {
                    var a1 = (line1Point1.Y - line1Point2.Y) / (line1Point1.X - line1Point2.X);
                    var b1 = line1Point1.Y - a1 * line1Point1.X;

                    px = (py - b1) / a1;
                }
            }

            return new Point(px, py);
        }

        private void InitCycleProperties()
        {
            var cycleCount = CycleProperties.Count;

            double len = 0.0;
            int pointSum = 0;
            var pixel = SpacingProperties.ActualPixelPerPoint;
            for (int cycleIdx = 0; cycleIdx < cycleCount; cycleIdx++)
            {
                var prop = CycleProperties[cycleIdx];
                prop.Index = cycleIdx;
                var pointSize = prop.PointsSize;
                len += pointSize * pixel;
                prop.DrawingXPosition = len;
                prop.LastPointsSum = pointSum;
                pointSum += pointSize;
            }
        }

        private void InitScrollValues()
        {
            var lastCycleXPosition = CycleProperties.LastOrDefault()?.DrawingXPosition ?? SpacingProperties.WW;
            MaxScrollHornizontal = lastCycleXPosition - SpacingProperties.WW;

            if (PinProperties?.Count is null)
            {
                MaxScrollVertical = SpacingProperties.N_WH;
            }
            else
            {
                MaxScrollVertical = PinProperties.Count(item => item.Show) * SpacingProperties.ActualWaveformHeight - (SpacingProperties.N_WH);
            }
            RaiseOnScrollMaxValueChanged();
        }

        private void InitPinProperties()
        {
            var pinList = PinProperties.Where(item => item.Show).OrderBy(item => item.Index).ToList();
            ShowingPinProperties = pinList;
        }

        private void InitPinBarWidth()
        {
            var pinIndexes = GetDrawingPinIndexes()?.ToList();
            if (pinIndexes is null) return;
            int maxIndex = pinIndexes.First();
            string maxLenPinName = string.Empty;
            string maxLenTopLabel = string.Empty;
            foreach (var pinIdx in pinIndexes)
            {
                var showPinProp = ShowingPinProperties[pinIdx];

                if (showPinProp.Name.Length > maxLenPinName.Length)
                {
                    maxLenPinName = ShowingPinProperties[pinIdx].Name;
                }

                if (!string.IsNullOrEmpty(showPinProp.TopLabel) && showPinProp.TopLabel.Length > maxLenTopLabel.Length)
                {
                    maxLenTopLabel = showPinProp.TopLabel;
                }
            }

            var pinNameBrush = new SolidColorBrush(ColorProperties.PinName);
            var topLabelBrush = new SolidColorBrush(ColorProperties.PinTopLabelText);
            pinNameBrush.Freeze();
            topLabelBrush.Freeze();
            var pinNameFormatText = GetDrawingFormattedText(maxLenPinName, pinNameBrush, SpacingProperties.PinNameTextSize);
            var topLabelFormatText = GetDrawingFormattedText(maxLenTopLabel, topLabelBrush, SpacingProperties.PinTopLabelTextSize);
            var pinNameBarWidth = pinNameFormatText.WidthIncludingTrailingWhitespace + SpacingProperties.PinNameTextWidthPadding * 2;
            var topLabelWidth = topLabelFormatText.WidthIncludingTrailingWhitespace + SpacingProperties.PinNameTextWidthPadding * 2;
            SpacingProperties.PBW = Math.Max(pinNameBarWidth, topLabelWidth);
        }

        private void InitVoltageBarWidth()
        {

            var textLength = 0.5d;
            var needDrawingWidth = 0.0d;
            var voltageSolidColor = new SolidColorBrush(ColorProperties.VoltageText);
            var pinIndexes = GetDrawingPinIndexes();

            if (pinIndexes is null) return;
            foreach (var pinIdx in pinIndexes)
            {
                var voltageRange = ShowingPinProperties[pinIdx].VoltageRange;

                var text = SpacingProperties.ValueToVoltageStr(voltageRange.MaxVolt);
                var tmpLen = text.Length;
                if (textLength < tmpLen)
                {
                    textLength = tmpLen;
                    needDrawingWidth = GetDrawingFormattedText(text, voltageSolidColor, SpacingProperties.VoltageBarTextSize).WidthIncludingTrailingWhitespace;
                }

                text = SpacingProperties.ValueToVoltageStr(voltageRange.MinVolt);
                tmpLen = text.Length;
                if (textLength < tmpLen)
                {
                    textLength = tmpLen;
                    needDrawingWidth = GetDrawingFormattedText(text, voltageSolidColor, SpacingProperties.VoltageBarTextSize).WidthIncludingTrailingWhitespace;
                }

                if (voltageRange.VoltageLevels is null) continue;

                foreach (var markValue in voltageRange.VoltageLevels)
                {
                    text = SpacingProperties.ValueToVoltageStr(markValue.Voltage);
                    tmpLen = text.Length;
                    if (textLength < tmpLen)
                    {
                        textLength = tmpLen;
                        needDrawingWidth = GetDrawingFormattedText(text, voltageSolidColor, SpacingProperties.VoltageBarTextSize).WidthIncludingTrailingWhitespace;
                    }
                }
            }

            SpacingProperties.VBW = needDrawingWidth + SpacingProperties.VoltageBarInnerTextPadding * 2 + SpacingProperties.VoltageBarScaleWidth / 2.0d;

        }

        private void InitDynamicSpacingProperties()
        {

            SpacingProperties.WW = SpacingProperties.ActualMW - SpacingProperties.VBW - SpacingProperties.PBW;

            SpacingProperties.WaveformTopLineY = SpacingProperties.TopLeft.Y + SpacingProperties.EBH;
            SpacingProperties.WaveformBottomLineY = SpacingProperties.WaveformTopLineY + SpacingProperties.N_WH;
            SpacingProperties.WaveformLeftLineX = SpacingProperties.TopLeft.X + SpacingProperties.PBW + SpacingProperties.VBW;
            SpacingProperties.WaveformRightLineX = SpacingProperties.TopLeft.X + SpacingProperties.ActualMW;

        }

        private IEnumerable<int> GetDrawingCycleIndexes()
        {
            int firstIndex = 0, lastIndex = 0;
            var cycleCount = CycleProperties.Count;
            //var lastCycle = CycleProperties.LastOrDefault(item => item.DrawingXPosition <= (ScrollHornizontalValue + SpacingProperties.WW));
            //var firstCycle = CycleProperties.FirstOrDefault(item => item.DrawingXPosition > ScrollHornizontalValue);

            var lastCycle = CycleProperties.ElementAtOrDefault(SearchCycleIndex(SpacingProperties.WW + SpacingProperties.WaveformLeftLineX));
            if (lastCycle is null) lastCycle = CycleProperties.LastOrDefault();

            var firstCycle = CycleProperties.ElementAtOrDefault(SearchCycleIndex(SpacingProperties.WaveformLeftLineX));

            Debug.WriteLine($"Curr Cycle Ranges:{firstCycle?.Index ?? -1} ~ {lastCycle?.Index ?? -1}");

            if (firstCycle is null) yield break;
            if (!(firstCycle is null) && lastCycle is null)
            {
                yield return firstCycle.Index;
                yield break;
            }

            firstIndex = firstCycle.Index;

            if (lastCycle.Index < cycleCount - 1)
            {
                lastIndex = lastCycle.Index + 1;
            }
            else
            {
                lastIndex = lastCycle.Index;
            }

            for (int index = firstIndex; index <= lastIndex; index++)
            {
                yield return index;
            }
        }

        private IEnumerable<int> GetDrawingPinIndexes()
        {
            int firstIndex = 0, lastIndex = 0;
            var pinCount = ShowingPinProperties.Count;
            firstIndex = (int)(ScrollVerticalValue / SpacingProperties.ActualWaveformHeight);
            lastIndex = (int)((ScrollVerticalValue + SpacingProperties.N_WH) / SpacingProperties.ActualWaveformHeight);
            if (firstIndex >= pinCount) return null;
            else if (lastIndex < pinCount) return Enumerable.Range(firstIndex, lastIndex - firstIndex + 1);
            else return Enumerable.Range(firstIndex, pinCount - firstIndex);
        }

        private int SearchCycleIndex(double positionX)
        {
            var pointIndex = (int)((ScrollHornizontalValue + positionX - SpacingProperties.WaveformLeftLineX) / SpacingProperties.ActualPixelPerPoint);
            return SearchCycleIndex(pointIndex);
        }

        private int SearchCycleIndex(int waveformPointIndex)
        {
            var startIndex = 0;
            var endIndex = CycleProperties.Count - 1;
            var minIndx = 0;
            CycleProperties cycleProp = null;
            while (startIndex <= endIndex)
            {
                minIndx = (startIndex + endIndex) / 2;
                cycleProp = CycleProperties[minIndx];

                if (cycleProp.LastPointsSum + cycleProp.PointsSize - 1 < waveformPointIndex)
                {
                    startIndex = minIndx + 1;
                }
                else if (cycleProp.LastPointsSum > waveformPointIndex)
                {
                    endIndex = minIndx - 1;
                }
                else
                {
                    return minIndx;
                }
            }

            return -1;
        }

        private FormattedText GetDrawingFormattedText(string text, Brush color, double Size)
        {
            double pixelsPerDip = 1.0d;
            try
            {
                if (waveformImage != null)
                {
                    pixelsPerDip = VisualTreeHelper.GetDpi(this.waveformImage).PixelsPerDip;
                }
                else if (Application.Current?.MainWindow != null)
                {
                    pixelsPerDip = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;
                }
            }
            catch
            {
                pixelsPerDip = 1.0d;
            }

            var formatText = new FormattedText(text,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                Size,
                color,
                pixelsPerDip);
            return formatText;
        }

        public void ExportToJPEG(string fileName)
        {
            if (CurrBitmapTmp is null) return;
            JpegBitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(CurrBitmapTmp));
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                bitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void ExportToPNG(string fileName)
        {
            if (CurrBitmapTmp is null) return;
            PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(CurrBitmapTmp));
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                bitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public void ExportToBitmap(string fileName)
        {
            if (CurrBitmapTmp is null) return;
            BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(CurrBitmapTmp));
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                bitmapEncoder.Save(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }
    }

    public static class DrawingWaveformContextExtensions
    {
        public static void SetShowPinMaxVoltage(this WaveformContext dwc, string pinName, double voltage)
        {
            var pinProp = dwc.PinProperties.FirstOrDefault(item => item.Name == pinName);
            if (pinProp is null) throw new NullReferenceException();
            pinProp.VoltageRange.MaxVolt = voltage;
            pinProp.VoltageRange.Assert();
        }

        public static void SetShowPinMinVoltage(this WaveformContext dwc, string pinName, double voltage)
        {
            var pinProp = dwc.PinProperties.FirstOrDefault(item => item.Name == pinName);
            if (pinProp is null) throw new NullReferenceException();
            pinProp.VoltageRange.MinVolt = voltage;
            pinProp.VoltageRange.Assert();
        }

        public static void SetAllShowPinMaxVoltage(this WaveformContext dwc, double voltage)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                range.MaxVolt = voltage;
                range.Assert();
            }
        }

        public static void SetAllShowPinMaxVoltage(this WaveformContext dwc, bool isShow)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                range.ShowMaxVolt = isShow;
            }
        }

        public static void SetAllPinShowMinVoltage(this WaveformContext dwc, double voltage)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                range.MinVolt = voltage;
                range.Assert();
            }
        }

        public static void SetAllShowPinMinVoltage(this WaveformContext dwc, bool isShow)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                range.ShowMinVolt = isShow;
            }
        }

        public static void SetAllPinVoltageLevelMarker(this WaveformContext dwc, string name, bool isShow)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                var target = range.VoltageLevels.FirstOrDefault(item => item.Name == name);
                if (target is null) continue;
                target.Show = isShow;
            }
        }

        public static void SetAllPinVoltageLevelMarker(this WaveformContext dwc, string name, double voltage)
        {
            foreach (var pinProp in dwc.PinProperties)
            {
                var range = pinProp.VoltageRange;
                var target = range.VoltageLevels.FirstOrDefault(item => item.Name == name);
                if (target is null) continue;
                target.Voltage = voltage;
                range.Assert();
            }
        }

        public static void ClearAllTimingMeasurementCursors(this WaveformContext dwc)
        {
            dwc._TimingCursors.Clear();
        }

        public static void ResetZoomInOut(this WaveformContext dwc)
        {
            dwc.SpacingProperties.DefaultZoom();

            dwc.RaiseOnZoomInOutChanged();
        }

    }

}
