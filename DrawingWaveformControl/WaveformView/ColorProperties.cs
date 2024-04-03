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
        public ColorProperties() { }

        public Color Grid { get; set; } = Colors.White;

        public Color Background { get; set; } = Colors.Black;

        public Color DefaultWaveformLine { get; set; } = Colors.Blue;

        public Color Text { get; set; } = Colors.White;


        internal Brush GridBrush { get; private set; }
        internal Brush BackgroundBrush { get; private set; }
        internal Brush DefaultWaveformLineBrush { get; private set; }
        internal Brush TextBrush { get; private set; }

        internal void Update()
        {

            GridBrush = new SolidColorBrush(Grid);
            GridBrush.Freeze();

            BackgroundBrush = new SolidColorBrush(Background);
            BackgroundBrush.Freeze();

            DefaultWaveformLineBrush = new SolidColorBrush(DefaultWaveformLine);
            DefaultWaveformLineBrush.Freeze();

            TextBrush = new SolidColorBrush(Text);
            TextBrush.Freeze();
        }
    }
}
