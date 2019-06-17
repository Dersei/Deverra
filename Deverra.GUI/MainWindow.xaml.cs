using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using VM;

namespace Deverra.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FilteredImage_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            FilteredImage.Width = position.X;
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bitmap = ((ViewModel)DataContext).OriginalImage = new Bitmap(openFileDialog.FileName);
                Width = (bitmap.Width / (double)bitmap.Height) * (Height - RunButton.ActualHeight - 50);
            }
        }
    }
}
