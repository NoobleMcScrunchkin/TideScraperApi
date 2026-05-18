namespace TideScraper.Api.Models;

public class Result<T, TError> where TError : System.Enum
{
    public T? Value;
 
    public TError Error;
    
    public string? ErrorMessage;
    
    public bool IsSuccess => this.Value != null;
    
    public static Result<T, TError> Success(T value) => new() {Value = value};
    
    public static Result<T, TError> Failure(TError error, string? message = null) => new() {Error = error, ErrorMessage = message};
}