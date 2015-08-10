using NHibernate.FormatSQL.Formatter;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LINQPad.NHibernate.FormatSQL
{
    public class NHibernateSqlOutputRedirector : TextWriter, IDisposable
    {
        private TextWriter stdOutWriter;
        private int counter;
        private bool notifyWhenSqlOutputRedirected;
        private NHibernateSqlOutputFormatter nhibernateFormatter;

        public override Encoding Encoding { get { return Encoding.ASCII; } }

        public NHibernateSqlOutputRedirector()
        {
            nhibernateFormatter = new NHibernateSqlOutputFormatter();
            nhibernateFormatter.SqlIdentifiers = new string[] { ";" };
        }

        public NHibernateSqlOutputRedirector(bool notifyWhenSqlOutputRedirected = false)
            : this()
        {
            this.stdOutWriter = Console.Out;
            this.counter = 0;
            this.notifyWhenSqlOutputRedirected = notifyWhenSqlOutputRedirected;
            Console.SetOut(this);
        }

        override public void Write(string s)
        {
            var output = FixNHibernateSqlOutput(s);
            if (this.notifyWhenSqlOutputRedirected || !CheckIfNHibernateOutput(s))
            {
                stdOutWriter.Write(output);
            }
        }

        override public void WriteLine(string s)
        {
            var output = FixNHibernateSqlOutput(s);
            if (this.notifyWhenSqlOutputRedirected || !CheckIfNHibernateOutput(s))
            {
                stdOutWriter.WriteLine(output);
            }
        }

        public new void Dispose()
        {
            Util.SqlOutputWriter.WriteLine("-- There " + (counter == 1 ? "was " : "were ") + counter + " quer" + (counter == 1 ? "y" : "ies"));
            this.Dispose(true);
        }

        private string FixNHibernateSqlOutput(string s)
        {
            if (CheckIfNHibernateOutput(s))
            {
                s = s.Remove(0, 12);
                string output = Regex.Replace(s, "(SELECT|[^0-9a-z]FROM|LEFT OUTER JOIN|INNER JOIN|WHERE|GROUP BY)", System.Environment.NewLine + "$1", RegexOptions.IgnoreCase);
                output = Regex.Replace(output, @"( as \w+,)", "$1" + System.Environment.NewLine + "    ", RegexOptions.IgnoreCase);
                var startOfParameters = output.IndexOf(";@p0") + 1;
                if (startOfParameters > 1)
                {
                    var paramSection = output.Substring(startOfParameters);
                    var parameters = paramSection.Split(',').Select(p =>
                    new
                    {
                        key = p.Split('=')[0].Trim(),
                        value = Regex.Replace(p.Split('=')[1], @"\[Type:.*?\]", "", RegexOptions.IgnoreCase).Trim()
                    });
                    output = output.Substring(0, startOfParameters - 1);
                    foreach (var parameter in parameters)
                    {
                        var newValue = parameter.value;
                        if (newValue.StartsWith("'") && newValue.EndsWith("'"))
                        {
                            newValue = "'" + newValue.Substring(1, newValue.Length - 2).Replace("'", "''") + "'";
                        }
                        output = output.Replace(parameter.key, newValue);
                    }
                }

                // ( wrap in an exception handler for now for safety until full test coverage )
                try {

                    // ( create more readable column and table name aliases for Sql output ) 
                    output = output.EnsureLastCharacterExists(';');

                    ISqlStatement sqlStatement = nhibernateFormatter.TryParsSql(output);
                    if (sqlStatement.SqlStatementParsingException == null)
                    {
                        string formattedInput = sqlStatement.ApplySuggestedFormat();

                        // ( formatted column and table names )
                        output = formattedInput;
                    }
                }
                catch (Exception exception) {
                    // ( dont throw the exception instead continue with unformatted output )
                    Debug.Print(exception.ToString());
                }

                output = "-- Region Query: " + ++counter + "\r\n" + output.Trim() + "\r\n-- EndRegion";
                Util.SqlOutputWriter.WriteLine(output);
                return "NHibernate query redirected to SQL Tab (" + counter + " quer" + (counter == 1 ? "y" : "ies") + " redirected in this batch)";
            }
            return s;
        }

        private bool CheckIfNHibernateOutput(string s)
        {
            return s.StartsWith("NHibernate: ");
        }
    }
}
