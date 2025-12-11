using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public WaveformContext Instance { get; private set; }

        public WaveformViewer()
        {
            InitializeComponent();
        }

        private bool IsMouseEnter { get; set; }

        private bool IsMouseLeftDown { get; set; }

        private bool IsMouseRightDown { get; set; }

        private Point MouseRightDownPoint { get; set; }

        private Point MousePoint { get; set; }

        private Point LastMousePoint { get; set; }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Instance = new WaveformContext(myImage);

            DrawingWaveform_Initialize();
        }

        private void DrawingWaveform_Initialize()
        {
            pixelXSlider.Maximum = Instance.SpacingProperties.MaxScopeXScaleValue;
            pixelXSlider.Minimum = Instance.SpacingProperties.MinScopeXScaleValue;

            pixelYSlider.Maximum = Instance.SpacingProperties.MaxScopeYScaleValue;
            pixelYSlider.Minimum = Instance.SpacingProperties.MinScopeYScaleValue;

            Instance.OnScrollValueChanged += Instance_OnScrollValueChanged;
            Instance.OnScrollMaxValueChanged += Instance_OnScrollMaxValueChanged;
            Instance.OnZoomInOutChanged += Instance_OnZoomInOutChanged;
        }

        private void Instance_OnZoomInOutChanged()
        {
            if (Instance is null) return;
            pixelXSlider.Value = Instance.SpacingProperties.ScopeXScaleValue;
            pixelYSlider.Value = Instance.SpacingProperties.ScopeYScaleValue;
        }

        private void Instance_OnScrollValueChanged()
        {
            if (Instance is null) return;
            hornizontalScroll.Value = Instance.ScrollHornizontalValue < 0 ? 0 : (Instance.ScrollHornizontalValue > Instance.MaxScrollHornizontal ? Instance.MaxScrollHornizontal : Instance.ScrollHornizontalValue);
            Instance.ScrollHornizontalValue = hornizontalScroll.Value;
            verticalScroll.Value = Instance.ScrollVerticalValue < 0 ? 0 : (Instance.ScrollVerticalValue > Instance.MaxScrollVertical ? Instance.MaxScrollVertical : Instance.ScrollVerticalValue);
            Instance.ScrollVerticalValue = verticalScroll.Value;
            Instance.Render(WaveformContext.ERenderDirect.All);
        }

        private void Instance_OnScrollMaxValueChanged()
        {
            if (Instance is null) return;
            InitScrollBarValues();
        }

        private void MyGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.IsEmpty) return;
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0) return;
            if (Instance is null) return;
            Instance.SpacingProperties.MW = e.NewSize.Width;
            Instance.SpacingProperties.MH = e.NewSize.Height;
            Debug.WriteLine($"Actual Window Width: {Instance.SpacingProperties.ActualMW}");
            Debug.WriteLine($"Actual Window Height: {Instance.SpacingProperties.ActualMH}");
            Instance.Render(WaveformContext.ERenderDirect.All);
            InitScrollBarValues();
        }

        private void MyImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is null || Instance is null) return;

            MousePoint = e.GetPosition(sender as Image);

            Instance.MouseMove(MousePoint);

            Instance.Render(WaveformContext.ERenderDirect.MouseMoving);

            var hitTestInfo = Instance.HitTest(MousePoint);

            if (hitTestInfo is null)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                return;
            }

            switch (hitTestInfo.HitTestInfo)
            {
                case EHitTestInfo.Normal:
                    {
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Cross;
                        if (IsMouseLeftDown)
                        {
                            var vector = LastMousePoint - MousePoint;
                            ScrollVerticalOffset(vector.Y);
                            ScrollHorizontalOffset(vector.X);

                            Instance.Render(WaveformContext.ERenderDirect.All);
                        }
                        break;
                    }
                case EHitTestInfo.TimingMeasurement:
                    {
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeWE;
                        if (IsMouseLeftDown)
                        {
                            var timingCursor = hitTestInfo as HitTestTimingMeasurement;
                            Instance.MovingTimingCursor(timingCursor.TimingCursor);
                            Instance.Render(WaveformContext.ERenderDirect.MouseMoving);
                        }
                        break;
                    }
            }

            LastMousePoint = MousePoint;
        }

        private void MyImage_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseEnter = true;
        }

        private void MyImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;

            IsMouseLeftDown = false;

            IsMouseEnter = false;

            Instance.MouseMove(null);

            Instance.Render(WaveformContext.ERenderDirect.MouseMoving);
        }

        private void MyImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseLeftDown = true;
        }

        private void MyImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseLeftDown = false;

            if (Instance is null) return;

            Instance.AddTimingCursorDone();
        }

        private void MyImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Instance is null) return;

            if (IsMouseRightDown)
            {
                var vector = MousePoint - MouseRightDownPoint;

                Instance.MouseZoomInOut(MouseRightDownPoint, vector);

                pixelXSlider.Value = Instance.SpacingProperties.ScopeXScaleValue;

                pixelYSlider.Value = Instance.SpacingProperties.ScopeYScaleValue;
            }
        }

        private void MyImage_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Instance is null) return;

            var hitTestInfo = Instance.HitTest(MousePoint);

            if (hitTestInfo is null) return;

            if (hitTestInfo.HitTestInfo == EHitTestInfo.TimingMeasurement)
            {
                var timingCursor = hitTestInfo as HitTestTimingMeasurement;
                Instance.RemoveTimingCursor(timingCursor.TimingCursor);
                Instance.Render(WaveformContext.ERenderDirect.MouseMoving);
            }
            else
            {
                IsMouseRightDown = true;
                MouseRightDownPoint = MousePoint;
            }
        }

        private void InitScrollBarValues()
        {
            if (Instance is null) return;

            hornizontalScroll.ViewportSize = Instance.SpacingProperties.WW < 0 ? 0 : Instance.SpacingProperties.WW;
            verticalScroll.ViewportSize = Instance.SpacingProperties.N_WH < 0 ? 0 : Instance.SpacingProperties.N_WH;

            hornizontalScroll.Maximum = Instance.MaxScrollHornizontal;
            verticalScroll.Maximum = Instance.MaxScrollVertical;
        }

        private void ScrollVerticalValue(double newValue)
        {
            Instance.ScrollVerticalValue = newValue < 0 ? 0 : newValue;
            Instance.Render(WaveformContext.ERenderDirect.Vertical);
        }

        private void ScrollHorizontalValue(double newValue)
        {
            Instance.ScrollHornizontalValue = newValue < 0 ? 0 : newValue;
            Instance.Render(WaveformContext.ERenderDirect.Hornizontal);
        }

        private void ScrollVerticalOffset(double newValue)
        {
            var temp = Instance.ScrollVerticalValue + newValue;
            if (temp < 0 || temp > Instance.MaxScrollVertical) return;
            Instance.ScrollVerticalValue = temp;
            verticalScroll.Value = temp;
        }

        private void ScrollHorizontalOffset(double newValue)
        {
            var temp = Instance.ScrollHornizontalValue + newValue;
            if (temp < 0 || temp > Instance.MaxScrollHornizontal) return;
            Instance.ScrollHornizontalValue = temp;
            hornizontalScroll.Value = temp;
        }

        private void VerticalScroll_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (Instance is null) return;
            ScrollVerticalValue(e.NewValue);
        }

        private void HornizontalScroll_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (Instance is null) return;
            ScrollHorizontalValue(e.NewValue);
        }

        private void PixelXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Instance is null) return;

            Instance.SpacingProperties.ScopeXScaleValue = e.NewValue;
            Instance.Render(WaveformContext.ERenderDirect.ZoomInOut);
            InitScrollBarValues();
        }

        private void PixelYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Instance is null) return;
            Instance.SpacingProperties.ScopeYScaleValue = e.NewValue;
            Instance.Render(WaveformContext.ERenderDirect.ZoomInOut);
            InitScrollBarValues();
        }

        private void MyImage_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Instance is null || !IsMouseEnter) return;

            var value = e.Delta / 120;
            var absVal = Math.Abs(value);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Instance.MouseZoomInOut(MousePoint, value < 0);

                pixelXSlider.Value = Instance.SpacingProperties.ScopeXScaleValue;

                pixelYSlider.Value = Instance.SpacingProperties.ScopeYScaleValue;
            }
            else if (verticalScroll.Maximum > 0.0d)
            {
                var adjustVal = Instance.SpacingProperties.ActualWaveformHeight / 2.0d;
                var currVerticalValue = Instance.ScrollVerticalValue;

                if (value > 0) adjustVal *= -1;

                for (int index = 0; index < absVal; index++)
                {
                    currVerticalValue += adjustVal;

                    if (value < 0 && currVerticalValue > Instance.MaxScrollVertical)
                    {
                        currVerticalValue = Instance.MaxScrollVertical;
                        break;
                    }
                    else if (value > 0 && currVerticalValue < 0)
                    {
                        currVerticalValue = 0;
                        break;
                    }
                }
                verticalScroll.Value = currVerticalValue;
                Instance.ScrollVerticalValue = currVerticalValue;
            }
        }

        private void MyImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Instance is null || !IsMouseEnter || !verticalScroll.IsEnabled)
            {
                Instance.ScrollVerticalValue = 0;
                return;
            }

            Instance.Render(WaveformContext.ERenderDirect.Vertical);
        }

        private void DefaultZoomInOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Instance is null) return;

            Instance.ResetZoomInOut();

            Instance.Render(WaveformContext.ERenderDirect.All);
        }
    }
}
