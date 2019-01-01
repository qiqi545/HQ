using System.Data;
using System.Data.DocumentDb;
using HQ.Connect;
using Microsoft.Azure.Documents.Client;

namespace HQ.Lingo.DocumentDb
{
    public static class DbConnectionExtensions
    {
        public static DocumentClient GetClient(this IDbConnection connection)
        {
            return connection is WrapDbConnection wrapped && wrapped.Inner is DocumentDbConnection docDbConnection
                ? docDbConnection.Client
                : null;
        }

        public static string GetDatabaseId(this IDbConnection connection)
        {
            return connection is WrapDbConnection wrapped && wrapped.Inner is DocumentDbConnection docDbConnection
                ? docDbConnection.Database
                : null;
        }
    }
}
