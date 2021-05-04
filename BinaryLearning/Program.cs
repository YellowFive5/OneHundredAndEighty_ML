#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Vision;

#endregion

namespace BinaryLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
            var dataSetDir = Path.GetFullPath(Path.Combine(projectDir, "../../DataSet/Сlear_bull_25_x4/Images"));

            var imagesData = LoadImagesFromDirectory(dataSetDir);

            MLContext mlContext = new MLContext();
            IDataView imgData = mlContext.Data.LoadFromEnumerable(imagesData);

            // var model = TrainModel(mlContext, imgData, dataSetDir);
            // mlContext.Model.Save(model, imgData.Schema, "binaryModel.zip");

            DataViewSchema modelSchema;
            var model = mlContext.Model.Load(Path.Combine(projectDir, "../../DataSet/Сlear_bull_25_x4/binaryModel.zip"), out modelSchema);

            // ClassifyImg(mlContext, imgData, model);

            Console.WriteLine("Hello ML!");
            Console.ReadKey();
        }

        private static ITransformer TrainModel(MLContext mlContext, IDataView dataView, string dataSetDir)
        {
            IDataView shuffledData = mlContext.Data.ShuffleRows(dataView);
            var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Label",
                                                                                      outputColumnName: "LabelKey")
                                                 .Append(mlContext.Transforms.LoadRawImageBytes(outputColumnName: "Img",
                                                                                                imageFolder: dataSetDir,
                                                                                                inputColumnName: "ImgPath")); // > InputData.cs
            IDataView preProcData = preprocessingPipeline.Fit(shuffledData).Transform(shuffledData);
            DataOperationsCatalog.TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcData, testFraction: 0.25);
            DataOperationsCatalog.TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

            IDataView trainSet = trainSplit.TrainSet;
            IDataView validationSet = validationTestSplit.TrainSet;
            IDataView testSet = validationTestSplit.TestSet;

            var classifierOptions = new ImageClassificationTrainer.Options
                                    {
                                        FeatureColumnName = "Img",
                                        LabelColumnName = "LabelKey",
                                        ValidationSet = validationSet,
                                        Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
                                        MetricsCallback = metrics => Console.WriteLine(metrics),
                                        TestOnTrainSet = false,
                                        ReuseTrainSetBottleneckCachedValues = true,
                                        ReuseValidationSetBottleneckCachedValues = true
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

        private static void ClassifyImg(MLContext myContext, IDataView data, ITransformer trainedModel)
        {
            PredictionEngine<InputData, Output> predEngine = myContext.Model.CreatePredictionEngine<InputData, Output>(trainedModel);
            InputData image = myContext.Data.CreateEnumerable<InputData>(data, reuseRowObject: true).ElementAt(new Random().Next(1, 515));
            Output prediction = predEngine.Predict(image);
            Console.WriteLine("Prediction for single image");
            OutputPred(prediction);
        }

        private static void OutputPred(Output pred)
        {
            string imgName = Path.GetFileName(pred.ImgPath);
            Console.WriteLine($"Image: {imgName} | Actual Label: {pred.Label} | Predicted Label: {pred.PredictedLabel}");
        }
    }
}