using System.Net;
using System.Net.Http.Json;
using BeerCompetition.BFF.ApiGateway.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BeerCompetition.BFF.ApiGateway.Tests.Services;

/// <summary>
/// Unit tests for KeycloakTokenExchangeService.
/// Tests OAuth 2.0 Token Exchange (RFC 8693) implementation.
/// </summary>
public class TokenExchangeServiceTests
{
    private readonly IHttpClientFactory _mockHttpClientFactory;
    private readonly ILogger<KeycloakTokenExchangeService> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly string _testTokenEndpoint = "http://localhost:8080/realms/beercomp/protocol/openid-connect/token";
    private readonly string _testClientId = "bff-api";
    private readonly string _testClientSecret = "test-secret";

    public TokenExchangeServiceTests()
    {
        _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        _mockLogger = Substitute.For<ILogger<KeycloakTokenExchangeService>>();
        
        // Setup test configuration
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:TokenEndpoint"] = _testTokenEndpoint,
            ["Keycloak:ClientId"] = _testClientId,
            ["Keycloak:ClientSecret"] = _testClientSecret
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ValidRequest_ReturnsAccessToken()
    {
        // Arrange
        var subjectToken = "valid-subject-token";
        var targetAudience = "competition-service";
        var expectedAccessToken = "exchanged-access-token";
        
        var tokenResponse = new
        {
            access_token = expectedAccessToken,
            token_type = "Bearer",
            expires_in = 900
        };

        var mockHandler = new MockHttpMessageHandler(
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(tokenResponse)
            });

        var httpClient = new HttpClient(mockHandler);
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act
        var result = await service.ExchangeTokenAsync(subjectToken, targetAudience);

        // Assert
        result.Should().Be(expectedAccessToken);
        mockHandler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task ExchangeTokenAsync_EmptySubjectToken_ThrowsArgumentException()
    {
        // Arrange
        var targetAudience = "competition-service";
        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExchangeTokenAsync("", targetAudience));
    }

    [Fact]
    public async Task ExchangeTokenAsync_EmptyTargetAudience_ThrowsArgumentException()
    {
        // Arrange
        var subjectToken = "valid-subject-token";
        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.ExchangeTokenAsync(subjectToken, ""));
    }

    [Fact]
    public async Task ExchangeTokenAsync_KeycloakReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        var subjectToken = "invalid-token";
        var targetAudience = "competition-service";
        
        var errorResponse = new
        {
            error = "invalid_grant",
            error_description = "Invalid token"
        };

        var mockHandler = new MockHttpMessageHandler(
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = JsonContent.Create(errorResponse)
            });

        var httpClient = new HttpClient(mockHandler);
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await service.ExchangeTokenAsync(subjectToken, targetAudience));
    }

    [Fact]
    public async Task ExchangeTokenAsync_ResponseMissingAccessToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var subjectToken = "valid-subject-token";
        var targetAudience = "competition-service";
        
        var invalidResponse = new
        {
            token_type = "Bearer",
            expires_in = 900
            // Missing access_token
        };

        var mockHandler = new MockHttpMessageHandler(
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(invalidResponse)
            });

        var httpClient = new HttpClient(mockHandler);
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.ExchangeTokenAsync(subjectToken, targetAudience));
    }

    [Fact]
    public async Task ExchangeTokenAsync_NetworkError_ThrowsHttpRequestException()
    {
        // Arrange
        var subjectToken = "valid-subject-token";
        var targetAudience = "competition-service";

        var mockHandler = new MockHttpMessageHandler(
            new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler);
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await service.ExchangeTokenAsync(subjectToken, targetAudience));
    }

    [Fact]
    public async Task ExchangeTokenAsync_ValidRequest_SendsCorrectRFC8693Parameters()
    {
        // Arrange
        var subjectToken = "valid-subject-token";
        var targetAudience = "competition-service";
        var expectedAccessToken = "exchanged-access-token";
        
        var tokenResponse = new
        {
            access_token = expectedAccessToken,
            token_type = "Bearer",
            expires_in = 900
        };

        var mockHandler = new MockHttpMessageHandler(
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(tokenResponse)
            });

        var httpClient = new HttpClient(mockHandler);
        _mockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var service = new KeycloakTokenExchangeService(
            _mockHttpClientFactory,
            _configuration,
            _mockLogger);

        // Act
        await service.ExchangeTokenAsync(subjectToken, targetAudience);

        // Assert
        mockHandler.LastRequest.Should().NotBeNull();
        mockHandler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        mockHandler.LastRequest.RequestUri.Should().Be(_testTokenEndpoint);
        
        var content = mockHandler.LastRequestContent;
        content.Should().NotBeNull();
        content.Should().Contain("grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Atoken-exchange");
        content.Should().Contain($"client_id={_testClientId}");
        content.Should().Contain($"subject_token={subjectToken}");
        content.Should().Contain($"audience={targetAudience}");
        content.Should().Contain("subject_token_type=urn%3Aietf%3Aparams%3Aoauth%3Atoken-type%3Aaccess_token");
        content.Should().Contain("requested_token_type=urn%3Aietf%3Aparams%3Aoauth%3Atoken-type%3Aaccess_token");
    }

    [Fact]
    public void Constructor_MissingTokenEndpoint_ThrowsInvalidOperationException()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            // Missing TokenEndpoint
            ["Keycloak:ClientId"] = _testClientId,
            ["Keycloak:ClientSecret"] = _testClientSecret
        };
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new KeycloakTokenExchangeService(
                _mockHttpClientFactory,
                invalidConfig,
                _mockLogger));
    }

    [Fact]
    public void Constructor_MissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:TokenEndpoint"] = _testTokenEndpoint,
            // Missing ClientId
            ["Keycloak:ClientSecret"] = _testClientSecret
        };
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new KeycloakTokenExchangeService(
                _mockHttpClientFactory,
                invalidConfig,
                _mockLogger));
    }

    [Fact]
    public void Constructor_MissingClientSecret_ThrowsInvalidOperationException()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:TokenEndpoint"] = _testTokenEndpoint,
            ["Keycloak:ClientId"] = _testClientId
            // Missing ClientSecret
        };
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new KeycloakTokenExchangeService(
                _mockHttpClientFactory,
                invalidConfig,
                _mockLogger));
    }

    /// <summary>
    /// Mock HttpMessageHandler for testing HttpClient.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _response;
        private readonly Exception? _exception;
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestContent { get; private set; }
        public int RequestCount { get; private set; }

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public MockHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            RequestCount++;

            // Capture request content before it gets disposed
            if (request.Content != null)
            {
                LastRequestContent = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            if (_exception != null)
            {
                throw _exception;
            }

            return await Task.FromResult(_response!);
        }
    }
}
