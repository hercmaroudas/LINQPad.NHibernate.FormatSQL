using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.FormatSQL.Formatter
{
    [Serializable]
    public class ParsingException : Exception
    {
        public string ActualSql { get; set; }

        public ParsingException() : base() { }

        public ParsingException(string message) : base(message) { }

        public ParsingException(string message, string actualSql) : base(message) { ActualSql = actualSql; }

        public ParsingException(string message, Exception innerException) : base(message, innerException) { }

        public ParsingException(string message, string actualSql, Exception innerException) : base(message, innerException) { ActualSql = actualSql; }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("ActualSql", ActualSql);
            base.GetObjectData(info, context);
        }
    }
}
