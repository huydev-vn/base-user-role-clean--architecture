namespace Application.Exceptions;

/// <summary>
/// Ném khi user không có quyền thực hiện hành động — Controller bắt và trả 403.
/// </summary>
public class ForbiddenException(string message = "You do not have permission to perform this action.")
    : Exception(message);
