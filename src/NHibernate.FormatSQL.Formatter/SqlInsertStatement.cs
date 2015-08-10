using System.Diagnostics;

namespace NHibernate.FormatSQL.Formatter
{
    public class SqlInsertStatement : SqlStatement
    {
        public override ISqlStatement Parse()
        {
            Debug.Write("SqlInsertStatement.Parse(): Currently only parsing SqlSelectStatement's are supported.");
            return new SqlInsertStatement();
        }
    }
}
