namespace SFA.DAS.RoatpOversight.Domain
{
    public class FileUpload
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
    }
}
