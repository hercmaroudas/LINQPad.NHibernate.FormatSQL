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
                nhibernateFormatter.SqlIdentifiers = new string[] { "NHibernate.SQL:" };

                IList<ISqlStatement> sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL1);
                var sqlStatement = sqlStatements[0];
                var sqlSelectStatement = sqlStatement as SqlSelectStatement;
                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 3454);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 48);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 3061);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 6);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 30);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 29);

                sqlStatement = sqlStatements[1];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 454);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 11);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 361);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 23);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 21);

                sqlStatement = sqlStatements[2];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 2136);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 29);
                Assert.IsTrue(sqlStatement.Parameters.Count == 4);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 1121);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 595);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 121);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL3);
                sqlStatement = sqlStatements[3];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 366);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 4);
                Assert.IsTrue(sqlStatement.Parameters.Count == 2);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 93);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 32);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 48);

                sqlStatement = sqlStatements[5];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 378);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 4);
                Assert.IsTrue(sqlStatement.Parameters.Count == 2);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 93);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 32);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 48);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL5);
                sqlStatement = sqlStatements[7];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1634);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 8);
                Assert.IsTrue(sqlStatement.Parameters.Count == 0);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 1016);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 5);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 325);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 65);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL6);
                sqlStatement = sqlStatements[8];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1811);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 27);
                Assert.IsTrue(sqlStatement.Parameters.Count == 6);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 931);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 270);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 187);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL7);
                sqlStatement = sqlStatements[9];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 885);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 2);
                Assert.IsTrue(sqlStatement.Parameters.Count == 56);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 45);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 33);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 426);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL8);
                sqlStatement = sqlStatements[10];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 282);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 26);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 33);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 46);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL9);
                sqlStatement = sqlStatements[11];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 735);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 17);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 588);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 27);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 37);

                sqlStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL10);
                sqlStatement = sqlStatements[12];
                sqlSelectStatement = sqlStatement as SqlSelectStatement;
                appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 156);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatement.SelectPart.Value.Length == 27);
                Assert.IsTrue(sqlSelectStatement.SelectPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.FromPart.Value.Length == 27);
                Assert.IsTrue(sqlSelectStatement.FromPart.SqlStatements.Count == 0);
                Assert.IsTrue(sqlSelectStatement.WherePart.Value.Length == 43);

                IList<ISqlStatement> sqlSelectStatements = nhibernateFormatter.GetSqlFromDebugOutput(Properties.SecureResources.SQL3);
                foreach (var statement in sqlSelectStatements)
                {
                    string sql = statement.ApplySuggestedFormat();
                    #if DEBUG
                    Debug.WriteLine(sql);
                    #endif
                }
            }
            catch
            {
                throw;
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
            catch
            {
                throw;
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }

        [TestMethod]
        public void ApplySuggestedFormatAliasQuotedValuesTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();
                
                nhibernateFormatter.SqlIdentifiers = new string[] { Environment.NewLine };
                sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.ColumnNameAliasQuotedValues);
                Assert.IsTrue(sqlStatement.Parameters.Count == 2);

                string appliedParameterSql = sqlStatement.ApplyParameters();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 1375);
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
        public void ApplySuggestedFormatNoColumnTableNameAliasTest()
        {
            try
            {
                NHibernateSqlOutputFormatter nhibernateFormatter = new NHibernateSqlOutputFormatter();

                nhibernateFormatter.SqlIdentifiers = new string[] { Environment.NewLine };
                sqlStatement = nhibernateFormatter.TryParsSql(SecureResources.NoColumnOrTableNameAliases);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);

                string appliedParameterSql = sqlStatement.ApplyParameters();
                Assert.IsFalse(appliedParameterSql.Contains("@"));

                string appliedFormatSql = sqlStatement.ApplySuggestedFormat();
                Assert.IsTrue(appliedFormatSql.Length == 195);
            }
            catch
            {
                throw;
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }
        }
    }
}
