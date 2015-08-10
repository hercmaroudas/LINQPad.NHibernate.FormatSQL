using System.Diagnostics;

namespace NHibernate.FormatSQL.Formatter
{
    public class SqlUpdateStatement : SqlStatement
    {
        public override ISqlStatement Parse()
        {
            Debug.Write("SqlUpdateStatement.Parse(): Currently only parsing SqlSelectStatement's are supported.");
            return new SqlUpdateStatement();
        }
    }
}
