using System;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Infrastructure.ApiClients
{
    [TestFixture]
    public class RoatpOversightApiClientTests
    {
        private const string RoatpCourseManagementOuterApiBaseAddress = "http://localhost:5334";
        //private RoatpOversightApiClient _apiClient;

        // [SetUp]
        // public void Before_each_test()
        // {
        //     var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        //     handlerMock
        //         .Protected()
        //         .Setup<Task<HttpResponseMessage>>(
        //             "SendAsync",
        //             ItExpr.IsAny<HttpRequestMessage>(),
        //             ItExpr.IsAny<CancellationToken>()
        //         )
        //         .ReturnsAsync(new HttpResponseMessage()
        //         {
        //             StatusCode = HttpStatusCode.OK,
        //         //    Content = new StringContent("[{'ProviderCourseId':1,'CourseName':'Test','Level':1,'IsImported':'false'}]", Encoding.UTF8, "application/json"),
        //         })
        //         .Verifiable();
        //
        //     var httpClient = new HttpClient(handlerMock.Object)
        //     {
        //         BaseAddress = new Uri(RoatpCourseManagementOuterApiBaseAddress),
        //     };
        //     httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        //
        //
        //     var logger = new Mock<ILogger<ApiClient>>();
        //
        //     _apiClient = new RoatpOversightApiClient(httpClient, logger.Object);
        // }

        // [Test]
        // public async Task ApiClient_GetMethodRetrieves_listOfStandards()
        // {
        //     var request = new CreateRoatpV2ProviderRequest();
        //     var result = await _apiClient.CreateProvider(request);
        //     result.Should().BeTrue();
        // }
    }
}
