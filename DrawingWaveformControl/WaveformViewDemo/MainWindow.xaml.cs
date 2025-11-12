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

        private void Window_Initialized(object sender, EventArgs e)
        {
            ((App)App.Current).WCInstance = waveformViewer.Instance;
        }

        private void GenBtn_Click(object sender, RoutedEventArgs e)
        {
            var wcInstance = ((App)App.Current).WCInstance;
            var pinProperties = new List<PinProperties>(PinCount);
            var cycleProperties = new List<CycleProperties>(CycleCount);
            var lineProperties = new List<WaveformLineProperties>();
            
            lineProperties.Add(new WaveformLineProperties("L0") {Thickness = 1.0d, LineColor = Colors.Blue, Show = true });
            lineProperties.Add(new WaveformLineProperties("L1") { Thickness = 2.0d, LineColor = Colors.Red, Show = true });
            lineProperties.Add(new WaveformLineProperties("L2") { Thickness = 0.5d, LineColor = Colors.Green, Show = true });
            lineProperties.Add(new WaveformLineProperties("L3") { Thickness = 0.5d, LineColor = Colors.Orange, Show = true });


            for (int cycleIndex = 0; cycleIndex < CycleCount; cycleIndex++)
            {
                var random = new Random(DateTime.Now.GetHashCode());
                var cycleProp = new CycleProperties(1, random.Next(MinPointSize, MaxPointSize));
                cycleProperties.Add(cycleProp);
            }

            for (int pinIndex = 0; pinIndex < PinCount; pinIndex++)
            {
                var pinName = $"Pin {pinIndex}";
                var voltageRange = new VoltageRange(MaxVolt, MinVolt);
                var pinProp = new PinProperties(pinName, CycleCount, voltageRange);
                var lineCount = (pinIndex % 2 == 0) ? 2 : 1;


                for (int cycleIndex = 0; cycleIndex < CycleCount; cycleIndex++)
                {
                    var cycleResult = cycleProperties[cycleIndex];
                    var cyclePointSize = cycleResult.PointsSize;
                    var pinCycleResult = new CycleResults(lineCount, cyclePointSize);

                    pinProp.DrawingCycles[cycleIndex] = pinCycleResult;
                    
                    var random = new Random((int)DateTime.Now.Ticks + pinIndex);
                    for(int pointIndex = 0; pointIndex < cyclePointSize; pointIndex++)
                    {
                        var voltage0 = random.NextDouble() * (MaxVolt - MinVolt) + MinVolt;
                        pinCycleResult[0, pointIndex] = voltage0;

                        if(lineCount == 2)
                        {
                            var voltage1 = random.NextDouble() * (MaxVolt - MinVolt) + MinVolt;
                            pinCycleResult[1, pointIndex] = voltage1;
                        }
                    }
                }
                pinProperties.Add(pinProp);
            }

            wcInstance.Setup(cycleProperties);
            wcInstance.Setup(pinProperties);
            wcInstance.Setup(lineProperties);

            wcInstance.SpacingProperties.TimingResolution = 0.001; // 1 ms
            wcInstance.SpacingProperties.TimingUnit = SpacingProperties.ETimeUnit.Auto;
            

            wcInstance.SpacingProperties.TimingMeasurement.CursorName1 = "X0";
            wcInstance.SpacingProperties.TimingMeasurement.CursorName2 = "X1";

            App.Current.Dispatcher.Invoke(() =>
            {
                wcInstance.Render(WaveformContext.ERenderDirect.All);
            });
        }

        private const int PinCount = 16;
        private const int CycleCount = 10;
        private const double MaxVolt = 3.3;
        private const double MinVolt = 0.0;
        private const int MaxPointSize = 100;
        private const int MinPointSize = 16;

        private void AddTimCursor_Click(object sender, RoutedEventArgs e)
        {
            var wcInstance = ((App)App.Current).WCInstance;
            wcInstance.AddTimingCursor();
        }
    }
}
