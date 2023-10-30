#region Usings

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace RenderImagesConverter
{
    public class ImageProcessor
    {
        private static readonly List<List<PointF>> ProjectionToImgWarpsKeyPoints = new()
                                                                                   {
                                                                                       new List<PointF> // projection
                                                                                       {
                                                                                           new(881, 196),
                                                                                           new(1104, 881),
                                                                                           new(419, 1104),
                                                                                           new(196, 419),
                                                                                       }
                                                                                       ,
                                                                                       new List<PointF> // blenderCam
                                                                                       {
                                                                                           new(839, 974),
                                                                                           new(136, 330),
                                                                                           new(932, 81),
                                                                                           new(1663, 435),
                                                                                       }
                                                                                        ,
                                                                                       // new List<PointF> // live cam
                                                                                       // {
                                                                                       //     new(925, 1016),
                                                                                       //     new(216, 272),
                                                                                       //     new(985, 52),
                                                                                       //     new(1697, 412),
                                                                                       // }
                                                                                   };

        private readonly int[] bilateralSetups = { 11, 41, 21 };

        public List<Image<Gray, byte>> ConvertImage(Image<Bgr, byte> backgroundImage,
                                                    Image<Bgr, byte> throwImage)
        {
            var images = new List<Image<Gray, byte>>(); // todo temp to pass images to app front 

            var diffs = PrepareDiffImage(backgroundImage, throwImage);
            var diffImage = diffs.First();
            var cannyImage = diffs.Last();

            images.Add(diffImage);
            images.Add(cannyImage);

            // warp perspective
            var warpMat = CvInvoke.GetPerspectiveTransform(ProjectionToImgWarpsKeyPoints.Last().ToArray(), ProjectionToImgWarpsKeyPoints.First().ToArray());
            var warpedImage = new Image<Gray, byte>(Drawer.ProjectionFrameSide, Drawer.ProjectionFrameSide);
            CvInvoke.WarpPerspective(cannyImage, warpedImage, warpMat, warpedImage.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(0));
            
            //contours filter
            var numberToFiltrate = 3;
            var filtratedContoursImage = new Image<Gray, byte>(warpedImage.Width, warpedImage.Height);
            var allContours = Measurer.FindContours(warpedImage);
            var tooNoisy = Measurer.FindNumberOfContours(allContours) >= numberToFiltrate;
            var filtrated = tooNoisy
                                ? Measurer.FindBiggestContoursByArea(allContours, numberToFiltrate)
                                : allContours;
            filtrated = new VectorOfVectorOfPoint(filtrated.ToArrayOfArray()
                                                           .Where(cp => Measurer.FindContourArea(Measurer.PArrToVoVoP(cp)) > 20)
                                                           .ToArray());
            
            Drawer.DrawContour(filtratedContoursImage, filtrated, new Bgr(Color.White).MCvScalar);
            images.Add(filtratedContoursImage);
            
            // images.Add(warpedImage);

            return images;
        }

        private List<Image<Gray, byte>> PrepareDiffImage(Image<Bgr, byte> backgroundImage,
                                                         Image<Bgr, byte> throwImage)
        {
            var clearDiff = throwImage.Convert<Gray, byte>().AbsDiff(backgroundImage.Convert<Gray, byte>());

            var backgroundImageGray = backgroundImage.Convert<Gray, byte>()
                                                     .SmoothBilateral(bilateralSetups[0], bilateralSetups[1], bilateralSetups[2]);

            var nextImageGrey = throwImage.Convert<Gray, byte>()
                                          .SmoothBilateral(bilateralSetups[0], bilateralSetups[1], bilateralSetups[2]);

            var diffImage = nextImageGrey.AbsDiff(backgroundImageGray);
            CvInvoke.Threshold(diffImage, diffImage, 0, 255, ThresholdType.Otsu);

            return new List<Image<Gray, byte>> { clearDiff, diffImage };
        }
    }
}