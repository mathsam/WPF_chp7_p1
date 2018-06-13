#define DEBUG
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Ink;

namespace BackEnd
{
    enum DrawMode
    {
        FreeScratch,
        Line,
        Arc
    }

    class CanvasCanMeasure : InkCanvas
    {

        BitmapImage BackgroundImag;

        //total length of lines that user has drawn
        public double TotalDrawingLength {
            get => MeasureStrokesLength();
        }

        TextBlock MeasurementDisplay;

        public DrawMode CurrDrawMode { get; set; } = DrawMode.FreeScratch;

        double _strokethickness;
        public double StrokeThickness
        {
            get => _strokethickness;
            set
            {
                _strokethickness = value;
                DefaultDrawingAttributes.Width = DefaultDrawingAttributes.Height = value;
            }
        }

        public CanvasCanMeasure()
        {
            Background = new SolidColorBrush(Colors.Gray);
            //IsEnabled = false;
            ResizeEnabled = false;
            ClipToBounds = true;
            StrokeThickness = 6;
            DefaultDrawingAttributes.FitToCurve = true;
            DefaultDrawingAttributes.IsHighlighter = true;
            DefaultDrawingAttributes.IgnorePressure = true;
            DefaultDrawingAttributes.Color = Colors.SpringGreen;
            DefaultDrawingAttributes.StylusTip = StylusTip.Ellipse;

            MeasurementDisplay = new TextBlock();
            SetLeft(MeasurementDisplay, 20);
            SetTop(MeasurementDisplay, 10);
            MeasurementDisplay.Text = "Length = 0";
            MeasurementDisplay.Background = Brushes.LightYellow;
            MeasurementDisplay.Padding = new Thickness(10, 5, 10, 5);
            Children.Add(MeasurementDisplay);

            AllowDrop = true;
            Drop += CanvasCanMeasure_Drop;
        }

        protected void ResetCanvas()
        {
            Strokes.Clear();
            Children.Clear();
            MeasurementDisplay.Text = "Length = 0";
            Children.Add(MeasurementDisplay);
        }

        public void SetBackgroundImage(string FilePath)
        {
            ResetCanvas();

            ImageBrush imgBrush = new ImageBrush();
            BackgroundImag = new BitmapImage(new Uri(FilePath, UriKind.RelativeOrAbsolute));
            imgBrush.ImageSource = BackgroundImag;
            Width = imgBrush.ImageSource.Width;
            Height = imgBrush.ImageSource.Height;
            Background = imgBrush;
            IsEnabled = true;
        }

        private void CanvasCanMeasure_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                SetBackgroundImage(files[0]);
            }
        }

        public double MeasureStrokesLength()
        {
            double total_length = 0;
            foreach(Stroke stroke in Strokes)
            {
                total_length += MeasureOneStrokeLength(stroke.GetBezierStylusPoints());
            }
#if (DEBUG)
            Console.WriteLine(total_length);
#endif
            return total_length;

        }

        public double MeasureOneStrokeLength(StylusPointCollection points)
        {
            double length = 0;
            for (int i=1; i<points.Count; i++)
            {
                Point p1 = (Point) points[i - 1];
                Point p2 = (Point) points[i];
                if (p1.X >= 0 && p1.X <= Width && p1.Y >= 0 && p1.Y <= Height &&
                    p2.X >= 0 && p2.X <= Width && p2.Y >= 0 && p2.Y <= Height)
                {
                    length += Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }
            }
#if (DEBUG)
            PointCollection polygonPoints = new PointCollection();
            foreach (Point p in points)
                polygonPoints.Add(p);
            Polyline poly = new Polyline();
            poly.Stroke = new SolidColorBrush(Colors.Black);
            poly.StrokeThickness = 3;
            poly.Points = polygonPoints;
            this.Children.Add(poly);
            Console.WriteLine(polygonPoints);
#endif
            return length;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (BackgroundImag == null)
            {
                e.Handled = true;
                MessageBox.Show("Load an image to start measuring");
                base.OnPreviewMouseLeftButtonDown(e);
                return;
            }

            if (CurrDrawMode == DrawMode.FreeScratch)
            {
                base.OnPreviewMouseLeftButtonDown(e);
            }
            else
            {

            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            double total_length = TotalDrawingLength;
            MeasurementDisplay.Text = String.Format("Length = {0:F}", total_length);
        }
    }
}
