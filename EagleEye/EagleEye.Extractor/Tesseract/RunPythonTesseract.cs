using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Tesseract
{
    public class RunPythonTesseract
    {
        private readonly string _pythonPath;
        private readonly string _captchaSolvePath;

        public RunPythonTesseract(string pythonPath, string captchaSolvePath)
        {
            _pythonPath = pythonPath;
            _captchaSolvePath = captchaSolvePath;
        }

        public async Task<string> ExecuteAsync(string base64)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = _pythonPath;
            startInfo.Arguments = _captchaSolvePath + " " + base64;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            var process = new Process();
            process.StartInfo = startInfo;
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
    }
}