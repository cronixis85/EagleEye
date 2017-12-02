using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace EagleEye.Extractor.Tesseract
{
    public class RotateCaptcha
    {
        public void Execute(Mat src, double angle)
        {
            RotateImage(src, src, angle, 1);
        }

        private static void RotateImage(Mat src, Mat dst, double angle, double scale)
        {
            var imageCenter = new Point2f(src.Cols / 2f, src.Rows / 2f);
            var rotationMat = Cv2.GetRotationMatrix2D(imageCenter, angle, scale);
            
            Cv2.WarpAffine(src, dst, rotationMat, src.Size());
        }
    }
}