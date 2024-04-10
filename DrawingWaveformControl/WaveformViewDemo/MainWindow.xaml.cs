using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WaveformView;

namespace WaveformViewDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public WaveformViewControl Instance { get; set; }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Instance = waveformViewer.WaveformVieweControl;
        }

        private void GenBtn_Click(object sender, RoutedEventArgs e)
        {
            var CyclePropertyItemsSource = new List<CycleProperties>()
            {
                new CycleProperties(10), new CycleProperties(20),
            };

            for(int cycleIndex = 0; cycleIndex < 50; cycleIndex++)
            {
                int pointSize = (new Random(DateTime.Now.Ticks.GetHashCode())).Next(2, 100);
                CyclePropertyItemsSource.Add(new CycleProperties(pointSize));
            }

            var PinPropertyItemsSource = new List<PinProperties>()
            {
            };

            for(int pinIndex = 0;pinIndex < 10; pinIndex++)
            {
                PinPropertyItemsSource.Add(new PinProperties($"pin{pinIndex}", 2, CyclePropertyItemsSource.Count, 3.3, -1.2));
            }

            var WaveformLinePropertyItemsSource = new List<WaveformLineProperties>()
            {
                new WaveformLineProperties("line1", Colors.Blue),
                new WaveformLineProperties("line2", Colors.Red),
            };


            for(int pinIndex = 0; pinIndex < PinPropertyItemsSource.Count; pinIndex++)
            {
                var pinProp = PinPropertyItemsSource[pinIndex];

                for (int cycleIndex = 0; cycleIndex < CyclePropertyItemsSource.Count; cycleIndex++)
                {
                    int pointSize = CyclePropertyItemsSource[cycleIndex].PointSize;
                    for(int lineIndex = 0; lineIndex < pinProp.LineSize; lineIndex++)
                    {
                        var cycleResult = new CycleResult(pointSize);

                        var random = new Random(pinIndex + cycleIndex + lineIndex + DateTime.Now.Millisecond.GetHashCode());
                        for (int pointIndex = 0; pointIndex < pointSize; pointIndex++)
                        {
                            cycleResult[pointIndex] = random.NextDouble() * - 1.2d + random.NextDouble() * 4.5d;
                        }

                        pinProp[lineIndex, cycleIndex] = cycleResult;
                    }
                    
                }
            }
            


            Instance.Setup(CyclePropertyItemsSource, PinPropertyItemsSource, WaveformLinePropertyItemsSource);

            Instance.Update();
        }
    }
}
