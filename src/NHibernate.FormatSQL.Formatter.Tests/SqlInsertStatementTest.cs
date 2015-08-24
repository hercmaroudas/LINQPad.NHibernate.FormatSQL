using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.FormatSQL.Formatter.Tests.Properties;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    [TestClass]
    public class SqlInsertStatementTest
    {
        private ISqlStatement sqlStatement;

        [TestMethod]
        public void ParseTest()
        {
            NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
            sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.SQLInsert1);
            Assert.IsTrue(sqlStatement.Parameters.Count == 4);

            string appliedParameterSql = sqlStatement.ApplyParameters();
            Assert.IsFalse(appliedParameterSql.Contains("@"));

            string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
            Assert.IsTrue(appliedFormatSql.Length == 161);
        }
    }
}
