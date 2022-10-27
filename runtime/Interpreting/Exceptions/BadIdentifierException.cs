namespace Runtime.Interpreting.Exceptions;


[Serializable]
public class BadIdentifierException : Exception
{
    public BadIdentifierException() { }
    public BadIdentifierException(string message) : base(message) { }
    public BadIdentifierException(string message, Exception inner) : base(message, inner) { }
    protected BadIdentifierException(
   System.Runtime.Serialization.SerializationInfo info,
   System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}