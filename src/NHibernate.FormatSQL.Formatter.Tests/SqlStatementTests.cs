using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.FormatSQL.Formatter.Tests.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    [TestClass]
    public class SqlStatementTests
    {
        private ISqlStatement sqlStatement;

        [TestMethod]
        public void ApplyParametersTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
                sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.SQL6);
                Assert.IsTrue(sqlStatement.Parameters.Count == 6);

                string appliedParameterSql = sqlStatement.ApplyParameters();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1811);
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void ApplyParametersComplexTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
                sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.SQL14);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);

                string appliedParameterSql = sqlStatement.ApplyParameters();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 3454);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL7);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL8);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsFalse(appliedParameterSql.Contains("@"));
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void ApplyParametersSqlColesceSelectTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
                nhibernateFormatter.SqlIdentifiers = new string[] { "NHibernate.SQL:" };
                sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.ColesceSelectSql);
                Assert.IsTrue(sqlStatement.Parameters.Count == 3);

                string appliedParameterSql = sqlStatement.ApplyParameters();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 371);
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void ApplySuggestedFormatTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
                nhibernateFormatter.SqlIdentifiers = new string[] { "NHibernate.SQL:", Environment.NewLine };

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL1);
                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 3304);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL3);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 347);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL5);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1634);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL6);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1811);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL7);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 885);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL8);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 282);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL9);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 735);

                sqlStatement = nhibernateFormatter.TryParsSql(Properties.SecureResources.SQL10);
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 156);

                IList<ISqlStatement> sqlSelectStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL3);
                foreach (var statement in sqlSelectStatements)
                {
                    string sql = statement.ApplySuggestedFormat();
                    #if DEBUG
                    Debug.WriteLine(sql);
                    #endif
                }
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void ApplySuggestedFormatSqlStartIdentifierTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();

                nhibernateFormatter.SqlIdentifiers = new string[] { "NHibernate.SQL:", Environment.NewLine };
                IList<ISqlStatement> sqlSelectStatements = nhibernateFormatter.GetSqlFromDebugOutput(SecureResources.SQL3);
                Assert.IsTrue(sqlSelectStatements.Count == 4);

                foreach (var sqlStatement in sqlSelectStatements)
                {
                    string sql = sqlStatement.ApplySuggestedFormat();
                    #if DEBUG
                    Debug.WriteLine(sql);
                    #endif
                }
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void SqlWhereExistsTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();

                nhibernateFormatter.SqlIdentifiers = new string[] { "NHibernate.SQL:" };
                IList<ISqlStatement> sqlSelectStatements = nhibernateFormatter.GetSqlFromDebugOutput(SecureResources.SQLWhereExists);
                Assert.IsTrue(sqlSelectStatements.Count == 4);

                foreach (var sqlStatement in sqlSelectStatements)
                {
                    string sql = sqlStatement.ApplySuggestedFormat();
                    #if DEBUG
                    Debug.WriteLine(sql);
                    #endif
                }
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }
    }
}
