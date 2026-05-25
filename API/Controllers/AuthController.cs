using Application.DTOs.Auth;
using Application.Features.Auth.Login;
using Application.Features.Auth.Logout;
using Application.Features.Auth.RefreshToken;
using Application.Features.Auth.Register;
using Application.Features.Auth.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/auth")]
public sealed class AuthController(ISender sender) : BaseController
{
    /// <summary>
    /// Đăng ký tài khoản mới. Trả về thông tin user và hướng dẫn xác thực email.
    /// Token chỉ được cấp sau khi verify email và đăng nhập.
    /// </summary>
    /// <response code="201">Đăng ký thành công — kiểm tra email để xác thực.</response>
    /// <response code="400">Validation thất bại.</response>
    /// <response code="409">Username hoặc email đã tồn tại.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Username,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ConfirmPassword);

        var result = await sender.Send(command, cancellationToken);
        return ToCreatedResult(result);
    }

    /// <summary>
    /// Đăng nhập bằng username + password. Yêu cầu email đã được xác thực.
    /// </summary>
    /// <response code="200">Đăng nhập thành công, trả về JWT token.</response>
    /// <response code="400">Validation thất bại.</response>
    /// <response code="401">Username hoặc mật khẩu không đúng / tài khoản bị khóa.</response>
    /// <response code="403">Tài khoản bị cấm, bị vô hiệu hóa hoặc chưa xác thực email.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Username, request.Password),
            cancellationToken);

        return OkOrError(result);
    }

    /// <summary>
    /// Cấp mới access token bằng refresh token còn hạn (rotation — refresh token cũ bị thu hồi).
    /// </summary>
    /// <response code="200">Token mới được cấp.</response>
    /// <response code="401">Refresh token không hợp lệ hoặc đã hết hạn.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return OkOrError(result);
    }

    /// <summary>
    /// Đăng xuất — thu hồi refresh token hiện tại.
    /// </summary>
    /// <response code="204">Đăng xuất thành công.</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogoutCommand(), cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Xác thực email qua token nhận trong email.
    /// </summary>
    /// <response code="204">Xác thực thành công, tài khoản đã Active.</response>
    /// <response code="400">Token không hợp lệ hoặc đã dùng.</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new VerifyEmailCommand(request.Token),
            cancellationToken);

        return ToActionResult(result);
    }
}

public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record VerifyEmailRequest(string Token);
