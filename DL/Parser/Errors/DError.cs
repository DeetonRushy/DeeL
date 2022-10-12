namespace DL.Parser.Errors;

public class DError: Exception
{
    public DError()
        : base() 
    {
        Message = $"DL{(int)Code} ({Code}): ";
    }

    public new string Message { get; set; }
    public DErrorCode Code { get; set; }
}