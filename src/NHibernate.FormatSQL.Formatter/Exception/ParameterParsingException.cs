using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.FormatSQL.Formatter
{
    [Serializable]
    public class ParameterParsingException : ParsingException
    {
        public string ParameterSection { get; set; }

        public ParameterParsingException() : base() { }

        public ParameterParsingException(string message) : base(message) { }

        public ParameterParsingException(string message, string actualSql, string parameterSection) : base(message) { ActualSql = actualSql; ParameterSection = parameterSection; }

        public ParameterParsingException(string message, Exception innerException) : base(message, innerException) { }

        public ParameterParsingException(string message, string actualSql, string parameterSection, Exception innerException) : base(message, innerException) { ActualSql = actualSql; ParameterSection = parameterSection; }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
