using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class AppealOrchestratorTests
    {
        private AppealOrchestrator _orchestrator;
        private Mock<IApplyApiClient> _applyApiClient;

        private readonly Guid _applicationId = Guid.NewGuid();
        private readonly Guid _fileId = Guid.NewGuid();
        private FileUpload _fileUpload;
        private readonly string _userId = "userid";
        private readonly string _userName = "username";

        [SetUp]
        public void SetUp()
        {
            _applyApiClient = new Mock<IApplyApiClient>();

            _applyApiClient.Setup(x => x.UploadAppealFile(It.IsAny<UploadAppealFileCommand>()))
                .Returns(Task.CompletedTask);

            _fileUpload = new FileUpload {ContentType = "pdf", FileName = "testfile"};

            _orchestrator = new AppealOrchestrator(_applyApiClient.Object);
        }

        [Test]
        public async Task UploadAppealFile_Uploads_File()
        {
            await _orchestrator.UploadAppealFile(_applicationId, _fileUpload, _userId, _userName);

            _applyApiClient.Verify(x => x.UploadAppealFile(It.Is<UploadAppealFileCommand>(c =>
                c.ApplicationId == _applicationId &&
                c.File == _fileUpload &&
                c.UserId == _userId &&
                c.UserName == _userName)));
        }

        [Test]
        public async Task RemoveAppealFile_Removes_File()
        {
            await _orchestrator.RemoveAppealFile(_applicationId, _fileId, _userId, _userName);

            _applyApiClient.Verify(x => x.RemoveAppealFile(It.Is<RemoveAppealFileCommand>(c =>
                c.ApplicationId == _applicationId &&
                c.FileId == _fileId &&
                c.UserId == _userId &&
                c.UserName == _userName)));
        }

        [Test]
        public async Task GetAppealViewModel_Gets_Staged_Upload_Files()
        {
            var files = new AppealFiles{ Files = new List<AppealFile>()};
            files.Files.Add(new AppealFile { Id = Guid.NewGuid(), Filename = "test1.pdf" });
            files.Files.Add(new AppealFile { Id = Guid.NewGuid(), Filename = "test2.pdf" });
            files.Files.Add(new AppealFile { Id = Guid.NewGuid(), Filename = "test3.pdf" });

            _applyApiClient.Setup(x =>
                x.GetStagedUploads(It.Is<GetStagedFilesRequest>(r =>
                    r.ApplicationId == _applicationId))).ReturnsAsync(files);

            var result = await _orchestrator.GetAppealViewModel(new AppealRequest{ApplicationId = _applicationId }, string.Empty);

            Assert.AreEqual(files.Files.Count, result.UploadedFiles.Count);

            var i = 0;
            foreach (var expectedFile in files.Files)
            {
                Assert.AreEqual(expectedFile.Id, result.UploadedFiles[i].Id);
                Assert.AreEqual(expectedFile.Filename, result.UploadedFiles[i].Filename);
                i++;
            }
        }

        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        [TestCase(999, false)]
        public async Task GetAppealViewModel_Prevents_Uploads_Beyond_A_Maximum(int filesUploaded, bool expectUploadsEnabled)
        {
            var files = new AppealFiles { Files = new List<AppealFile>() };
            for (var i = 0; i < filesUploaded; i++)
            {
                files.Files.Add(new AppealFile { Id = Guid.NewGuid(), Filename = $"test{i}.pdf" });
            }

            _applyApiClient.Setup(x =>
                x.GetStagedUploads(It.Is<GetStagedFilesRequest>(r =>
                    r.ApplicationId == _applicationId))).ReturnsAsync(files);

            var result = await _orchestrator.GetAppealViewModel(new AppealRequest{ApplicationId = _applicationId}, string.Empty);

            Assert.AreEqual(expectUploadsEnabled, result.AllowAdditionalUploads);
        }
    }
}
