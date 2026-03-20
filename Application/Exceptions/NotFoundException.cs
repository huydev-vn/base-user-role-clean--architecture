namespace Application.Exceptions;

/// <summary>
/// Ném khi không tìm thấy resource — Controller bắt và trả 404.
/// </summary>
public class NotFoundException(string name, object key)
    : Exception($"'{name}' with key '{key}' was not found.");
