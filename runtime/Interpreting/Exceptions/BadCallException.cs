using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runtime.Interpreting.Exceptions
{

    [Serializable]
    public class BadCallException : Exception
    {
        public BadCallException() { }
        public BadCallException(string message) : base(message) { }
        public BadCallException(string message, Exception inner) : base(message, inner) { }
        protected BadCallException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
