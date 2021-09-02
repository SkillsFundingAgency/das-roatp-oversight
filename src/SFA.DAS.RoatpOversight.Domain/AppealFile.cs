using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class AppealFile
    {
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
    }
}