﻿using System;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealPostRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid OversightReviewId { get; set; }
        public string Message { get; set; }
        public SubmitOption SelectedOption { get; set; }
        public IFormFile FileUpload { get; set; }
        public Guid FileId { get; set; }

        public enum SubmitOption
        {
            SaveAndContinue,
            Upload,
            RemoveFile
        }
    }
}
