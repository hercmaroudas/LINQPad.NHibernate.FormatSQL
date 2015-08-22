using System.Diagnostics;

namespace NHibernate.FormatSQL.Formatter
{
    public class SqlUpdateStatement : SqlStatement
    {
        public override ISqlStatement Parse()
        {
            SetParameterKeyValuePairs();
            return this;
        }
    }


}
