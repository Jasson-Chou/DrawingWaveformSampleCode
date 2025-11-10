using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WaveformView
{
    public class WaveformLineProperties
    {

        public WaveformLineProperties(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public double Thickness { get; set; } = DefaultValues.LineWidth;

        public Nullable<Color> LineColor { get; set; } = null;

        public bool Show { get; set; } = true;
    }
}
