namespace Runtime.Interpreting.Exceptions
{
    [Serializable]
    public class AssignmentException : Exception
    {
        public AssignmentException() { }
        public AssignmentException(string message) : base(message) { }
        public AssignmentException(string message, Exception inner) : base(message, inner) { }
        protected AssignmentException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
