using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.FormatSQL.Formatter.Tests
{
    [TestClass]
    public class ExtentionsTest
    {
        [TestMethod]
        public void MakeEqualSpaceTest()
        {
            string input = "This  is  a   test     period.";
            string output = input.MakeEqualSpace();
            Assert.IsTrue(output == "This is a test period.");
        }

        [TestMethod]
        public void SplitAndJoinTest()
        {
            string input = "This is a hello world split by word example. Hello world!";
            string[] output = input.SplitAndJoin(new string[] { "hello", "world" });
            Assert.IsTrue(output.Length == 3);
            Assert.IsTrue(output[0] == "This is a");
            Assert.IsTrue(output[1] == "split by word example. Hello");
            Assert.IsTrue(output[2] == "!");

            input = "This is a hello world split by word example. hello world!";
            output = input.SplitAndJoin(new string[] { "hello", "world" });
            Assert.IsTrue(output.Length == 3);
            Assert.IsTrue(output[0] == "This is a");
            Assert.IsTrue(output[1] == "split by word example.");
            Assert.IsTrue(output[2] == "!");

            input = "hello world split by word example. hello world!";
            output = input.SplitAndJoin(new string[] { "hello", "world" });
            Assert.IsTrue(output.Length == 2);
            Assert.IsTrue(output[0] == "split by word example.");
            Assert.IsTrue(output[1] == "!");

            input = "hello world!";
            output = input.SplitAndJoin(new string[] { "hello", "world" });
            Assert.IsTrue(output.Length == 1);
            Assert.IsTrue(output[0] == "!");

            input = "hello world";
            output = input.SplitAndJoin(new string[] { "hello", "world" });
            Assert.IsTrue(output.Length == 0);
        }

        [TestMethod]
        public void SplitByWordTest()
        {
            var input = @"@p0 = 'fsiadmin', @p1 = '', @p2 = 'FSI Admin', @p3 = 'h9uS5eD5qrclJrbRXnI6YA==', @p4 = 'VSjNlEBnNk24uDYh08EsQg==', @p5 = '21/10/2111 00:00:00', @p6 = '1',  @p7 = '', @p8 = '', @p9 = 'False', @p10 = '10746', @p11 = '', @p12 = '21/08/2015 14:08:01', @p13 = '0c0de31aca794eae80fdc98132b14b5b', @p14 = '0c0de31aca794eae80fdc98132b14b5b';";
            var output = input.SplitByWord(new string[] { "=", "," });
            Assert.IsTrue(output.Length == 30);
        }

        [TestMethod]
        public void SplitAndKeepTest()
        {
            string input = "Split this like so, so that we can say [hello world]!";
            string[] output = input.SplitAndKeep(new string[] { ",", "[", "]" });
            Assert.IsTrue(output[0] == "Split this like so");
            Assert.IsTrue(output[1] == ",");
            Assert.IsTrue(output[2] == " so that we can say ");
            Assert.IsTrue(output[3] == "[");
            Assert.IsTrue(output[4] == "hello world");
            Assert.IsTrue(output[5] == "]");
            Assert.IsTrue(output[6] == "!");

            input = "[hello world] A split, so that we can say hello world!";
            output = input.SplitAndKeep(new string[] { ",", "[", "]" });
            Assert.IsTrue(output[0] == "[");
            Assert.IsTrue(output[1] == "hello world");
            Assert.IsTrue(output[2] == "]");
            Assert.IsTrue(output[3] == " A split");
            Assert.IsTrue(output[4] == ",");
            Assert.IsTrue(output[5] == " so that we can say hello world!");

            input = "[hello world] A split, so that we can say hello world!";
            output = input.SplitAndKeep(new string[] { ",", "[", "]", "!" });
            Assert.IsTrue(output[0] == "[");
            Assert.IsTrue(output[1] == "hello world");
            Assert.IsTrue(output[2] == "]");
            Assert.IsTrue(output[3] == " A split");
            Assert.IsTrue(output[4] == ",");
            Assert.IsTrue(output[5] == " so that we can say hello world");
            Assert.IsTrue(output[6] == "!");
        }

        [TestMethod]
        public void EnsureLastCharacterExistsTest()
        {
            string input = "Ensure that this sentence contains an exclamation";
            string output = input.EnsureLastCharacterExists('!');
            Assert.IsTrue(output == "Ensure that this sentence contains an exclamation!");

            input = "Ensure that this sentence contains an exclamation!";
            output = input.EnsureLastCharacterExists('!');
            Assert.IsTrue(output == "Ensure that this sentence contains an exclamation!");

            input = "";
            output = input.EnsureLastCharacterExists('!');
            Assert.IsTrue(output == "");

            input = "e";
            output = input.EnsureLastCharacterExists('!');
            Assert.IsTrue(output == "e!");
        }

        [TestMethod]
        public void ValuesBetweenDiscardCharsTest()
        {
            string input = "[hello world]";

            string output = input.FirstValueBetweenChars('[',']');
            Assert.IsTrue(output == "hello world");

            input = "This is a test to get [hello world] from a string.";
            output = input.FirstValueBetweenChars('[', ']');
            Assert.IsTrue(output == "hello world");

            input = "[hello world] i say to you.";
            output = input.FirstValueBetweenChars('[', ']');
            Assert.IsTrue(output == "hello world");

            input = "I say to you [hello world]";
            output = input.FirstValueBetweenChars('[', ']');
            Assert.IsTrue(output == "hello world");
        }

        [TestMethod]
        public void ValuesBetweenKeepCharsTest()
        {
            string input = "[hello world]";
            string output = input.FirstValueBetweenChars('[', ']', true);
            Assert.IsTrue(output == "[hello world]");

            input = "This is a test to get [hello world] from a string.";
            output = input.FirstValueBetweenChars('[', ']', true);
            Assert.IsTrue(output == "[hello world]");

            input = "[hello world] i say to you.";
            output = input.FirstValueBetweenChars('[', ']', true);
            Assert.IsTrue(output == "[hello world]");

            input = "I say to you [hello world]";
            output = input.FirstValueBetweenChars('[', ']', true);
            Assert.IsTrue(output == "[hello world]");
        }

        [TestMethod]
        public void ValuesBetweenKeepDiscardCharsLastIndexTest()
        {
            int lastIndex = 0;

            string input = "[hello world]";
            string output = input.FirstValueBetweenChars('[', ']', out lastIndex, true);
            Assert.IsTrue(lastIndex == 12);

            input = "This is a test to get [hello world] from a string.";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex, true);
            Assert.IsTrue(lastIndex == 34);

            input = "[hello world] i say to you.";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex, true);
            Assert.IsTrue(lastIndex == 12);

            input = "I say to you [hello world]";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex, true);
            Assert.IsTrue(lastIndex == 25);
        }

        [TestMethod]
        public void ValuesBetweenKeepCharsLastIndexTest()
        {
            int lastIndex = 0;

            string input = "[hello world]";
            string output = input.FirstValueBetweenChars('[', ']', out lastIndex);
            Assert.IsTrue(lastIndex == 13);

            input = "This is a test to get [hello world] from a string.";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex);
            Assert.IsTrue(lastIndex == 35);

            input = "[hello world] i say to you.";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex);
            Assert.IsTrue(lastIndex == 13);

            input = "I say to you [hello world]";
            output = input.FirstValueBetweenChars('[', ']', out lastIndex);
            Assert.IsTrue(lastIndex == 26);
        }

        [TestMethod]
        public void ValueAfterLastCharTest()
        {
            string input = "((This is the value to keep)),This is the value that will be discarded.";
            string output = input.InnerValueAfterLastChar(')', ',');
            Assert.IsTrue(output == "((This is the value to keep))");
        }
    }
}
