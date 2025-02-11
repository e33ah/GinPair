namespace GinPair.Models;

public class ApiResponse
{
    public ApiResponse()
    {
    }
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = string.Empty;
    public object? Data { get; set; }
    public BsColor BsColor { get; set; }
}