using System;
using System.Collections.Generic;

namespace EagleEye.Extractor.Amazon.Models
{
    public class ValidateCaptchaResult
    {
        public Uri SubmitFormUri { get; set; }

        public Dictionary<string, string> HiddenInputs { get; set; }

        public Uri CaptchaImageUri { get; set; }

        public string CaptchaBase64 { get; set; }
    }
}