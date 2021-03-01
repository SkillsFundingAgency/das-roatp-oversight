using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public interface IMultipartFormDataService
    {
        MultipartFormDataContent CreateMultipartFormDataContent(object request);
    }

    public class MultipartFormDataService : IMultipartFormDataService
    {
        public MultipartFormDataContent CreateMultipartFormDataContent(object request)
        {
            var result = new MultipartFormDataContent();

            var targetType = request.GetType();

            foreach (var property in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propertyType = property.PropertyType;
                var isClass = propertyType.IsClass;
                var isString = propertyType == typeof(string);
                var isEnumerable = propertyType.GetInterface(nameof(IEnumerable)) != null;
                var isAbstract = propertyType.IsAbstract;

                if (propertyType == typeof(IFormFile))
                {
                    var file = (IFormFile)property.GetValue(request);
                    var fileContent = new StreamContent(file.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = file.Length, ContentType = new MediaTypeHeaderValue(file.ContentType)
                        }
                    };
                    result.Add(fileContent, property.Name, file.FileName);
                }

                if (isString || (!isClass && !isEnumerable && !isAbstract))
                {
                    result.Add(new StringContent(Convert.ToString(property.GetValue(request))), property.Name);
                }
            }

            return result;
        }
    }
}
