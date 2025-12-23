using BeerCompetition.BFF.ApiGateway.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace BeerCompetition.BFF.ApiGateway.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoCorrelationIdInRequest_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue();
        context.Request.Headers.ContainsKey("X-Correlation-ID").Should().BeTrue();
        var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }
    
    [Fact]
    public async Task InvokeAsync_ExistingCorrelationIdInRequest_PreservesIt()
    {
        // Arrange
        var existingCorrelationId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = existingCorrelationId;
        
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new CorrelationIdMiddleware(next);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        nextCalled.Should().BeTrue();
        context.Request.Headers["X-Correlation-ID"].ToString().Should().Be(existingCorrelationId);
    }
}
