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

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ColorPicker_UserControl.xaml
    /// </summary>
    public partial class ColorPicker_UserControl : UserControl
    {
        public ColorPicker_UserControl()
        {
            InitializeComponent();            
            UpdateColorFromThumbPosition();

        }
        private bool isTrackingMouse = false;
        private Color baseSpectrumColor = Colors.Red; // Stores color without brightness modifications


        private void ColorSpectrum_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isTrackingMouse = true;
            ColorSpectrum.CaptureMouse();
            UpdateThumbPosition(e.GetPosition(ColorSpectrum));
        }


        private void ColorSpectrum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isTrackingMouse)
            {
                isTrackingMouse = false;
                ColorSpectrum.ReleaseMouseCapture();
            }
        }

        private void UpdateThumbPosition(Point point)
        {
            // Keep the tracking point strict within the 0 to 250 boundary box
            double x = Math.Max(0, Math.Min(point.X, ColorSpectrum.Width));
            double y = Math.Max(0, Math.Min(point.Y, ColorSpectrum.Height));

            // Position the thumb circle centrally over the cursor coordinate
            Canvas.SetLeft(ColorThumb, x - (ColorThumb.Width / 2));
            Canvas.SetTop(ColorThumb, y - (ColorThumb.Height / 2));

            UpdateColorFromThumbPosition();
        }

        private void UpdateColorFromThumbPosition()
        {
            // 1. Get the current position of the center of the thumb
            double x = Canvas.GetLeft(ColorThumb) + (ColorThumb.Width / 2);
            double y = Canvas.GetTop(ColorThumb) + (ColorThumb.Height / 2);

            // Safety check against component boundaries during scaling updates
            if (x < 0 || x >= ColorSpectrum.Width || y < 0 || y >= ColorSpectrum.Height) return;

            // 2. Render out a snapshot image sequence of the spectrum element
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                (int)ColorSpectrum.Width, (int)ColorSpectrum.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(ColorSpectrum);

            // 3. Extract the exact individual pixel bytes under the thumb location
            byte[] pixels = new byte[4];
            renderTarget.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, 4, 0);

            // Store the true un-brightness-modified color block
            baseSpectrumColor = Color.FromRgb(pixels[2], pixels[1], pixels[0]);

            // Apply the final calculated value combining spectrum color with slider modifiers
            ApplyFinalColorCalculations();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyFinalColorCalculations();
        }

        private void ApplyFinalColorCalculations()
        {
            if (ColorPreview == null || BrightnessSlider == null) return;

            // The slider goes from 0 (all black) to 1 (full saturation)
            double brightnessFactor = BrightnessSlider.Value;

            // Mathematically downscale RGB channels linearly based on brightness value
            byte finalR = (byte)(baseSpectrumColor.R * brightnessFactor);
            byte finalG = (byte)(baseSpectrumColor.G * brightnessFactor);
            byte finalB = (byte)(baseSpectrumColor.B * brightnessFactor);

            Color finalColor = Color.FromRgb(finalR, finalG, finalB);

            // Update UI elements 
            ColorPreview.Background = new SolidColorBrush(finalColor);
            HexDisplay.Text = $"#{finalColor.R:X2}{finalColor.G:X2}{finalColor.B:X2}";
        }

        private void ColorSpectrum_MouseMove(object sender, MouseEventArgs e)
        {
            if (isTrackingMouse)
            {
                UpdateThumbPosition(e.GetPosition(ColorSpectrum));
            }
        }
    }
}
