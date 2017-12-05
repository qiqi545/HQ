# tophat

```
PM> Install-Package tophat
```

### Introduction
Tophat gives you a unit of work to perform data operations against, independent of your database.
Any database that provides support for `IDbConnection` works in tophat. Most ORMs today provide
some kind of session scoping (NHibernate uses Session, Linq-To-SQL uses DataContext, etc.) but
for many use cases, ORMs are unnecessary. New micro-ORM layers like Dapper and PetaPoco simply extend
the `IDbConnection` interface. Tophat provides scoping the same way.

### Features

* Clean lifetime management for your `IDbConnection` using latest non-static dependency patterns
* Simple interface for plugging in your own connection factory
* Common scopes provided, or pass in a custom scope function
* Why, that's a lovely top hat you're wearing! (works nicely with Dapper) 

### Installing a default connection factory

```csharp
// One database connection per web request
var connectionString = "...";
services.AddDatabaseConnection<SqlServerConnectionFactory>(connectionString, ConnectionScope.ByRequest);
```

### Using a scoped connection (per-class resolution, supporting multiple database types)
```csharp
// Somewhere in your DI code...
var connectionString = "...";
services.AddDatabaseConnection<MyRepository, SqlServerConnectionFactory>(connectionString, ConnectionScope.ByRequest);

using tophat;
using Dapper;

public class MyRepository
{
    private IDataConnection _connection;

    public MyRepository(IDataConnection<MyRepository> connection)
    {
        _connection = connection;
    }

    public IEnumerable<Foo> GetFooz()
    {
        // Automatic handling of connection scope
        IDbConnection db = _connection.Current;

        // Regular Dapper operations against an IDbConnection
        return db.Query<Fooz>("SELECT * FROM Fooz");
    }
}
```

### Using the DataContext directly
```csharp
using tophat;
using Dapper;

public class MyRepository
{
    public IEnumerable<Foo> GetFooz()
    {
         // Manual handling of connection scope
         var cs = "...";
         using (var db = new SqlServerDataContext(cs))
         {
             // Regular Dapper operations against an IDbConnection
             return db.Query<Foo>("SELECT * FROM Fooz");
         }        
    }
}
```

### Implementing support for a custom database (in this case, SQLite)
```csharp
using tophat;
using System.Data;
using Microsoft.Data.Sqlite;

public class SqliteConnectionFactory : ConnectionFactory
{
    public override IDbConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}

public class SqliteDataContext : DataContext
{
    public SqliteDataContext(string connectionString) : base(new SqliteConnectionFactory { ConnectionString = connectionString })
    {

    }
}
```
