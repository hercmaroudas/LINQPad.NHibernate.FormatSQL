using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace NHibernate.FormatSQL.Formatter
{
    public interface ISqlStatementFactory
    {
        bool IsSql(string value, out string possibleSql);
    }

    /// <summary>
    /// A class used 
    /// </summary>
    public class SqlStatementFactory : ISqlStatementFactory
    {
        // ( collection of names that already exist )
        private Dictionary<string, int> _names;

        // ( sql key words for sql statements that can be returned )
        List<string> keyWords = new List<string>(new string[] { "select", "update", "insert" });

        /// <summary>
        /// Creates a new instance of NHibernate.FormatSQL.Formatter.SqlStatementFactory.
        /// </summary>
        public SqlStatementFactory()
        {
            _names = new Dictionary<string, int>();
        }

        /// <summary>
        /// Attempts to obtain a valid sql statement from the value passed in and create from it an NHibernate.FormatSQL.Formatter.ISqlStatement.
        /// </summary>
        /// <param name="value">
        /// The value to parse.
        /// </param>
        /// <returns>
        /// NHibernate.FormatSQL.Formatter.ISqlStatement that can be of types:
        /// NHibernate.FormatSQL.Formatter.SqlSelectStatement
        /// NHibernate.FormatSQL.Formatter.SqlUpdateStatement
        /// NHibernate.FormatSQL.Formatter.SqlInsertStatement
        /// </returns>
        internal ISqlStatement TryGetSqlStatementType(string value)
        {
            var trimedValue = value.ToLower().Trim().TrimStart('(').TrimEnd(')');
            if (trimedValue.StartsWith("select".ToLower()))
            {
                return new SqlSelectStatement() { Sql = value.MakeEqualSpace() };
            }
            else if (trimedValue.StartsWith("update".ToLower()))
            {
                return new SqlUpdateStatement() { Sql = value.MakeEqualSpace() };
            }
            else if (trimedValue.StartsWith("insert".ToLower()))
            {
                return new SqlInsertStatement() { Sql = value.MakeEqualSpace() };
            }
            else
                throw new Exception("Sql type could not be determined.");
        }

        /// <summary>
        /// Determines if the value passed in is a valid sql statement.
        /// </summary>
        /// <param name="value">
        /// The value to check and parse.
        /// </param>
        /// <param name="possibleSql">
        /// The sql that is outputted if value is valid sql.
        /// </param>
        /// <returns>
        /// True if the value is a valid sql statement.
        /// </returns>
        public bool IsSql(string value, out string possibleSql)
        {
            try
            {
                var startIndex = 0;
                var keywordIndexer = new List<KeyWordIndexer>();

                // ( replace all carriage return, newlines and spaces with an empty string ) 
                possibleSql = Regex.Replace(value, @"\r\n|\n$|^\s+|\s+$", " ");

                foreach (var keyword in keyWords)
                {
                    if (value.Trim().Length > keyword.Length)
                    {
                        string search = string.Format(@"\b{0}\b", keyword);
                        var match = Regex.Match(possibleSql, search, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            keywordIndexer.Add(new KeyWordIndexer() { MatchIndex = match.Index, KeyWord = keyword });
                        }
                    }
                }

                if (keywordIndexer.Count > 0)
                {
                    // ( Get the first found keyword in the string )
                    var item = keywordIndexer.OrderBy(k => k.MatchIndex).FirstOrDefault();
                    startIndex = item.MatchIndex;

                    string nextChar = possibleSql.Substring(item.MatchIndex + item.KeyWord.Length, 1);
                    if (string.IsNullOrWhiteSpace(nextChar))
                    {
                        possibleSql = possibleSql.Substring(startIndex, possibleSql.Length - startIndex).Trim().EnsureLastCharacterExists(';');
                        return true;
                    }
                }
                
                possibleSql = string.Empty;
            }
            catch 
            {    
                throw;
            }
            return false;
        }


        // need to add sql to collection and sort to get the first occurrence of the sql keyword type. 
        private struct KeyWordIndexer
        {
            public int MatchIndex { get; set; }
            public string KeyWord { get; set; }
        }

        /// <summary>
        /// Determines if the value passed in is a valid sql statement and if it contains a from part.
        /// </summary>
        /// <param name="value">
        /// The value to parse.
        /// </param>
        /// <param name="possibleSql">
        /// The sql that is outputted if value is valid sql.
        /// </param>
        /// <returns>
        /// True if the select statement contains a from clause.
        /// </returns>
        public bool IsSqlSelectWithFrom(string value, out string possibleSql)
        {
            bool isSql = IsSql(value, out possibleSql);
            var search = @"\bfrom\b";
            var match = Regex.Match(possibleSql, search, RegexOptions.IgnoreCase);
            return (isSql && match.Length > 0);
        }

        /// <summary>
        /// Gets all values between equal number of spaces and adds them to a generic list.
        /// </summary>
        /// <param name="value">
        /// The value from which to get the values from.
        /// </param>
        /// <param name="includeOrphans">
        /// If true the value after the last found brace is returned in the result. 
        /// </param>
        /// <returns>
        /// IList<string> of values obtained between braces. 
        /// </returns>
        public IList<string> GetValuesBetweenEvenBraces(string value, bool includeOrphans = false)
        {
            int level = 0;
            List<string> values = new List<string>();
            StringBuilder builder = new StringBuilder();
            try {
                string input = value;
                string sql = string.Empty;
                for (int ix = 0; ix < value.Length; ix++)
                {
                    var current = value[ix];

                    level = current == '(' ? level += 1 : level;
                    level = current == ')' ? level -= 1 : level;
                    if (level > 0 || current == ')')
                        builder.Append(current);

                    if (level == 0 && (current == '(' || current == ')'))
                    {
                        if (includeOrphans)
                        {
                            string currentstring = value.Substring(ix + 1);
                            for (int iy = 0; iy < currentstring.Length; iy++)
                            {
                                current = currentstring[iy];
                                if (current == '(')
                                    break;

                                ix++;
                                builder.Append(current);
                            }
                        }

                        values.Add(builder.ToString());
                        builder = new StringBuilder();
                    }
                }
            }
            catch {
                throw;
            }

            return values;
        }

        /// <summary>
        /// Attempts to get possible sql column aliases where column names can be simple names or be complex sql select statements.
        /// </summary>
        /// <param name="value">
        /// The sql statement select clause part.
        /// </param>
        /// <returns>
        /// IList<string> of columns along with their aliases. E.g ( column1 as col1 ) or ( (select column1 from table1 where x = y) as col1 )
        /// </returns>
        public IList<string> GetPossibleColumnNameAliases(string value)
        {
            int level = 0;
            List<string> values = new List<string>();
            StringBuilder builder = new StringBuilder();

            for (int ix = 0; ix < value.Length; ix++)
            {
                char current = value[ix];
                char next = value.Length > ix + 1 ? value[ix + 1] : ' ';

                level = current == '(' ? level += 1 : level;
                level = current == ')' ? level -= 1 : level;
                
                if (current == ',' && level == 0)
                {
                    values.Add(builder.ToString());
                    builder = new StringBuilder();
                }
                else
                {
                    if (current == '(')
                    {
                        string statement = string.Empty;
                        string searchString = value.Substring(ix);
                        IList<string> sqlStatements = GetValuesBetweenEvenBraces(searchString);
                        if (sqlStatements.Count > 0)
                        {
                            statement = sqlStatements[0];
                            if (statement.Contains("select"))
                            {
                                StringBuilder wordBuilder = new StringBuilder();
                                for (int iy = 0; iy < searchString.Length; iy++)
                                {
                                    if (searchString[iy] != ' ')
                                    {
                                        wordBuilder.Append(searchString[iy]);
                                        if (wordBuilder.ToString().ToLower().Contains("select"))
                                        {
                                            sqlStatements = GetValuesBetweenEvenBraces(searchString, true);
                                            if (sqlStatements.Count > 0)
                                            {
                                                level = 0;
                                                statement = sqlStatements[0].InnerValueAfterLastChar(')', ',');
                                                ix = ix + statement.Length;
                                                builder.Append(statement);
                                                values.Add(builder.ToString());
                                                builder = new StringBuilder();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                builder.Append(current);
                            }
                        }
                    }
                    else 
                    {
                        builder.Append(current);
                    }
                }
            }
            if (builder.Length > 0) values.Add(builder.ToString());
            return values;
        }

        /// <summary>
        /// Ensures that a value passed is contained within the even number of open and closed braces. If not the value is inserted between even braces.
        /// </summary>
        /// <param name="value">
        /// The value to parse. 
        /// </param>
        /// <returns>
        /// The value between the even number of braces. 
        /// </returns>
        public string EnsureValueIsBetweenEvenBraces(string value)
        {
            string returnValue = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return string.Empty;

                var openBraceMatches = Regex.Matches(value, @"\(");
                var closeBraceMatches = Regex.Matches(value, @"\)");

                if (openBraceMatches.Count == closeBraceMatches.Count)
                    return value;
                
                int closeBraceLastIndex = 0;
                if (closeBraceMatches.Count > 0)
                {
                    closeBraceLastIndex = closeBraceMatches[closeBraceMatches.Count - 1].Index;
                    returnValue = value.Substring(0, closeBraceLastIndex);
                }
            }
            catch {    
                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// Ensures that the value is inserted between open and close braces.
        /// </summary>
        /// <param name="value">
        /// The value to parse. 
        /// </param>
        /// <returns>
        /// The value between an open and close brace.
        /// </returns>
        public string EnsureValueStartBraceEndsWithEvenBrace(string value)
        {
            string returnValue = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return string.Empty;

                var openBraceMatches = Regex.Matches(value, @"\(");
                var closeBraceMatches = Regex.Matches(value, @"\)");

                if (openBraceMatches.Count == closeBraceMatches.Count)
                    return value;

                if (value.Substring(0, 1) != "(")
                    return value;

                returnValue = string.Format("{0}{1}", value, ")");
            }
            catch {
                throw;
            }

            return returnValue;
        }

        /// <summary>
        /// Determines if a Sql keyword alias (as) is between braces.
        /// </summary>
        /// <param name="value">
        /// The statement containing the (as) keyword.
        /// </param>
        public bool IsAsBetweenBraces(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                var search = @"\bas\b";
                var asMatches = Regex.Matches(value, search, RegexOptions.IgnoreCase);
                if (asMatches.Count <= 0)
                    return false;

                var closeBraceMatches = Regex.Matches(value, @"\)");
                if (closeBraceMatches.Count <= 0)
                    return false;

                return (asMatches[asMatches.Count - 1].Index < closeBraceMatches[closeBraceMatches.Count - 1].Index);
                
            }
            catch
            {
                throw;
            }
            
        }

        /// <summary>
        /// Gets a unique name for the value passed in. If not unique a unique index is appended.
        /// </summary>
        /// <param name="name">
        /// The value to ensure is unique.
        /// </param>
        /// <returns>
        /// A name that doesn't already internally exist.
        /// </returns>
        internal string GetUniqueName(string name)
        {
            name = name.Trim().FirstValueBetweenChars('[', ']');
            string newName = string.Empty;

            // ( check if key already exists )
            if (_names.ContainsKey(name))
            {
                // ( Add 1 to the current existing index for new name )
                var index = _names[name];
                // ( Update existing index of new name )
                _names[name] = index + 1;
                newName = string.Format("{0}_{1}", name, index + 1);
            }
            else
            {
                _names.Add(name, 0);
                newName = string.Format("{0}_{1}", name, 0);
            }
            return newName;
        }
    }
}
