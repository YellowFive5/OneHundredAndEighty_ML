#region Usings

using System;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

#endregion

namespace RenderImagesConverter
{
    public static class ImageSaver
    {
        public static void Save(Image<Gray, byte> image, string folder = @"C:\Users\YellowFive\Desktop", string fileName = null)
        {
            image.ToBitmap().Save($"{folder}/{fileName ?? $"{Guid.NewGuid():N}"}.jpeg", ImageFormat.Jpeg);
        }

        public static void Save(Image<Bgr, byte> image, string folder = @"C:\Users\YellowFive\Desktop", string fileName = null)
        {
            image.ToBitmap().Save($"{folder}/{fileName ?? $"{Guid.NewGuid():N}"}.jpeg", ImageFormat.Jpeg);
        }
    }
}