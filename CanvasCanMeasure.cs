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

        //current shape under drawing
        Shape currShape;
        List<Shape> shapesOnCanvas = new List<Shape>();


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
            MeasurementDisplay.FontSize = 16;
            Children.Add(MeasurementDisplay);

            AllowDrop = true;
            Drop += CanvasCanMeasure_Drop;
        }

        public void ResetCanvas()
        {
            Strokes.Clear();
            Children.Clear();
            shapesOnCanvas.Clear();
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

            MeasurementDisplay.FontSize = imgBrush.ImageSource.Height / 15;
        }

        private void CanvasCanMeasure_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    SetBackgroundImage(files[0]);
                }
                catch (System.NotSupportedException)
                {
                    MessageBox.Show("File format not supported: " + files[0]);
                }
            }
        }

        public void SaveCanvas(string FilePath)
        {
            string ext = System.IO.Path.GetExtension(FilePath).ToLower();
            BitmapEncoder encoder;
            switch (ext)
            {
                case ".jpg":
                    {
                        encoder = new JpegBitmapEncoder();
                        break;
                    }
                case ".jpeg":
                    {
                        encoder = new JpegBitmapEncoder();
                        break;
                    }
                case ".bmp":
                    {
                        encoder = new BmpBitmapEncoder();
                        break;
                    }
                case ".tif":
                    {
                        encoder = new TiffBitmapEncoder();
                        break;
                    }
                case ".tiff":
                    {
                        encoder = new TiffBitmapEncoder();
                        break;
                    }
                default:
                    {
                        encoder = new PngBitmapEncoder();
                        if (ext != ".png") FilePath += ".png";
                        break;
                    }
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.Width, (int)this.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(this);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = System.IO.File.Create(FilePath))
            {
                encoder.Save(stream);
            }
        }

        public double MeasureStrokesLength()
        {
            double total_length = 0;
            foreach(Stroke stroke in Strokes)
            {
                total_length += MeasureOneStrokeLength(stroke.GetBezierStylusPoints());
            }
            foreach(Shape curve in shapesOnCanvas)
            {
                if (curve.GetType() == typeof(Line))
                {
                    Line currLine = curve as Line;
                    total_length += Math.Sqrt(Math.Pow(currLine.X1 - currLine.X2, 2) + 
                                              Math.Pow(currLine.Y1 - currLine.Y2, 2));

                    PointCollection polygonPoints = new PointCollection();
                    polygonPoints.Add(new Point(currLine.X1, currLine.Y1));
                    polygonPoints.Add(new Point(currLine.X2, currLine.Y2));
                    Polyline poly = new Polyline();
                    poly.Stroke = new SolidColorBrush(Colors.LightGray);
                    poly.StrokeThickness = 1;
                    poly.Points = polygonPoints;
                    this.Children.Add(poly);
                }
            }
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
            PointCollection polygonPoints = new PointCollection();
            foreach (Point p in points)
                polygonPoints.Add(p);
            Polyline poly = new Polyline();
            poly.Stroke = new SolidColorBrush(Colors.LightGray);
            poly.StrokeThickness = 1;
            poly.Points = polygonPoints;
            this.Children.Add(poly);
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
                Point currPoint = e.GetPosition(this);
                if (CurrDrawMode == DrawMode.Line)
                {
                    if (currShape == null)
                    {
                        Line currLine = new Line();
                        currLine.StrokeThickness = this.StrokeThickness;
                        currLine.Stroke = new SolidColorBrush(Colors.SpringGreen);
                        currLine.Stroke.Opacity = 0.5;
                        currLine.X1 = currPoint.X;
                        currLine.X2 = currPoint.X;
                        currLine.Y1 = currPoint.Y;
                        currLine.Y2 = currPoint.Y;
                        currShape = currLine;
                        Children.Add(currLine);
                        shapesOnCanvas.Add(currShape);
                    }
                    else
                    {
                        Line currLine = currShape as Line;
                        currLine.X2 = currPoint.X;
                        currLine.Y2 = currPoint.Y;
                    }
                }
                e.Handled = true;
                base.OnPreviewMouseLeftButtonDown(e);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (CurrDrawMode == DrawMode.Line && currShape != null)
            {
                Line currLine = currShape as Line;
                Point currPoint = e.GetPosition(this);
                currLine.X2 = currPoint.X;
                currLine.Y2 = currPoint.Y;
                e.Handled = true;
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (CurrDrawMode == DrawMode.FreeScratch)
            {
                base.OnMouseLeftButtonUp(e);
            }
            else if (CurrDrawMode == DrawMode.Line && currShape != null)
            {
                Line currLine = currShape as Line;
                Point currPoint = e.GetPosition(this);
                currLine.X2 = currPoint.X;
                currLine.Y2 = currPoint.Y;
                currShape = null;
            }
            
            double total_length = TotalDrawingLength;
            MeasurementDisplay.Text = String.Format("Length = {0:F}", total_length);
        }
    }
}
