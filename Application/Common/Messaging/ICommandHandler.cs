using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Handler cho ICommand (không trả giá trị).
/// </summary>
public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

/// <summary>
/// Handler cho ICommand{TResponse}.
/// </summary>
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }
