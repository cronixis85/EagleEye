using System;
using System.IO;
using SkiaSharp;

namespace EagleEye.Captcha.Net.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //SolveImage("ACYKRB");
            //SolveImage("AKBFRA");
            SolveImage("BPKURG");
            //SolveImage("CBTRUR");
            //SolveImage("CKFEBX");
            //SolveImage("CLMFFE");
            //SolveImage("CRYNRM");
            //SolveImage("EAAMGR");
            //SolveImage("HERYME");
            //SolveImage("HLGGNU");
            //SolveImage("JPUYJP");
            //SolveImage("JRMBJG");
            //SolveImage("JTNLHN");
            //SolveImage("KHNRFG");
            //SolveImage("LEAMXK");
            //SolveImage("LYGJHY");
            //SolveImage("MXJAAG");
            //SolveImage("NNPPHP");
            //SolveImage("NTJLBM");
            //SolveImage("PUFTXE");
            //SolveImage("PXCACE");
            //SolveImage("TEERXM");
            //SolveImage("XNKHFY");
            //SolveImage("YMEPAX");
        }

        private static void SolveImage(string name)
        {
            using (var stream = File.OpenRead($@"images\{name}.jpg"))
            {
                var byteImages = new SplitCaptcha().Execute(stream);
                var captchaText = new TesseractService(@"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe").RunAsync(byteImages).Result;
                System.Console.WriteLine($"{name} == {captchaText} --> {name == captchaText}");
            }
        }

        private static string ReadFileAsBase64(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int) stream.Length);
                var base64 = Convert.ToBase64String(bytes);
                return base64;
            }
        }

        private static MemoryStream ConvertBase64ToStream(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var stream = new MemoryStream(bytes);
            return stream;
        }
    }
}