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
    }
}
