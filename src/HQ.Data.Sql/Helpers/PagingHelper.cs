// <copyright company="PetaPoco - CollaboratingPlatypus">
//      Apache License, Version 2.0 https://github.com/CollaboratingPlatypus/PetaPoco/blob/master/LICENSE.txt
// </copyright>
// <author>PetaPoco - CollaboratingPlatypus</author>
// <date>2015/12/13</date>

// ReSharper disable once CheckNamespace

using System.Linq;
using System.Text.RegularExpressions;
using PetaPoco.Utilities;

// ReSharper disable once CheckNamespace
namespace HQ.PetaPoco.Utilities
{
	public static class PagingHelper
    {
        public static readonly Regex RegexColumns = new Regex(
            @"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public static readonly Regex RegexDistinct = new Regex(@"\ADISTINCT\s",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public static readonly Regex RegexOrderBy =
            new Regex(
                @"\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
                RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline |
                RegexOptions.Compiled);

        /// <summary>
        ///     Splits the given <paramref name="sql" /> into <paramref name="parts" />;
        /// </summary>
        /// <param name="sql">The SQL to split.</param>
        /// <param name="parts">The SQL parts.</param>
        /// <returns><c>True</c> if the SQL could be split; else, <c>False</c>.</returns>
        public static bool SplitSql(string sql, out SqlParts parts)
        {
            parts.Sql = sql;
            parts.SqlCount = null;
            parts.SqlFrom = null;
            parts.SqlSelectRemoved = null;
            parts.SqlOrderBy = null;
            parts.SqlOrderByFields = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = RegexColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            parts.SqlSelectRemoved = sql.Substring(g.Index);

            var from = sql.Substring(g.Index + g.Length);
            parts.SqlFrom = from.Replace("FROM ", string.Empty);

            if (RegexDistinct.IsMatch(parts.SqlSelectRemoved))
                parts.SqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + from;
            else
                parts.SqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + from;

            // Look for the last "ORDER BY <whatever>" clause not part of a ROW_NUMBER expression
            m = RegexOrderBy.Match(parts.SqlCount);

            if (m.Success)
            {
                g = m.Groups[0];
                parts.SqlOrderBy = g.ToString();
                parts.SqlOrderByFields = m.Groups.OfType<Group>()
                    .Select(x => x.Value.Replace("ORDER BY", string.Empty).Trim()).Where(x => x != string.Empty)
                    .ToArray();
                parts.SqlCount = parts.SqlCount.Substring(0, g.Index) + parts.SqlCount.Substring(g.Index + g.Length);
            }

            return true;
        }
    }
}
