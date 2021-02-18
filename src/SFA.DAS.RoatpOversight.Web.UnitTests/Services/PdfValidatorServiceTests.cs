using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Services;


namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class PdfValidatorServiceTests
    {
        private PdfValidatorService _pdfValidatorService;

        [SetUp]
        public void Setup()
        {
            _pdfValidatorService = new PdfValidatorService();
        }

        [Test]
        public async Task IsPdf_Returns_True_If_File_Has_Pdf_Header()
        {
            var file = GenerateFile(true);
            var result = await _pdfValidatorService.IsPdf(file);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsPdf_ReturnsFalse_If_File_Does_Not_Have_Pdf_Header()
        {
            var file = GenerateFile(false);
            var result = await _pdfValidatorService.IsPdf(file);
            Assert.IsFalse(result);
        }

        private static IFormFile GenerateFile(bool hasPdfHeader)
        {
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 };

            var fileContent = new MemoryStream();

            if (hasPdfHeader)
            {
                fileContent.Write(pdfHeader);
            }

            var remainingContentToGenerate = 100 - (int)fileContent.Length;

            var contentToGenerate = Enumerable.Repeat((byte)0x20, remainingContentToGenerate);
            fileContent.Write(contentToGenerate.ToArray());

            return new FormFile(fileContent, 0, fileContent.Length, "test", "test");
        }
    }
}
