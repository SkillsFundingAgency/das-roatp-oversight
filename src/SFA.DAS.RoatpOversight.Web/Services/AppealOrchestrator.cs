using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private const int MaxFileUploads = 4;

        public AppealOrchestrator(IApplyApiClient applyApiClient)
        {
            _applyApiClient = applyApiClient;
        }

        public async Task UploadAppealFile(Guid applicationId, IFormFile file, string userId, string userName)
        {
            var request = new UploadAppealFileRequest
            {
                ApplicationId = applicationId,
                UserId = userId,
                UserName = userName,
                File = file
            };

            await _applyApiClient.UploadAppealFile(applicationId, request);
        }

        public async Task<AppealViewModel> GetAppealViewModel(AppealRequest request, string message)
        {
            var oversightReview = await _applyApiClient.GetOversightDetails(request.ApplicationId);
            var stagedUploads = await _applyApiClient.GetStagedUploads(new GetStagedFilesRequest {ApplicationId = request.ApplicationId});

            var result = new AppealViewModel
            {
                ApplicationId = request.ApplicationId,
                OversightReviewId = oversightReview.OversightReviewId.Value,
                AllowAdditionalUploads = stagedUploads.Files.Count < MaxFileUploads,
                UploadedFiles = stagedUploads.Files.Select(x => new UploadedFileViewModel{Id = x.Id, Filename = x.Filename}).ToList(),
                Message = message
            };

            return result;
        }

        public async Task RemoveAppealFile(Guid applicationId, Guid fileId, string userId, string userName)
        {
            var command = new RemoveAppealFileCommand
            {
                UserId = userId,
                UserName = userName
            };

            await _applyApiClient.RemoveAppealFile(applicationId, fileId, command);
        }

        public async Task CreateAppeal(Guid applicationId, Guid oversightReviewId, string message, string userId, string userName)
        {
            var request = new CreateAppealRequest
            {
                Message = message,
                UserId = userId,
                UserName = userName
            };

            await _applyApiClient.CreateAppeal(applicationId, oversightReviewId, request);
        }

        public async Task<FileUpload> GetAppealFile(Guid applicationId, Guid appealId, Guid fileId)
        {
            var result = await _applyApiClient.GetAppealFile(applicationId, appealId, fileId);

            return new FileUpload
            {
                FileName = result.Filename,
                ContentType = result.ContentType,
                Data = result.Content
            };
        }
    }
}