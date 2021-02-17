using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class AppealOrchestratorTests
    {
        private AppealOrchestrator _orchestrator;
        private Mock<IApplyApiClient> _applyApiClient;

        private readonly Guid _applicationId = Guid.NewGuid();
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
    }
}
