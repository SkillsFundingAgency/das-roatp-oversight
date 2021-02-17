using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IAppealOrchestrator
    {
        Task UploadAppealFile(Guid applicationId, FileUpload file, string userId, string userName);
    }

    public class AppealOrchestrator : IAppealOrchestrator
    {
        private readonly IApplyApiClient _applyApiClient;

        public AppealOrchestrator(IApplyApiClient applyApiClient)
        {
            _applyApiClient = applyApiClient;
        }

        public async Task UploadAppealFile(Guid applicationId, FileUpload file, string userId, string userName)
        {
            var command = new UploadAppealFileCommand
            {
                ApplicationId = applicationId,
                File = file,
                UserId = userId,
                UserName = userName
            };

            await _applyApiClient.UploadAppealFile(command);
        }
    }
}
