using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class PdfValidatorService : IPdfValidatorService
    {
        public async Task<bool> IsPdf(IFormFile file)
        {
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            using (var fileContents = file.OpenReadStream())
            {
                var headerOfActualFile = new byte[pdfHeader.Length];
                await fileContents.ReadAsync(headerOfActualFile, 0, headerOfActualFile.Length);
                fileContents.Position = 0;

                return headerOfActualFile.SequenceEqual(pdfHeader);
            }
        }
    }

    public interface IPdfValidatorService
    {
        Task<bool> IsPdf(IFormFile file);
    }
}
