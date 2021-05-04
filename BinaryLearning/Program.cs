#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Vision;

#endregion

namespace BinaryLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            // var arch = ImageClassificationTrainer.Architecture.ResnetV2101;
            var epochs = 1000;
            // var testFraction = 0.3d;

            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            var dataSetDir = Path.GetFullPath(Path.Combine(projectDir, "../../DataSet/Сlear_bull_25_x4/TrainingImages"));
            var imagesData = LoadImagesFromDirectory(dataSetDir);
            var modelsFolder = Path.Combine(projectDir, "../../DataSet/Сlear_bull_25_x4/TrainedModels");
            var testImagesFolder = Path.Combine(dataSetDir, "../TestImages");

            // train and save all models
            // List<ImageClassificationTrainer.Architecture> architectures = new List<ImageClassificationTrainer.Architecture>
            //                                                               {
            //                                                                   ImageClassificationTrainer.Architecture.ResnetV2101,
            //                                                                   ImageClassificationTrainer.Architecture.ResnetV250,
            //                                                                   ImageClassificationTrainer.Architecture.MobilenetV2,
            //                                                                   ImageClassificationTrainer.Architecture.InceptionV3
            //                                                               };
            // List<double> testFractions = new List<double>
            //                              {
            //                                  0.01,
            //                                  0.05,
            //                                  0.1,
            //                                  0.15,
            //                                  0.2,
            //                                  0.25,
            //                                  0.3,
            //                                  0.35
            //                              };
            //
            // foreach (var architecture in architectures)
            // {
            //     foreach (var fraction in testFractions)
            //     {
            //         MLContext mlContext = new MLContext();
            //         IDataView imgData = mlContext.Data.LoadFromEnumerable(imagesData);
            //         var modelName = $"{architecture}_e_{epochs}_{fraction}.zip";
            //         var model = TrainModel(mlContext, imgData, dataSetDir, architecture, epochs, fraction);
            //         mlContext.Model.Save(model, imgData.Schema, Path.Combine(modelsFolder, modelName));
            //     }
            // }
            
            
            // load and test all models
            var results = new ConcurrentBag<(string, int)>();
            var tasks = new List<Task>();
            foreach (var modelPath in Directory.GetFiles(modelsFolder, "*", SearchOption.AllDirectories)
                                               .ToArray())
            {
                tasks.Add(Task.Run(() =>
                                   {
                                       MLContext mlContext = new MLContext();
                                       var model = mlContext.Model.Load(modelPath, out var modelSchema);
                                       ClassifyImagesFromFolder(mlContext, model, Path.Combine(projectDir, testImagesFolder), modelPath, results);
                                   })
                         );
            }

            Task.WaitAll(tasks.ToArray());
            foreach (var result in results.OrderByDescending(i => i.Item2))
            {
                Console.WriteLine($"{result.Item1} - {result.Item2}/26");
            }

            //save one
            // var modelName = $"{arch}_e_{epochs}_{testFraction}.zip";
            // var model = TrainModel(mlContext, imgData, dataSetDir, arch, epochs, testFraction);
            // mlContext.Model.Save(model, imgData.Schema, Path.Combine(modelsFolder, modelName));

            //load and test one
            //var modelName = $"{arch}_e_{epochs}_{testFraction}.zip";
            // var model = mlContext.Model.Load(Path.Combine(modelsFolder, modelName), out var modelSchema);
            // ClassifyImagesFromFolder(mlContext, model, Path.Combine(projectDir, testImagesFolder));

            Console.WriteLine("Hello ML!");
            Console.ReadKey();
        }

        private static ITransformer TrainModel(MLContext mlContext, IDataView dataView, string dataSetDir, ImageClassificationTrainer.Architecture architecture, int epochs, double testFraction)
        {
            IDataView shuffledData = mlContext.Data.ShuffleRows(dataView);
            var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Label",
                                                                                      outputColumnName: "LabelKey")
                                                 .Append(mlContext.Transforms.LoadRawImageBytes(outputColumnName: "Img",
                                                                                                imageFolder: dataSetDir,
                                                                                                inputColumnName: "ImgPath")); // > InputData.cs
            IDataView preProcData = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
            DataOperationsCatalog.TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcData, testFraction: testFraction);
            DataOperationsCatalog.TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

            IDataView trainSet = trainSplit.TrainSet;
            IDataView validationSet = validationTestSplit.TrainSet;
            IDataView testSet = validationTestSplit.TestSet;

            var classifierOptions = new ImageClassificationTrainer.Options
                                    {
                                        FeatureColumnName = "Img",
                                        LabelColumnName = "LabelKey",
                                        ValidationSet = validationSet,
                                        Arch = architecture,
                                        MetricsCallback = metrics => Console.WriteLine(metrics),
                                        TestOnTrainSet = false,
                                        ReuseTrainSetBottleneckCachedValues = true,
                                        ReuseValidationSetBottleneckCachedValues = true,
                                        Epoch = epochs,
                                    };

            var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
                                            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            return trainingPipeline.Fit(trainSet);
        }

        private static IEnumerable<ImgData> LoadImagesFromDirectory(string folder)
        {
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                                          .Where(f => Path.GetExtension(f) == ".jpeg"))
            {
                var splitted = Path.GetFileName(file)
                                   .Split(new[] {'_'})
                                   .Where(w => w != string.Empty)
                                   .ToArray();

                yield return new ImgData
                             {
                                 ImgPath = file,
                                 Label = splitted[1]
                             };
            }
        }

        private static void ClassifyImagesFromFolder(MLContext myContext, ITransformer trainedModel, string imagesFolderPath, string modelPath, ConcurrentBag<(string, int)> mainResults)
        {
            var imageFiles = Directory.GetFiles(imagesFolderPath, "*", SearchOption.AllDirectories)
                                      .Where(f => Path.GetExtension(f) == ".jpeg")
                                      .ToArray();

            var modelName = Path.GetFileName(modelPath);
            var results = new ConcurrentBag<(string, string)>();

            foreach (var imageFile in imageFiles)
            {
                var splitted = Path.GetFileName(imageFile)
                                   .Split(new[] {'_'})
                                   .Where(w => w != string.Empty)
                                   .ToArray();

                InputData image = new InputData
                                  {
                                      ImgPath = imageFile,
                                      Img = File.ReadAllBytes(imageFile),
                                      Label = splitted[0]
                                  };


                PredictionEngine<InputData, Output> predEngine = myContext.Model.CreatePredictionEngine<InputData, Output>(trainedModel);
                Output prediction = predEngine.Predict(image);

                results.Add((image.Label, prediction.PredictedLabel));

                // OutputPred(prediction);
            }

            mainResults.Add((modelName, results.Count(r => r.Item1 == r.Item2)));
            // Console.WriteLine($"{modelName} complete");
        }

        private static void OutputPred(Output pred)
        {
            string imgName = Path.GetFileName(pred.ImgPath);
            Console.WriteLine($"Image: {imgName} | Actual Label: {pred.Label} | Predicted Label: {pred.PredictedLabel}");
        }
    }
}