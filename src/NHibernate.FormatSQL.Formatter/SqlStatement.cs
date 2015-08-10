using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NHibernate.FormatSQL.Formatter
{
    public interface ISqlStatement
    {
        string Sql { get; set; }
        List<SqlColumnNameAliases> ColumnNames { get; }
        List<SqlTableNameAliases> TableNames { get; }
        List<SqlParamKeyValuePair> Parameters { get; }
        ISqlStatement Parse();
        string ApplyParameters();
        string ApplySuggestedFormat();
        ParsingException SqlStatementParsingException { get; set; }
    }

    public abstract class SqlStatement : ISqlStatement
    {
        private string sql;
        public string Sql { get { return sql; } set { sql = string.IsNullOrWhiteSpace(value) ? value : value.Trim(); } }

        private List<SqlColumnNameAliases> columnNames;
        public List<SqlColumnNameAliases> ColumnNames { get { return columnNames ?? (columnNames = new List<SqlColumnNameAliases>()); } }

        private List<SqlTableNameAliases> tableNames;
        public List<SqlTableNameAliases> TableNames { get { return tableNames ?? (tableNames = new List<SqlTableNameAliases>()); } }

        List<SqlParamKeyValuePair> parameters;
        public List<SqlParamKeyValuePair> Parameters { get { return parameters ?? (parameters = new List<SqlParamKeyValuePair>()); } }

        public ParsingException SqlStatementParsingException { get; set; }

        internal SqlStatementFactory sqlStatementFactory = new SqlStatementFactory();

        public abstract ISqlStatement Parse();

        // ( attempts to set the parameter and values in an organised list of key value pairs )
        internal virtual void SetParameterKeyValuePairs()
        {
            int paramStartIndex = 0;
            string sqlSection = string.Empty;
            string parameterSection = string.Empty;
            
            try
            {
                // ( parameters are found after the first found semi colon (;) )
                paramStartIndex = Sql.IndexOf(";") + 1;
                parameterSection = Sql.Substring(paramStartIndex, Sql.Length - paramStartIndex).Trim();
                if (!string.IsNullOrWhiteSpace(parameterSection))
                {
                    sqlSection = Sql.Substring(0, paramStartIndex + 1).Replace(";", string.Empty).Trim();
                    string[] parameters = parameterSection.Split(new char[] { '=', ',' });

                    if (parameters.Length > 1)
                    {
                        // ( parse parameters )
                        for (int ix = 0; ix < parameters.Length; ix += 2)
                        {
                            string key = parameters[ix].Trim();
                            if ((ix + 1) < parameters.Length)
                            {
                                string dataType = string.Empty;
                                // ( get the value of the parameter )
                                string value = parameters[ix + 1].Split('[')[0].Replace(";", string.Empty).Trim();

                                // ( covers parameter values where string values contain pipes (|) )
                                if (value.Contains("|"))
                                {
                                    // ( continue looping until no comma found )
                                    while (ix + 1 < parameters.Length - 1)
                                    {
                                        ix++;
                                        value = value + "," + parameters[ix + 1];
                                    }
                                }

                                // ( signals that a parameter datatype has been found )
                                if (parameters[ix + 1].Contains("["))
                                {
                                    dataType = parameters[ix + 1].Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new string[] { "Type:", "(" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                                }

                                // ( signals that a parameter key has been found )
                                if (key.StartsWith("@"))
                                {
                                    Parameters.Add(new SqlParamKeyValuePair() { Key = key, Value = value, DataType = dataType });
                                }
                                else
                                    break; // ( were not interested in anything after this for this statement)
                            }
                            else 
                                break; // ( were not interested in anything after this for this statement)
                        }
                    }
                    else
                    {
                        // ( abnormal parameter length )
                        SqlStatementParsingException = new ParameterParsingException("Exception parsing parameters.", sqlSection, parameterSection);
                    }
                }
            }
            catch (Exception exception)
            {
                var sqlStatementParsingException = new ParameterParsingException("Unhandled exception parsing parameters", sqlSection, parameterSection, exception);
                SqlStatementParsingException = sqlStatementParsingException;
                throw sqlStatementParsingException;
            }
        }

        /// <summary>
        /// Parse and retrieve sql with parameter and values applied
        /// </summary>
        /// <returns>
        /// Parsed sql with parameter and values applied.
        /// </returns>
        public string ApplyParameters()
        {
            string replaceValue = string.Empty;
            string sql = Sql.Substring(0, Sql.IndexOf(";") + 1);

            foreach (var parameter in Parameters)
            {
                switch (parameter.DataType.ToLower())
                {
                    case "string":
                    case "ansistring":
                        replaceValue = parameter.Value;
                        break;
                    case "boolean":
                        replaceValue = string.Format("'{0}'", parameter.Value);
                        break;
                    case "datetime":
                        replaceValue = string.Format("'{0}'", Convert.ToDateTime(parameter.Value).ToString("MM/dd/yyyy hh:mm:ss"));
                        break;
                    case "int16":
                    case "int32":
                    case "int64":
                        replaceValue = parameter.Value;
                        break;
                    default:
                        // try parse dates that dont have a data type.
                        if (parameter.Value.Contains("/"))
                        {
                            replaceValue = string.Format("'{0}'", Convert.ToDateTime(parameter.Value.Replace("'", string.Empty)).ToString("MM/dd/yyyy hh:mm:ss"));
                        }
                        else
                        {
                            replaceValue = parameter.Value;
                        }
                        break;
                }

                var isMatchCount = Regex.Match(sql, @"\@p0\b");
                string search = string.Format(@"\{0}\b", parameter.Key);
                sql = Regex.Replace(sql, search, replaceValue);
            }
            return sql.ToString();
        }

        /// <summary>
        /// Parse and retrieve sql with parameter and values applied as well as with suggested column and table name aliases. 
        /// </summary>
        /// <returns>
        /// Formatted sql with parameter and values applied as well as with suggested column and table name aliases. 
        /// </returns>
        public string ApplySuggestedFormat()
        {
            // ( get applied parametered sql )
            string search = string.Empty;
            string sql = ApplyParameters();

            // ( replace all table names with proposed table names )
            foreach (var table in TableNames)
            {
                search = string.Format(@"\b{0}\b", table.OriginalTableAliasName);
                sql = Regex.Replace(sql, search, table.ProposedTableAliasName);
            }

            // ( replace all column names with proposed column names )
            foreach (var column in ColumnNames)
            {
                search = string.Format(@"\b{0}\b", column.OriginalAliasName);
                sql = Regex.Replace(sql, search, column.ProposedAliasName);
            }

            // ( naughty but we i cant be arsed to find duplicate ";" right now and maybe forever :) ) 
            return sql.Replace(";", string.Empty).EnsureLastCharacterExists(';');
        }

        public override string ToString() { return Sql; }
    }
}
