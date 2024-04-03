using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing.PrintSupport;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace graphlogic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            MyCanvas.ManipulationStarted += MyCanvas_ManipulationStarted;
            MyCanvas.ManipulationDelta += MyCanvas_ManipulationDelta;
            MyCanvas.ManipulationCompleted += MyCanvas_ManipulationCompleted;
        }

        private void MyCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // Handle manipulation started event if needed
        }


        private void MyCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Handle dragging logic
            var translation = e.Delta.Translation;
            foreach (UIElement child in MyCanvas.Children)
            {
                double left = Canvas.GetLeft(child);
                double top = Canvas.GetTop(child);

                Canvas.SetLeft(child, left + translation.X);
                Canvas.SetTop(child, top + translation.Y);
            }

            // Handle zooming logic
            var scale = e.Delta.Scale;
            if (Math.Abs(scale - 1.0) > Double.Epsilon) // Check if there is a meaningful scale change
            {
                foreach (UIElement child in MyCanvas.Children)
                {
                    // Get the current left and top values again as they may have changed
                    double left = Canvas.GetLeft(child);
                    double top = Canvas.GetTop(child);

                    // Initialize the transforms if they haven't been already
                    if (child.RenderTransform as TransformGroup == null)
                    {
                        child.RenderTransform = new TransformGroup
                        {
                            Children = new TransformCollection
                    {
                        new ScaleTransform(),
                        new TranslateTransform()
                    }
                        };
                    }

                    var transformGroup = (TransformGroup)child.RenderTransform;
                    var scaleTransform = (ScaleTransform)transformGroup.Children[0];
                    var translateTransform = (TranslateTransform)transformGroup.Children[1];

                    // Calculate the new scale factor and apply it
                    scaleTransform.ScaleX *= scale;
                    scaleTransform.ScaleY *= scale;

                    // Adjust the position to account for the new scale
                    double newLeft = (left + translateTransform.X - MyCanvas.ActualWidth / 2) * scale + MyCanvas.ActualWidth / 2 - translateTransform.X;
                    double newTop = (top + translateTransform.Y - MyCanvas.ActualHeight / 2) * scale + MyCanvas.ActualHeight / 2 - translateTransform.Y;

                    Canvas.SetLeft(child, newLeft);
                    Canvas.SetTop(child, newTop);
                }
            }
        }

        private void MyCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Handle manipulation completed event if needed
        }

        private void MyCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint(MyCanvas);

            // Determine the direction of the wheel scroll (positive away from user, negative towards)
            int delta = pointerPoint.Properties.MouseWheelDelta;

            // Set your scaling factor, could be a constant or based on the delta
            // Common practice is to make the scale factor per wheel delta increment (usually 120)
            double scale = delta > 0 ? 1.1 : 0.9;

            foreach (UIElement child in MyCanvas.Children)
            {
                // Apply the transformation
                ApplyScale(child, scale, pointerPoint.Position);
            }
        }

        /*private void ApplyScale(UIElement element, double scale, Point centerPoint)
        {
            if (element.RenderTransform is TransformGroup transformGroup)
            {
                var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();

                if (scaleTransform == null)
                {
                    scaleTransform = new ScaleTransform();
                    transformGroup.Children.Add(scaleTransform);
                }

                if (translateTransform == null)
                {
                    translateTransform = new TranslateTransform();
                    transformGroup.Children.Add(translateTransform);
                }

                // Adjust the scale from the center of the pointer
                scaleTransform.CenterX = centerPoint.X;
                scaleTransform.CenterY = centerPoint.Y;
                scaleTransform.ScaleX *= scale;
                scaleTransform.ScaleY *= scale;

                // Adjust the translate transform to account for the new scaling origin
                translateTransform.X -= (centerPoint.X - translateTransform.X) * (scale - 1);
                translateTransform.Y -= (centerPoint.Y - translateTransform.Y) * (scale - 1);
            }
            else
            {
                // Initialize the render transform if it does not exist
                TransformGroup newTransformGroup = new TransformGroup();
                ScaleTransform newScaleTransform = new ScaleTransform
                {
                    CenterX = centerPoint.X,
                    CenterY = centerPoint.Y,
                    ScaleX = scale,
                    ScaleY = scale
                };
                TranslateTransform newTranslateTransform = new TranslateTransform
                {
                    X = (1 - scale) * centerPoint.X,
                    Y = (1 - scale) * centerPoint.Y
                };

                newTransformGroup.Children.Add(newScaleTransform);
                newTransformGroup.Children.Add(newTranslateTransform);
                element.RenderTransform = newTransformGroup;
            }
        }*/

        private const double MinScale = 0.3; // Minimum zoom level
        private const double MaxScale = 5.0; // Maximum zoom level

        private void ApplyScale(UIElement element, double requestedScale, Point centerPoint)
        {
            if (element.RenderTransform is TransformGroup transformGroup)
            {
                var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();

                if (scaleTransform == null)
                {
                    scaleTransform = new ScaleTransform();
                    transformGroup.Children.Add(scaleTransform);
                }

                if (translateTransform == null)
                {
                    translateTransform = new TranslateTransform();
                    transformGroup.Children.Add(translateTransform);
                }

                // Calculate the expected new scale
                double newScaleX = scaleTransform.ScaleX * requestedScale;
                double newScaleY = scaleTransform.ScaleY * requestedScale;

                // Check if we're already at the limit, and if so, do not apply further scaling or translation
                if ((newScaleX >= MaxScale && requestedScale > 1) || (newScaleX <= MinScale && requestedScale < 1))
                {
                    return; // Stop here if we're already at the zoom limit
                }

                // Clamp the new scale within the limits
                newScaleX = Math.Max(MinScale, Math.Min(MaxScale, newScaleX));
                newScaleY = Math.Max(MinScale, Math.Min(MaxScale, newScaleY));

                // Adjust the scale from the center of the pointer
                scaleTransform.CenterX = centerPoint.X;
                scaleTransform.CenterY = centerPoint.Y;

                // Determine the actual scale change after clamping
                double actualScaleChangeX = newScaleX / scaleTransform.ScaleX;
                double actualScaleChangeY = newScaleY / scaleTransform.ScaleY;

                scaleTransform.ScaleX = newScaleX;
                scaleTransform.ScaleY = newScaleY;

                // Adjust the translate transform to account for the new scaling origin only if there was an actual scale change
                if (actualScaleChangeX != 1)
                {
                    translateTransform.X -= (centerPoint.X - translateTransform.X) * (actualScaleChangeX - 1);
                }
                if (actualScaleChangeY != 1)
                {
                    translateTransform.Y -= (centerPoint.Y - translateTransform.Y) * (actualScaleChangeY - 1);
                }
            }
            else
            {
                // Initialize the render transform if it does not exist
                TransformGroup newTransformGroup = new TransformGroup();
                ScaleTransform newScaleTransform = new ScaleTransform
                {
                    CenterX = centerPoint.X,
                    CenterY = centerPoint.Y,
                    ScaleX = requestedScale,
                    ScaleY = requestedScale
                };
                TranslateTransform newTranslateTransform = new TranslateTransform
                {
                    X = (1 - requestedScale) * centerPoint.X,
                    Y = (1 - requestedScale) * centerPoint.Y
                };

                newTransformGroup.Children.Add(newScaleTransform);
                newTransformGroup.Children.Add(newTranslateTransform);
                element.RenderTransform = newTransformGroup;
            }
        }
    }
}
