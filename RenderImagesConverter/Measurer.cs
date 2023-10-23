#region Usings

using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace RenderImagesConverter
{
    // todo delete unused
    public static class Measurer
    {
        public const double SectorStepRad = 0.314159;
        public const double SemiSectorStepRad = SectorStepRad / 2;
        public const double StartRadSector11 = -3.14159;
        public const double StartRadSector1114 = -2.9845105;
        public const double StartRadSector14 = -2.827431;

        public static PointF FindLinesIntersection(PointF line1Point1,
                                                   PointF line1Point2,
                                                   PointF line2Point1,
                                                   PointF line2Point2)
        {
            var tolerance = 0.001;
            var x1 = line1Point1.X;
            var y1 = line1Point1.Y;
            var x2 = line1Point2.X;
            var y2 = line1Point2.Y;
            var x3 = line2Point1.X;
            var y3 = line2Point1.Y;
            var x4 = line2Point2.X;
            var y4 = line2Point2.Y;
            float x;
            float y;

            if (Math.Abs(x1 - x2) < tolerance)
            {
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = x1;
                y = c2 + m2 * x1;
            }
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;
            }

            return new PointF(x, y);
        }

        public static VectorOfVectorOfPoint FindContours(IInputOutputArray input)
        {
            var allContours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(input,
                                  allContours,
                                  new Mat(),
                                  RetrType.External,
                                  ChainApproxMethod.ChainApproxNone);
            return allContours;
        }

        public static VectorOfVectorOfPoint FindContoursIntersection(VectorOfVectorOfPoint firstContour, VectorOfVectorOfPoint secondContour)
        {
            try
            {
                var img1 = Drawer.DrawBlackProjectionBlank().Convert<Gray, byte>();
                var img2 = Drawer.DrawBlackProjectionBlank().Convert<Gray, byte>();
                CvInvoke.DrawContours(img1, firstContour, -1, new Bgr(Color.White).MCvScalar, -1, LineType.FourConnected);
                CvInvoke.DrawContours(img2, secondContour, -1, new Bgr(Color.White).MCvScalar, -1, LineType.FourConnected);

                var intersectImg = Drawer.DrawBlackProjectionBlank().Convert<Gray, byte>();
                CvInvoke.BitwiseAnd(img1, img2, intersectImg);

                var intersectContours = FindContours(intersectImg);

                return intersectContours[0].Size > 0
                           ? intersectContours
                           : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static VectorOfVectorOfPoint PlainContours(VectorOfVectorOfPoint contours)
        {
            return PArrToVoVoP(contours.ToArrayOfArray()
                                       .SelectMany(ap => ap)
                                       .ToArray());
        }

        public static VectorOfVectorOfPoint FindBiggestContoursByNumberOfPoints(VectorOfVectorOfPoint inputPoints, int numberOfContoursToReturn = 1)
        {
            return new VectorOfVectorOfPoint(inputPoints.ToArrayOfArray()
                                                        .OrderByDescending(p => p.Length)
                                                        .Take(numberOfContoursToReturn)
                                                        .Select(p => new VectorOfPoint(p))
                                                        .ToArray());
        }

        public static VectorOfVectorOfPoint FindBiggestContoursByArea(VectorOfVectorOfPoint inputPoints, int numberOfContoursToReturn = 1)
        {
            return new VectorOfVectorOfPoint(inputPoints.ToArrayOfArray()
                                                        .OrderByDescending(p => FindContourArea(PArrToVoVoP(p)))
                                                        .Take(numberOfContoursToReturn)
                                                        .Select(p => new VectorOfPoint(p))
                                                        .ToArray());
        }

        public static VectorOfVectorOfPoint FindNearestContoursByCom(VectorOfVectorOfPoint inputPoints, VectorOfVectorOfPoint nearestTo, int numberOfContoursToReturn = 1)
        {
            return new VectorOfVectorOfPoint(inputPoints.ToArrayOfArray()
                                                        .OrderBy(ip => FindDistance(FindCenterOfMass(ip), FindCenterOfMass(nearestTo)))
                                                        .Take(numberOfContoursToReturn + 1)
                                                        .Select(p => new VectorOfPoint(p))
                                                        .ToArray());
        }

        public static VectorOfVectorOfPoint PArrToVoVoP(Point[] points)
        {
            return new VectorOfVectorOfPoint(new VectorOfPoint(points));
        }

        public static double FindContourArea(VectorOfVectorOfPoint inputPoints)
        {
            return CvInvoke.ContourArea(inputPoints[0]);
        }

        public static double FindContourArc(VectorOfVectorOfPoint inputPoints)
        {
            return CvInvoke.ArcLength(inputPoints[0], false);
        }

        public static PointF FindMiddle(PointF point1,
                                        PointF point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new PointF(mpX, mpY);
        }

        public static float FindDistance(PointF point1,
                                         PointF point2)
        {
            return (float)Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public static float FindAngle(PointF point1,
                                      PointF point2)
        {
            return (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        }

        public static PointF FindCenterOfMass(VectorOfVectorOfPoint inputPoints)
        {
            var arrOfArrs = inputPoints.ToArrayOfArray().FirstOrDefault();
            return new PointF((float)arrOfArrs.Average(p => p.X),
                              (float)arrOfArrs.Average(p => p.Y));
        }

        public static PointF FindCenterOfMass(params Point[] points)
        {
            return new PointF((float)points.Average(p => p.X),
                              (float)points.Average(p => p.Y));
        }

        public static PointF PointToPointF(Point point)
        {
            return new PointF(point.X, point.Y);
        }

        public static Point PointFToPoint(PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public static PointF FindCenterOfMass(params PointF[] points)
        {
            return new PointF(points.Average(p => p.X),
                              points.Average(p => p.Y));
        }

        public static PointF FindCentroid(VectorOfVectorOfPoint inputPoints)
        {
            var contourMoments = CvInvoke.Moments(inputPoints);
            return new PointF((float)(contourMoments.M10 / contourMoments.M00),
                              (float)(contourMoments.M01 / contourMoments.M00));
        }

        public static PointF[] FindBoxPoints(VectorOfVectorOfPoint inputPoints)
        {
            return CvInvoke.BoxPoints(CvInvoke.MinAreaRect(inputPoints[0]));
        }

        public static (PointF[] box1, PointF[] box2) SeparateBox(PointF[] box)
        {
            var boxSide1 = FindDistance(box[0], box[1]);
            var boxSide2 = FindDistance(box[3], box[0]);
            var middlePoint1 = boxSide2 < boxSide1
                                   ? FindMiddle(box[0], box[1])
                                   : FindMiddle(box[3], box[0]);
            var middlePoint2 = boxSide2 < boxSide1
                                   ? FindMiddle(box[3], box[2])
                                   : FindMiddle(box[2], box[1]);
            return (new[] { box[0], middlePoint1, middlePoint2, box[3] },
                       new[] { middlePoint1, box[1], box[2], middlePoint2 });
        }

        public static VectorOfVectorOfPoint FindPointsInsideBox(VectorOfVectorOfPoint inputPoints, PointF[] box)
        {
            return PArrToVoVoP(inputPoints.ToArrayOfArray()
                                          .First()
                                          .Where(p => CvInvoke.PointPolygonTest(new VectorOfPoint(box.Select(i => new Point((int)i.X,
                                                                                                                            (int)i.Y))
                                                                                                     .ToArray()),
                                                                                p,
                                                                                false) >= 0)
                                          .ToArray());
        }

        public static PointF FindRayPoint(PointF from, PointF through, int rayDistance = 500)
        {
            var angle = FindAngle(from, through);
            return new PointF((float)(from.X + Math.Cos(angle) * rayDistance),
                              (float)(from.Y + Math.Sin(angle) * rayDistance));
        }

        public static int FindNumberOfContours(VectorOfVectorOfPoint inputPoints)
        {
            return inputPoints.Size;
        }
    }
}