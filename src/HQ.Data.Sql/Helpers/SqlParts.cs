// <copyright company="PetaPoco - CollaboratingPlatypus">
//      Apache License, Version 2.0 https://github.com/CollaboratingPlatypus/PetaPoco/blob/master/LICENSE.txt
// </copyright>
// <author>PetaPoco - CollaboratingPlatypus</author>
// <date>2015/12/13</date>

// ReSharper disable once CheckNamespace

namespace HQ.Data.Sql.Helpers
{
    /// <summary>
    ///     Presents the SQL parts.
    /// </summary>
    public struct SqlParts
    {
        /// <summary>
        ///     The SQL.
        /// </summary>
        public string Sql;

        /// <summary>
        ///     The SQL COUNT.
        /// </summary>
        public string SqlCount;

        /// <summary>
        ///     The SQL FROM
        /// </summary>
        public string SqlFrom;

        /// <summary>
        ///     The SQL SELECT
        /// </summary>
        public string SqlSelectRemoved;

        /// <summary>
        ///     The SQL ORDER BY
        /// </summary>
        public string SqlOrderBy;

        /// <summary>
        ///     The SQL ORDER BY Fields
        /// </summary>
        public string[] SqlOrderByFields;
    }
}
