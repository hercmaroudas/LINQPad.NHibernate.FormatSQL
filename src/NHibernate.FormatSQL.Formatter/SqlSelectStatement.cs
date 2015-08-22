using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernate.FormatSQL.Formatter
{
    public class SqlSelectStatement : SqlStatement
    {
        // ( pointer to the last found index where the from clause begins )
        private int _lastFromIndex = 0;
        // ( pointer to the last found index where the where clause begins )
        private int _lastWhereIndex = 0;

        // ( helper to create unique table and column names )
        private SqlStatementFactory uniqueNameFactory;

        // ( the select part of the current sql statement ( these parts can in turn contain nested select parts because select statements can have select statements as columns ) 
        public SqlSelectPart SelectPart { get; internal set; }

        // ( the from part of the current sql statement ( these parts can in turn contain nested select parts because from statements can have select statements as table names ) 
        public SqlFromPart FromPart { get; internal set; }

        // ( the where part of the current sql statement )
        public SqlWherePart WherePart { get; internal set; }

        // ( the order part of the current sql statement )
        public string OrderByPart { get; set; }

        // ( constructor )
        public SqlSelectStatement()
        {
            uniqueNameFactory = new SqlStatementFactory();
        }

        // ( get all the parts of the current sql statement string )
        public override ISqlStatement Parse()
        {
            try
            {
                if (IsValidateSql())
                {
                    SetParameterKeyValuePairs();
                    SetSelectParts();
                    SetFromParts();
                    SetOtherParts();
                    SetTableNameAlias(FromPart.Value);
                    SetColumnNameAlias(SelectPart.Value);
                }
            }
            catch
            {
                throw;
            }
            return this;
        }
        
        // ( get the sql select part ( unsupported currently UNIONS or nested SQL Statements such as a Union (does this even ever happen in NHibernate? ) ) 
        private void SetSelectParts()
        {
            List<string> words = new List<string>();
            StringBuilder builder = new StringBuilder();
            try
            {
                for (int ix = 0; ix < Sql.Length; ix++)
                {
                    var current = Sql[ix];
                    var previous = ix > 0 ? Sql[ix - 1] : ' ';
                    var next = ix + 1 < Sql.Length ? Sql[ix + 1]  : ' ';
                    
                    builder.Append(current);
                    if (current == ' ')
                    {
                        var cleaned = builder.ToString().SplitAndKeep(new string[] {"(", ")", "[", "]" });
                        if (cleaned.Length > 0)
                        {
                            for (int iy = 0; iy < cleaned.Length; iy++)
                            {
                                words.Add(cleaned[iy].Trim().ToLower());
                            }
                        }
                        else { words.Add(builder.ToString().Trim().ToLower()); }
                        
                        builder = new StringBuilder();
                    }

                    if (words.Count > 0 && words[words.Count - 1] == "from" && words.Count(p => p == "(") == words.Count(p => p == ")"))
                    {
                        if (Sql.Length > (ix + "select".Length))
                        {
                            if (char.IsLetter(next) || Sql.Substring(ix + 1, "select".Length).ToLower() == "select")
                            {
                                _lastFromIndex = ix - ("from".Length + 1);
                                break;
                            }
                        }
                    }
                }

                int length = _lastFromIndex <= 0 ? "select".Length + 1 : _lastFromIndex - ("select".Length + 1);
                string select = Sql.Substring("select".Length + 1, length).Trim();

                // ( attempt to get the inline select statement ( this is select statements used as column names ) )
                IList<ISqlStatement> sqlSelectStatements = new NHibernateSqlOutputFormatter().GetSqlFromInlineSelect(select);

                // ( set the select part of current sql statement and all sql statement column names )
                SelectPart = new SqlSelectPart() { Value = select, SqlStatements = sqlSelectStatements };
            }
            catch
            {
                throw;
            }
        }

        // ( get the sql from part from the last index of the from clause found )
        private void SetFromParts()
        {
            try
            {
                var indexOfLastWhere = 0;
                var fromPart = string.Empty;

                int fromindex = Regex.Matches(Sql.Substring(_lastFromIndex), @"\bfrom\b", RegexOptions.IgnoreCase).Count;
                if (fromindex <= 0)
                    return;

                var fromStatement = Sql.Substring(_lastFromIndex, Sql.Length - _lastFromIndex);
                var whereMatches = Regex.Matches(fromStatement, "where", RegexOptions.IgnoreCase);

                // ( no from clause )
                if (whereMatches.Count == 0)
                {
                    fromPart = Sql.Substring(_lastFromIndex + "from".Length + 1, fromStatement.Length - ("from".Length + 1));
                }
                // ( simple from clause )
                else
                {
                    indexOfLastWhere = whereMatches[whereMatches.Count - 1].Index + (Sql.Length - fromStatement.Length);

                    fromPart = Sql.Substring(_lastFromIndex + "from".Length + 1, (indexOfLastWhere - _lastFromIndex) - ("from".Length + 1)).Trim();
                }

                // ( parameter still exists )
                if (fromPart.Count(e => e == ';') > 1)
                {
                    fromPart = fromPart.Substring(0, fromPart.IndexOf(';') +  1);
                }

                // ( attempt to get the inline select statement ( this is select statements used as table names ) )
                IList<ISqlStatement> sqlSelectStatements = new NHibernateSqlOutputFormatter().GetSqlFromInlineSelect(fromPart);

                // ( set the from part of current sql statement and all sql statement table names )
                FromPart = new SqlFromPart() { Value = fromPart, SqlStatements = sqlSelectStatements };

                // ( set the last index of the where clause found )
                _lastWhereIndex = indexOfLastWhere;
            }
            catch 
            {
                throw;
            }
        }

        // ( unsupported: (to add group by, having clause etc.. possibly) )
        private void SetOtherParts()
        {
            string wherePart = string.Empty;

            var whereIndex = Regex.Matches(Sql.Substring(_lastWhereIndex), @"\bwhere\b", RegexOptions.IgnoreCase).Count;
            var orderbyMatches = Regex.Matches(Sql.Substring(_lastWhereIndex), @"\border by\b", RegexOptions.IgnoreCase);
            
            var indexOfOrderBy = orderbyMatches.Count > 0 ? (orderbyMatches[orderbyMatches.Count - 1].Index + _lastWhereIndex): 0;
            var indexOfSemiColon = Sql.EnsureLastCharacterExists(';').IndexOf(";", _lastWhereIndex);

            // ( order by exists )
            if (indexOfOrderBy > 0)
            {
                // ( +1 at end if wish to include the semi colon )
                OrderByPart = Sql.Substring(indexOfOrderBy + "order by".Length + 1, (indexOfSemiColon - indexOfOrderBy) - ("order by".Length + 1));
                if (whereIndex > 0)
                {
                    wherePart = Sql.Substring(_lastWhereIndex + "where".Length + 1, (indexOfOrderBy - _lastWhereIndex) - ("where".Length + 1));
                }
            }
            else
            {
                if (whereIndex > 0)
                {
                    // ( +1 at end if wish to include the semi colon )
                    wherePart = sqlStatementFactory.EnsureValueIsBetweenEvenBraces(Sql.Substring(_lastWhereIndex + "where".Length + 1, (indexOfSemiColon - _lastWhereIndex) - ("where".Length + 1)));
                }
            }

            WherePart = new SqlWherePart() { Value = wherePart };
        }

        // ( attempt to obtain column name aliases ( keeping in mind columns can also be sql statements )
        private void SetColumnNameAlias(string selectPart)
        {
            int inlineSelectIndex = -1;
            string columnName = string.Empty;

            string aliasName = string.Empty;
            string poroposedAliasName = string.Empty;

            // ( selectPart were looking at seeing something like: table1.column1 as column1a, table1.column2 as column1b, etc...)
            string[] columnNameAliases = sqlStatementFactory.GetPossibleColumnNameAliases(selectPart).ToArray();
            for (int ix = 0; ix < columnNameAliases.Length; ix++)
            {
                // ( columnNameAlias were looking at something like: table1.column1 as column1a )
                string columnNameAlias = columnNameAliases[ix];

                string[] tableColumnAliasName = new string[] { columnNameAlias };
                bool asBetweenBraces = sqlStatementFactory.IsAsBetweenBraces(columnNameAlias);
                if (!asBetweenBraces)
                {
                    tableColumnAliasName = columnNameAlias.SplitAndJoin(new string[] { "as", " " });
                }

                // ( get column name aliases ) 
                if (tableColumnAliasName.Length == 1)
                {
                    // ( column alias does not exist )
                    columnName = tableColumnAliasName[tableColumnAliasName.GetLowerBound(0)].Trim();
                    ColumnNames.Add(new SqlColumnNameAliases() { ActualColumnName = columnName, OriginalAliasName = string.Empty, ProposedAliasName = string.Empty });
                }
                else if (tableColumnAliasName.Length > 0)
                {
                    aliasName = tableColumnAliasName[tableColumnAliasName.Length - 1].Trim();

                    var tableColumnName = tableColumnAliasName[0];
                    var originalTableColumnName = tableColumnName;
                    var tableColumnNameArray = new string[] { tableColumnName };
                    // ( usually sql functions in select clause (use function name as the column name alias) )
                    if (Regex.Match(tableColumnName, @"\(").Success)
                    {
                        tableColumnName = tableColumnNameArray[tableColumnNameArray.GetLowerBound(0)].Trim();
                    }
                    // ( usually table and column name separated by a period (.) )
                    else
                    {
                        tableColumnNameArray = tableColumnName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        tableColumnName = tableColumnNameArray[tableColumnNameArray.Length - 1].Trim();
                    }

                    // ( check for an inline SQL column name )
                    string sql = columnNameAlias.Trim().TrimStart('(').TrimEnd(')').Trim().EnsureLastCharacterExists(';');
                    bool isSql = sqlStatementFactory.IsSqlSelectWithFrom(sql, out sql);

                    // ( column name is an inline sql select statement )
                    if (isSql)
                    {
                        inlineSelectIndex++;
                        SqlSelectStatement inlineSqlStatement = SelectPart.SqlStatements[inlineSelectIndex] as SqlSelectStatement;

                        string[] fromSplit, whereSplit = null;
                        string fromColumnName, whereColumnName = string.Empty;

                        if (!string.IsNullOrWhiteSpace(inlineSqlStatement.FromPart.Value))
                        {
                            fromSplit = inlineSqlStatement.FromPart.Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            // ( get table name minus the period )
                            if (fromSplit.Length > 0)
                            {
                                fromColumnName = fromSplit[fromSplit.GetLowerBound(0)];
                                fromSplit = fromColumnName.Split('.');
                                if (fromSplit.Length > 0)
                                {
                                    fromColumnName = fromSplit[fromSplit.GetUpperBound(0)];
                                }

                                columnName = fromColumnName;
                            }
                        }

                        // ( get the first column name from the where clause )
                        if (!string.IsNullOrWhiteSpace(inlineSqlStatement.WherePart.Value))
                        {
                            if (Regex.Matches(inlineSqlStatement.WherePart.Value, @"\band\b", RegexOptions.IgnoreCase).Count > 0)
                            {
                                whereSplit = inlineSqlStatement.WherePart.Value.SplitAndJoin(new string[] { "and" });
                            }
                            else
                            {
                                whereSplit = inlineSqlStatement.WherePart.Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            }

                            // ( get the last column name from the where clause that doesn't contain braces )
                            for (int iy = whereSplit.Length - 1; iy >= 0; iy--)
                            {
                                whereColumnName = whereSplit[iy];
                                if (((whereColumnName.IndexOf("=") > 0 && char.IsLetter(whereColumnName.Trim(), 0)) || whereColumnName.IndexOf("(") < 0 && char.IsLetter(whereColumnName.Trim(), 0)))
                                {
                                    whereSplit = whereColumnName.Split('=');
                                    whereColumnName = whereSplit[whereSplit.GetLowerBound(0)];
                                    whereSplit = whereColumnName.Split('.');
                                    if (whereSplit.Length > 0)
                                    {
                                        whereColumnName = whereSplit[whereSplit.GetUpperBound(0)];
                                        break;
                                    }
                                }
                            }

                            columnName = string.IsNullOrWhiteSpace(columnName) ? whereColumnName : string.Format("{0}.{1}", columnName, whereColumnName);
                        }

                        poroposedAliasName = string.Format("'{0}'", uniqueNameFactory.GetUniqueName(columnName));
                        ColumnNames.Add(new SqlColumnNameAliases() { ActualColumnName = inlineSqlStatement.Sql, OriginalAliasName = aliasName, ProposedAliasName = poroposedAliasName });
                    }
                    else
                    {
                        // ( original column name to propose a column name alias )
                        columnName = tableColumnNameArray[tableColumnNameArray.Length - 1]
                            .Replace(")", string.Empty)
                            .Replace("(", string.Empty)
                            .Replace("*", string.Empty)
                            .Replace("'", string.Empty);

                        // ( propose new alias name (add space for before as column ) )
                        if (columnName.Trim().Contains(" ") || columnName.Trim().Contains("."))
                        {
                            // TODO: Fix column name aliases can not be larger than 128 characters long
                            poroposedAliasName = " " + string.Format("'{0}'", originalTableColumnName
                                .Replace("_", string.Empty)
                                .Replace("'", string.Empty));
                        }
                        else
                        {
                            poroposedAliasName = " " + uniqueNameFactory.GetUniqueName(columnName.Replace("_", string.Empty));
                        }

                        ColumnNames.Add(new SqlColumnNameAliases() { ActualColumnName = columnName, OriginalAliasName = aliasName, ProposedAliasName = poroposedAliasName });
                    }
                }
            }
        }

        // ( attempt to obtain table name aliases ( keeping in mind that tables can also be sql statements )
        private void SetTableNameAlias(string fromPart)
        {
            string actualTableName = string.Empty;
            string originalAliasName = string.Empty;
            string proposedAliasName = string.Empty;

            // ( complex inner sql selects as table names )
            if (this.FromPart.SqlStatements.Count > 0)
            {
                for (int ix = 0; ix < FromPart.SqlStatements.Count; ix++)
                {
                    var current = FromPart.SqlStatements[ix].Sql.ToLower().Trim(';');
                    string[] fromSections = current.Split(new string[] { "from" }, StringSplitOptions.RemoveEmptyEntries);
                    if (fromSections.Length > 0)
                    {
                        current = fromSections[fromSections.GetUpperBound(0)];
                        fromSections = current.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (fromSections.Length  > 0)
                        {
                            current = fromSections[fromSections.GetUpperBound(0)];
                        }
                    }

                    actualTableName = FromPart.Value;
                    originalAliasName = current;
                    proposedAliasName = uniqueNameFactory.GetUniqueName(current);
                    
                    TableNames.Add(new SqlTableNameAliases() { ActualTableName = actualTableName, OriginalTableAliasName = originalAliasName, ProposedTableAliasName = proposedAliasName });
                }
            }
            else
            {
                // ( split by word (own extension otherwise split function removes characters containing on and join ) )
                string[] fromSections = fromPart.ToLower().SplitAndJoin(new string[] { "on", "join" });
                for (int ix = 0; ix < fromSections.Length; ix++)
                {
                    string current = fromSections[ix];

                    if (!current.Contains("="))
                    {
                        string[] array = current.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length > 1)
                        {
                            actualTableName = array[0].Trim();
                            originalAliasName = array[1].Trim();

                            if (originalAliasName.Contains("@"))
                            {
                                // ( this is occurring because the statement is a stored procedure )
                                proposedAliasName = string.Empty;
                                originalAliasName = string.Join(" ", array, 1, array.Length - 1);
                            }
                            else
                            { 
                                string[] tableNameSplit = actualTableName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                proposedAliasName = tableNameSplit.Length > 1
                                    ? uniqueNameFactory.GetUniqueName(tableNameSplit[tableNameSplit.Length - 1])
                                    : uniqueNameFactory.GetUniqueName(tableNameSplit[0]);
                            }
                            TableNames.Add(new SqlTableNameAliases() { ActualTableName = actualTableName, OriginalTableAliasName = originalAliasName, ProposedTableAliasName = proposedAliasName });
                        }
                        // else error ? (not sure yet...)
                    }
                }
            }
        }

        // ( validates the sql before constructing its parts )
        private bool IsValidateSql()
        {
            var openbraceMatches = Regex.Matches(Sql, @"\(");
            var closebraceMatches = Regex.Matches(Sql, @"\)");
            if (openbraceMatches.Count > 0 && openbraceMatches.Count != closebraceMatches.Count)
            {
                SqlStatementParsingException = new ParsingException("There was an error parsing this SQL statement. (Enclosing braces do not match).", Sql);
            }
            var fromMatches = Regex.Matches(Sql, @"\bfrom\b", RegexOptions.IgnoreCase);
            if (fromMatches.Count <= 0)
            {
                SqlStatementParsingException = new ParsingException("The SQL select does not contain a 'From' clause.", Sql);

            }

            return (SqlStatementParsingException == null);
        }
    }
}
