using System;
using System.IO;
using EagleEye.Extractor.Tesseract;

namespace EagleEye.Captcha.Net.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SolveImageWithOpenCV("ACYKRB");
            //SolveImageWithOpenCV("AKBFRA");
            //SolveImageWithOpenCV("BPKURG");
            //SolveImageWithOpenCV("CBTRUR");
            //SolveImageWithOpenCV("CKFEBX");
            //SolveImageWithOpenCV("CLMFFE");
            //SolveImageWithOpenCV("CRYNRM");
            //SolveImageWithOpenCV("EAAMGR");
            //SolveImageWithOpenCV("HERYME");
            //SolveImageWithOpenCV("HLGGNU");
            //SolveImageWithOpenCV("JPUYJP");
            //SolveImageWithOpenCV("JRMBJG");
            //SolveImageWithOpenCV("JTNLHN");
            //SolveImageWithOpenCV("KHNRFG");
            //SolveImageWithOpenCV("LEAMXK");
            //SolveImageWithOpenCV("LYGJHY");
            //SolveImageWithOpenCV("MXJAAG");
            //SolveImageWithOpenCV("NNPPHP");
            //SolveImageWithOpenCV("NTJLBM");
            //SolveImageWithOpenCV("PUFTXE");
            //SolveImageWithOpenCV("PXCACE");
            //SolveImageWithOpenCV("TEERXM");
            //SolveImageWithOpenCV("XNKHFY");
            //SolveImageWithOpenCV("YMEPAX");

            //SolveImage("ACYKRB");
            //SolveImage("AKBFRA");
            //SolveImage("BPKURG");
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

        private static void SolveImageWithOpenCV(string name)
        {
            var splitImages = new SplitCaptchaWithOpenCv().Execute($@"images\{name}.jpg");
            var captchaText = new RunDotNetTesseract(@"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe").ExecuteAsync(splitImages).Result;
            System.Console.WriteLine($"{name} == {captchaText} --> {name == captchaText}");
        }
    }
}