namespace Runtime.Parser.Errors;

public class DError: Exception
{
    public DError()
        : base() 
    {}

    public new string Message { get; set; } = null!;
    public DErrorCode Code { get; set; }
}