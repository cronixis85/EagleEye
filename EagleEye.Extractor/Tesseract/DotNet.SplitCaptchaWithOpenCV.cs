using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using OpenCvSharp;

namespace EagleEye.Extractor.Tesseract
{
    public class SplitCaptchaWithOpenCv
    {
        private const double Thresh = 70;
        private const double ThresholdMaxVal = 255;

        public List<byte[]> Execute(string path)
        {
            var src = Cv2.ImRead(path);

            var gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGRA2GRAY);

            var threshImage = new Mat();
            Cv2.Threshold(gray, threshImage, 30, ThresholdMaxVal, ThresholdTypes.BinaryInv); // Threshold to find contour

            Cv2.FindContours(
                threshImage,
                out var contours,
                out var hierarchyIndexes,
                RetrievalModes.CComp,
                ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                throw new NotSupportedException("Couldn't find any object in the image.");

            var results = new List<OrderedMatResult>();

            var contourIndex = 0;
            while (contourIndex >= 0)
            {
                var contour = contours[contourIndex];
                var leftmostX = contour.Min(c => c.X);

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour

                //Cv2.Rectangle(src,
                //    new Point(boundingRect.X, boundingRect.Y),
                //    new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
                //    new Scalar(0, 0, 0),
                //    2);

                var roi = new Mat(threshImage, boundingRect); //Crop the image
                Cv2.Threshold(roi, roi, Thresh, ThresholdMaxVal, ThresholdTypes.Binary);

                const int marginFactor = 3;
                var paddedWidth = roi.Width * marginFactor;
                var paddedHeight = roi.Height * marginFactor;

                var roiPadded = new Mat(paddedWidth, paddedHeight, roi.Type());
                roiPadded.SetTo(new Scalar(0, 0, 0));

                var shiftWidth = (roiPadded.Width - roi.Width) / 2;
                var shiftHeight = (roiPadded.Height - roi.Height) / 2;
                
                var destRoi = new Rect(shiftWidth, shiftHeight - 2, roi.Width, roi.Height);

                roi.CopyTo(roiPadded.SubMat(destRoi));

                results.Add(new OrderedMatResult
                {
                    Data = roiPadded,
                    Order = leftmostX
                });

                contourIndex = hierarchyIndexes[contourIndex].Next;
            }

            var ordered = results
                .OrderBy(x => x.Order)
                .Select(x => x.Data)
                .ToList();

            // rotations
            if (ordered.Count == 6)
                for (var i = 0; i < ordered.Count; i++)
                {
                    var current = ordered[i];
                    
                    switch (i)
                    {
                        case 0:
                            new RotateCaptcha().Execute(current, 10);
                            break;
                        case 1:
                            new RotateCaptcha().Execute(current, -15);
                            break;
                        case 2:
                            new RotateCaptcha().Execute(current, 15);

                            // Skew
                            var w = current.Rows;
                            var h = current.Cols;

                            var wMid = w / 2;
                            var hMid = h / 2;

                            var srcTri = new[]
                            {
                                new Point2f(0, hMid),
                                new Point2f(w, hMid),
                                new Point2f(0, h),
                                //new Point2f(w, h)
                            };

                            var dstTri = new[]
                            {
                                new Point2f(0, hMid),
                                new Point2f(w, hMid),
                                new Point2f(-w / 10f, h),
                                //new Point2f(w / 20f, h)
                            };

                            // Get the Affine Transform
                            var warpMat = Cv2.GetAffineTransform(srcTri, dstTri);

                            Cv2.WarpAffine(current, current, warpMat, current.Size(), InterpolationFlags.Area);

                            //Cv2.ImShow("sharpen", current);
                            //Cv2.WaitKey();

                            break;
                        case 3:
                            new RotateCaptcha().Execute(current, -15);
                            break;
                        case 4:
                            new RotateCaptcha().Execute(current, 5);
                            break;
                        case 5:
                            new RotateCaptcha().Execute(current, -15);
                            break;
                    }



                    Cv2.GaussianBlur(current, current, new Size(), 0.001);
                    Cv2.AddWeighted(current, 1, current, 1, 0, current);

                    Cv2.Threshold(current, current, 10, ThresholdMaxVal, ThresholdTypes.BinaryInv);

                    Cv2.Resize(current, current, new Size(300, 300), 0, 0, InterpolationFlags.Cubic);

                    //Cv2.ImShow("sharpen", current);
                    //Cv2.WaitKey();
                }

            var datas = ordered
                .Select(x => x.ImEncode(".jpg"))
                .ToList();

            return datas;
        }

        private class OrderedMatResult
        {
            public Mat Data { get; set; }

            public int Order { get; set; }
        }
    }
}