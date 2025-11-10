using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace WaveformView
{
    public enum ECompare
    {
        NullValue = DefaultValues.NullValue,
        High,
        Low,
    }

    public enum ECompareInterface
    {
        Strobe,
        Window,
    }

    public interface IDrawingCompare
    {
        ECompareInterface CompareInterface { get; }
        ECompare Compare { get; set; }
        string MarkValueName { get; set; }
    }

    internal class MarkValue
    {
        public string Name { get; set; }
        public System.Windows.Point LeftPoint { get; set; }
        public double Voltage { get; set; }
    }

    public class DrawingCompareStrobe : IDrawingCompare
    {
        public DrawingCompareStrobe()
        {
            CompareInterface = ECompareInterface.Strobe;
        }

        /// <summary>
        /// Readonly
        /// </summary>
        public ECompareInterface CompareInterface { get; }

        public int PointIndex { get; set; } = DefaultValues.NullValue;

        public ECompare Compare { get; set; } = ECompare.NullValue;

        /// <summary>
        /// MarkValueName: 輸入箭頭指到線的名稱
        /// </summary>
        public string MarkValueName { get; set; }
    }

    public class DrawingCompareWindow : IDrawingCompare
    {
        public DrawingCompareWindow()
        {
            CompareInterface = ECompareInterface.Window;
        }

        /// <summary>
        /// Readonly
        /// </summary>
        public ECompareInterface CompareInterface { get; }

        public int PointIndex1 { get; set; } = DefaultValues.NullValue;

        public int PointIndex2 { get; set; } = DefaultValues.NullValue;

        public ECompare Compare { get; set; } = ECompare.NullValue;

        /// <summary>
        /// MarkValueName: 輸入箭頭指到線的名稱
        /// Ex: "MaxVolt", "MinVolt" or Other MarkValueNames
        /// </summary>
        public string MarkValueName { get; set; }
    }

    public class CycleResults
    {

        public CycleResults(int LineCount, int PointSize)
        {
            Voltages = new double[LineCount, PointSize];
            this.PointSize = PointSize;
            this.LineCount = LineCount;
            this.DrawingCompares = new List<IDrawingCompare>();
        }

        public int LineCount { get; }

        public int PointSize { get; }

        internal double[,] Voltages { get; }

        /// <summary>
        /// Create "DrawingCompareStrobe" or "DrawingCompareWindow" Instances Collection.
        /// </summary>
        public List<IDrawingCompare> DrawingCompares { get; }

        public string TopLabel { get; set; }

        public bool IsFail { get; set; } = false;

        public double this[int lineIndex, int pointIndex]
        {
            get
            {
                return Voltages[lineIndex, pointIndex];
            }
            set
            {
                Voltages[lineIndex, pointIndex] = value;
            }
        }

        public void AddCompare(DrawingCompareStrobe strobe)
        {
            DrawingCompares.Add(strobe);
        }

        public void AddCompare(DrawingCompareWindow window)
        {
            DrawingCompares.Add(window);
        }

        public void ClearCompares()
        {
            DrawingCompares.Clear();
        }
    }
}
