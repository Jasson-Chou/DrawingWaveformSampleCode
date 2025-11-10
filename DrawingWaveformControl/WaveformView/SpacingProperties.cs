using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WaveformView
{
    public class SpacingProperties
    {
        public enum ETimeUnit
        {
            Auto = -1,
            Sec,
            mS,
            uS,
            nS,
            fS,
        }

        public enum EVoltUnit
        {
            Auto = -1,
            V,
            mV,
            uV,
            nV,
            fV,
        }

        public SpacingProperties()
        {
            WindowPadding = 5.0d;
            PinNameTextWidthPadding = 5.0d;
            VoltageBarTextTopBottomPadding = 0.1d;
            VoltageBarInnerTextPadding = 5.0d;
            TimingMeasurement = new TimingMeasurement();

            TimeUnitValues = new Dictionary<ETimeUnit, double>();
            TimeUnitValues.Add(ETimeUnit.Sec, 1.0d);
            TimeUnitValues.Add(ETimeUnit.mS, 1e-3);
            TimeUnitValues.Add(ETimeUnit.uS, 1e-6);
            TimeUnitValues.Add(ETimeUnit.nS, 1e-9);
            TimeUnitValues.Add(ETimeUnit.fS, 1e-12);

            VoltUnitValues = new Dictionary<EVoltUnit, double>();
            VoltUnitValues.Add(EVoltUnit.Auto, -1.0d);
            VoltUnitValues.Add(EVoltUnit.V, 1.0d);
            VoltUnitValues.Add(EVoltUnit.mV, 1e-3);
            VoltUnitValues.Add(EVoltUnit.uV, 1e-6);
            VoltUnitValues.Add(EVoltUnit.nV, 1e-9);
            VoltUnitValues.Add(EVoltUnit.fV, 1e-12);
        }

        private Dictionary<EVoltUnit, double> VoltUnitValues { get; }

        private Dictionary<ETimeUnit, double> TimeUnitValues { get; }

        internal Point TopLeft { get; private set; }

        internal double WindowPadding { get; }

        internal double PinNameTextWidthPadding { get; }

        internal double VoltageBarTextTopBottomPadding { get; }

        internal double VoltageBarInnerTextPadding { get; }

        internal double VoltageBarScaleWidth => ScopeXScaleValue * DefaultValues.VoltageBarScaleWidth;


        public double MW { get; set; }

        public double MH { get; set; }

        public double ActualMW { get; private set; }

        public double ActualMH { get; private set; }

        public double EBH { get; } = 26.0d;

        public double PBW { get; internal set; } = 10.0d;

        public double VBW { get; set; } = 40.0d;

        public double WW { get; set; }

        public double TBH { get; } = 26.0d;

        public double N_WH { get; private set; }

        internal double LegendHeight { get; } = 30.0d;

        internal double MouseInfoHeight { get; } = 30.0d;

        public double ErrorBarFailCircleRadius { get; set; } = DefaultValues.FailCircleRadius;

        //public double ErrorBarFailCircleRadius
        //{
        //    get
        //    {
        //        var newValue = DrawingDefaultValues.FailCircleRadius * ScopeXScaleValue;
        //        if (newValue * 2 > TBH) newValue = TBH / 2.0d;
        //        return newValue;
        //    }
        //}

        public double ScopeXScaleValue { get; set; } = 1.0d;

        public double ScopeYScaleValue { get; set; } = 1.0d;

        public double MaxScopeXScaleValue { get; } = 5.0d;

        public double MinScopeXScaleValue { get; } = 0.05d;

        public double MaxScopeYScaleValue { get; } = 5.0d;

        public double MinScopeYScaleValue { get; } = 0.05d;

        public double ActualPixelPerPoint => DefaultValues.PixelPerPoint * ScopeXScaleValue;

        public double ActualWaveformHeight => DefaultValues.WaveformHeight * ScopeYScaleValue;

        public double LineWidth { get; set; } = DefaultValues.LineWidth;

        /// <summary>
        /// Unit: second
        /// Default: 1 ns
        /// </summary>
        public double TimingResolution { get; set; } = 1e-9;

        public ETimeUnit TimingUnit { get; set; } = ETimeUnit.Auto;

        public EVoltUnit VoltUnit { get; set; } = EVoltUnit.Auto;

        public int VoltUnitDecimals { get; set; } = 2;

        public int TimeUnitDecimals { get; set; } = 2;

        internal double TimeBarTextSize { get; set; } = DefaultValues.TimeBarTextSize;// * ScopeXScaleValue > 20.0d ? 20.0d : DrawingDefaultValues.TimeBarTextSize * ScopeXScaleValue;

        public double PinNameTextSize { get; set; } = DefaultValues.PinNameTextSize;

        public double VoltageBarTextSize { get; set; } = DefaultValues.VoltageBarTextSize;// * ScopeYScaleValue > 20.0d ? 20.0d : DrawingDefaultValues.VoltageBarTextSize * ScopeXScaleValue;

        internal double LegendTextSize => DefaultValues.LegendTextSize;

        internal double TimingCursorTextSize => DefaultValues.TimingCursorTextSize;

        internal double CycleTopLabelTextSize => DefaultValues.CycleTopLabelTextSize;

        internal double NaNWaveformTextSize => DefaultValues.NaNWaveformTextSize;

        internal double PinTopLabelTextSize => DefaultValues.PinTopLabelTextSize;

        internal double MouseCursorTextSize => DefaultValues.MouseCursorTextSize;

        internal double MouseCursorThickness => DefaultValues.MouseCursorThickness;

        public double CompareArrowAngle { get; set; } = 45.0d;

        public double CompareArrowInnerScale { get; set; } = DefaultValues.CompareArrowInnerScale;

        public double InformationTextPadding { get; } = 5.0d;

        internal double WaveformTopLineY { get; set; }

        internal double WaveformBottomLineY { get; set; }

        internal double WaveformRightLineX { get; set; }

        internal double WaveformLeftLineX { get; set; }

        public TimingMeasurement TimingMeasurement { get; }

        internal void Update()
        {
            ActualMW = MW - 2 * WindowPadding;
            ActualMH = MH - 2 * WindowPadding - LegendHeight - MouseInfoHeight;
            N_WH = ActualMH - EBH - TBH;// - LegendHeight - MouseInfoHeight;
            WW = ActualMW - VBW - PBW;
            TopLeft = new Point(WindowPadding, WindowPadding + LegendHeight);
        }

        /// <summary>
        /// Default Zoon In, Out Values and Render All
        /// </summary>
        internal void DefaultZoom()
        {
            ScopeXScaleValue = 1.0d;
            ScopeYScaleValue = 1.0d;
        }


        internal string ValueToTimingStr(double value)
        {
            if (double.IsNaN(value)) return "NaN";
            if (TimingUnit == ETimeUnit.Auto)
            {
                var enumCnt = Enum.GetValues(typeof(ETimeUnit)).Cast<ETimeUnit>().Count();
                for (int enumIdx = 0; enumIdx < enumCnt - 1; enumIdx++)
                {
                    var eUnit = (ETimeUnit)enumIdx;
                    var unitValue = TimeUnitValues[eUnit];
                    var divi = value / unitValue;
                    if (divi == 0.0d)
                    {
                        return $"{divi} {eUnit.ToString()}";
                    }
                    else if ((int)(divi) != 0)
                    {
                        return $"{Math.Round(divi, TimeUnitDecimals)} {eUnit.ToString()}";
                    }
                }
                return "error";
            }
            else
            {
                var unitValue = TimeUnitValues[TimingUnit];
                var divi = value / unitValue;
                return $"{Math.Round(divi, TimeUnitDecimals)} {TimingUnit.ToString()}";
            }
        }

        internal string ValueToVoltageStr(double value)
        {
            if (double.IsNaN(value)) return "NaN";
            if (VoltUnit == EVoltUnit.Auto)
            {
                var enumCnt = Enum.GetValues(typeof(EVoltUnit)).Cast<EVoltUnit>().Count();
                for (int enumIdx = 0; enumIdx < enumCnt - 1; enumIdx++)
                {
                    var eVolt = (EVoltUnit)enumIdx;
                    var unitValue = VoltUnitValues[eVolt];
                    var divi = value / unitValue;
                    if (divi == 0.0d)
                    {
                        return $"{divi} {eVolt.ToString()}";
                    }
                    else if ((int)(divi) != 0)
                    {
                        return $"{Math.Round(divi, VoltUnitDecimals)} {eVolt.ToString()}";
                    }
                }
                return "error";
            }
            else
            {
                var unitValue = VoltUnitValues[VoltUnit];
                var divi = value / unitValue;
                return $"{Math.Round(divi, VoltUnitDecimals)} {VoltUnit.ToString()}";
            }
        }

        internal double VoltageToPointY(double volt, double maxVolt, double minVolt, double maxVoltToMinVoltHeight)
        {
            if (double.IsNaN(volt))
            {
                return maxVoltToMinVoltHeight / 2.0d;
            }
            else
            {
                var pointY = (volt - minVolt) * maxVoltToMinVoltHeight / (maxVolt - minVolt);
                var Ylocation = maxVoltToMinVoltHeight - pointY;
                return Ylocation;
            }
        }

        internal double PointYToVoltage(double pointY, double maxVolt, double maxVoltPointY, double minVolt, double minVoltPointY)
        {
            var volt = (maxVolt - minVolt) * (pointY - minVoltPointY) / (maxVoltPointY - minVoltPointY) + minVolt;
            return volt;
        }


    }
}
