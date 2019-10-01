using System.IO;
using System.Windows.Media.Imaging;

namespace Deverra.GUI
{
    public static class BitmapHelper
    {
        public static void Save(this BitmapSource @this, string filename)
        {
            if (string.IsNullOrEmpty(filename)) return;
            using var fileStream = new FileStream(filename, FileMode.Create);
            var pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(@this));
            pngBitmapEncoder.Save(fileStream);
        }

    }
}
