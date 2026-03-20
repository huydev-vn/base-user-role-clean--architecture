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

/// <summary>
/// Authentication endpoints — không cần [Authorize] trên class.
/// Chỉ /logout yêu cầu đăng nhập.
/// </summary>
[Route("api/auth")]
public sealed class AuthController(ISender sender) : BaseController
{
    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    /// <response code="201">Đăng ký thành công, trả về JWT token.</response>
    /// <response code="400">Validation thất bại (thiếu field, password yếu...).</response>
    /// <response code="409">Username hoặc email đã tồn tại.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
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
        return result.IsSuccess
            ? StatusCode(201, result.Value)
            : Conflict(new { error = result.Error });
    }

    /// <summary>
    /// Đăng nhập bằng username + password.
    /// </summary>
    /// <response code="200">Đăng nhập thành công, trả về JWT token.</response>
    /// <response code="400">Validation thất bại.</response>
    /// <response code="401">Username hoặc mật khẩu không đúng / tài khoản bị khóa.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Username, request.Password),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }

    /// <summary>
    /// Cấp mới access token bằng refresh token còn hạn.
    /// </summary>
    /// <response code="200">Token mới được cấp (refresh token cũng được rotate).</response>
    /// <response code="401">Refresh token không hợp lệ hoặc đã hết hạn.</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
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
    [ProducesResponseType(400)]
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

// ── Inline request records (body đơn giản không cần file riêng) ──────────
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record VerifyEmailRequest(string Token);
