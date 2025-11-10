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
        public PinProperties(string name, int cycleCount, VoltageRange voltageRange)
        {
            this.Name = name;
            VoltageRange = voltageRange;
            this.DrawingCycles = new CycleResults[cycleCount];
            TopLabel = null;
        }

        public PinProperties(string name, string topLabel, int cycleCount, VoltageRange voltageRange)
        {
            this.Name = name;
            VoltageRange = voltageRange;
            this.DrawingCycles = new CycleResults[cycleCount];
            TopLabel = topLabel;
        }

        public string Name { get; }

        /// <summary>
        /// Label On Pin Header.
        /// </summary>
        public string TopLabel { get; set; }

        public bool Show { get; set; } = true;

        /// <summary>
        /// Changing Show Sequence With Index
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Fail Marker On Pin Header
        /// </summary>
        public bool HasFail => DrawingCycles.Any(item => item.IsFail);

        public VoltageRange VoltageRange { get; }

        public CycleResults[] DrawingCycles { get; }
    }
}
