using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    public static class Helper
    {
        static public void DebugWriteline(IList<ISqlStatement> sqlStatements)
        {
            int lineNum = 0;
            System.Diagnostics.Debug.Flush();
            foreach (var item in sqlStatements)
            {
                lineNum++;
                DebugWriteline(item, lineNum);
            }
        }

        static public void DebugWriteline(ISqlStatement sqlStatement, int lineNum)
        {
            if (sqlStatement is SqlSelectStatement)
            {
                System.Diagnostics.Debug.WriteLine("------------");
                System.Diagnostics.Debug.WriteLine("SELECT PARTS");
                System.Diagnostics.Debug.WriteLine("------------");
                var selectItem = (SqlSelectStatement)sqlStatement;
                var selectPart = selectItem.SelectPart.Value;
                {
                    lineNum++;
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}.]{1}\n", lineNum, selectPart));
                }

                System.Diagnostics.Debug.WriteLine("----------");
                System.Diagnostics.Debug.WriteLine("FROM PARTS");
                System.Diagnostics.Debug.WriteLine("----------");
                var fromPart = selectItem.FromPart.Value;
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}.]{1}\n", lineNum, fromPart));
                }

                System.Diagnostics.Debug.WriteLine("-----------");
                System.Diagnostics.Debug.WriteLine("WHERE PARTS");
                System.Diagnostics.Debug.WriteLine("-----------");
                var wherePart = selectItem.WherePart.Value;
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}.]{1}\n", lineNum, wherePart));
                }

                System.Diagnostics.Debug.WriteLine("-------------");
                System.Diagnostics.Debug.WriteLine("ORDER BY PART");
                System.Diagnostics.Debug.WriteLine("-------------");
                System.Diagnostics.Debug.WriteLine(string.Format("{0}.]{1}\n", lineNum, selectItem.OrderByPart));
            }
        }
    }
}
