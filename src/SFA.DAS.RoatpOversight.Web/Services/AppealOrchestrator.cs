using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.ApiTypes;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Services
{
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

        public async Task<AppealViewModel> GetAppealViewModel(AppealRequest request, string message)
        {
            var stagedUploads = await _applyApiClient.GetStagedUploads(new GetStagedFilesRequest {ApplicationId = request.ApplicationId});

            var result = new AppealViewModel
            {
                ApplicationId = request.ApplicationId,
                AllowAdditionalUploads = stagedUploads.Files.Count < 4,
                UploadedFiles = stagedUploads.Files.Select(x => new UploadedFileViewModel{Id = x.Id, Filename = x.Filename}).ToList(),
                Message = message
            };

            return result;
        }

        public async Task RemoveAppealFile(Guid applicationId, Guid fileId, string userId, string userName)
        {
            var command = new RemoveAppealFileCommand
            {
                ApplicationId = applicationId,
                FileId = fileId,
                UserId = userId,
                UserName = userName
            };

            await _applyApiClient.RemoveAppealFile(command);
        }
    }
}