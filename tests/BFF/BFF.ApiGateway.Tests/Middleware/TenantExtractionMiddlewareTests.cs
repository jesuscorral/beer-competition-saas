using BeerCompetition.BFF.ApiGateway.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Xunit;

namespace BeerCompetition.BFF.ApiGateway.Tests.Middleware;

public class TenantExtractionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AuthenticatedUserWithTenantId_InjectsHeaders()
    {
        // Arrange
        var tenantId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        
        var claims = new[]
        {
            new Claim("tenant_id", tenantId),
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new TenantExtractionMiddleware(next, NullLogger<TenantExtractionMiddleware>.Instance);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue();
        context.Request.Headers["X-Tenant-ID"].ToString().Should().Be(tenantId);
        context.Request.Headers["X-User-ID"].ToString().Should().Be(userId);
    }
    
    [Fact]
    public async Task InvokeAsync_AuthenticatedUserWithoutTenantId_Returns403()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var claims = new[]
        {
            new Claim("sub", userId)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new TenantExtractionMiddleware(next, NullLogger<TenantExtractionMiddleware>.Instance);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
    
    [Fact]
    public async Task InvokeAsync_UnauthenticatedUser_PassesThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new TenantExtractionMiddleware(next, NullLogger<TenantExtractionMiddleware>.Instance);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("X-Tenant-ID").Should().BeFalse();
    }
}
