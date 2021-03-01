using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class MultipartFormDataServiceTests
    {
        private MultipartFormDataService _multipartFormDataService;
        private TestClass _testClass;
        private static readonly Fixture AutoFixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _multipartFormDataService = new MultipartFormDataService();
            _testClass = new TestClass
            {
                Property1 = AutoFixture.Create<Guid>(),
                Property2 = AutoFixture.Create<int>(),
                Property3 = AutoFixture.Create<string>(),
                Property4 = AutoFixture.Create<long>(),
                Property5 = GenerateFile()
            };
        }

        [Test]
        public async Task CreateMultipartFormData_adds_http_content_for_each_request_property()
        {
            var result = _multipartFormDataService.CreateMultipartFormDataContent(_testClass).ToList();
           
            Assert.AreEqual(5, result.Count);

            await AssertContent(result[0], "Property1", _testClass.Property1);
            await AssertContent(result[1], "Property2", _testClass.Property2);
            await AssertContent(result[2], "Property3", _testClass.Property3);
            await AssertContent(result[3], "Property4", _testClass.Property4);
            await AssertFileContent(result[4], "Property5");
        }


        private async Task AssertContent(HttpContent content, string key, object expectedValue)
        {
            Assert.AreEqual(key, content.Headers.ContentDisposition.Name);
            var value = await content.ReadAsStringAsync();
            Assert.AreEqual(Convert.ToString(expectedValue), value);
        }

        private async Task AssertFileContent(HttpContent content, string key)
        {
            Assert.AreEqual(key, content.Headers.ContentDisposition.Name);
            Assert.AreEqual(_testClass.Property5.ContentType, content.Headers.ContentType.ToString());
            Assert.AreEqual(_testClass.Property5.FileName, content.Headers.ContentDisposition.FileName);
            
            var actualValue = await content.ReadAsStringAsync();
            var expectedValue = "";

            using (var reader = new StreamReader(_testClass.Property5.OpenReadStream()))
            {
                expectedValue = reader.ReadToEnd();
            }
            
            Assert.AreEqual(expectedValue, actualValue);
        }

        private class TestClass
        {
            public Guid Property1 { get; set; }
            public int Property2 { get; set; }
            public string Property3 { get; set; }
            public long Property4 { get; set; }
            public IFormFile Property5 { get; set; }
            private string Hidden1 { get; set; }
            private List<string> Hidden2 { get; set; } = new List<string>();
        }

        private static IFormFile GenerateFile()
        {
            var fileName = AutoFixture.Create<string>();
            var content = AutoFixture.Create<string>();
            return new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(content)),
                0,
                content.Length,
                fileName,
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }
    }
}
