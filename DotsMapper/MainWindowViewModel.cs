#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        private readonly Bgr projectionDigitsColor = new Bgr(Color.White);
        private const int ProjectionGridThickness = 2;
        private const double SectorStepRad = 0.314159;
        private const double SemiSectorStepRad = SectorStepRad / 2;
        private const double StartRadSector_11 = -3.14159;
        private const double ProjectionDigitsScale = 2;
        private const int ProjectionDigitsThickness = 2;

        public void ProjectionPrepare()
        {
            ProjectionBackgroundImage = new Image<Bgr, byte>(ProjectionFrameSide, ProjectionFrameSide);

            var projectionCenterPoint = new PointF((float) ProjectionBackgroundImage.Width / 2,
                                                   (float) ProjectionBackgroundImage.Height / 2);

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

            DartboardImage = EmguImageToBitmapImage(ProjectionBackgroundImage);
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