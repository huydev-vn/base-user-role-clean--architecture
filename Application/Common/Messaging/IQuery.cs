using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Query luôn trả về giá trị — không bao giờ thay đổi state.
/// Dùng cho: GetById, GetAll, Search...
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
