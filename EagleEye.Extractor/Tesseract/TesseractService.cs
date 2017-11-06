using System;
using System.Diagnostics;
using System.IO;

namespace EagleEye.Extractor.Tesseract
{
    public class TesseractService
    {
        private readonly string _pythonPath;
        private readonly string _captchaSolvePath;

        public TesseractService(string pythonPath, string captchaSolvePath)
        {
            _pythonPath = pythonPath;
            _captchaSolvePath = captchaSolvePath;
        }

        public string Run(string base64)
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
                var captcha = myStreamReader.ReadLine();

                // wait exit signal from the app we called 
                process.WaitForExit();

                // close the process 
                process.Close();

                return captcha;
            }
        }
    }
}