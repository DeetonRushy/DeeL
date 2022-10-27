namespace Runtime.Interpreting.Exceptions;


[Serializable]
public class BadArgumentsException : Exception
{
    public BadArgumentsException() { }
    public BadArgumentsException(string message) : base(message) { }
    public BadArgumentsException(string message, Exception inner) : base(message, inner) { }
    protected BadArgumentsException(
   System.Runtime.Serialization.SerializationInfo info,
   System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}