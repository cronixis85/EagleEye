using System.IO;
using System.Linq;
using EagleEye.Extractor.Tesseract;

namespace EagleEye.Captcha.Net.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var results = new[]
            {
                SolveImageWithOpenCV("ACYKRB"),
                SolveImageWithOpenCV("AKBFRA"),
                SolveImageWithOpenCV("BPKURG"),
                SolveImageWithOpenCV("CBTRUR"),
                SolveImageWithOpenCV("CKFEBX"),
                SolveImageWithOpenCV("CLMFFE"),
                SolveImageWithOpenCV("CRYNRM"),
                SolveImageWithOpenCV("EAAMGR"),
                SolveImageWithOpenCV("HERYME"),
                SolveImageWithOpenCV("HLGGNU"),
                SolveImageWithOpenCV("JPUYJP"),
                SolveImageWithOpenCV("JRMBJG"),
                SolveImageWithOpenCV("JTNLHN"),
                SolveImageWithOpenCV("KHNRFG"),
                SolveImageWithOpenCV("LEAMXK"),
                SolveImageWithOpenCV("LYGJHY"),
                SolveImageWithOpenCV("MXJAAG"),
                SolveImageWithOpenCV("NNPPHP"),
                SolveImageWithOpenCV("NTJLBM"),
                SolveImageWithOpenCV("PUFTXE"),
                SolveImageWithOpenCV("PXCACE"),
                SolveImageWithOpenCV("TEERXM"),
                SolveImageWithOpenCV("XNKHFY"),
                SolveImageWithOpenCV("YMEPAX")
            };

            System.Console.WriteLine($"{results.Count(x => x)} of {results.Length}");
        }

        private static bool SolveImageWithOpenCV(string name)
        {
            var buffer = File.ReadAllBytes($@"images\{name}.jpg");
            //var splitImages = new SplitCaptchaWithOpenCv().Execute(buffer);
            var captchaText = new RunDotNetTesseract(@"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe", ".tmp").ExecuteAsync(buffer).Result;
            System.Console.WriteLine($"{name} == {captchaText} --> {name == captchaText}");

            return name == captchaText;
        }
    }
}