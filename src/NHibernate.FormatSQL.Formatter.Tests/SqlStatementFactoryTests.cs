using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.FormatSQL.Formatter.Tests.Properties;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    [TestClass]
    public class SqlStatementFactoryTests
    {
        private SqlStatementFactory context;

        [TestInitialize]
        public void Init()
        {
            context = new SqlStatementFactory();
        }

        [TestMethod]
        public void GetValuesBetweenEvenBracesTest()
        {
            string orphan1 = string.Empty;
            string orphan2 = string.Empty;

            IList<string> values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces1);
            Assert.IsTrue(values.Count == 1);

            values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces1, true);
            orphan1 = values[0];
            Assert.IsTrue(orphan1.Contains("page"));

            values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces2);
            Assert.IsTrue(values.Count == 5);

            values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces3, true);
            Assert.IsTrue(values.Count == 5);
            orphan1 = values[0];
            Assert.IsTrue(orphan1.Contains("orphan1"));
            orphan2 = values[3];
            Assert.IsTrue(orphan2.Contains("orphan2"));

            values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces4, false);
            Assert.IsTrue(values.Count == 4);

            values = context.GetValuesBetweenEvenBraces(SecureResources.ValuesBetweenBraces4, true);
            Assert.IsTrue(values.Count == 4);
            orphan1 = values[0];
            Assert.IsTrue(orphan1.Contains("something2"));
            orphan2 = values[1];
            Assert.IsTrue(orphan2.Contains("something3"));
            orphan2 = values[2];
            Assert.IsTrue(orphan2.Contains("something4"));
            orphan2 = values[3];
            Assert.IsTrue(orphan2.Contains("something5"));
        }

        [TestMethod]
        public void EnsureValueIsBetweenEvenBraces()
        {
            // ( Note: most if not all of the functionality in SqlStatementFactory is used by other parts of the application that have test coverage )
        }

        [TestMethod]
        public void GetPossibleColumnNameAliases()
        {
            IList<string> values = context.GetPossibleColumnNameAliases(SecureResources.ColumnNameAliasValue1);
            Assert.IsTrue(values.Count == 7);

            values = context.GetPossibleColumnNameAliases(SecureResources.ColumnNameAliasValue2);
            Assert.IsTrue(values.Count == 9);

            values = context.GetPossibleColumnNameAliases(SecureResources.ColumnNameAliasValue3);
            Assert.IsTrue(values.Count == 7);

            string sql = Regex.Replace(SecureResources.ColumnNameAliasValue4, @"\r\n|\n$|^\s+|\s+$", " ").MakeEqualSpace();
            values = context.GetPossibleColumnNameAliases(sql);
            Assert.IsTrue(values.Count == 1);
        }
    }
}
