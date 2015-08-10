
using System.Collections;
using System.Collections.Generic;
namespace NHibernate.FormatSQL.Formatter
{
    public struct SqlParamKeyValuePair
    {
        public string Key { get;  internal set; }
        public string Value { get; set; }
        public string DataType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}={1}", Key, Value);
        }
    }

    public struct SqlColumnNameAliases
    {
        public string ActualColumnName { get; internal set; }
        public string OriginalAliasName { get; internal set; }
        public string ProposedAliasName { get; set; }

        public override string ToString()
        {
            return string.Format("ActualColumnName:{0} ProposedAliasName:{1}", ActualColumnName, ProposedAliasName);
        }
    }

    public struct SqlTableNameAliases
    {
        public string ActualTableName { get; internal set; }
        public string OriginalTableAliasName { get; internal set; }
        public string ProposedTableAliasName { get; set; }

        public override string ToString()
        {
            return string.Format("ActualTableName:{0} ProposedTableAliasName:{1}", ActualTableName, ProposedTableAliasName);
        }
    }

    public struct SqlSelectPart
    {
        IList<ISqlStatement> sqlStatements;
        /// <summary>
        /// Select parts can contain select statements known as inline column select statements.
        /// </summary>
        public IList<ISqlStatement> SqlStatements
        {
            get { return sqlStatements; }
            internal set { sqlStatements = value; }
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
        
        public SqlSelectPart(string value) : this()
        {
            Value = value;
            sqlStatements = new List<ISqlStatement>();
        }
    }
    
    public struct SqlFromPart
    {
        IList<ISqlStatement> sqlStatements;
        /// <summary>
        /// From parts can contain select statements known as inline table select statements.
        /// </summary>
        public IList<ISqlStatement> SqlStatements
        {
            get { return sqlStatements; }
            internal set { sqlStatements = value; }
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public SqlFromPart(string value)
            : this()
        {
            Value = value;
            sqlStatements = new List<ISqlStatement>();
        }
    }

    public struct SqlWherePart
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
