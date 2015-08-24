using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernate.FormatSQL.Formatter
{
	public static class Extentions
	{

		public static string MakeEqualSpace(this string @this)
		{
			var regexMatches = Regex.Replace(@this, "\x20{2,20}", " ");
			return regexMatches;
		}

		public static string FirstValueBetweenChars(this string @this, char a, char b, bool keepChars = false)
		{
			int lastIndex = 0;
			return FirstValueBetweenChars(@this, a, b, out lastIndex, keepChars);
		}

		public static string FirstValueBetweenChars(this string @this, char a, char b, out int lastIndex, bool keepChars = false)
		{
			lastIndex = 0;

			int indexOfA = @this.IndexOf(a);
			int indexOfB = @this.IndexOf(b);
			if (indexOfA < 0 || indexOfB < 0)
				return @this;

			if (indexOfA + indexOfB == @this.Length)
				return @this; // basically send back [] 

			int start = keepChars ? indexOfA + 0 : indexOfA + 1;
			int length = keepChars ? (indexOfB - indexOfA) + 1 : (indexOfB - indexOfA) - 1;
			lastIndex = keepChars ? indexOfB : indexOfB + 1;

			string returnvalue = @this.Substring(start, length);
			return returnvalue;
		}

		public static string[] SplitAndKeep(this string @this, params string[] seperator)
		{
			List<string> returnList = new List<string>();

			string search = string.Empty;
			foreach (var item in seperator)
			{
				search = string.Format(@"{0}\{1}|", search, item);
			}

			search = string.Format("({0})", search.Remove(search.Length - 1, 1));
			var matches = Regex.Matches(@this, search, RegexOptions.IgnoreCase);
			if (matches.Count > 0)
			{
				int start = 0;
				string value = string.Empty;
				for (int ix = 0; ix < matches.Count; ix++)
				{
					value = @this.Substring(start, matches[ix].Index - start);
					if (!string.IsNullOrWhiteSpace(value))
						returnList.Add(value);

					string matchValue = matches[ix].Value;
					returnList.Add(matchValue);

					start = matches[ix].Index + 1;
				}

				value = @this.Substring(start);
				if (!string.IsNullOrWhiteSpace(value))
					returnList.Add(value);
			}

			return returnList.ToArray();
		}

		public static string[] SplitAndJoin(this string @this, params string[] seperator)
		{
			StringBuilder a = new StringBuilder();
			StringBuilder b = new StringBuilder();
			List<string> returnList = new List<string>();

			string[] splitBySpaces = @this.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			for (int ix = 0; ix < splitBySpaces.Length; ix++)
			{
				b = new StringBuilder();
				string currentWord = splitBySpaces[ix];
				for (int iy = 0; iy < currentWord.Length; iy++)
				{
					char current = currentWord[iy];
					a.Append(current);
					b.Append(current);
					if (seperator.Contains(b.ToString().Trim()))
					{
						string value = a.ToString().Substring(0, a.ToString().Length - b.ToString().Length).Trim();
						if (!string.IsNullOrEmpty(value))
						{
							returnList.Add(value.ToString());
						}
						a = new StringBuilder();
					}
				}
				a.Append(" ");
			}
			if (a.ToString().Trim().Length > 0 && a.Length > 0)
			{
				returnList.Add(a.ToString().Trim());
			}
			return returnList.ToArray();
		}

		/// <summary>
		/// The same as the split function however the seperator value must be alone and not part of another word to be split by.
		/// </summary>
		/// <param name="this">
		/// The string value to split by.
		/// </param>
		/// <param name="seperator">
		/// The seperator values to split the string up by.
		/// </param>
		/// <returns></returns>
		public static string[] SplitByWord(this string @this, params string[] seperator)
		{
			return SplitByWord(@this, null, seperator);
		}

		
		public static string[] SplitByWord(this string @this, string[] ignoreWords, params string[] seperator)
		{
			StringBuilder a = new StringBuilder();
			List<string> returnList = new List<string>();

			for (var ix = 0; ix <= @this.Length - 1; ix++ )
			{
				var previous = ix > 0 ? @this[ix - 1] : '\0';
				char current = @this[ix];

				var seperatorExistsCount = seperator.Count(s =>
				{
					return (s.ToString() == current.ToString() && (char.IsPunctuation(previous) || char.IsWhiteSpace(previous)));
				});

				if (seperatorExistsCount > 0)
				{
					if (ignoreWords == null)
					{
						returnList.Add(a.ToString());
					}
					else
					{
						// ( filter out word that we dont want included )
						var ignoreWordFound = ignoreWords.Count(w => w.ToLower() == a.ToString().ToLower()) > 0;
						if (!ignoreWordFound)
						{
							returnList.Add(a.ToString());
						}
					}
					a = new StringBuilder();
				}
				else
					a.Append(current);
			}

			if (a.ToString().Trim().Length > 0 && a.Length > 0)
			{
				returnList.Add(a.ToString().Trim());
			}
			return returnList.ToArray();
		}


		public static string EnsureLastCharacterExists(this string @this, char character)
		{
			var indexOfCharacter = -1;
            var characterMatces = Regex.Matches(@this, string.Format(@"\{0}", character));
			if (characterMatces.Count > 0)
				indexOfCharacter = characterMatces[characterMatces.Count - 1].Index;
	
			if (indexOfCharacter == @this.Trim().Length-1)
				return @this;
			else
				return string.Format("{0}{1}", @this, character);
		}

		/// <summary>
		/// Inserts the current value between prefix and suffix value.
		/// </summary>
		/// <param name="this">
		/// The value to insert between prefix and suffix.
		/// </param>
		/// <param name="prefixValue">
		/// The value that comes first.
		/// </param>
		/// <param name="suffixValue">
		/// The value that comes last.
		/// </param>
		/// <returns>
		/// This value between prefix and suffix. 
		/// </returns>
		public static string InsertBetween(this string @this, string prefixValue, string suffixValue)
		{
			if (string.IsNullOrWhiteSpace(@this))
				return @this;

			return string.Format("{0}{1}{2}", prefixValue, @this, suffixValue);
		}

		public static string InnerValueAfterLastChar(this string @this, char lastSearchValue, char lastChar) 
		{
			string returnValue = string.Empty;

			try 
			{
				var search = string.Format(@"\{0}", lastSearchValue);
				var lastSearchMatches = Regex.Matches(@this, search, RegexOptions.IgnoreCase);

				if (lastSearchMatches.Count <= 0)
					return @this;

				var lastMatchIndex = lastSearchMatches[lastSearchMatches.Count - 1].Index;
				var indexOfLastChar = @this.IndexOf(lastChar, lastMatchIndex);

				if (indexOfLastChar > 0)
					returnValue = @this.Substring(0, indexOfLastChar);
				else
					return @this;
			}
			catch {
				throw;
			}

			return returnValue;
		}
	}
}
