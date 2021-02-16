using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealPostRequest
    {
        public Guid ApplicationId { get; set; }
        public string Message { get; set; }
        public SubmitOption SelectedOption { get; set; }

        public enum SubmitOption
        {
            SaveAndContinue,
            Upload
        }
    }
}
