using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EagleEye.Captcha.Net
{
    public class TesseractService
    {
        private const string Config = @"stdout -c tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZ -psm 7";
        private const string TempDir = ".tmp";
        private readonly string _tesseractPath;

        public TesseractService(string tesseractPath)
        {
            _tesseractPath = tesseractPath;
            EnsureDirectoryExist(TempDir);
        }

        public async Task<string> RunAsync(byte[][] data)
        {
            var tasks = data.Select(async (x, i) =>
            {
                var result = await RunAsync(x);
                return new { Index = i, CaptchaText = result };
            }).ToArray();

            await Task.WhenAll(tasks);

            var results = tasks
                .Select(x => x.Result)
                .OrderBy(x => x.Index)
                .Select(x => x.CaptchaText)
                .ToArray();

            var text = string.Concat(results);

            return text;
        }

        private Task<string> RunAsync(byte[] data)
        {
            var ticks = DateTime.Now.Ticks;
            var filePath = Path.Combine(TempDir, ticks + ".jpg");

            File.WriteAllBytes(filePath, data);

            return RunAsync(filePath);
        }

        public Task<string> RunAsync(string imagePath)
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
                var captcha = myStreamReader.ReadLineAsync();

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