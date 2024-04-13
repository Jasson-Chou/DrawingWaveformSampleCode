# Drawing Waveform With DrawingContext
## Introduction
In cases where a large number of pins are displayed, standard components may not be able to handle the load (resulting in lag and limited functionality). Therefore, I have provided a free example of how to efficiently draw large numbers of pin waveforms using C# WPF.
## Development Environment
* Visual Studio 2022
* .Net Framework 4.8
* Language Visual CSharp
## User Manual
### API Example
#### Create Waveoform View Control Instance
```XAML
xmlns:waveformView="clr-namespace:WaveformView;assembly=WaveformView"

<waveformView:WaveformViewer x:Name="waveformViewer"/>
```

```C#
using WaveformView;

namespace WaveformViewDemo
{
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
    }
}
```

#### Add Pins / Cycles / Waveform Line Properties
##### Create Pins
```C#
var PinPropertyItemsSource = new List<PinProperties>();
for(int pinIndex = 0;pinIndex < 10; pinIndex++)
{
    // PinProperties(# pin name, # line count, # Cycle Count, # Max volt, # Min Volt)
    // pin name : string
    // line count : int
    // cycle count : int
    // max volt(Volt) : double
    // min volt(Volt) : double
    PinPropertyItemsSource.Add(new PinProperties($"pin{pinIndex}", 2, CyclePropertyItemsSource.Count, 3.3, -1.2));
}
```
##### Create Cycles
```C#
var CyclePropertyItemsSource = new List<CycleProperties>();
// Create 50 Cycle Count
for(int cycleIndex = 0; cycleIndex < 50; cycleIndex++)
{
    // Example: Random Cycle Points
    int pointSize = (new Random(DateTime.Now.Ticks.GetHashCode())).Next(2, 100);
    CyclePropertyItemsSource.Add(new CycleProperties(pointSize));
}
```
##### Create Waveform Properties Line
```C#
// Create 2 waveform line
var WaveformLinePropertyItemsSource = new List<WaveformLineProperties>()
{
    new WaveformLineProperties("line1", Colors.Blue), // line1 blue color
    new WaveformLineProperties("line2", Colors.Red), // line2 red color
};
```
##### Instance Setup
```C#
Instance.Setup(CyclePropertyItemsSource, PinPropertyItemsSource, WaveformLinePropertyItemsSource);
```

#### Timing Setting
#### Scroll Hornizontal/Vertical 
#### VerticalScrollValue
#### Color Change
#### Zoom In/Out
#### Show Voltage/ Timing Unit

![fulldemo](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/4524ec21-730d-4db3-a687-cde8c4de160f)
![mouseMove](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/ec3aecc1-778c-4094-8990-3c801ed17839)
![timing cursor](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/aef4bef4-8214-42fd-84ca-98d77e21ec29)
