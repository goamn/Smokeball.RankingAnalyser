using Moq;
using Moq.Protected;
using Smokeball.RankingAnalyser.WpfApp.Core.Services;
using System.Net;
using Xunit;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Tests.xUnit;

public class SearchRequestSenderServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly SearchRequestService _service;

    public SearchRequestSenderServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Fake response")
            });

        var client = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

        _service = new SearchRequestService(_mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task SendSearchRequest_ReturnsValidResponse()
    {
        var keywords = "test";

        var result = await _service.SendSearchRequest(keywords);

        Assert.NotNull(result);
        Assert.Equal("Fake response", result);
    }

    [Fact]
    public async Task SendSearchRequest_ThrowsExceptionOnHttpClientError()
    {
        var keywords = "test";
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() =>
            {
                var mockHandler = new Mock<HttpMessageHandler>();
                mockHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ThrowsAsync(new HttpRequestException("Network issue"));

                return new HttpClient(mockHandler.Object);
            });

        await Assert.ThrowsAsync<Exception>(() => _service.SendSearchRequest(keywords));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SendSearchRequest_HandlesEmptyOrNullOrWhitespaceKeywords(string keywords)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SendSearchRequest(keywords));
    }
}
