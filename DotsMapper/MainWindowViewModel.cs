#region Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace DotsMapper
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        public BitmapImage DartboardImage
        {
            get => dartboardImage;
            set
            {
                dartboardImage = value;
                OnPropertyChanged(nameof(DartboardImage));
            }
        }

        private BitmapImage dartboardImage = new();
        private Image<Bgr, byte> ProjectionBackgroundImage { get; set; }
        private const int ProjectionFrameSide = 1300;
        private const int ProjectionCoefficient = 3;
        private readonly MCvScalar projectionGridColor = new Bgr(Color.DarkGray).MCvScalar;
        private readonly Bgr projectionDigitsColor = new(Color.White);
        private readonly MCvScalar poiDotColor = new Bgr(Color.BlueViolet).MCvScalar;
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
            ProjectionBackgroundImage = new Image<Bgr, byte>(ProjectionFrameSide, ProjectionFrameSide);

            // Draw dartboard projection
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 7, projectionGridColor, ProjectionGridThickness);
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 17, projectionGridColor, ProjectionGridThickness);
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 95, projectionGridColor, ProjectionGridThickness);
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 105, projectionGridColor, ProjectionGridThickness);
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 160, projectionGridColor, ProjectionGridThickness);
            DrawCircle(ProjectionBackgroundImage, projectionCenterPoint, ProjectionCoefficient * 170, projectionGridColor, ProjectionGridThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new PointF((float) (projectionCenterPoint.X + Math.Cos(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 170),
                                               (float) (projectionCenterPoint.Y + Math.Sin(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 170));
                var segmentPoint2 = new PointF((float) (projectionCenterPoint.X + Math.Cos(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 17),
                                               (float) (projectionCenterPoint.Y + Math.Sin(SectorStepRad * i - SemiSectorStepRad) * ProjectionCoefficient * 17));
                DrawLine(ProjectionBackgroundImage, segmentPoint1, segmentPoint2, projectionGridColor, ProjectionGridThickness);
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
                DrawString(ProjectionBackgroundImage,
                           sector.ToString(),
                           (int) (projectionCenterPoint.X - 40 + Math.Cos(radSector) * ProjectionCoefficient * 190),
                           (int) (projectionCenterPoint.Y + 20 + Math.Sin(radSector) * ProjectionCoefficient * 190),
                           ProjectionDigitsScale,
                           projectionDigitsColor,
                           ProjectionDigitsThickness);
                radSector += SectorStepRad;
            }

            var dots = GetDots();

            //  Draw POI dots
            foreach (var dot in dots)
            {
                DrawCircle(ProjectionBackgroundImage, dot.Item1, 2, new MCvScalar(new Random().Next(50, 254), new Random().Next(50, 254), new Random().Next(50, 254)), 4);
            }

            // Save dots to db
            SaveDots(dots);

            DartboardImage = EmguImageToBitmapImage(ProjectionBackgroundImage);
        }

        private void SaveDots(IEnumerable<(PointF, string)> dots)
        {
            var connection = new SQLiteConnection("Data Source=Dots.db; Pooling=true;");

            foreach (var dot in dots)
            {
                var query = $"INSERT INTO [Dots] (X,Y,Result) " +
                            $"VALUES ({dot.Item1.X},{dot.Item1.Y},'{dot.Item2}')";
                var cmd = new SQLiteCommand(query) {Connection = connection};
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        private IEnumerable<(PointF, string)> GetDots()
        {
            var dots = new Collection<(PointF, string)>();
            var betweenRowPX = 10;
            var betweenRowSemiPX = betweenRowPX / 2;

            dots.Add((projectionCenterPoint, "Bull"));

            #region Bull

            for (int i = 1; i < 2; i++) // bull / center / down
            {
                dots.Add((new PointF(projectionCenterPoint.X, projectionCenterPoint.Y + i * betweenRowPX), "Bull"));
            }

            for (int i = 1; i < 2; i++) // bull / center / up
            {
                dots.Add((new PointF(projectionCenterPoint.X, projectionCenterPoint.Y - i * betweenRowPX), "Bull"));
            }

            var topYDotBull = new PointF(projectionCenterPoint.X, projectionCenterPoint.Y - 2 * betweenRowPX);


            for (int i = 1; i < 3; i++) // bull / row 2,8
            {
                dots.Add((new PointF(topYDotBull.X - betweenRowSemiPX * 5 + betweenRowPX, topYDotBull.Y + betweenRowSemiPX + i * betweenRowPX), "Bull"));
                dots.Add((new PointF(topYDotBull.X + betweenRowSemiPX * 5 - betweenRowPX, topYDotBull.Y + betweenRowSemiPX + i * betweenRowPX), "Bull"));
            }

            for (int i = 1; i < 4; i++) // bull / row 3,7
            {
                dots.Add((new PointF(topYDotBull.X - betweenRowSemiPX * 2, topYDotBull.Y + i * betweenRowPX), "Bull"));
                dots.Add((new PointF(topYDotBull.X + betweenRowSemiPX * 2, topYDotBull.Y + i * betweenRowPX), "Bull"));
            }

            for (int i = 0; i < 4; i++) // bull / row 4,6
            {
                dots.Add((new PointF(topYDotBull.X - betweenRowSemiPX * 3 + betweenRowPX, topYDotBull.Y + betweenRowSemiPX + i * betweenRowPX), "Bull"));
                dots.Add((new PointF(topYDotBull.X + betweenRowSemiPX * 3 - betweenRowPX, topYDotBull.Y + betweenRowSemiPX + i * betweenRowPX), "Bull"));
            }

            #endregion

            #region _25

            for (int i = 1; i < 3; i++) // 25 / center / down
            {
                dots.Add((new PointF(projectionCenterPoint.X, projectionCenterPoint.Y + betweenRowPX * 2 + i * betweenRowPX), "_25"));
            }

            for (int i = 1; i < 3; i++) // 25 / center / up
            {
                dots.Add((new PointF(projectionCenterPoint.X, projectionCenterPoint.Y - betweenRowPX * 2 - i * betweenRowPX), "_25"));
            }

            var topYDot_25 = new PointF(projectionCenterPoint.X, projectionCenterPoint.Y - 5 * betweenRowPX);

            for (int i = 4; i < 8; i++) // 25 / row 1,25
            {
                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 4 - betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 4 + betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
            }

            for (int i = 3; i < 8; i++) // 25 / row 2,24
            {
                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 4, topYDot_25.Y + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 4, topYDot_25.Y + i * betweenRowPX), "_25"));
            }

            for (int i = 3; i < 9; i++) // 25 / row 3,23
            {
                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 3 - betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 3 + betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
            }

            for (int i = 2; i < 9; i++) // 25 / row 4,22
            {
                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 3, topYDot_25.Y + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 3, topYDot_25.Y + i * betweenRowPX), "_25"));
            }

            for (int i = 2; i < 10; i++) // 25 / row 5,21
            {
                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 2 - betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 2 + betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
            }

            for (int i = 1; i < 10; i++) // 25 / row 6,20
            {
                if (i is > 3 and <= 6)
                {
                    continue;
                }

                dots.Add((new PointF(topYDot_25.X - betweenRowPX * 2, topYDot_25.Y + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX * 2, topYDot_25.Y + i * betweenRowPX), "_25"));
            }

            for (int i = 1; i < 11; i++) // 25 / row 7,19
            {
                if (i is > 3 and <= 7)
                {
                    continue;
                }

                dots.Add((new PointF(topYDot_25.X - betweenRowPX - betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX + betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
            }

            for (int i = 1; i < 10; i++) // 25 / row 8,18
            {
                if (i is > 2 and <= 7)
                {
                    continue;
                }

                dots.Add((new PointF(topYDot_25.X - betweenRowPX, topYDot_25.Y + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowPX, topYDot_25.Y + i * betweenRowPX), "_25"));
            }

            for (int i = 1; i < 11; i++) // 25 / row 9,17
            {
                if (i is > 3 and <= 7)
                {
                    continue;
                }

                dots.Add((new PointF(topYDot_25.X - betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
                dots.Add((new PointF(topYDot_25.X + betweenRowSemiPX, topYDot_25.Y - betweenRowSemiPX + i * betweenRowPX), "_25"));
            }

            #endregion

            return dots.Distinct();
        }

        private BitmapImage EmguImageToBitmapImage(Image<Bgr, byte> image)
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}