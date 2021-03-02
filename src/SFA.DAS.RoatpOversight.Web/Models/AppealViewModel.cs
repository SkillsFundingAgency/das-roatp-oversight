using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealViewModel
    {
        public Guid ApplicationId { get; set; }
        public string Message { get; set; }
        public IFormFile FileUpload { get; set; }
        public bool AllowAdditionalUploads { get; set; }

        public List<UploadedFileViewModel> UploadedFiles { get; set; }

        public AppealViewModel()
        {
            UploadedFiles = new List<UploadedFileViewModel>();
        }
    }
}
