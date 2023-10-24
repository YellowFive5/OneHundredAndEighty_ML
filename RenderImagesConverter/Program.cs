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
            var sourceFolder = @"Z:\SyntheticData\";
            var sourceFolderBull = @$"{sourceFolder}Renders\Bull\";
            var sourceFolder25 = $@"{sourceFolder}Renders\25\";
            var destFolderBull = $@"{sourceFolder}Dataset\Projection\Bull\";
            var destFolder25 = $@"{sourceFolder}Dataset\Projection\25\";
            // var destFolder25 = @"D:\25\";

            var renderClearBackground = new Image<Bgr, byte>($"{sourceFolder}RenderClearBackground.png");

            var dir1 = new DirectoryInfo(sourceFolderBull);
            var dir2 = new DirectoryInfo(sourceFolder25);
            var dirOut = new DirectoryInfo(destFolder25);
            var readyFiles = dirOut.GetFiles("*.jpeg");

            var files = dir2.GetFiles("*.png");
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
                                 ImageSaver.Save(warpOnProjection, destFolder25, $"{f.Name.Replace(".png", "")}");
                                 Console.WriteLine($"{processedCounter++}/{filesCount}");
                             });
        }
    }
}