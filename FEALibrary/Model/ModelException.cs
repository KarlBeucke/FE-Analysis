using System;
using System.Runtime.Serialization;

namespace FEALibrary.Model
{
    [Serializable]
    public class ModelException : Exception
    {
        public ModelException(string message)
            : base(string.Format("Fehler in Modelldaten " + message))
        {
        }

        public ModelException()
        {
        }

        public ModelException(string message, Exception innerException)
            : base(string.Format(message), innerException)
        {
        }

        protected ModelException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}