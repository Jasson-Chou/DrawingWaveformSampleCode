using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformView
{
    public class TimingCursor
    {
        public string Name { get; set; }

        public int CycleIndx { get; internal set; }

        public int PointIndx { get; internal set; }

        /// <summary>
        /// Unit: second
        /// </summary>
        public double Time { get; internal set; }

        internal bool Moving { get; set; }
    }
}
