using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace NHibernate.FormatSQL.Formatter
{
    /// <summary>
    /// Class used to obtain NHibernate Sql from Visual Studio output window and obtain NHibernate.FormatSQL.Formatter.ISqlStatement objects that can be used to format obscured table and column names.
    /// </summary>
    public class NHibernateSqlOutputFormatter
    {
        private SqlStatementFactory sqlStatementFactory;

        private IList<ISqlStatement> sqlStatements;
        /// <summary>
        /// Gets a list of IList<ISqlStatement> objects generated when invoking GetSqlFromDebugOutput.
        /// </summary>
        public IList<ISqlStatement> SqlStatements { get { return sqlStatements ?? (sqlStatements = new List<ISqlStatement>()) ;} }

        /// <summary>
        /// Identifiers used to find and split the input used when invoking method GetSqlFromDebugOutput.
        /// </summary>
        public string[] SqlIdentifiers { get; set; }

        /// <summary>
        /// Filters used when determining Sql statements. Sql statements with these filters will be ignored. 
        /// </summary>
        public string[] IgnoreFilters { get; set; }

        /// <summary>
        /// Attempts to obtain Nhibernate sql statements from the Visual Studios debug output window and create a list of NHibernate.FormatSQL.Formatter.ISqlStatement objects.
        /// </summary>
        /// <param name="input">Viaual Studio debug output.</param>
        /// <returns>
        /// NHibernate.FormatSQL.Formatter.ISqlStatement objects that can be used to perform table and column name changes. 
        /// </returns>
        public IList<ISqlStatement> GetSqlFromDebugOutput(string input)
        {
            string output = string.Empty;
            try
            {
                string[] splitInput = input.Split(SqlIdentifiers, StringSplitOptions.RemoveEmptyEntries);
                foreach (var possibleSqlStatement in splitInput)
                {
                    string sql = string.Empty;
                    bool isSql = sqlStatementFactory.IsSql(possibleSqlStatement, out sql);
                    if (isSql)
                    {
                        var ignoreFilterCount = IgnoreFilters.Count(f => 
                        {
                            return possibleSqlStatement.Contains(f);
                        });
                        if (ignoreFilterCount <= 0)
                        {
                            ISqlStatement sqlStatement = sqlStatementFactory.TryGetSqlStatementType(sql).Parse();
                            SqlStatements.Add(sqlStatement);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return SqlStatements;
        }

        /// <summary>
        /// Attempts to obtain a sql statement and create a NHibernate.FormatSQL.Formatter.ISqlStatement object.
        /// </summary>
        /// <param name="value">
        /// A sql statement.
        /// </param>
        /// <returns>
        /// NHibernate.FormatSQL.Formatter.ISqlStatement that can be used to perform table and column name changes. 
        /// </returns>
        public ISqlStatement TryParsSql(string value)
        {
            string sql = string.Empty;
            try
            {
                bool isSql = sqlStatementFactory.IsSql(value, out sql);
                if (isSql)
                {
                    ISqlStatement sqlStatement = sqlStatementFactory.TryGetSqlStatementType(sql).Parse();
                    SqlStatements.Add(sqlStatement);
                    return sqlStatement;
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        /// <summary>
        /// Attempts to obtain sql statements between braces and create a list of NHibernate.FormatSQL.Formatter.ISqlStatement objects from it.
        /// </summary>
        /// <param name="input">
        /// A sql statement between braces.
        /// </param>
        /// <returns>
        /// NHibernate.FormatSQL.Formatter.ISqlStatement objects that can be used to perform table and column name changes. 
        /// </returns>
        internal IList<ISqlStatement> GetSqlFromInlineSelect(string input)
        {
            string output = string.Empty;
            string cleanedInput = input.Trim().TrimStart('(').TrimEnd(')');
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();
                ISqlStatement sqlStatement;
                sqlStatements = new List<ISqlStatement>();

                IList<string> potentialSqlValues = sqlStatementFactory.GetValuesBetweenEvenBraces(sqlStatementFactory.EnsureValueStartBraceEndsWithEvenBrace(input));
                foreach (var value in potentialSqlValues)
                {
                    // ( double check SQL before attempting to parse again )
                    string sql = string.Empty;
                    bool isSql = sqlStatementFactory.IsSql(value, out sql);
                    if (isSql)
                    {
                        string inlineSql = sqlStatementFactory.EnsureValueIsBetweenEvenBraces(sql).EnsureLastCharacterExists(';');
                        sqlStatement = sqlStatementFactory.TryGetSqlStatementType(inlineSql).Parse();
                        SqlStatements.Add(sqlStatement);
                    }
                }
            }
            catch
            {
                throw;
            }

            return SqlStatements;
        }

        /// <summary>
        /// Creates a new instance of NHibernate.FormatSQL.Formatter.NHibernateSqlOutputFormatter.
        /// </summary>
        public NHibernateSqlOutputFormatter()
        {
            sqlStatementFactory = new SqlStatementFactory();
            SqlIdentifiers = new string[] { };
            IgnoreFilters = new string[] { };
        }
    }
}
