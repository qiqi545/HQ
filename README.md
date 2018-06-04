snippets
==============

Helper functions for working with Roslyn and arbitrary code. 

Also contains wrapper functions for executing JavaScript via Node.js.

C#
--

```csharp
var handlers = new HandlerFactory();

var info = new HandlerInfo {
	Namespace = "hq",
	Function = "Execute",
	Entrypoint = "hq.Main",
	Code = @"
namespace hq
{ 
    public class Main
    { 
        public static string Execute()
        { 
            return ""Hello, World!"";
        }
    }
}"};

  var h = handlers.BuildCSharpHandler(info);
  var r = (string)h.DynamicInvoke(null, null)

  Console.WriteLine(r); // Hello, World!
}
```

JavaScript
----------
_(Requires Node.js installed with a PATH system environment variable set_)

```csharp
var handlers = new HandlerFactory();

var info = new HandlerInfo {
	Namespace = "module",
	Entrypoint = "exports",
	Code = @"
function(callback) { 
  var result = 'Hello, World!';
  callback(null, result); 
};"};

  var h = handlers.BuildJavaScriptHandler(info);
  var r = (string)h.DynamicInvoke(null, null)

  Console.WriteLine(r); // Hello, World!
}
```