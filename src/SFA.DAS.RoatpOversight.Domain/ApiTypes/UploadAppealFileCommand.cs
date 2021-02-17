using System;

namespace SFA.DAS.RoatpOversight.Domain.ApiTypes
{
    public class UploadAppealFileCommand
    {
        public Guid ApplicationId { get; set; }
        public FileUpload File { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
