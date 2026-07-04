using Contracts.Auth;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthServiceHttpClient authServiceHttpClient;

    public AuthController(AuthServiceHttpClient authServiceHttpClient)
    {
        this.authServiceHttpClient = authServiceHttpClient;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        return await ToProxyResult(await authServiceHttpClient.RegisterAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return await ToProxyResult(await authServiceHttpClient.LoginAsync(request, cancellationToken));
    }

    private static async Task<IActionResult> ToProxyResult(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
