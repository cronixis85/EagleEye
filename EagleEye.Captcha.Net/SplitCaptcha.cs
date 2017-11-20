using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace EagleEye.Captcha.Net
{
    public class SplitCaptcha
    {
        private static int EncodeQuality = 80;

        public SplitCaptcha()
        {
            EnsureDirectoryExist(".tmp");
        }

        public byte[][] Execute(Stream ms)
        {
            using (var inputStream = new SKManagedStream(ms))
            using (var bitmap = SKBitmap.Decode(inputStream))
            {
                var width = bitmap.Width;
                var height = bitmap.Height;

                using (var surface = SKSurface.Create(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
                {
                    SetGrayScale(surface, bitmap);

                    if (Debugger.IsAttached)
                        WriteImageToFile(surface.Snapshot(), "grayscale");

                    var images = SplitImageByCharacters(surface);

                    if (Debugger.IsAttached)
                        for (var i = 0; i < images.Count; i++)
                            WriteImageToFile(images[i], "debug_" + i);

                    images = AutoRotateImage(images);

                    if (Debugger.IsAttached)
                        for (var i = 0; i < images.Count; i++)
                            WriteImageToFile(images[i], "rotated_" + i);

                    var imageBytes = images.Select(x => x.Encode(SKEncodedImageFormat.Jpeg, EncodeQuality).ToArray()).ToArray();

                    return imageBytes;
                }
            }
        }

        private void SetGrayScale(SKSurface surface, SKBitmap bitmap)
        {
            // do not grayscale too much so that image can be split easily
            using (var cf = SKColorFilter.CreateHighContrast(true, SKHighContrastConfigInvertStyle.NoInvert, 0.5f))
            using (var paint = new SKPaint())
            {
                paint.ColorFilter = cf;

                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(bitmap, SKRect.Create(bitmap.Width, bitmap.Height), paint);
            }
        }

        private List<SKImage> SplitImageByCharacters(SKSurface surface, int threshold = 243)
        {
            var image = surface.Snapshot();

            var bitmap = SKBitmap.FromImage(image);

            var meanColors = new Dictionary<int, bool>();

            for (var x = 0; x < bitmap.Width; x++)
            {
                var colorTotal = 0;

                for (var y = 0; y < bitmap.Height; y++)
                    colorTotal += bitmap.GetPixel(x, y).Blue;

                // round down
                var colorAverage = colorTotal / bitmap.Height;

                meanColors.Add(x, colorAverage < threshold);
            }

            var orderedColors = meanColors
                .OrderBy(x => x.Key)
                .ToArray();

            var min = orderedColors.First(x => x.Value);
            var max = orderedColors.Last(x => x.Value);

            var separators = new List<int>();

            for (var x = min.Key + 1; x < max.Key - 1; x++)
            {
                var previous = orderedColors[x - 1];
                var current = orderedColors[x];
                var next = orderedColors[x + 1];

                if (current.Value == false)
                {
                    separators.Add(current.Key);
                }
                else
                {
                    if (previous.Value != current.Value && current.Value != next.Value)
                        separators.Add(current.Key);
                }
            }

            separators.Insert(0, min.Key);
            separators.Add(max.Key);

            // remove consecutive indexes
            var keyToRemove = new List<int>();

            for (var i = 1; i < separators.Count; i++)
            {
                var previous = separators[i - 1];
                var current = separators[i];

                if (current - previous == 1)
                {
                    keyToRemove.Add(previous);
                }
            }

            separators = separators.Except(keyToRemove).ToList();

            return SplitImageBySeparators(image, separators);
        }

        private static List<SKImage> SplitImageBySeparators(SKImage image, IReadOnlyList<int> separators)
        {
            var subsets = new List<SKImage>();

            for (var i = 0; i < separators.Count - 1; i++)
            {
                var left = separators[i];
                var right = separators[i + 1];
                var width = right - left;
                var height = image.Height;

                var crop = SKRectI.Create(width, height);
                crop.Left = left;
                crop.Right = right;

                subsets.Add(image.Subset(crop));
            }

            return subsets;
        }

        private static List<SKImage> AutoRotateImage(List<SKImage> images)
        {
            var rotated = new List<SKImage>();

            for (var i = 0; i < images.Count; i++)
            {
                var img = images[i];

                using (var surface = SKSurface.Create(img.Width + 20, img.Height + 20, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
                //using (var cf = SKColorFilter.CreateHighContrast(true, SKHighContrastConfigInvertStyle.NoInvert, 1.0f))
                //using (var paint = new SKPaint())
                {
                    //paint.IsAntialias = true;
                    //paint.ColorFilter = cf;

                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.White);

                    switch (i)
                    {
                        case 0:
                            canvas.RotateDegrees(-12, img.Width / 2f + 50, img.Height / 2f);
                            break;
                        case 1:
                            canvas.RotateDegrees(15, img.Width / 2f, img.Height / 2f + 20);
                            break;
                        case 2:
                            canvas.RotateDegrees(-20, img.Width / 2f + 50, img.Height / 2f - 10);
                            break;
                        case 3:
                            canvas.RotateDegrees(13, img.Width / 2f, img.Height / 2f);
                            break;
                        case 4:
                            canvas.RotateDegrees(-7, img.Width / 2f, img.Height / 2f);
                            break;
                        case 5:
                            canvas.RotateDegrees(15, img.Width / 2f, img.Height / 2f);
                            break;
                    }
                    
                    canvas.DrawImage(img, 10, 0);

                    rotated.Add(surface.Snapshot());
                }
            }

            return rotated;
        }

        private void WriteImageToFile(SKImage image, string fileName)
        {
            var data = image.Encode(SKEncodedImageFormat.Jpeg, EncodeQuality);
            File.WriteAllBytes(Path.Combine(".tmp", fileName + ".jpg"), data.ToArray());
        }

        private static void EnsureDirectoryExist(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}