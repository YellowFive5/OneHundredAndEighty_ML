﻿#region Usings

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace RenderImagesConverter
{
    public class ImageProcessor
    {
        private static readonly List<List<PointF>> ProjectionToImgWarpsKeyPoints = new()
                                                                                   {
                                                                                       new List<PointF>
                                                                                       {
                                                                                           new(800, 181),
                                                                                           new(1141, 649),
                                                                                           new(499, 1118),
                                                                                           new(249, 363),
                                                                                       },
                                                                                       new List<PointF>
                                                                                       {
                                                                                           new(839, 974),
                                                                                           new(932, 81),
                                                                                           new(136, 330),
                                                                                           new(1663, 435),
                                                                                       }
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
            images.Add(warpedImage);

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