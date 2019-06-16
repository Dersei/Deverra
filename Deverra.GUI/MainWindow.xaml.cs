using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Deverra.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void FilteredImage_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            FilteredImage.Width = position.X;
        }

        private Bitmap _originalBitmap;

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _originalBitmap = new Bitmap(openFileDialog.FileName);
                OriginalImage.Source = BitmapToImageSource(_originalBitmap);
                Width = (_originalBitmap.Width / (double)_originalBitmap.Height) * (Height - RunButton.ActualHeight - 50);
            }
        }

        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            FilteredImage.Source = BitmapToImageSource(Main.run(_originalBitmap, (Main.Filter)FilterCombo.SelectedIndex));
        }
    }
}
