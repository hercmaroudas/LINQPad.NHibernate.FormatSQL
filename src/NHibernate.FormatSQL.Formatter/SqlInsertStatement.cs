using System.Diagnostics;

namespace NHibernate.FormatSQL.Formatter
{
    public class SqlInsertStatement : SqlStatement
    {
        public override ISqlStatement Parse()
        {
            SetParameterKeyValuePairs();
            return this;
        }
    }
}
