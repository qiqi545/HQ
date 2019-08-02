#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HQ.UI.Internal.UriTemplates
{
	public class Result
	{
		private const string _UriReservedSymbols = ":/?#[]@!$&'()*+,;=";
		private const string _UriUnreservedSymbols = "-._~";

		private static readonly char[] HexDigits =
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
		};

		private readonly StringBuilder _Result = new StringBuilder();

		public Result() => ParameterNames = new List<string>();

		public bool ErrorDetected { get; set; }
		public List<string> ParameterNames { get; set; }

		public StringBuilder Append(char value)
		{
			return _Result.Append(value);
		}

		public StringBuilder Append(string value)
		{
			return _Result.Append(value);
		}

		public override string ToString()
		{
			return _Result.ToString();
		}

		public void AppendName(string variable, OperatorInfo op, bool valueIsEmpty)
		{
			_Result.Append(variable);
			if (valueIsEmpty)
				_Result.Append(op.IfEmpty);
			else
				_Result.Append("=");
		}


		public void AppendList(OperatorInfo op, bool explode, string variable, IList list)
		{
			foreach (var item in list)
			{
				if (op.Named && explode)
				{
					_Result.Append(variable);
					_Result.Append("=");
				}

				AppendValue(item.ToString(), 0, op.AllowReserved);

				_Result.Append(explode ? op.Separator : ',');
			}

			if (list.Count > 0) _Result.Remove(_Result.Length - 1, 1);
		}

		public void AppendDictionary(OperatorInfo op, bool explode, IDictionary<string, string> dictionary)
		{
			foreach (var key in dictionary.Keys)
			{
				_Result.Append(Encode(key, op.AllowReserved));
				if (explode) _Result.Append('=');
				else _Result.Append(',');
				AppendValue(dictionary[key], 0, op.AllowReserved);

				if (explode)
					_Result.Append(op.Separator);
				else
					_Result.Append(',');
			}

			if (dictionary.Count() > 0) _Result.Remove(_Result.Length - 1, 1);
		}

		public void AppendValue(string value, int prefixLength, bool allowReserved)
		{
			if (prefixLength != 0)
				if (prefixLength < value.Length)
					value = value.Substring(0, prefixLength);

			_Result.Append(Encode(value, allowReserved));
		}


		private static string Encode(string p, bool allowReserved)
		{
			var result = new StringBuilder();
			foreach (var c in p)
				if (c >= 'A' && c <= 'z' //Alpha
				    || c >= '0' && c <= '9' // Digit
				    || _UriUnreservedSymbols.IndexOf(c) !=
				    -1 // Unreserved symbols  - These should never be percent encoded
				    || allowReserved && _UriReservedSymbols.IndexOf(c) != -1
				) // Reserved symbols - should be included if requested (+)
					result.Append(c);
				else
				{
					var bytes = Encoding.UTF8.GetBytes(new[] {c});
					foreach (var abyte in bytes) result.Append(HexEscape(abyte));
				}

			return result.ToString();
		}

		public static string HexEscape(byte i)
		{
			var esc = new char[3];
			esc[0] = '%';
			esc[1] = HexDigits[(i & 240) >> 4];
			esc[2] = HexDigits[i & 15];
			return new string(esc);
		}

		public static string HexEscape(char c)
		{
			var esc = new char[3];
			esc[0] = '%';
			esc[1] = HexDigits[(c & 240) >> 4];
			esc[2] = HexDigits[c & 15];
			return new string(esc);
		}
	}
}