using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WaveformView
{
    public class ColorProperties
    {
        public Color Background { get; set; } = Colors.White;

        public Color Line { get; set; } = Colors.Black;

        public Color DefaultNaNWaveformLine { get; set; } = Colors.Red;

        public Color DefaultWaveformLine { get; set; } = Colors.Black;

        public Color CompareLine { get; set; } = Colors.Black;

        public Color CompareWindow { get; set; } = new Color() { A = 128, R = Colors.Blue.R, G = Colors.Blue.G, B = Colors.Blue.B };

        public Color FailCycle { get; set; } = new Color() { A = 85, R = Colors.Red.R, G = Colors.Red.G, B = Colors.Red.B };

        public Color FailCircle { get; set; } = Colors.Red;

        public Color PinTopLabelRect { get; set; } = Colors.Orange;

        public Color PinTopLabelText { get; set; } = Colors.White;

        public Color PinName { get; set; } = Colors.Blue;

        public Color TimingText { get; set; } = Colors.Black;

        public Color VoltageText { get; set; } = Colors.Black;


        public Color CycleTopLabel { get; set; } = Colors.Black;

        public Color MaxMinVoltageLine { get; set; } = Colors.Green;

        public Color[] VoltageLevels { get; set; } = null;

        public Color DefaultVoltageLevel { get; set; } = Colors.Orange;

        public Color[] TimingCursorMeasurements { get; set; } = null;

        public Color DefaultTimingCursorMeasurement { get; set; } = Colors.Blue;

        public Color MousePointText { get; set; } = Colors.Blue;

        public Color MouseCursorLine { get; set; } = Colors.DimGray;

        public Color MouseCursorText { get; set; } = Colors.Red;

        public Nullable<Color> GridLine { get; }

        public Color InformationText { get; set; } = Colors.Blue;

        public Color IgnoreLastPoint { get; set; } = Colors.LightGray;
    }
}
