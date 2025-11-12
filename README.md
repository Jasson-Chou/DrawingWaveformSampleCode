# Drawing Waveform With DrawingContext
Github: https://github.com/Jasson-Chou/DrawingWaveformSampleCode.git

### **Overview**

This project is a lightweight user control for drawing waveform diagrams. With simple configuration, real-time drawing functionality can be achieved. It also provides highly customizable display options, such as showing the position of Strobes in each Channel, displaying text in Cycles, and more.

---

### **Requirements**

- .NET Framework 4.8

---

### **Build & Run**

1. Open DrawingWaveformControlSolution.sln
2. Select Release | Debug and build
3. Run the program

---

### **Sample Code Explanation**

In the `MainWindow.xaml.cs` file of the `WaveformViewDemo` project, there is a simple sample program. The execution steps of the `GenBtn_Click` event are as follows:

1. Create lineProperties—two lines (blue and red), and configure their thickness and display properties.
2. Create cycleProperties—add CycleProperties for each cycle, with PointsSize determined by a random number.
3. Create pinProperties—create PinProperties for each pin, where each pin contains CycleResults for multiple cycles. Each point is filled with a random double-precision voltage value (ranging between MinVolt and MaxVolt), and even-numbered pins contain data for two lines.
4. Inject cycleProperties, pinProperties, and lineProperties into WaveformContext sequentially through wcInstance.Setup(...).
5. Configure SpacingProperties—including timing resolution (TimingResolution), timing unit (TimingUnit), and timing cursor name.
6. Use Dispatcher.Invoke to call wcInstance.Render(WaveformContext.ERenderDirect.All) on the UI thread to render the waveform.

---

### **Demo - Multi-channel Waveform View**

- Each row represents a Pin (Pin0 ~ Pin4).
- The waveform has two lines: Line0 (blue) and Line1 (red), which can be overlaid to display two sets of measurements for the same Pin.
- The Y-axis is marked with voltage, with dashed lines at 0 V and 3.3 V serving as references.
- The X-axis represents time, with scales and annotations at the bottom and middle (for example, 19 ms, 99 ms).
- The top-left corner has a legend indicating the line names and colors.
- The status bar at the bottom of the window displays real-time information corresponding to the cursor position: Pin name, Offset, Index, and the voltage values (Volts) of both lines at that point in time, along with the mouse coordinates.
- The X and Y sliders at the very bottom are used to pan or zoom the viewing range.

![image.png](ReadmeSrc/image.png)

---

### **GUI Layout**

**Line Legend**

![image.png](ReadmeSrc/image%201.png)

Displays the names of the lines at the top of the screen. Users can define the number of lines, their colors, names, and thickness.

**Pin (Channel)**

Displays user-defined pin names on the left side of the screen.

![image.png](ReadmeSrc/image%202.png)

**Cycles**

Based on the time width defined by the user for each Cycle.

![image.png](ReadmeSrc/image%203.png)

**Time Bar**

Located at the bottom of the screen, it displays Cycle time, mouse position time, and Timing Cursor time to help users identify specific time points.

![image.png](ReadmeSrc/image%204.png)

**Voltage Bar**

Located on the left side of the screen, between the pin names and the waveform plot, 
it displays the voltage range being observed as well as annotations for the voltage value at the mouse cursor position.

![image.png](ReadmeSrc/image%205.png)

**Cursor**

Add Timing Cursor, which can be repositioned by dragging with the mouse.

![image.png](ReadmeSrc/image%206.png)

**Cursor Information**

- **Mouse Cursor**
Located at the very bottom of the screen, it displays the pin name currently pointed to by the mouse, Offset, Index, the voltage values of each line, and the corresponding time point.
    
    ![image.png](ReadmeSrc/image%207.png)
    
- **Timing Cursor**
Located in the top-right corner of the screen, it displays the time difference between Timing Cursors.
    
    ![image.png](ReadmeSrc/image%208.png)
    

**Waveform Plot**

Located in the center of the screen, displaying the voltage values of each pin at discontinuous but evenly spaced time intervals.

![image.png](ReadmeSrc/image%209.png)

---

### **APIs** [Under Construction]

**Line**

**Mouse Cursor**

**Timing Cursor**

**Jump Offset**

**Zoom In/Out**

Export Picture

---

### **Future Plans**

To improve the Demo functionality showcase, the Demo does not yet fully demonstrate all features.

- [x]  Improve the GUI Layout documentation
- [ ]  Improve the APIs documentation
- [ ]  Add user-defined Channel quantity
- [ ]  Add user-defined Cycle quantity
- [ ]  Add user-defined number of points per Cycle
- [ ]  Add functionality to dynamically modify Channel and line names
- [ ]  Add Waveform auto-fit to screen functionality