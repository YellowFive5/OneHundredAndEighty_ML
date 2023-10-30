#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace RenderImagesConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var ip = new ImageProcessor();

            var sourceFolder = @$"E:\SyntheticData\Renders\Bull\";
            // var sourceFolder = $@"E:\SyntheticData\Renders\25\";
            // var sourceFolder = @"D:\Dataset\TestSet\Cam";

            var destFolder = $@"D:\Dataset\LearnSet\224Clear\Bull\";
            // var destFolder = $"D:\Dataset\LearnSet\224Clear\25\";
            // var destFolder = @"D:\25\";
            // var destFolder = @"D:\Dataset\TestSet\Cam\Projection";

            var renderClearBackground = new Image<Bgr, byte>(@"D:\Dataset\TestSet\RenderClearBackground.png");

            var dir = new DirectoryInfo(sourceFolder);
            var readyFiles = new DirectoryInfo(destFolder).GetFiles("*.jpeg");

            var files = dir.GetFiles("*.png");
            // var files = dir.GetFiles("*.jpg");
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
                                 // var warpOnProjection = Drawer.DrawProjection(warpedDiff[2].Convert<Bgr, byte>());
                                 
                                 var warpMat = CvInvoke.GetPerspectiveTransform(new List<PointF>
                                                                                {
                                                                                    new(0, 0),
                                                                                    new(0, 1300),
                                                                                    new(1300, 1300),
                                                                                    new(1300, 0),
                                                                                }.ToArray(),
                                                                                new List<PointF> // live cam
                                                                                {
                                                                                    new(0, 0),
                                                                                    new(0, 224),
                                                                                    new(224, 224),
                                                                                    new(224, 0),
                                                                                }.ToArray());
                                 var tinnyImage = new Image<Gray, byte>(224, 224);
                                 CvInvoke.WarpPerspective(warpedDiff[2], tinnyImage, warpMat, tinnyImage.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(0));
                                 
                                 ImageSaver.Save(tinnyImage, destFolder, $"{f.Name.Replace(".png", "")}");
                                 // ImageSaver.Save(warpOnProjection, destFolder, $"{f.Name.Replace(".jpg", "")}");
                                 Console.WriteLine($"{processedCounter++}/{filesCount}");
                             });
        }
    }
}