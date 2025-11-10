# Drawing Waveform With DrawingContext
## Introduction
In cases where a large number of pins are displayed, standard components may not be able to handle the load (resulting in lag and limited functionality). Therefore, I have provided a free example of how to efficiently draw large numbers of pin waveforms using C# WPF.
## Development Environment
* Visual Studio 2022
* .Net Framework 4.8
* Language Visual CSharp
## Next objectives to implement
- [ ] Fix the issue where the first item fails to render.
- [ ] Add a demo feature for horizontal and vertical zoom in/out.
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
```C#
Instance.TimeResolution = 0.001; // Sec
Instance.TimeUnit = ETimeUnit.Auto; // Auto Trans
Instance.TimeUnitDecimals= 2; // 1 ms in UI.
```
#### Scroll Hornizontal/Vertical 
```C#
Instance.HornizontalScrollValue= 25.1; // scroll hornizontal to 25.1 position.
Instance.VerticalScrollValue= 60.1; // scroll vertical to 60.1 position.
```
#### Color Change
```C#
Instance.ColorProperties = new ColorProperties()
{
    Background = Colors.White,
    DefaultWaveformLine = Colors.Green,
    Grid = Colors.Black,
    MaxMinVoltLine= Colors.Red,
    Text = Colors.Black,
};

Instance.Update();
```
![change color](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/a2d1cf0d-1e40-4de7-ace9-013d9b28e7c3)

#### Zoom In/Out
```C#
//Zoom Out
Instance.VerticalScale = 2.0d; // default 1.0d
Instance.HornizontalScale = 3.0d; // default 1.0d

//Zoom In
Instance.VerticalScale = 0.3d; // default 1.0d
Instance.HornizontalScale = 0.5d; // default 1.0d
```
#### Show Voltage Unit
```C#
Instance.VoltUnit = EVoltUnit.Auto;
Instance.VoltUnitDecimals = 2;
```

![fulldemo](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/4524ec21-730d-4db3-a687-cde8c4de160f)
![mouseMove](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/ec3aecc1-778c-4094-8990-3c801ed17839)
![timing cursor](https://github.com/Jasson-Chou/DrawingWaveformSampleCode/assets/74143452/aef4bef4-8214-42fd-84ca-98d77e21ec29)
