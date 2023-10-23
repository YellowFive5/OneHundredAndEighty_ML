#region Usings

using Emgu.CV;
using Emgu.CV.Structure;

#endregion

namespace RenderImagesConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var ip = new ImageProcessor();
            var folder = @"Z:\SyntheticRenders\";

            var renderClearBackground = new Image<Bgr, byte>($"{folder}RenderClearBackground.png");
            var nextThrowImage = new Image<Bgr, byte>($"{folder}testThrow.png");

            var warpedDiff = ip.ConvertImage(renderClearBackground, nextThrowImage);

            ImageSaver.Save(warpedDiff[0], folder, "diff1");
            ImageSaver.Save(warpedDiff[1], folder, "diff2");
            ImageSaver.Save(warpedDiff[2], folder, "diff3");
        }
    }
}