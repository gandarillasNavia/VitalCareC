namespace MSProveedor.Dominio.Validadores;

public class Result<T> : IResult<T>
{
    public bool Success { get; }
    public string Message { get; }
    public T? Data { get; }

    private Result(bool success, string message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public static Result<T> Exito(T data, string message = "")
    {
        return new Result<T>(true, message, data);
    }

    public static Result<T> Falla(string message)
    {
        return new Result<T>(false, message, default);
    }
}