# Script

A simple portable scripting engine for C#. Think Python syntax with JavaScript objects and strict typing.

## Documentation

Please view the [wiki](https://github.com/Templarian/Script/wiki) for a comprehensive overview and code samples.

## Features

* Written from the ground up for readability.
* Intuitive C# syntax on intepreter and script side.
* Comprehensive syntax errors and script errors.
* Strict typing allows method overloading based on datatype.
* Simple... `int`, `double`, `string`, `bool`, and `regex` [data types](https://github.com/Templarian/Script/wiki/Types).
* Implicit conversions for all types `("foo1" = "foo" + 1)`


## Hello World

```csharp
static void Main()
{
    var engine = new ScriptEngine();
    engine.AddAction<string>("log", Log);
    engine.Execute("log('Hello World!')");
    Console.In.Read();
}
static void Log(string message)
{
    Console.WriteLine(message);
}
```
