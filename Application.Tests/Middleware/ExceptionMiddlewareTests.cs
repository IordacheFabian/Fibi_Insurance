using System.Text.Json;
using API.Middleware;
using Application.Core;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldReturn404ForNotFoundException()
    {
        var context = BuildContext();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _ => throw new NotFoundException("Policy not found"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var body = await ReadBodyAsync(context);
        body.Should().Contain("Policy not found");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturn400ForValidationException()
    {
        var context = BuildContext();
        var middleware = CreateMiddleware();

        var failures = new[]
        {
            new ValidationFailure("ClientId", "ClientId is required")
        };

        await middleware.InvokeAsync(context, _ => throw new ValidationException(failures));

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var body = await ReadBodyAsync(context);
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("errors")
            .GetProperty("ClientId")[0]
            .GetString()
            .Should().Be("ClientId is required");
    }

    private static ExceptionMiddleware CreateMiddleware() =>
        new ExceptionMiddleware(NullLogger<ExceptionMiddleware>.Instance, new TestHostEnvironment());

    private static DefaultHttpContext BuildContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}