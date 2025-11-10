using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WaveformView
{
    public class CycleProperties
    {
        public CycleProperties(int offset, int pointsSize)
        {
            Offset = offset;
            PointsSize = pointsSize;
        }
        public bool IsFail { get; set; }
        public int PointsSize { get; }
        public int Index { get; internal set; }

        public int Offset { get; }
        /// <summary>
        /// Record Length
        /// </summary>
        internal double DrawingXPosition { get; set; }

        internal int LastPointsSum { get; set; }
    }
}
