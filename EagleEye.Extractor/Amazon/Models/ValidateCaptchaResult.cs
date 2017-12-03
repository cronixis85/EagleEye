using System;
using System.Collections.Generic;

namespace EagleEye.Extractor.Amazon.Models
{
    public class ValidateCaptchaResult
    {
        public Uri SubmitFormUri { get; set; }

        public Dictionary<string, string> HiddenInputs { get; set; }

        public Uri CaptchaImageUri { get; set; }

        public byte[] CaptchaImageBytes { get; set; }
    }
}