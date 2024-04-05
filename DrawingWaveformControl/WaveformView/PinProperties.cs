using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WaveformView
{
    public class PinProperties
    {
        public PinProperties(string name, int lineSize, int cycleSize, double maxScopeVoltage, double minScopeVoltage) 
        { 
            Name = name;
            LineSize = lineSize;
            CycleSize = cycleSize;
            CycleResults = new CycleResult[lineSize, cycleSize];
            MaxScopeVoltage = maxScopeVoltage;
            MinScopeVoltage = minScopeVoltage;
        }

        public string Name { get; }

        public int CycleSize { get; }

        public int LineSize { get; }

        private CycleResult[,] CycleResults { get; }

        public CycleResult this[int lineIndex, int cycleIndex]
        {
            get { return CycleResults[lineIndex, cycleIndex]; }
            set { CycleResults[lineIndex, cycleIndex] = value; }
        }

        public double MaxScopeVoltage { get; internal set; }

        public double MinScopeVoltage { get; internal set; }


        internal int Index { get; set; }

        public bool IsVisible { get; set; } = true;

        internal FormattedText NameFormattedText { get; set; }

        internal FormattedText MaxVoltFormattedText { get; set; }

        internal FormattedText MinVoltFormattedText { get; set; }
    }
}
