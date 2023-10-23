#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace RenderImagesConverter
{
    public static class Drawer
    {
        public static readonly List<int> Sectors = new() { 14, 9, 12, 5, 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11 };
        public static readonly PointF ProjectionCenterPoint = new((float)ProjectionFrameSide / 2, (float)ProjectionFrameSide / 2);
        public const int ProjectionFrameSide = 1300;
        public const int ProjectionCoefficient = 3;
        private const int ProjectionGridThickness = 1;
        private const double ProjectionDigitsScale = 2;
        private const int ProjectionDigitsThickness = 2;
        private static readonly MCvScalar ProjectionGridColor = new Bgr(Color.DarkGray).MCvScalar;
        private static readonly Bgr ProjectionDigitsColor = new(Color.DarkGray);

        public static readonly MCvScalar PoiColor = new Bgr(Color.Red).MCvScalar;
        public static readonly MCvScalar PoiContourAreaColor = new Bgr(Color.Chartreuse).MCvScalar;

        public static readonly List<Bgr> DartContoursColors = new()
                                                              {
                                                                  new Bgr(Color.DarkGoldenrod),
                                                                  new Bgr(Color.DarkGreen),
                                                                  new Bgr(Color.DarkBlue),
                                                              };

        public static readonly List<Bgr> DartContoursComs = new()
                                                            {
                                                                new Bgr(Color.Yellow),
                                                                new Bgr(Color.Lime),
                                                                new Bgr(Color.Blue),
                                                            };

        public static Image<Bgr, byte> DrawBlackProjectionBlank()
        {
            return new Image<Bgr, byte>(ProjectionFrameSide, ProjectionFrameSide);
        }

        public static Image<Bgr, byte> DrawProjection(Image<Bgr, byte> canvasImage = null)
        {
            var projectionImage = canvasImage ?? DrawBlackProjectionBlank();

            // Draw dartboard projection
            var values = new List<int> { 7, 17, 95, 105, 160, 170 };
            values.ForEach(v => DrawCircle(projectionImage, ProjectionCenterPoint, ProjectionCoefficient * v, ProjectionGridThickness, ProjectionGridColor));

            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new PointF((float)(ProjectionCenterPoint.X + Math.Cos(Measurer.SectorStepRad * i - Measurer.SemiSectorStepRad) * ProjectionCoefficient * 170),
                                               (float)(ProjectionCenterPoint.Y + Math.Sin(Measurer.SectorStepRad * i - Measurer.SemiSectorStepRad) * ProjectionCoefficient * 170));
                var segmentPoint2 = new PointF((float)(ProjectionCenterPoint.X + Math.Cos(Measurer.SectorStepRad * i - Measurer.SemiSectorStepRad) * ProjectionCoefficient * 17),
                                               (float)(ProjectionCenterPoint.Y + Math.Sin(Measurer.SectorStepRad * i - Measurer.SemiSectorStepRad) * ProjectionCoefficient * 17));
                DrawLine(projectionImage, segmentPoint1, segmentPoint2, ProjectionGridThickness, ProjectionGridColor);
            }

            // Draw digits
            var radSector = Measurer.StartRadSector14;
            foreach (var sector in Sectors)
            {
                DrawString(projectionImage,
                           sector.ToString(),
                           new PointF((float)(ProjectionCenterPoint.X - 40 + Math.Cos(radSector) * ProjectionCoefficient * 190),
                                      (float)(ProjectionCenterPoint.Y + 20 + Math.Sin(radSector) * ProjectionCoefficient * 190)),
                           ProjectionDigitsScale,
                           ProjectionDigitsThickness,
                           ProjectionDigitsColor);
                radSector += Measurer.SectorStepRad;
            }

            return projectionImage;
        }

        public static void DrawCircle(IInputOutputArray image,
                                      PointF centerPoint,
                                      int radius,
                                      int thickness,
                                      MCvScalar color
        )
        {
            CvInvoke.Circle(image,
                            Measurer.PointFToPoint(centerPoint),
                            radius,
                            color,
                            thickness);
        }

        public static void DrawLine(IInputOutputArray image,
                                    PointF point1,
                                    PointF point2,
                                    int thickness,
                                    MCvScalar color)
        {
            CvInvoke.Line(image,
                          Measurer.PointFToPoint(point1),
                          Measurer.PointFToPoint(point2),
                          color,
                          thickness);
        }

        public static void DrawString(Image<Bgr, byte> image,
                                      string text,
                                      PointF point,
                                      double scale,
                                      int thickness,
                                      Bgr color)
        {
            image.Draw(text,
                       Measurer.PointFToPoint(point),
                       FontFace.HersheySimplex,
                       scale,
                       color,
                       thickness);
        }

        public static void DrawString(Image<Gray, byte> image,
                                      string text,
                                      PointF point,
                                      double scale,
                                      int thickness,
                                      Gray color)
        {
            image.Draw(text,
                       Measurer.PointFToPoint(point),
                       FontFace.HersheySimplex,
                       scale,
                       color,
                       thickness);
        }

        public static void DrawContour(IInputArray image,
                                       VectorOfVectorOfPoint contour,
                                       MCvScalar color,
                                       int thickness = -1,
                                       int contourIndex = -1)
        {
            switch (image)
            {
                case Image<Bgr, byte> imageBgr:
                    CvInvoke.DrawContours(imageBgr, contour, contourIndex, color, thickness, LineType.FourConnected);
                    break;
                case Image<Gray, byte> imageGrey:
                    CvInvoke.DrawContours(imageGrey, contour, contourIndex, color, thickness, LineType.FourConnected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MCvScalar RandomColor()
        {
            var from = 50;
            var to = 150;
            return new MCvScalar(new Random().Next(from, to),
                                 new Random().Next(from, to),
                                 new Random().Next(from, to));
        }
    }
}