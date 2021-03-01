using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IAppealOrchestrator
    {
        Task UploadAppealFile(Guid applicationId, IFormFile file, string userId, string userName);
        Task<AppealViewModel> GetAppealViewModel(AppealRequest request, string message);
        Task RemoveAppealFile(Guid applicationId, Guid fileId, string userId, string userName);
    }
}
