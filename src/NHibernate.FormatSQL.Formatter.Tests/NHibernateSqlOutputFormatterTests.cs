using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.FormatSQL.Formatter.Tests.Properties;
using System;
using System.Collections.Generic;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    [TestClass]
    public class NHibernateSqlOutputFormatterTests
    {
        private NHibernateSqlOutputFormatter context;

        [TestInitialize]
        public void Init()
        {
            context = new NHibernateSqlOutputFormatter();
            context.SqlIdentifiers = new string[] { "NHibernate.SQL:" };
        }

        [TestMethod]
        public void GetSqlFromDebugOutputTest()
        {
            try
            {
                IList<ISqlStatement> sqlSelectStatements = context.GetSqlFromDebugOutput(SecureResources.SQL2);
                Assert.IsTrue(sqlSelectStatements.Count == 154);

                Assert.IsTrue(sqlSelectStatements[0].TableNames.Count == 1);
                Assert.IsTrue(sqlSelectStatements[0].Parameters.Count == 1);
                Assert.IsTrue(sqlSelectStatements[0].ColumnNames.Count == 11);

                #if DEBUG 
                Helper.DebugWriteline(sqlSelectStatements);
                #endif
            }
            catch (Exception exception)
            {
                Assert.IsNotInstanceOfType(exception, typeof(Exception));
            }
            finally
            {
                SecureResources.ResourceManager.ReleaseAllResources();
            }

            try
            {
                IList<ISqlStatement> sqlSelectStatements = context.GetSqlFromDebugOutput(SecureResources.SQL3);
                Helper.DebugWriteline(sqlSelectStatements);
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
        public void TryParsSqlTest()
        {
            try
            {
                ISqlStatement sqlStatement = null;
                try
                {
                    sqlStatement = context.TryParsSql(SecureResources.SQL4);

                    #if DEBUG
                    Helper.DebugWriteline(sqlStatement, 1);
                    #endif
                    
                    Assert.IsTrue(sqlStatement.SqlStatementParsingException != null, "SqlStatementParsingException should exist");
                }
                catch (Exception exception)
                {
                    Assert.IsNotInstanceOfType(exception, typeof(Exception));
                }
                try
                {
                    sqlStatement = context.TryParsSql(SecureResources.SQL5);
                    
                    Assert.IsTrue(sqlStatement.TableNames.Count == 4);
                    Assert.IsTrue(sqlStatement.Parameters.Count == 0);
                    Assert.IsTrue(sqlStatement.ColumnNames.Count == 8);
                    
                    #if DEBUG
                    Helper.DebugWriteline(sqlStatement, 1);
                    #endif
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
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
        public void TryParsSqlComplexTest()
        {
            try
            {
                ISqlStatement sqlStatement = context.TryParsSql(SecureResources.SQL13);
                string sql = sqlStatement.ApplyParameters();
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 8);

                sqlStatement = context.TryParsSql(SecureResources.SQL12);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 0);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 2);

                sqlStatement = context.TryParsSql(SecureResources.SQL11);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 2);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 1);

                sqlStatement = context.TryParsSql(SecureResources.SQL10);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 1);

                sqlStatement = context.TryParsSql(SecureResources.SQL9);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 17);

                sqlStatement = context.TryParsSql(SecureResources.SQL7);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 56);
                Assert.IsTrue(sqlStatement.ColumnNames.Count == 2);

                sqlStatement = context.TryParsSql(SecureResources.SQL8);
                sql = sqlStatement.ApplySuggestedFormat();

                Assert.IsTrue(sqlStatement.TableNames.Count == 1);
                Assert.IsTrue(sqlStatement.Parameters.Count == 1);
                Assert.IsTrue(sqlStatement.ColumnNames.Count ==1);
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
