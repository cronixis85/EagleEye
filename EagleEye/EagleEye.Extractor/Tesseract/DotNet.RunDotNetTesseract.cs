using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Tesseract
{
    public class RunDotNetTesseract
    {
        private const string Config = @"stdout -c tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZ --psm 7";
        private readonly string _tempDir;
        private readonly string _tesseractPath;

        public RunDotNetTesseract(string tesseractPath, string tempDir)
        {
            if (string.IsNullOrEmpty(tesseractPath))
                throw new ArgumentNullException(nameof(tesseractPath));

            if (string.IsNullOrEmpty(tempDir))
                throw new ArgumentNullException(nameof(tempDir));

            _tesseractPath = tesseractPath;
            _tempDir = tempDir;
            EnsureDirectoryExist(tempDir);
        }

        public async Task<string> ExecuteAsync(byte[] data)
        {
            // split characters in captcha image
            var sprites = new SplitCaptchaWithOpenCv().Execute(data);

            var batchId = DateTime.Now.Ticks;

            // solve each sprite
            var tasks = sprites.Select(async (x, i) =>
            {
                var result = await ExecuteSingleSpriteAsync(x, $"{batchId}_{i}".ToString());
                return new {Index = i, CaptchaText = result};
            }).ToArray();

            await Task.WhenAll(tasks);

            var results = tasks
                .Select(x => x.Result)
                .OrderBy(x => x.Index)
                .Select(x => x.CaptchaText)
                .ToArray();

            var text = string.Concat(results);

            return text.ToUpper().Replace(".", string.Empty);
        }

        private async Task<string> ExecuteSingleSpriteAsync(byte[] data, string uniqueId)
        {
            if (data?.Length == 0)
                throw new ArgumentNullException(nameof(data));
            
            var filePath = Path.Combine(_tempDir, uniqueId + ".jpg");

            try
            {
                await File.WriteAllBytesAsync(filePath, data);
                return await ExecuteAsync(filePath);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        private async Task<string> ExecuteAsync(string imagePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = imagePath + " " + Config,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            // Read the standard output of the app we called.  
            using (var myStreamReader = process.StandardOutput)
            {
                var captcha = await myStreamReader.ReadLineAsync();

                // wait exit signal from the app we called 
                process.WaitForExit();

                // close the process 
                process.Close();

                return captcha;
            }
        }

        private static void EnsureDirectoryExist(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}