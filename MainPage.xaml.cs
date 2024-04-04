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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace graphlogic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    
    public sealed partial class MainPage : Page
    {
        public PointerEventHandler pointerPressedHandler;
        PointerEventHandler pointerMovedHandler;
        PointerEventHandler pointerReleasedHandler;
        public MainPage()
        {
            this.InitializeComponent();
            MyCanvas.ManipulationStarted += MyCanvas_ManipulationStarted;
            MyCanvas.ManipulationDelta += MyCanvas_ManipulationDelta;
            MyCanvas.ManipulationCompleted += MyCanvas_ManipulationCompleted;
            GraphHelper.CreateNewPoint(MyCanvas, "point1",
                                        Node_PointerPressed,
                                        Node_PointerMoved,
                                        Node_PointerReleased);
            pointerPressedHandler = Node_PointerPressed;
            pointerMovedHandler = Node_PointerMoved;
            pointerReleasedHandler = Node_PointerReleased;
            
            foreach (UIElement node in MyCanvas.Children)
            {
                node.PointerPressed += Node_PointerPressed;
                node.PointerMoved += Node_PointerMoved;
                node.PointerReleased += Node_PointerReleased;
            }
        }

        public void Node_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var node = sender as UIElement;
            if (node != null)
            {
                isDragging = true;
                selectedNode = node;
                originalPosition = e.GetCurrentPoint(MyCanvas).Position;
                node.CapturePointer(e.Pointer); // To track the pointer outside the element's bounds
            }
        
        
        }

        private bool isDragging = false;
        private UIElement selectedNode = null;
        private Point originalPosition;




        public void Node_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("node_pointermoved outside IF is working");
            if (isDragging && selectedNode != null)
            {
                System.Diagnostics.Debug.WriteLine("node_pointermoved IF is working");
                var currentPosition = e.GetCurrentPoint(MyCanvas).Position;
                var transform = selectedNode.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    selectedNode.RenderTransform = transform;
                }

                // Move the node
                transform.X += currentPosition.X - originalPosition.X;
                transform.Y += currentPosition.Y - originalPosition.Y;

                // Update the original position
                originalPosition = currentPosition;

                // Update connected edges
/*                UpdateConnectedEdges(selectedNode, transform.X, transform.Y);
*/            }
        }

        public void Node_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (isDragging)
            {
                var node = sender as UIElement;
                node?.ReleasePointerCapture(e.Pointer);
                isDragging = false;
                selectedNode = null;
            }
        }

        /*private void UpdateConnectedEdges(UIElement node, double offsetX, double offsetY)
        {
            // Assuming nodes and edges have been previously mapped
            foreach (var edge in GetConnectedEdges(node))
            {
                // Update the position of the edge's start or end point depending on the node it's connected to.
                // You'll need to implement GetConnectedEdges and a way to determine whether the node is at the start or end of the edge.
            }
        }*/


        private void MyCanvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // Handle manipulation started event if needed
        }


        private void MyCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (isDragging == false)
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
            double scale = delta > 0 ? 1.05 : 0.95;

            foreach (UIElement child in MyCanvas.Children)
            {
                // Apply the transformation
                ApplyScale(child, scale, pointerPoint.Position);
            }
        }

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
                //f
                newTransformGroup.Children.Add(newScaleTransform);
                newTransformGroup.Children.Add(newTranslateTransform);
                element.RenderTransform = newTransformGroup;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement child in MyCanvas.Children)
            {
                if (child is TextBox textBox && textBox.Name == "NameTextBox")
                {
                    GraphHelper.CreateNewPoint(MyCanvas, textBox.Text,
                                                Node_PointerPressed, 
                                                Node_PointerMoved,
                                                Node_PointerReleased);
                }
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (UIElement child in MyCanvas.Children)
            {
                if (child is TextBox textBox && textBox.Name == "NameTextBox")
                {
                    GraphHelper.FindPoint(MyCanvas, textBox.Text);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TextBox fisttextbox = MyCanvas.Children.OfType<TextBox>()
                              .FirstOrDefault(el => el.Name == "NameTextBox");
            TextBox secondtextbox = MyCanvas.Children.OfType<TextBox>()
                              .FirstOrDefault(el => el.Name == "SecondTextBox");
            GraphHelper.ConnectPoints(MyCanvas, fisttextbox.Text, secondtextbox.Text);
        }
    }
}
