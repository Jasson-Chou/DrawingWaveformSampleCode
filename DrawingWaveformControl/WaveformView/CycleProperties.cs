using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformView
{
    public class CycleProperties
    {
        public CycleProperties(int pointSize)
        {
            PointSize = pointSize;
        }

        internal int Index { get; set; }

        public int PointSize { get; }

        internal int PointAccumulator { get; set; }
    }
}
