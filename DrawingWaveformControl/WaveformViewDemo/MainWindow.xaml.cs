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
            var CyclePropertyItemsSource = new List<CycleProperties>();

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

            Instance.VoltUnit = EVoltUnit.Auto;
            Instance.VoltUnitDecimals = 2;

            Instance.TimeResolution = 0.001; // Sec
            Instance.TimeUnit = ETimeUnit.Auto; // Auto Trans
            Instance.TimeUnitDecimals= 2; // 1 ms in UI.

            Instance.Setup(CyclePropertyItemsSource, PinPropertyItemsSource, WaveformLinePropertyItemsSource);

            Instance.HornizontalScrollValue= 25.1; // scroll hornizontal to 25.1 position.
            Instance.VerticalScrollValue= 60.1; // scroll vertical to 60.1 position.


            Instance.ColorProperties = new ColorProperties()
            {
                Background = Colors.White,
                DefaultWaveformLine = Colors.Green,
                Grid = Colors.Black,
                MaxMinVoltLine= Colors.Red,
                Text = Colors.Black,
            };

            //Zoom In
            Instance.VerticalScale = 2.0d; // default 1.0d
            //Instance.HornizontalScale = 3.0d; // default 1.0d

            ////Zoom Out
            //Instance.VerticalScale = 0.3d; // default 1.0d
            //Instance.HornizontalScale = 0.5d; // default 1.0d

            Instance.Update();
        }
    }
}
