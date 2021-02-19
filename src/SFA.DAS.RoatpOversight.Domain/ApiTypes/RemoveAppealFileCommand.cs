using System;

namespace SFA.DAS.RoatpOversight.Domain.ApiTypes
{
    public class RemoveAppealFileCommand
    {
        public Guid ApplicationId { get; set; }
        public Guid FileId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
