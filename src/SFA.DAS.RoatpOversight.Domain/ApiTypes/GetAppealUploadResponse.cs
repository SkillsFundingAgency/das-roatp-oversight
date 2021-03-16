namespace SFA.DAS.RoatpOversight.Domain.ApiTypes
{
    public class GetAppealUploadResponse
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
