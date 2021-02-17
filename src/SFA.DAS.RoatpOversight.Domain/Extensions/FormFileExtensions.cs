﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.RoatpOversight.Domain.Extensions
{
    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static async Task<FileUpload> ToFileUpload(this IFormFile formFile)
        {
            return new FileUpload
            {
                FileName = formFile.FileName,
                Data = await formFile.GetBytes(),
                ContentType = formFile.ContentType
            };
        }
    }
}
