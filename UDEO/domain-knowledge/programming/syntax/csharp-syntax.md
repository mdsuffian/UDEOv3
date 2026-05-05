# C# Syntax Reference

This document provides a comprehensive reference for C# syntax, covering all language constructs from basic to advanced features.

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Data Types](#data-types)
3. [Variables and Constants](#variables-and-constants)
4. [Operators](#operators)
5. [Control Flow](#control-flow)
6. [Methods and Functions](#methods-and-functions)
7. [Classes and Objects](#classes-and-objects)
8. [Inheritance and Polymorphism](#inheritance-and-polymorphism)
9. [Interfaces](#interfaces)
10. [Generics](#generics)
11. [Collections](#collections)
12. [Exception Handling](#exception-handling)
13. [LINQ](#linq)
14. [Async Programming](#async-programming)
15. [Attributes](#attributes)
16. [Delegates and Events](#delegates-and-events)
17. [Lambda Expressions](#lambda-expressions)
18. [Pattern Matching](#pattern-matching)
19. [Records](#records)
20. [Nullable Reference Types](#nullable-reference-types)

## Basic Syntax

### Program Structure

```csharp
using System;
using System.Collections.Generic;

namespace MyNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            // Program entry point
            Console.WriteLine("Hello, World!");
        }
    }
}
```

### Comments

```csharp
// Single-line comment

/* Multi-line comment
   that spans multiple lines */

/// XML documentation comment
/// <summary>
/// This method does something
/// </summary>
/// <param name="input">The input parameter</param>
/// <returns>The result</returns>
public int MyMethod(int input) { return input * 2; }
```

## Data Types

### Value Types

```csharp
// Integral types
byte b = 255;           // 8-bit unsigned (0-255)
sbyte sb = -128;        // 8-bit signed (-128 to 127)
short s = 32767;        // 16-bit signed
ushort us = 65535;      // 16-bit unsigned
int i = 2147483647;     // 32-bit signed
uint ui = 4294967295u;  // 32-bit unsigned
long l = 9223372036854775807L;  // 64-bit signed
ulong ul = 18446744073709551615UL; // 64-bit unsigned

// Floating-point types
float f = 3.14f;        // 32-bit
double d = 3.14159265359; // 64-bit
decimal m = 79228162514264337593543950335m; // 128-bit

// Other value types
bool flag = true;       // Boolean
char c = 'A';           // 16-bit Unicode
DateTime dt = DateTime.Now;
Guid guid = Guid.NewGuid();
```

### Reference Types

```csharp
// Strings
string str = "Hello";
string interpolated = $"Hello, {name}!";
string verbatim = @"C:\Path\To\File";

// Arrays
int[] numbers = new int[5];
int[] initialized = { 1, 2, 3, 4, 5 };
int[,] multiDimensional = new int[3, 4];
int[][] jagged = new int[3][];

// Objects
object obj = new object();
dynamic dyn = 42; // Runtime binding

// Classes
MyClass myClass = new MyClass();
```

### Type Conversions

```csharp
// Implicit conversion
int i = 42;
long l = i; // int to long

// Explicit conversion
double d = 3.14;
int i = (int)d; // double to int (truncates)

// Parsing
string str = "123";
int i = int.Parse(str);
int i2;
bool success = int.TryParse(str, out i2);

// Convert class
string s = "123.45";
double d = Convert.ToDouble(s);
```

## Variables and Constants

### Variable Declaration

```csharp
// Declaration with initialization
int x = 10;
string name = "John";
bool isActive = true;

// Multiple declaration
int a = 1, b = 2, c = 3;

// var keyword (type inference)
var number = 42; // Compiler infers int
var text = "Hello"; // Compiler infers string

// Dynamic typing
dynamic dyn = 42;
dyn = "Now a string"; // No compile-time checking
```

### Constants

```csharp
// Compile-time constants
const int MAX_VALUE = 100;
const string APP_NAME = "MyApp";

// Readonly fields (runtime constants)
public readonly DateTime CreatedAt;
public MyClass()
{
    CreatedAt = DateTime.Now;
}
```

## Operators

### Arithmetic Operators

```csharp
int a = 10, b = 3;
int sum = a + b;        // 13
int diff = a - b;       // 7
int product = a * b;    // 30
int quotient = a / b;   // 3 (integer division)
int remainder = a % b;  // 1

// Compound assignment
a += 5;  // a = a + 5
a -= 3;  // a = a - 3
a *= 2;  // a = a * 2
a /= 4;  // a = a / 4
a %= 3;  // a = a % 3
```

### Comparison Operators

```csharp
int a = 5, b = 10;
bool isEqual = a == b;      // false
bool notEqual = a != b;     // true
bool greater = a > b;       // false
bool less = a < b;          // true
bool greaterOrEqual = a >= b; // false
bool lessOrEqual = a <= b;  // true
```

### Logical Operators

```csharp
bool x = true, y = false;
bool and = x && y;          // false (logical AND)
bool or = x || y;           // true (logical OR)
bool not = !x;              // false (logical NOT)

bool bitwiseAnd = x & y;    // false (bitwise AND)
bool bitwiseOr = x | y;     // true (bitwise OR)
bool xor = x ^ y;           // true (exclusive OR)
```

### Null-Conditional Operators

```csharp
string name = null;
int? length = name?.Length; // null if name is null
string result = name ?? "Default"; // "Default" if name is null

// Null-coalescing assignment
name ??= "Default"; // Assign if null
```

## Control Flow

### Conditional Statements

```csharp
// if-else
if (condition)
{
    // Code to execute if condition is true
}
else if (anotherCondition)
{
    // Code to execute if anotherCondition is true
}
else
{
    // Code to execute if all conditions are false
}

// switch statement
switch (value)
{
    case 1:
        // Code for case 1
        break;
    case 2:
    case 3:
        // Code for cases 2 and 3
        break;
    default:
        // Code for all other cases
        break;
}

// Switch expressions (C# 8.0+)
string result = value switch
{
    1 => "One",
    2 => "Two",
    _ => "Other"
};
```

### Loops

```csharp
// for loop
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}

// while loop
int count = 0;
while (count < 10)
{
    Console.WriteLine(count);
    count++;
}

// do-while loop
int num = 0;
do
{
    Console.WriteLine(num);
    num++;
} while (num < 10);

// foreach loop
int[] numbers = { 1, 2, 3, 4, 5 };
foreach (int num in numbers)
{
    Console.WriteLine(num);
}
```

### Jump Statements

```csharp
// break和continue
for (int i = 0; i < 10; i++)
{
    if (i == 5)
        break;  // Exit loop
    
    if (i % 2 == 0)
        continue; // Skip to next iteration
    
    Console.WriteLine(i);
}

// goto (use sparingly)
goto label;
label: Console.WriteLine("Jumped here");

// return
int Add(int a, int b)
{
    return a + b;
}
```

## Methods and Functions

### Method Declaration

```csharp
// Basic method
public int Add(int a, int b)
{
    return a + b;
}

// Method with multiple parameters
public void PrintDetails(string name, int age, bool isActive)
{
    Console.WriteLine($"Name: {name}, Age: {age}, Active: {isActive}");
}

// Method with optional parameters
public void Log(string message, string level = "INFO")
{
    Console.WriteLine($"[{level}] {message}");
}

// Method with named arguments
Log(message: "Error occurred", level: "ERROR");
```

### Method Overloading

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    public double Add(double a, double b)
    {
        return a + b;
    }
    
    public int Add(int a, int b, int c)
    {
        return a + b + c;
    }
}
```

### Local Functions

```csharp
public void ProcessData()
{
    // Local function
    int Square(int x) => x * x;
    
    for (int i = 0; i < 5; i++)
    {
        Console.WriteLine(Square(i));
    }
}
```

### Expression-Bodied Members

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Expression-bodied property
    public string FullName => $"{FirstName} {LastName}";
    
    // Expression-bodied method
    public override string ToString() => FullName;
}
```

## Classes and Objects

### Class Definition

```csharp
public class Person
{
    // Fields
    private string _firstName;
    private string _lastName;
    private int _age;
    
    // Properties
    public string FirstName
    {
        get { return _firstName; }
        set { _firstName = value; }
    }
    
    // Auto-implemented property
    public string LastName { get; set; }
    
    // Read-only auto-implemented property
    public int Id { get; }
    
    // Constructor
    public Person(string firstName, string lastName, int id)
    {
        _firstName = firstName;
        _lastName = lastName;
        Id = id;
    }
    
    // Default constructor
    public Person()
    {
        // Initialize default values
    }
    
    // Methods
    public void SetAge(int age)
    {
        if (age >= 0 && age <= 150)
            _age = age;
        else
            throw new ArgumentOutOfRangeException(nameof(age));
    }
    
    public int GetAge() => _age;
    
    // Static method
    public static Person CreateDefault()
    {
        return new Person("John", "Doe", 0);
    }
}
```

### Object Creation

```csharp
// Using constructor
Person person1 = new Person("John", "Doe", 1);

// Using object initializer
Person person2 = new Person
{
    FirstName = "Jane",
    LastName = "Smith",
    // Note: Id cannot be set here as it's read-only
};

// With var keyword
var person3 = new Person("Bob", "Johnson", 2);
```

### Static Members

```csharp
public class MathHelper
{
    public static double PI = 3.14159;
    
    public static int Add(int a, int b)
    {
        return a + b;
    }
    
    // Static constructor
    static MathHelper()
    {
        // Initialize static members
        Console.WriteLine("MathHelper initialized");
    }
}

// Usage
int sum = MathHelper.Add(5, 3);
double piValue = MathHelper.PI;
```

### Partial Classes

```csharp
// File1.cs
public partial class MyClass
{
    public void Method1()
    {
        // Implementation
    }
}

// File2.cs
public partial class MyClass
{
    public void Method2()
    {
        // Implementation
    }
}
```

## Inheritance and Polymorphism

### Base and Derived Classes

```csharp
// Base class
public class Animal
{
    public string Name { get; set; }
    
    public virtual void MakeSound()
    {
        Console.WriteLine("Some generic animal sound");
    }
    
    public void Sleep()
    {
        Console.WriteLine("Sleeping...");
    }
}

// Derived class
public class Dog : Animal
{
    public string Breed { get; set; }
    
    // Override base method
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
    
    // New method specific to Dog
    public void WagTail()
    {
        Console.WriteLine("Wagging tail...");
    }
}
```

### Abstract Classes

```csharp
public abstract class Shape
{
    public abstract double GetArea();
    public abstract double GetPerimeter();
    
    // Regular method with implementation
    public void DisplayInfo()
    {
        Console.WriteLine($"Area: {GetArea()}, Perimeter: {GetPerimeter()}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }
    
    public override double GetArea()
    {
        return Math.PI * Radius * Radius;
    }
    
    public override double GetPerimeter()
    {
        return 2 * Math.PI * Radius;
    }
}
```

### Sealed Classes

```csharp
public sealed class FinalClass
{
    // Cannot be inherited from
}

// Cannot inherit from sealed class
// public class DerivedClass : FinalClass { } // Error
```

## Interfaces

### Interface Definition

```csharp
public interface IDrawable
{
    void Draw();
    int X { get; set; }
    int Y { get; set; }
}

public interface IMovable
{
    void Move(int deltaX, int deltaY);
}
```

### Interface Implementation

```csharp
public class Shape : IDrawable, IMovable
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public void Draw()
    {
        Console.WriteLine($"Drawing shape at ({X}, {Y})");
    }
    
    public void Move(int deltaX, int deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }
}
```

### Explicit Interface Implementation

```csharp
public class MultiInterface : IInterface1, IInterface2
{
    // Explicit implementation
    void IInterface1.Method()
    {
        Console.WriteLine("Interface1 method");
    }
    
    void IInterface2.Method()
    {
        Console.WriteLine("Interface2 method");
    }
}
```

### Default Interface Methods (C# 8.0+)

```csharp
public interface ILogger
{
    void Log(string message);
    
    // Default implementation
    void LogError(string error)
    {
        Log($"ERROR: {error}");
    }
}
```

## Generics

### Generic Classes

```csharp
public class Container<T>
{
    private T _item;
    
    public Container(T item)
    {
        _item = item;
    }
    
    public T GetItem()
    {
        return _item;
    }
    
    public void SetItem(T item)
    {
        _item = item;
    }
}
```

### Generic Constraints

```csharp
// Where T must be a class
public class DataStore<T> where T : class
{
    public T Data { get; set; }
}

// Where T must implement IDisposable
public class DisposableContainer<T> where T : IDisposable
{
    public void Process(T item)
    {
        try
        {
            // Process item
        }
        finally
        {
            item.Dispose();
        }
    }
}

// Multiple constraints
public class Repository<T> where T : class, new()
{
    public T Create()
    {
        return new T();
    }
}
```

### Generic Methods

```csharp
public class Utilities
{
    public static T Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
        return temp;
    }
    
    public static bool AreEqual<T>(T a, T b) where T : IEquatable<T>
    {
        return a.Equals(b);
    }
}
```

## Collections

### Lists

```csharp
// List<T>
List<int> numbers = new List<int>();
numbers.Add(1);
numbers.AddRange(new int[] { 2, 3, 4 });

// Access elements
int first = numbers[0];
int count = numbers.Count;

// Remove elements
numbers.Remove(1);
numbers.RemoveAt(0);
numbers.RemoveAll(x => x > 2);

// Iterate
foreach (int num in numbers)
{
    Console.WriteLine(num);
}
```

### Dictionaries

```csharp
// Dictionary<TKey, TValue>
Dictionary<string, int> ages = new Dictionary<string, int>();
ages["John"] = 30;
ages.Add("Jane", 25);

// Check if key exists
if (ages.ContainsKey("John"))
{
    int johnAge = ages["John"];
}

// TryGetValue
int age;
if (ages.TryGetValue("Jane", out age))
{
    Console.WriteLine($"Jane is {age} years old");
}

// Iterate
foreach (KeyValuePair<string, int> pair in ages)
{
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}
```

### Other Collections

```csharp
// HashSet<T>
HashSet<int> uniqueNumbers = new HashSet<int>();
uniqueNumbers.Add(1);
uniqueNumbers.Add(2);
uniqueNumbers.Add(1); // Duplicate, won't be added

// Queue<T>
Queue<string> queue = new Queue<string>();
queue.Enqueue("First");
queue.Enqueue("Second");
string first = queue.Dequeue(); // "First"

// Stack<T>
Stack<int> stack = new Stack<int>();
stack.Push(1);
stack.Push(2);
int top = stack.Pop(); // 2
```

## LINQ (Language Integrated Query)

### Basic LINQ

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Where (filter)
var evenNumbers = numbers.Where(n => n % 2 == 0);

// Select (projection)
var squares = numbers.Select(n => n * n);

// OrderBy
var sorted = numbers.OrderByDescending(n => n);

// First/FirstOrDefault
int firstEven = numbers.FirstOrDefault(n => n % 2 == 0);

// Any/All
bool hasEven = numbers.Any(n => n % 2 == 0);
bool allPositive = numbers.All(n => n > 0);

// Count
int evenCount = numbers.Count(n => n % 2 == 0);
```

### Query Syntax

```csharp
var query = from n in numbers
           where n % 2 == 0
           orderby n descending
           select new { Number = n, Square = n * n };

foreach (var item in query)
{
    Console.WriteLine($"{item.Number} squared is {item.Square}");
}
```

### Grouping and Joining

```csharp
// GroupBy
var grouped = numbers.GroupBy(n => n % 3);

foreach (var group in grouped)
{
    Console.WriteLine($"Numbers with remainder {group.Key}:");
    foreach (var num in group)
        Console.WriteLine($"  {num}");
}

// Join
var students = new List<Student>
{
    new Student { Id = 1, Name = "John" },
    new Student { Id = 2, Name = "Jane" }
};

var grades = new List<Grade>
{
    new Grade { StudentId = 1, Value = "A" },
    new Grade { StudentId = 2, Value = "B" }
};

var studentGrades = from student in students
                   join grade in grades on student.Id equals grade.StudentId
                   select new { student.Name, grade.Value };
```

## Async Programming

### async and await

```csharp
public async Task<string> DownloadDataAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}

// Calling async method
public async Task ProcessDataAsync()
{
    string data = await DownloadDataAsync("https://example.com");
    Console.WriteLine(data);
}
```

### Task Creation

```csharp
// Running task
Task<int> task = Task.Run(() =>
{
    // Do some work
    Thread.Sleep(1000);
    return 42;
});

// Wait for result
int result = await task;

// Creating and starting task
Task task2 = new Task(() =>
{
    Console.WriteLine("Task running");
});
task2.Start();
await task2;
```

### Multiple Tasks

```csharp
// Run tasks in parallel
Task<string> task1 = DownloadDataAsync("url1");
Task<string> task2 = DownloadDataAsync("url2");

// Wait for all to complete
string[] results = await Task.WhenAll(task1, task2);

// Wait for any to complete
Task<string> firstCompleted = await Task.WhenAny(task1, task2);
string firstResult = await firstCompleted;
```

## Attributes

### Using Attributes

```csharp
[Obsolete("This method is deprecated. Use NewMethod instead.")]
public void OldMethod()
{
    // Implementation
}

[Serializable]
public class MyClass
{
    [NonSerialized]
    private int _internalState;
    
    [XmlAttribute("Name")]
    public string Name { get; set; }
}
```

### Custom Attributes

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorAttribute : Attribute
{
    public string Name { get; }
    public DateTime Date { get; }
    
    public AuthorAttribute(string name)
    {
        Name = name;
        Date = DateTime.Now;
    }
}

[Author("John Doe")]
public class MyClass
{
    [Author("Jane Smith")]
    public void MyMethod()
    {
        // Implementation
    }
}
```

## Delegates and Events

### Delegates

```csharp
// Delegate declaration
public delegate int Operation(int a, int b);

// Using delegate
Operation add = (a, b) => a + b;
Operation multiply = (a, b) => a * b;

int result1 = add(5, 3);      // 8
int result2 = multiply(5, 3); // 15

// Built-in delegates
Func<int, int, int> addFunc = (a, b) => a + b;
Action<string> printAction = (s) => Console.WriteLine(s);
Predicate<int> isEven = (n) => n % 2 == 0;
```

### Events

```csharp
public class Publisher
{
    // Event declaration
    public event EventHandler<EventArgs> SomethingHappened;
    
    public void DoSomething()
    {
        // Do work
        OnSomethingHappened(EventArgs.Empty);
    }
    
    protected virtual void OnSomethingHappened(EventArgs e)
    {
        SomethingHappened?.Invoke(this, e);
    }
}

public class Subscriber
{
    public void Subscribe(Publisher publisher)
    {
        publisher.SomethingHappened += HandleSomethingHappened;
    }
    
    private void HandleSomethingHappened(object sender, EventArgs e)
    {
        Console.WriteLine("Something happened!");
    }
}
```

## Lambda Expressions

### Lambda Basics

```csharp
// Expression lambda
Func<int, int> square = x => x * x;

// Statement lambda
Action<int> printAndDouble = x =>
{
    Console.WriteLine(x);
    Console.WriteLine(x * 2);
};

// Lambda with multiple parameters
Func<int, int, int> add = (x, y) => x + y;

// Lambda with no parameters
Action greet = () => Console.WriteLine("Hello!");
```

### Advanced Lambdas

```csharp
// Capturing outer variables
int multiplier = 5;
Func<int, int> multiply = x => x * multiplier;

// Using lambdas with LINQ
List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
var evenNumbers = numbers.Where(n => n % 2 == 0);

// Lambda as event handler
button.Click += (sender, e) => MessageBox.Show("Button clicked!");
```

## Pattern Matching

### Basic Pattern Matching

```csharp
public string DescribeType(object obj)
{
    return obj switch
    {
        int i => $"Integer: {i}",
        string s => $"String: {s}",
        double d when d > 0 => $"Positive double: {d}",
        null => "Null value",
        _ => "Unknown type"
    };
}
```

### Property Pattern Matching

```csharp
public string DescribePerson(Person person)
{
    return person switch
    {
        { Age: < 18 } => "Minor",
        { Age: >= 18 and < 65 } => "Adult",
        { Age: >= 65 } => "Senior",
        null => "No person",
        _ => "Unknown"
    };
}
```

### Tuple Pattern Matching

```csharp
public string GetQuadrant((int x, int y) point)
{
    return point switch
    {
        (0, 0) => "Origin",
        var (x, y) when x > 0 && y > 0 => "First quadrant",
        var (x, y) when x < 0 && y > 0 => "Second quadrant",
        var (x, y) when x < 0 && y < 0 => "Third quadrant",
        var (x, y) when x > 0 && y < 0 => "Fourth quadrant",
        var (x, 0) => $"On X-axis at {x}",
        var (0, y) => $"On Y-axis at {y}",
        _ => "Unknown"
    };
}
```

## Records

### Record Types

```csharp
// Positional record
public record Person(string FirstName, string LastName);

// Record with properties
public record Employee
{
    public int Id { get; init; }
    public string Name { get; init; }
    public decimal Salary { get; init; }
}

// Using records
var person1 = new Person("John", "Doe");
var person2 = person1 with { LastName = "Smith" }; // Non-destructive mutation

// Records have built-in equality
bool isEqual = person1 == person2; // false
```

### Record Classes vs Structs

```csharp
// Reference type record
public record class PersonRef(string Name, int Age);

// Value type record
public record struct PersonVal(string Name, int Age);
```

## Nullable Reference Types

### Enabling Nullable Context

```csharp
#nullable enable

public class Example
{
    // Non-nullable reference type
    public string NonNullableString { get; set; }
    
    // Nullable reference type
    public string? NullableString { get; set; }
    
    public void Process()
    {
        // Warning: Possible null reference
        int length = NonNullableString.Length;
        
        // No warning: Null check before access
        if (NullableString != null)
        {
            int length2 = NullableString.Length;
        }
        
        // Null-forgiving operator
        int length3 = NullableString!.Length;
        
        // Null-coalescing operator
        string result = NullableString ?? "Default";
    }
}
```

### Nullable Attributes

```csharp
public class ApiClient
{
    // Indicates method might return null
    [return: MaybeNull]
    public string GetData()
    {
        // Implementation
        return null;
    }
    
    // Indicates parameter won't be null even if marked as nullable
    public void ProcessData([NotNull] string? data)
    {
        // Implementation assumes data is not null
    }
}
```

This comprehensive C# syntax reference covers all major language features from basic syntax to advanced concepts like records and nullable reference types. It serves as a complete guide for developers working with C# in the UDEO system.