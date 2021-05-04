#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;

#endregion

namespace BinaryLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            var dataSetDir = Path.GetFullPath(Path.Combine(projectDir, "../../DataSet/Сlear_bull_25_x4"));
            var workspace = Path.Combine(projectDir, "workspace");
            var assets = Path.Combine(projectDir, "assets");

            MLContext mlContext = new MLContext();

            var imagesData = LoadImagesFromDirectory(dataSetDir);

            IDataView imgData = mlContext.Data.LoadFromEnumerable(imagesData);

            Console.WriteLine("Hello ML!");
            Console.ReadKey();
        }

        private static IEnumerable<ImageData> LoadImagesFromDirectory(string folder)
        {
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                                          .Where(f => Path.GetExtension(f) == ".jpeg"))
            {
                var splitted = Path.GetFileName(file)
                                   .Split(new[] {'_'})
                                   .Where(w => w != string.Empty)
                                   .ToArray();

                yield return new ImageData
                             {
                                 ImagePath = file,
                                 Label = splitted[1]
                             };
            }
        }
    }
}