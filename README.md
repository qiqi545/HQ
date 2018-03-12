HQ.io
==============

This library contains code for simplifying .NET Core platform execution. 
This means helper functions and running and compiling arbitrary code in isolation.

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

  var h = handlers.BuildHandler(info);
  var r = (string)h.Invoke(null, new object [] { })

  Console.WriteLine(r); // Hello, World!
}
```