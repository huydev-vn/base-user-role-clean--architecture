using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Handler cho IQuery{TResponse}.
/// </summary>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
