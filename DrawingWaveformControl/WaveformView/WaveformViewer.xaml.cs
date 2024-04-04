using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WaveformView
{
    /// <summary>
    /// UserControl1.xaml 的互動邏輯
    /// </summary>
    public partial class WaveformViewer : UserControl
    {

        public WaveformViewControl WaveformVieweControl { get; private set; }

        public WaveformViewer()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            WaveformVieweControl = waveformViewControl;
            WaveformVieweControl.OnUpdated += WaveformVieweControl_OnUpdated;
        }

        private void WaveformVieweControl_OnUpdated()
        {
            hornizontalSB.Maximum = WaveformVieweControl.MaxHornizontalScrollValue;
            verticalSB.Maximum = WaveformVieweControl.MaxVerticalScrollValue;
        }

        private void verticalSB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (waveformViewControl is null) { return; }
            WaveformVieweControl.VerticalScrollValue= e.NewValue;
        }

        private void hornizontalSB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (waveformViewControl is null) { return; }
            WaveformVieweControl.HornizontalScrollValue= e.NewValue;
        }
    }
}
