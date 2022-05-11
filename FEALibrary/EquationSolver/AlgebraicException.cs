using System;
using System.Runtime.Serialization;

namespace FEALibrary.EquationSolver
{
    [Serializable]
    public class AlgebraicException : Exception
    {
        public AlgebraicException(string message)
            : base(string.Format("Error in EquationSolver " + message))
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