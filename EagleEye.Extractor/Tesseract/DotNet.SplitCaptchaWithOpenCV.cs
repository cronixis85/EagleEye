using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace EagleEye.Extractor.Tesseract
{
    public class SplitCaptchaWithOpenCv
    {
        private const double Thresh = 30;
        private const double ThresholdMaxVal = 255;

        public List<byte[]> Execute(string path)
        {
            var src = Cv2.ImRead(path);

            var gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGRA2GRAY);

            var threshImage = new Mat();
            Cv2.Threshold(gray, threshImage, Thresh, ThresholdMaxVal, ThresholdTypes.BinaryInv); // Threshold to find contour

            Cv2.FindContours(
                threshImage,
                out var contours,
                out var hierarchyIndexes,
                RetrievalModes.CComp,
                ContourApproximationModes.ApproxSimple);

            if (contours.Length == 0)
                throw new NotSupportedException("Couldn't find any object in the image.");

            var results = new List<OrderedByteResult>();

            var contourIndex = 0;
            while (contourIndex >= 0)
            {
                var contour = contours[contourIndex];
                var leftmostX = contour.Min(c => c.X);

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour

                Cv2.Rectangle(src,
                    new Point(boundingRect.X, boundingRect.Y),
                    new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
                    new Scalar(0, 0, 255),
                    2);

                var roi = new Mat(threshImage, boundingRect); //Crop the image
                Cv2.Threshold(roi, roi, Thresh, ThresholdMaxVal, ThresholdTypes.BinaryInv);

                //Cv2.ImShow("roi", roi);
                //Cv2.WaitKey();

                var roiPadded = new Mat(roi.Width + 20, roi.Height + 20, roi.Type());
                roiPadded.SetTo(new Scalar(255, 255, 255));

                var shiftWidth = 5;
                var shiftHeight = 5;

                var sourceRoi = new Rect(Math.Max(-shiftWidth, 0), Math.Max(-shiftHeight, 0), roi.Width, roi.Height);
                var destRoi = new Rect(Math.Max(shiftWidth, 0), Math.Max(shiftHeight, 0), roi.Width, roi.Height);

                roi.SubMat(sourceRoi)
                   .CopyTo(roiPadded.SubMat(destRoi));

                //Cv2.ImShow("roiPadded", roiPadded);
                //Cv2.WaitKey();

                var bytes = roiPadded.ImEncode(".jpg");
                //Cv2.ImWrite($@".tmp\{contourIndex}.jpg", roiPadded);
                results.Add(new OrderedByteResult
                {
                    Data = bytes,
                    Order = leftmostX
                });

                contourIndex = hierarchyIndexes[contourIndex].Next;
            }

            //Cv2.ImShow("Segmented Source", src);
            //Cv2.ImShow("Detected", dst);

            //Cv2.ImWrite("dest.jpg", dst);

            //Cv2.WaitKey();

            var ordered = results
                .OrderBy(x => x.Order)
                .Select(x => x.Data)
                .ToList();

            return ordered;
        }

        private class OrderedByteResult
        {
            public byte[] Data { get; set; }

            public int Order { get; set; }
        }
    }
}