using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Command không trả về giá trị (chỉ trả Result thành công/thất bại).
/// Dùng cho: Create, Delete, Update không cần trả data.
/// </summary>
public interface ICommand : IRequest<Result> { }

/// <summary>
/// Command trả về giá trị trong Result.
/// Dùng cho: Register → trả AuthResponse, Create → trả Id...
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
