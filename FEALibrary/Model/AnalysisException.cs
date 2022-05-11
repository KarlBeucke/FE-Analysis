using System;
using System.Runtime.Serialization;

namespace FEALibrary.Model
{
    public class AnalysisException : Exception
    {
        public AnalysisException(string message)
            : base(string.Format("Analysis: Fehler in der Berechnung " + message))
        {
        }

        public AnalysisException()
        {
        }

        public AnalysisException(string message, Exception innerException)
            : base(string.Format(message), innerException)
        {
        }

        protected AnalysisException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}