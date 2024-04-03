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

        public WaveformLineProperties(string name, Color lineColor) 
        { 
            this.Name = name;
            this.LineColor = lineColor;
        }

        public string Name { get; }

        public double LineWidth { get; set; }

        public Color LineColor { get; set; }

        public bool IsVisible { get; set; }
    }
}
