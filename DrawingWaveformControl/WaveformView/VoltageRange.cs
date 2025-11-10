using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformView
{
    public class VoltageRange
    {
        public VoltageRange(double max, double min, params VoltageLevelMarker[] voltageLevels)
        {
            MaxVolt = max;
            MinVolt = min;
            VoltageLevels = voltageLevels;
            Assert();
        }

        public bool ShowMaxVolt { get; set; } = true;

        public double MaxVolt { get; internal set; }

        public bool ShowMinVolt { get; set; } = true;

        public double MinVolt { get; internal set; }

        public IReadOnlyList<VoltageLevelMarker> VoltageLevels { get; }

        internal void Assert()
        {
            if (MaxVolt <= MinVolt)
            {
                throw new Exception("The maximum voltage value must be greater than the minimum voltage value.");
            }

            //20240329 Remove
            //if (VoltageLevels.Any(item => item.Voltage > MaxVolt || item.Voltage < MinVolt))
            //{
            //    throw new InvalidOperationException("The set voltage level must be within the range of maximum and minimum voltage.");
            //}
        }
    }

    public class VoltageLevelMarker
    {
        public double Voltage { get; set; }

        public string Name { get; set; }

        public bool Show { get; set; } = true;
    }
}
