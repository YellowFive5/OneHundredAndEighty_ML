#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace DiffImagesCollector
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Bitmap

        public BitmapImage ProjectionBitmap
        {
            get => projectionBitmap;
            set
            {
                projectionBitmap = value;
                OnPropertyChanged(nameof(ProjectionBitmap));
            }
        }

        private BitmapImage projectionBitmap;

        public BitmapImage ThrowProcessedBitmap
        {
            get => throwProcessedBitmap;
            set
            {
                throwProcessedBitmap = value;
                OnPropertyChanged(nameof(ThrowProcessedBitmap));
            }
        }

        private BitmapImage throwProcessedBitmap;

        public BitmapImage DiffBitmap
        {
            get => diffBitmap;
            set
            {
                diffBitmap = value;
                OnPropertyChanged(nameof(DiffBitmap));
            }
        }

        private BitmapImage diffBitmap;

        #endregion

        #region Images

        private Image<Bgr, byte> projectionBackgroundImage;
        private Image<Bgr, byte> backgroundRawImage;
        private Image<Gray, byte> backgroundProcessedImage;
        private Image<Bgr, byte> throwRawImage;
        private Image<Gray, byte> throwProcessedImage;
        private Image<Gray, byte> diffImage;

        #endregion

        private const int ProjectionFrameSide = 1300;
        private const int ProjectionCoefficient = 3;
        private readonly MCvScalar projectionGridColor = new Bgr(Color.DarkGray).MCvScalar;
        private readonly Bgr projectionDigitsColor = new(Color.White);
        private readonly MCvScalar poiDotColor = new Bgr(Color.Red).MCvScalar;
        private const int ProjectionGridThickness = 2;
        private const double SectorStepRad = 0.314159;
        private const double SemiSectorStepRad = SectorStepRad / 2;
        private const double StartRadSector_11 = -3.14159;
        private const double ProjectionDigitsScale = 2;
        private const int ProjectionDigitsThickness = 2;

        private static readonly PointF projectionCenterPoint = new((float) ProjectionFrameSide / 2,
                                                                   (float) ProjectionFrameSide / 2);

        public void DrawProjection()
        {
            projectionBackgroundImage = new Image<Bgr, byte>(ProjectionFrameSide, ProjectionFrameSide);

            // Draw dartboard projection
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 7, projectionGridColor, ProjectionGridThickness);
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 17, projectionGridColor, ProjectionGridThickness);
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 95, projectionGridColor, ProjectionGridThickness);
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 105, projectionGridColor, ProjectionGridThickness);
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 160, projectionGridColor, ProjectionGridThickness);
            DrawCircle(projectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 170, projectionGridColor, ProjectionGridThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new PointF((float) (projectionCenterPoint.X + Math.Cos(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 170),
                                               (float) (projectionCenterPoint.Y + Math.Sin(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 170));
                var segmentPoint2 = new PointF((float) (projectionCenterPoint.X + Math.Cos(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 17),
                                               (float) (projectionCenterPoint.Y + Math.Sin(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 17));
                DrawLine(projectionBackgroundImage, segmentPoint1, segmentPoint2, projectionGridColor, ProjectionGridThickness);
            }

            // Draw digits
            var sectors = new List<int>
                          {
                              11, 14, 9, 12, 5,
                              20, 1, 18, 4, 13,
                              6, 10, 15, 2, 17,
                              3, 19, 7, 16, 8
                          };
            var startRadSector = StartRadSector_11;
            var radSector = startRadSector;
            foreach (var sector in sectors)
            {
                DrawString(projectionBackgroundImage,
                           sector.ToString(),
                           (int) (projectionCenterPoint.X - 40 + Math.Cos(radSector) * ProjectionCoefficient * 190),
                           (int) (projectionCenterPoint.Y + 20 + Math.Sin(radSector) * ProjectionCoefficient * 190),
                           ProjectionDigitsScale,
                           projectionDigitsColor,
                           ProjectionDigitsThickness);
                radSector += SectorStepRad;
            }

            ProjectionBitmap = ImageToBitmapImage(projectionBackgroundImage);
        }

        public void TakeBackgroundCapture()
        {
            var videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1280);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 720);
            backgroundRawImage = videoCapture.QueryFrame()
                                             .ToImage<Bgr, byte>();

            backgroundProcessedImage = backgroundRawImage.Clone().Convert<Gray, byte>();

            ThrowProcessedBitmap = ImageToBitmapImage(backgroundProcessedImage);

            var blackBlank = backgroundProcessedImage.Clone();
            blackBlank.SetValue(new Bgr(Color.Black).MCvScalar);
            DiffBitmap = ImageToBitmapImage(blackBlank);

            videoCapture.Dispose();
        }

        public void TakeCapture()
        {
            var videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1280);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 720);
            throwRawImage = videoCapture.QueryFrame()
                                        .ToImage<Bgr, byte>();

            backgroundRawImage = throwRawImage ?? backgroundRawImage;

            throwProcessedImage = throwRawImage.Clone().Convert<Gray, byte>();
            ThrowProcessedBitmap = ImageToBitmapImage(throwProcessedImage);

            diffImage = throwProcessedImage.AbsDiff(backgroundProcessedImage);
            diffImage._ThresholdBinary(new Gray(80), new Gray(255));

            DiffBitmap = ImageToBitmapImage(diffImage);

            backgroundProcessedImage = throwProcessedImage.Clone();

            videoCapture.Dispose();

            CollectImage();
            ShowNextPointToStick();
        }

        private Tuple<long, PointF, string> processingPoint;

        public void ShowNextPointToStick()
        {
            var connection = new SQLiteConnection("Data Source=Dots.db; Pooling=true;");

            var query = "SELECT [Dots].Id, X, Y, Result FROM [Dots] " +
                        "LEFT JOIN [Images] " +
                        "ON [Images].[DotId] = [Dots].[Id] " +
                        "GROUP BY [Images].[DotId] " +
                        "HAVING COUNT([Images].[DotId]) <= 3 " +
                        "LIMIT 1";

            var cmd = new SQLiteCommand(query) {Connection = connection};

            var table = new DataTable();
            connection.Open();
            using (var dataReader = cmd.ExecuteReader())
            {
                using (var dataSet = new DataSet())
                {
                    dataSet.Tables.Add(table);
                    dataSet.EnforceConstraints = false;
                    table.Load(dataReader);
                    dataReader.Close();
                }
            }

            connection.Close();

            if (table.Rows.Count == 0)
            {
                return;
            }

            processingPoint = new Tuple<long, PointF, string>((long) table.Rows[0][0],
                                                              new PointF((long) table.Rows[0][1],
                                                                         (long) table.Rows[0][2]),
                                                              table.Rows[0][3].ToString());

            var projectionImageWithDot = projectionBackgroundImage.Clone();
            DrawCircle(projectionImageWithDot, processingPoint.Item2, 2, poiDotColor, 4);

            ProjectionBitmap = ImageToBitmapImage(projectionImageWithDot);
        }

        private void CollectImage()
        {
            var bitmap = new Bitmap(DiffBitmap.StreamSource);
            Directory.CreateDirectory("Images");
            bitmap.Save("./Images/" +
                        $"{processingPoint.Item3}_" +
                        $"{processingPoint.Item2.X}_" +
                        $"{processingPoint.Item2.Y}.jpeg",
                        ImageFormat.Jpeg);

            string base64String;
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                var imageBytes = ms.ToArray();
                base64String = Convert.ToBase64String(imageBytes);
            }

            var connection = new SQLiteConnection("Data Source=Dots.db; Pooling=true;");

            var query = "INSERT INTO [Images] (DotId, Image) " +
                        $"VALUES ({processingPoint.Item1}, '{base64String}')";

            var cmd = new SQLiteCommand(query) {Connection = connection};
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private BitmapImage ImageToBitmapImage(Image<Bgr, byte> image)
        {
            var imageToSave = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.ToBitmap().Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
            }

            return imageToSave;
        }

        private BitmapImage ImageToBitmapImage(Image<Gray, byte> image)
        {
            var imageToSave = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.ToBitmap().Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
            }

            return imageToSave;
        }

        private void DrawCircle(Image<Bgr, byte> image,
                                PointF centerpoint,
                                int radius,
                                MCvScalar color,
                                int thickness)
        {
            CvInvoke.Circle(image,
                            new Point((int) centerpoint.X, (int) centerpoint.Y),
                            radius,
                            color,
                            thickness);
        }

        private void DrawLine(Image<Bgr, byte> image,
                              PointF point1,
                              PointF point2,
                              MCvScalar color,
                              int thickness)
        {
            CvInvoke.Line(image,
                          new Point((int) point1.X, (int) point1.Y),
                          new Point((int) point2.X, (int) point2.Y),
                          color,
                          thickness);
        }

        private void DrawString(Image<Bgr, byte> image,
                                string text,
                                int pointX,
                                int pointY,
                                double scale,
                                Bgr color,
                                int thickness)
        {
            image.Draw(text,
                       new Point(pointX, pointY),
                       FontFace.HersheySimplex,
                       scale,
                       color,
                       thickness);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}