using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformView
{
    public enum EHitTestInfo
    {
        Normal,
        TimingMeasurement,
    }
    public interface IDrawingWaveformHitTestInfo
    {
        EHitTestInfo HitTestInfo { get; }
    }

    public class HitTestNormal : IDrawingWaveformHitTestInfo
    {
        public EHitTestInfo HitTestInfo => EHitTestInfo.Normal;
    }

    public class HitTestTimingMeasurement : IDrawingWaveformHitTestInfo
    {
        internal HitTestTimingMeasurement(TimingCursor cursor)
        {
            TimingCursor = cursor;
        }

        public EHitTestInfo HitTestInfo => EHitTestInfo.TimingMeasurement;

        public TimingCursor TimingCursor { get; internal set; }
    }
}
