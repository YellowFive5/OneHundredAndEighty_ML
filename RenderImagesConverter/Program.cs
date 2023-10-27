#region Usings

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            // var sourceFolder = @$"Z:\SyntheticData\Renders\Bull\";
            // var sourceFolder = $@"Z:\SyntheticData\Renders\25\";
            var sourceFolder = @"D:\Dataset\TestSet\Cam";

            // var destFolder = $@"D:\\Dataset\\Projection\Bull\";
            // var destFolder = $"D:\\Dataset\\TestSet\\Cam\\Projection";
            // var destFolder = @"D:\25\";
            var destFolder = @"D:\Dataset\TestSet\Cam\Projection";

            var renderClearBackground = new Image<Bgr, byte>(@"D:\Dataset\TestSet\Cam\Projection\CamClearBackground.jpg");

            var dir = new DirectoryInfo(sourceFolder);
            var readyFiles = new DirectoryInfo(destFolder).GetFiles("*.jpeg");

            // var files = dir.GetFiles("*.png");
            var files = dir.GetFiles("*.jpg");
            var filesCount = files.Length;

            var processedCounter = 0;
            Parallel.ForEach(files,
                             new ParallelOptions
                             {
                                 MaxDegreeOfParallelism = 24
                             },
                             f =>
                             {
                                 if (readyFiles.Any(rf => rf.Name.Replace(".jpeg", "") == f.Name.Replace(".png", "")))
                                 {
                                     Console.WriteLine($"{processedCounter++}/{filesCount}");
                                     return;
                                 }

                                 var nextThrowImage = new Image<Bgr, byte>(f.FullName);
                                 var warpedDiff = ip.ConvertImage(renderClearBackground, nextThrowImage);
                                 var warpOnProjection = Drawer.DrawProjection(warpedDiff[2].Convert<Bgr, byte>());
                                 // ImageSaver.Save(warpOnProjection, destFolder, $"{f.Name.Replace(".png", "")}");
                                 ImageSaver.Save(warpOnProjection, destFolder, $"{f.Name.Replace(".jpg", "")}");
                                 Console.WriteLine($"{processedCounter++}/{filesCount}");
                             });
        }
    }
}