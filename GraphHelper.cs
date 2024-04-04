using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using Windows.Foundation;

namespace graphlogic
{
   internal class GraphHelper
    {
        public delegate void NodeEventDelegate(UIElement node, PointerRoutedEventArgs e);

        public static void CreateNewPoint(Canvas canvas, string pointname, NodeEventDelegate pointerPressedHandler, NodeEventDelegate pointerMovedHandler, NodeEventDelegate pointerReleasedHandler) 
        {
            Ellipse node = new Ellipse
            {
                Name = pointname,
                Width = 50,
                Height = 35,
                Fill = new SolidColorBrush(Colors.White)
            };

            // These will wrap the event handlers so they can be attached to the Pointer events
            node.PointerPressed += (sender, e) => pointerPressedHandler(sender as UIElement, e);
            node.PointerMoved += (sender, e) => pointerMovedHandler(sender as UIElement, e);
            node.PointerReleased += (sender, e) => pointerReleasedHandler(sender as UIElement, e);

            canvas.Children.Add(node);
        }

        public static void FindPoint(Canvas canvas, string pointname)
        {
            Ellipse targetEllipse = canvas.Children.OfType<Ellipse>()
                              .FirstOrDefault(el => el.Name == pointname);

            if (targetEllipse != null)
            {
                // Assume you want to center the ellipse in the canvas
                double canvasCenterX = canvas.ActualWidth / 2;
                double canvasCenterY = canvas.ActualHeight / 2;

                // Find the current position of the ellipse
                GeneralTransform gt = targetEllipse.TransformToVisual(canvas);
                Point ellipseCenter = gt.TransformPoint(new Point(targetEllipse.Width / 2, targetEllipse.Height / 2));

                // Calculate the displacement needed to center the ellipse
                double deltaX = canvasCenterX - ellipseCenter.X;
                double deltaY = canvasCenterY - ellipseCenter.Y;

                // Move all objects by this displacement
                foreach (UIElement child in canvas.Children)
                {
                    // Apply the translation to each child element
                    TranslateTransform translateTransform = child.RenderTransform as TranslateTransform ?? new TranslateTransform();
                    child.RenderTransform = new TranslateTransform
                    {
                        X = translateTransform.X + deltaX,
                        Y = translateTransform.Y + deltaY
                    };
                }
            }

        }

        public static void ConnectPoints(Canvas canvas, string point1, string point2)
        {
            Ellipse first = canvas.Children.OfType<Ellipse>()
                              .FirstOrDefault(el => el.Name == point1);
            Ellipse second = canvas.Children.OfType<Ellipse>()
                              .FirstOrDefault(el => el.Name == point2);

            if (first != null && second != null)
            {
                Line line = new Line
                {
                    X1 = Canvas.GetLeft(first) + 25,
                    X2 = Canvas.GetLeft(second) + 25,
                    Y1 = Canvas.GetTop(first) + 17.5,
                    Y2 = Canvas.GetTop(second) + 17.5,
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 3
                };

                canvas.Children.Add(line);
            } else
            {
                System.Diagnostics.Debug.WriteLine("One or both points don't exist.");
            }
            
        }

    }
}
