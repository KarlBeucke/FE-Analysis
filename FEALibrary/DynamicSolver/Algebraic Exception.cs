using System;
using System.Runtime.Serialization;

namespace FEALibrary.DynamicSolver
{
    [Serializable]
    public class AlgebraicException : Exception
    {
        public AlgebraicException(string message)
            : base(string.Format("Error in DynamicSolver " + message))
        {
        }

        public AlgebraicException()
        {
        }

        public AlgebraicException(string message, Exception innerException)
            : base(string.Format(message), innerException)
        {
        }

        protected AlgebraicException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}