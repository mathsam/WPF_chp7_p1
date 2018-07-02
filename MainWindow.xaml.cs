using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DrawAndMeasure
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        [STAThread]
        static public void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        void LoadImageToCanvas(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Types | *.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff;*.gif";
            if ((bool)dlg.ShowDialog())
            {
                canv.SetBackgroundImage(dlg.FileName);
            }
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        void SaveCanvasToFile(object sender, RoutedEventArgs args)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG file (*.png)|*.png|JPEG file (*.jpeg)|*.jpeg|Bitmap file (*.bmp)|*.bmp|TIFF file (*.tiff)|*.tiff";

            if ((bool)dlg.ShowDialog(this))
            {
                try
                {
                    canv.SaveCanvas(dlg.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Save file error {0}", e.Message);
                }
            }
        }

        void DisplayMeasuredLength(object sender, RoutedEventArgs args)
        {
            double total_length = canv.MeasureStrokesLength();
            MessageBox.Show("total length = " + total_length);
        }


        private void ClearCanvDrawing(object sender, RoutedEventArgs e)
        {
            canv.ResetCanvas();
        }

        private void DrawModeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DrawModeSelectionBox.SelectedIndex == -1)
                DrawModeSelectionBox.SelectedIndex = 0;
            if (canv == null) return;
            switch (DrawModeSelectionBox.SelectedIndex)
            {
      
                case 0: canv.CurrDrawMode = BackEnd.DrawMode.FreeScratch;
                    break;
                case 1: canv.CurrDrawMode = BackEnd.DrawMode.Line;
                    break;
                case 2: canv.CurrDrawMode = BackEnd.DrawMode.Arc;
                    break;
                default: throw new NotImplementedException("cannot happen");
            }
        }

        private void StrokeThicknessChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StrokeThicknessChoice.SelectedIndex == -1)
                StrokeThicknessChoice.SelectedIndex = 0;
            string selected = ((Label)StrokeThicknessChoice.SelectedItem).Content.ToString();
            if (canv == null) return;
            canv.StrokeThickness = Double.Parse(selected);
        }

    }
}
