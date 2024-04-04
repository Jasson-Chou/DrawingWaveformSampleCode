using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace WaveformView
{
    public class CycleResult
    {
        public CycleResult(int pointSize)
        {
            this.PointSize = pointSize;
            Values = new double[pointSize];

            for(int index = 0; index < pointSize; index++) { Values[index] = double.NaN; }
        }

        internal int Index { get; set; }

        public int PointSize { get; }

        private double[] Values { get; }

        public double this[int index]
        {
            get { return Values[index]; }
            set { Values[index] = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int index = 0; index < Values.Length; index++) 
            {
                sb.Append($"[{index},{Values[index]}]");
            }
            return sb.ToString();
        }
    }
}
