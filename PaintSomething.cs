using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawAndMeasure
{
    class PaintSomething : Window
    {
        enum MouseDownAction
        {
            DrawNewLine,
            MoveExistingLine,
            Nothing,
        }

        Point startPoint;
        Canvas canv;
        Line curr_line;
        Line line_selected;
        Point line_selected_startPoint;
        MouseDownAction pending_action = MouseDownAction.Nothing;

        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new PaintSomething());
        }

        public PaintSomething()
        {
            Title = "Paint";
            Height = 500;
            Width = 500;

            canv = new Canvas();
            canv.MouseLeftButtonDown += Canv_MouseDown;
            canv.MouseLeftButtonUp += Canv_MouseUp;
            canv.MouseMove += Canv_MouseMove;
            canv.Background = new SolidColorBrush(Colors.GreenYellow);
            Content = canv;

            canv.SetBinding(Window.HeightProperty, "Height");
            canv.SetBinding(Window.WidthProperty, "Width");
            canv.DataContext = this;
            //this.SizeToContent = SizeToContent.WidthAndHeight;
            canv.ClipToBounds = true;

            this.KeyDown += PaintSomething_KeyDown;
        }

        private void PaintSomething_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.Key);

            if (this.line_selected != null)
            {
                if (e.Key == Key.Delete || e.Key == Key.Back)
                {
                    this.canv.Children.Remove(this.line_selected);
                    this.line_selected = null;
                }
            }
            e.Handled = true;
            base.OnKeyDown(e);
        }

        private void Canv_MouseMove(object sender, MouseEventArgs e)
        {
            Point currPoint = e.GetPosition(null);
            if (this.pending_action == MouseDownAction.DrawNewLine)
            {
                curr_line.X2 = currPoint.X;
                curr_line.Y2 = currPoint.Y;
            }
            else if (this.pending_action == MouseDownAction.MoveExistingLine)
            {
                Vector move = currPoint - this.startPoint;
                Vector line_vec = new Point(this.line_selected.X2, this.line_selected.Y2) -
                    new Point(this.line_selected.X1, this.line_selected.Y1);
                this.line_selected.X1 = this.line_selected_startPoint.X + move.X;
                this.line_selected.Y1 = this.line_selected_startPoint.Y + move.Y;
                this.line_selected.X2 = this.line_selected.X1 + line_vec.X;
                this.line_selected.Y2 = this.line_selected.Y1 + line_vec.Y;
            }
            else if (this.pending_action == MouseDownAction.Nothing)
            {
                if (this.line_selected != null)
                {
                    if (this.line_selected.IsMouseOver)
                    {
                        return;
                    }
                    else
                    {
                        this.line_selected.Stroke = new SolidColorBrush(Colors.Blue);
                        this.line_selected = null;
                        this.canv.Focus();
                    }

                }

                foreach (Line l in this.canv.Children)
                {
                    if (l.IsMouseOver)
                    {
                        l.Stroke = new SolidColorBrush(Colors.Red);
                        this.line_selected = l;
                        this.line_selected_startPoint.X = l.X1;
                        this.line_selected_startPoint.Y = l.Y1;
                        this.line_selected.Focus();
                        break;
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Cannot happen");
            }
        }

        private void Canv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point endPoint = e.GetPosition(null);
            if (this.pending_action == MouseDownAction.DrawNewLine)
            {
                this.curr_line.X2 = endPoint.X;
                this.curr_line.Y2 = endPoint.Y;
                this.curr_line.Cursor = Cursors.Hand;
                this.curr_line = null;
            }
            else if (this.pending_action == MouseDownAction.MoveExistingLine)
            {
                this.line_selected.Stroke = new SolidColorBrush(Colors.Blue);
                this.line_selected = null;
            }

            this.pending_action = MouseDownAction.Nothing;
        }

        private void Canv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
            if (this.line_selected == null)
            {
                this.pending_action = MouseDownAction.DrawNewLine;
                this.curr_line = new Line();
                this.curr_line.X1 = this.startPoint.X;
                this.curr_line.Y1 = this.startPoint.Y;
                this.curr_line.X2 = this.startPoint.X;
                this.curr_line.Y2 = this.startPoint.Y;
                this.curr_line.StrokeThickness = 5;
                this.curr_line.Stroke = new SolidColorBrush(Colors.Blue);
                this.canv.Children.Add(this.curr_line);
            }
            else
            {
                this.pending_action = MouseDownAction.MoveExistingLine;
            }
        }
    }
}
