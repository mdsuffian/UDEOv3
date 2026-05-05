# Comprehensive Guide to Programming Errors

This document provides a comprehensive guide to different types of programming errors, including syntax errors, compile errors, semantic errors, and runtime errors across C#, PowerShell, and Python.

For a comprehensive reference of specific error codes and their solutions across C#, PowerShell, and Python, please refer to the [Error Reference Guide](error-reference.md).

## Table of Contents

1. [Types of Programming Errors](#types-of-programming-errors)
2. [Syntax Errors](#syntax-errors)
3. [Compile Errors](#compile-errors)
4. [Semantic Errors](#semantic-errors)
5. [Runtime Errors](#runtime-errors)
6. [Logic Errors](#logic-errors)
7. [Error Detection and Prevention](#error-detection-and-prevention)
8. [Error Handling Strategies](#error-handling-strategies)
9. [Language-Specific Error Handling](#language-specific-error-handling)
10. [Error Handling in UDEO](#error-handling-in-udeo)

## Types of Programming Errors

### Overview

Programming errors can be categorized into several types based on when they occur and their nature:

1. **Syntax Errors**: Violations of language grammar rules
2. **Compile Errors**: Errors detected during compilation
3. **Semantic Errors**: Errors in the meaning or logic of code
4. **Runtime Errors**: Errors that occur during program execution
5. **Logic Errors**: Errors where code runs but produces incorrect results

### Error Detection Timeline

```
Write Code → Syntax Check → Compile → Execute → Results
     |            |         |        |        |
  Syntax       Compile   Runtime  Logic    Correct
  Errors       Errors    Errors   Errors   Output
```

## Syntax Errors

### Definition

Syntax errors occur when code violates the grammatical rules of a programming language. These errors prevent the code from being parsed or compiled.

### C# Syntax Errors

```csharp
// Missing semicolon
int x = 5  // CS1002: ; expected
int y = 10;

// Mismatched parentheses
if (x > 0  // CS1003: ) expected
{
    Console.WriteLine("Positive");
}

// Missing closing brace
class MyClass
{   // CS1513: } expected

// Invalid identifier
int 123invalid = 0;  // CS1056: Unexpected character '1'

// Missing using directive
Console.WriteLine("Hello");  // CS0103: 'Console' does not exist in current context

// Incorrect method declaration
public void (int x)  // CS1001: Identifier expected
{
    return x;
}

// String without closing quote
string text = "Hello;  // CS1010: Newline in constant
```

### PowerShell Syntax Errors

```powershell
# Missing closing parenthesis
if ($true  # Missing closing parenthesis
{
    Write-Host "True"
}

# Invalid operator
$a = 5 +* 3  # Unexpected token '*'

# Missing closing brace
function Test-Function
{   # Missing closing brace
    Write-Host "Hello"

# Unclosed string
$text = "Hello  # Unclosed string literal

# Invalid variable name
$1invalid = "value"  # Invalid variable name

# Missing statement block
if ($true)  # Missing statement block
# No code following if statement

# Invalid hash table syntax
$hashtable = @{
    Key1 = "Value1"
    Key2 = "Value2"  # Missing closing brace
```

### Python Syntax Errors

```python
# Missing colon
if True  # SyntaxError: invalid syntax
    print("Hello")

# Mismatched parentheses
print("Hello"  # SyntaxError: unexpected EOF while parsing

# Invalid indentation
if True:
print("Not indented")  # IndentationError: expected an indented block

# Invalid identifier
def 1function():  # SyntaxError: invalid syntax
    pass

# Unclosed string
text = "Hello  # SyntaxError: EOL while scanning string literal

# Missing closing bracket
my_list = [1, 2, 3  # SyntaxError: unexpected EOF while parsing

# Invalid assignment
5 = x  # SyntaxError: cannot assign to literal
```

## Compile Errors

### Definition

Compile errors occur during the compilation process when the code is syntactically correct but cannot be compiled into executable code due to type mismatches, missing references, or other compilation issues.

### C# Compile Errors

```csharp
// Type mismatch
int x = "hello";  // CS0029: Cannot implicitly convert type 'string' to 'int'

// Method not found
string text = "hello";
int length = text.GetLength();  // CS1061: 'string' does not contain a definition for 'GetLength'

// Missing reference
var customer = new Customer();  // CS0246: The type or namespace name 'Customer' could not be found

// Accessibility issue
public class MyClass
{
    private void PrivateMethod() { }
}

public class OtherClass
{
    public void TestMethod()
    {
        MyClass mc = new MyClass();
        mc.PrivateMethod();  // CS0122: 'MyClass.PrivateMethod()' is inaccessible due to its protection level
    }
}

// Abstract class instantiation
public abstract class MyBaseClass { }

public class MyProgram
{
    public void Method()
    {
        MyBaseClass obj = new MyBaseClass();  // CS0144: Cannot create an instance of the abstract type
    }
}

// Generic type constraints violation
public class MyClass<T> where T : struct
{
    public void Method()
    {
        T obj = null;  // CS0403: Cannot convert null to type parameter 'T' because it could be a non-nullable value type
    }
}

// Overload resolution failure
public void Method(int x) { }
public void Method(string x) { }

public void Test()
{
    Method(5.5);  // CS1503: Argument 1: cannot convert from 'double' to 'int'
}
```

### PowerShell Compile Errors

```powershell
# PowerShell doesn't have traditional compile errors, but these errors are detected at parse time

# Invalid type constraint
function Test-Function()
{
    [int]$number = "hello"  # Cannot convert value "hello" to type "System.Int32"
}

# Invalid parameter set
function Test-ParameterSet
{
    [Parameter(ParameterSetName="Set1")]
    [string]$Name
    
    [Parameter(ParameterSetName="Set2")]
    [int]$Id
    
    [Parameter(ParameterSetName="Set1")]
    [Parameter(ParameterSetName="Set2")]
    [switch]$Common
}

# This would cause an error if parameters from different sets are used together
Test-ParameterSet -Name "Test" -Id 123  # Parameter set cannot be resolved
```

### Python Compile Errors

```python
# NameError (detected at compile time for some cases)
def function():
    print(undefined_variable)  # NameError: name 'undefined_variable' is not defined

# TypeError (sometimes detected at compile time)
def add_numbers(a: int, b: int) -> int:
    return a + b

result = add_numbers("5", "10")  # TypeError at runtime, but type checkers can detect this

# Import error
import non_existent_module  # ModuleNotFoundError: No module named 'non_existent_module'

# AttributeError (detected by static analyzers)
class MyClass:
    def method1(self):
        pass

obj = MyClass()
obj.method2()  # AttributeError: 'MyClass' object has no attribute 'method2'

# UnboundLocalError
def function():
    x = 1
    def inner():
        x += 1  # UnboundLocalError: local variable 'x' referenced before assignment
    inner()
```

## Semantic Errors

### Definition

Semantic errors occur when code is syntactically correct and compiles successfully, but the meaning or logic of the code is incorrect. These errors often result in unexpected behavior or incorrect results.

### C# Semantic Errors

```csharp
// Division by zero (semantic error)
int a = 10;
int b = 0;
int result = a / b;  // Compiles fine but throws DivideByZeroException at runtime

// Incorrect comparison
string input = "5";
if (input == 5)  // Comparing string to int (always false)
{
    Console.WriteLine("Equal");
}

// Off-by-one error
int[] numbers = { 1, 2, 3, 4, 5 };
for (int i = 0; i <= numbers.Length; i++)  // Should be i < numbers.Length
{
    Console.WriteLine(numbers[i]);  // IndexOutOfRangeException at i=5
}

// Incorrect operator precedence
int result = 5 + 3 * 2;  // Results in 11, not 16 (if you wanted (5+3)*2)

// Null reference error
string text = null;
int length = text.Length;  // Compiles fine but throws NullReferenceException at runtime

// Type conversion error
object obj = "hello";
int number = (int)obj;  // Compiles but throws InvalidCastException at runtime

// Infinite loop
int x = 0;
while (x < 10)
{
    Console.WriteLine(x);
    // Forgot to increment x
}

// Incorrect boolean logic
bool isLoggedIn = true;
bool isAdmin = false;
if (isLoggedIn = true)  // Assignment instead of comparison (should be isLoggedIn == true)
{
    Console.WriteLine("User is admin");  // Always executes
}
```

### PowerShell Semantic Errors

```powershell
# Division by zero
$a = 10
$b = 0
$result = $a / $b  # Attempted to divide by zero

# Incorrect comparison
$input = "5"
if ($input -eq 5) {  # String vs integer comparison
    Write-Host "Equal"
}

# Off-by-one error
$numbers = 1..5
for ($i = 0; $i -le $numbers.Count; $i++) {  # Should be -lt
    Write-Host $numbers[$i]  # Index out of range at i=5
}

# Null reference error
$text = $null
$length = $text.Length  # Cannot invoke method because its value is null

# Incorrect operator precedence
$result = 5 + 3 * 2  # Results in 11, not 16

# Infinite loop
$x = 0
while ($x -lt 10) {
    Write-Host $x
    # Forgot to increment $x
}

# Incorrect boolean logic
$isLoggedIn = $true
$isAdmin = $false
if ($isLoggedIn = $true) {  # Assignment instead of comparison
    Write-Host "User is admin"
}
```

### Python Semantic Errors

```python
# Division by zero
a = 10
b = 0
result = a / b  # ZeroDivisionError: division by zero

# Incorrect comparison
input_val = "5"
if input_val == 5:  # String vs integer comparison
    print("Equal")

# Off-by-one error
numbers = [1, 2, 3, 4, 5]
for i in range(len(numbers) + 1):  # Should be len(numbers)
    print(numbers[i])  # IndexError at i=5

# NoneType error
text = None
length = len(text)  # TypeError: object of type 'NoneType' has no len()

# Type conversion error
text = "hello"
number = int(text)  # ValueError: invalid literal for int() with base 10: 'hello'

# Infinite loop
x = 0
while x < 10:
    print(x)
    # Forgot to increment x

# Incorrect boolean logic
is_logged_in = True
is_admin = False
if is_logged_in = True:  # Assignment instead of comparison
    print("User is admin")
```

## Runtime Errors

### Definition

Runtime errors occur during program execution. The code may be syntactically correct and even semantically correct in some cases, but errors arise due to unexpected conditions or inputs.

### C# Runtime Errors

```csharp
// NullReferenceException
string text = null;
Console.WriteLine(text.Length);  // Throws NullReferenceException

// IndexOutOfRangeException
int[] array = { 1, 2, 3 };
Console.WriteLine(array[5]);  // Throws IndexOutOfRangeException

// InvalidOperationException
List<string> list = new List<string>();
string first = list.First();  // Throws InvalidOperationException

// ArgumentException
string path = "invalid|path";
File.ReadAllText(path);  // Throws ArgumentException

// FormatException
string invalidNumber = "abc";
int number = int.Parse(invalidNumber);  // Throws FormatException

// SocketException
using var client = new TcpClient();
client.Connect("invalid-host-name", 80);  // Throws SocketException

// OutOfMemoryException
byte[] hugeArray = new byte[int.MaxValue];  // Throws OutOfMemoryException

// StackOverflowException
public void RecursiveMethod()
{
    RecursiveMethod();  // Eventually throws StackOverflowException
}
```

### PowerShell Runtime Errors

```powershell
# InvalidOperation
$null.Length  # Cannot invoke method because its value is null

# ParameterBindingValidationException
function Test-Parameter {
    [Parameter(Mandatory=$true)]
    [string]$Name
}
Test-Parameter  # Throws ParameterBindingValidationException

# ItemNotFoundException
Get-Item "C:\NonExistentFile.txt"  # Throws ItemNotFoundException

# InvalidOperationException
Get-ChildItem | Where-Object { $_.NonExistentProperty -eq "value" }

# PipelineStoppedException
Get-Process | ForEach-Object { 
    if ($_.ProcessName -eq "explorer") { 
        throw "Stop pipeline" 
    } 
}

# ScriptHalted
$ErrorActionPreference = "Stop"
Get-Content "NonExistentFile.txt"  # Script halts on error
```

### Python Runtime Errors

```python
# AttributeError
text = "hello"
text.non_existent_method()  # AttributeError: 'str' object has no attribute 'non_existent_method'

# KeyError
my_dict = {"key1": "value1"}
value = my_dict["key2"]  # KeyError: 'key2'

# IndexError
my_list = [1, 2, 3]
value = my_list[5]  # IndexError: list index out of range

# TypeError
result = "5" + 5  # TypeError: can only concatenate str (not "int") to str

# ValueError
number = int("abc")  # ValueError: invalid literal for int() with base 10: 'abc'

# FileNotFoundError
with open("non_existent_file.txt") as f:  # FileNotFoundError
    content = f.read()

# ZeroDivisionError
result = 10 / 0  # ZeroDivisionError: division by zero

# MemoryError
# huge_list = [0] * 10**100  # MemoryError (if system doesn't have enough memory)
```

## Logic Errors

### Definition

Logic errors occur when the code runs without throwing any exceptions, but produces incorrect results due to flaws in the algorithm or logic.

### C# Logic Errors

```csharp
// Incorrect formula (should be π * r²)
double CalculateCircleArea(double radius)
{
    return 2 * Math.PI * radius;  // Wrong formula (returns circumference)
}

// Incorrect condition
bool IsEligibleForDiscount(int age, bool isStudent)
{
    return age < 18 || isStudent;  // Should be age > 65 || isStudent
}

// Incorrect loop bounds
int[] numbers = { 1, 2, 3, 4, 5 };
int sum = 0;
for (int i = 1; i < numbers.Length; i++)  // Starts at 1, skips first element
{
    sum += numbers[i];
}

// Incorrect comparison operator
int score = 85;
if (score > 90)  // Should be >= for inclusive range
{
    Console.WriteLine("Excellent");
}

// Incorrect return value
int CalculateAverage(int[] numbers)
{
    int sum = 0;
    foreach (int num in numbers)
    {
        sum += num;
    }
    return sum;  // Should return sum / numbers.Length
}
```

### PowerShell Logic Errors

```powershell
# Incorrect formula
function Get-CircleArea {
    param([double]$radius)
    return 2 * [Math]::PI * $radius  # Wrong formula
}

# Incorrect condition
function Test-DiscountEligibility {
    param([int]$age, [bool]$isStudent)
    return $age -lt 18 -or $isStudent  # Wrong logic
}

# Incorrect loop bounds
$numbers = 1..5
$sum = 0
for ($i = 1; $i -lt $numbers.Count; $i++) {  # Skips first element
    $sum += $numbers[$i]
}

# Incorrect comparison
$score = 85
if ($score -gt 90) {  # Should be -ge
    Write-Host "Excellent"
}

# Incorrect return value
function Get-Average {
    param([int[]]$numbers)
    $sum = 0
    foreach ($num in $numbers) {
        $sum += $num
    }
    return $sum  # Should return $sum / $numbers.Count
}
```

### Python Logic Errors

```python
# Incorrect formula
def calculate_circle_area(radius):
    return 2 * math.pi * radius  # Wrong formula

# Incorrect condition
def is_eligible_for_discount(age, is_student):
    return age < 18 or is_student  # Wrong logic

# Incorrect loop bounds
numbers = [1, 2, 3, 4, 5]
total = 0
for i in range(1, len(numbers)):  # Skips first element
    total += numbers[i]

# Incorrect comparison
score = 85
if score > 90:  # Should be >=
    print("Excellent")

# Incorrect return value
def calculate_average(numbers):
    total = 0
    for num in numbers:
        total += num
    return total  # Should return total / len(numbers)
```

## Error Detection and Prevention

### Static Analysis Tools

#### C# Static Analysis

```csharp
// Using Roslyn Analyzers
class Program
{
    // CA1801: Review unused parameters
    public void UnusedParameter(int unused)  // Analyzer warning
    {
        Console.WriteLine("Hello");
    }
    
    // CA1822: Mark members as static
    public void StaticMethod()  // Can be marked as static
    {
        Console.WriteLine("Static method");
    }
}

// Code contracts for runtime checking
using System.Diagnostics.Contracts;

public class MathOperations
{
    [ContractArgumentValidation]
    public static int Divide(int numerator, int denominator)
    {
        Contract.Requires(denominator != 0);
        Contract.Ensures(Contract.Result<int>() == numerator / denominator);
        
        return numerator / denominator;
    }
}
```

#### PowerShell Static Analysis

```powershell
# Using PSScriptAnalyzer
# Install-Module PSScriptAnalyzer

function Test-Function {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Name
    )
    
    # PSScriptAnalyzer can detect:
    # - Unused parameters
    # - Missing return types
    # - Inconsistent indentation
    # - Potential security issues
    
    Write-Output "Hello, $Name"
}

# Analyze script
Invoke-ScriptAnalyzer -Path ".\myscript.ps1"
```

#### Python Static Analysis

```python
# Using pylint, flake8, mypy

from typing import List

def process_numbers(numbers: List[int]) -> int:
    """
    Process numbers and return sum.
    
    Args:
        numbers: List of integers
        
    Returns:
        Sum of numbers
    """
    total = 0
    for num in numbers:
        total += num
    return total

# Type checking with mypy
# mypy script.py

# Linting with pylint
# pylint script.py

# Style checking with flake8
# flake8 script.py
```

### Unit Testing for Error Detection

#### C# Unit Testing

```csharp
using NUnit.Framework;
using System;

[TestFixture]
public class MathOperationsTests
{
    [Test]
    public void Divide_ValidInput_ReturnsCorrectResult()
    {
        // Arrange
        int numerator = 10;
        int denominator = 2;
        
        // Act
        int result = MathOperations.Divide(numerator, denominator);
        
        // Assert
        Assert.AreEqual(5, result);
    }
    
    [Test]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        // Arrange
        int numerator = 10;
        int denominator = 0;
        
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => 
            MathOperations.Divide(numerator, denominator));
    }
}
```

#### PowerShell Unit Testing

```powershell
# Using Pester

Describe "MathOperations" {
    Context "Divide" {
        It "Valid input returns correct result" {
            $result = Divide-NumeratorDenominator -Numerator 10 -Denominator 2
            $result | Should -Be 5
        }
        
        It "Division by zero throws exception" {
            { Divide-NumeratorDenominator -Numerator 10 -Denominator 0 } | 
                Should -Throw "Cannot divide by zero"
        }
    }
}
```

#### Python Unit Testing

```python
import unittest
from math_operations import divide

class TestMathOperations(unittest.TestCase):
    def test_divide_valid_input(self):
        """Test division with valid input."""
        result = divide(10, 2)
        self.assertEqual(result, 5)
    
    def test_divide_by_zero(self):
        """Test division by zero raises exception."""
        with self.assertRaises(ZeroDivisionError):
            divide(10, 0)
    
    def test_divide_negative_numbers(self):
        """Test division with negative numbers."""
        result = divide(-10, 2)
        self.assertEqual(result, -5)

if __name__ == "__main__":
    unittest.main()
```

## Error Handling Strategies

### Defensive Programming

#### C# Defensive Programming

```csharp
public class UserAccount
{
    public string Username { get; }
    public int Age { get; }
    
    public UserAccount(string username, int age)
    {
        // Defensive checks
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        
        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters", nameof(username));
        
        if (age < 0 || age > 150)
            throw new ArgumentOutOfRangeException(nameof(age), "Age must be between 0 and 150");
        
        Username = username;
        Age = age;
    }
    
    public bool CanPurchaseAlcohol()
    {
        // Defensive check
        return Age >= 21;
    }
}
```

#### PowerShell Defensive Programming

```powershell
function New-UserAccount {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [ValidateLength(3, 50)]
        [string]$Username,
        
        [Parameter(Mandatory=$true)]
        [ValidateRange(0, 150)]
        [int]$Age
    )
    
    # Defensive checks are handled by validation attributes
    # Additional business logic validation
    if ($Username -notmatch '^[a-zA-Z0-9_]+$') {
        throw "Username can only contain alphanumeric characters and underscores"
    }
    
    return [PSCustomObject]@{
        Username = $Username
        Age = $Age
    }
}
```

#### Python Defensive Programming

```python
class UserAccount:
    def __init__(self, username: str, age: int):
        # Defensive checks
        if not username or not isinstance(username, str):
            raise ValueError("Username must be a non-empty string")
        
        if len(username) < 3 or len(username) > 50:
            raise ValueError("Username must be between 3 and 50 characters")
        
        if not isinstance(age, int) or age < 0 or age > 150:
            raise ValueError("Age must be an integer between 0 and 150")
        
        self.username = username
        self.age = age
    
    def can_purchase_alcohol(self) -> bool:
        """Check if user can purchase alcohol based on age."""
        return self.age >= 21
```

### Input Validation

#### C# Input Validation

```csharp
using System.ComponentModel.DataAnnotations;

public class OrderRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public int ProductId { get; set; }
    
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
    
    [RegularExpression(@"^[A-Z0-9]{5}$", ErrorMessage = "Invalid postal code format")]
    public string PostalCode { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
}

public class OrderProcessor
{
    public void ProcessOrder(OrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            throw new ValidationException(string.Join(", ", 
                validationResults.Select(r => r.ErrorMessage)));
        }
        
        // Process valid order
    }
}
```

#### PowerShell Input Validation

```powershell
function New-OrderRequest {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [ValidateRange(1, 100)]
        [int]$ProductId,
        
        [Parameter(Mandatory=$true)]
        [ValidateRange(1, 100)]
        [int]$Quantity,
        
        [Parameter(Mandatory=$true)]
        [ValidatePattern("^[A-Z0-9]{5}$")]
        [string]$PostalCode,
        
        [Parameter(Mandatory=$true)]
        [ValidatePattern("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        [string]$Email
    )
    
    # Additional validation
    if ($Quantity -gt 10 -and $PostalCode -eq "12345") {
        throw "Large quantities cannot be shipped to postal code 12345"
    }
    
    return [PSCustomObject]@{
        ProductId = $ProductId
        Quantity = $Quantity
        PostalCode = $PostalCode
        Email = $Email
    }
}
```

#### Python Input Validation

```python
import re
from dataclasses import dataclass
from typing import Optional

@dataclass
class OrderRequest:
    product_id: int
    quantity: int
    postal_code: str
    email: Optional[str] = None
    
    def __post_init__(self):
        """Validate fields after initialization."""
        if not (1 <= self.product_id <= 100):
            raise ValueError("Product ID must be between 1 and 100")
        
        if not (1 <= self.quantity <= 100):
            raise ValueError("Quantity must be between 1 and 100")
        
        if not re.match(r'^[A-Z0-9]{5}$', self.postal_code):
            raise ValueError("Invalid postal code format")
        
        if self.email and not re.match(r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$', self.email):
            raise ValueError("Invalid email address format")

class OrderProcessor:
    def process_order(self, request: OrderRequest):
        """Process a validated order request."""
        # Additional business logic validation
        if request.quantity > 10 and request.postal_code == "12345":
            raise ValueError("Large quantities cannot be shipped to postal code 12345")
        
        # Process valid order
        print(f"Processing order for product {request.product_id}")
```

## Language-Specific Error Handling

### C# Error Handling Best Practices

```csharp
// 1. Use specific exception types
try
{
    ProcessFile(filePath);
}
catch (FileNotFoundException ex)
{
    logger.LogError(ex, "File not found: {FilePath}", filePath);
    throw new BusinessException($"File not found: {filePath}", "FILE_NOT_FOUND", ex);
}
catch (UnauthorizedAccessException ex)
{
    logger.LogError(ex, "Access denied: {FilePath}", filePath);
    throw new BusinessException($"Access denied: {filePath}", "ACCESS_DENIED", ex);
}

// 2. Use exception filters for conditional handling
try
{
    ProcessData(data);
}
catch (ArgumentException ex) when (ex.ParamName == "data")
{
    logger.LogError(ex, "Invalid data parameter");
    throw;
}

// 3. Properly dispose resources
using var connection = new DatabaseConnection(connectionString);
try
{
    connection.Execute(query);
}
catch (DatabaseException ex)
{
    logger.LogError(ex, "Database error");
    throw;
}

// 4. Use custom exceptions for business logic
public class BusinessException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

// 5. Don't catch generic Exception unless absolutely necessary
try
{
    RiskyOperation();
}
catch (Exception ex)
{
    // Only catch Exception at the top level for logging
    logger.LogCritical(ex, "Unhandled exception");
    throw;
}
```

### PowerShell Error Handling Best Practices

```powershell
# 1. Use ErrorAction parameter appropriately
try {
    Get-Content "nonexistent.txt" -ErrorAction Stop
}
catch {
    Write-Error "File not found"
}

# 2. Use $ErrorActionPreference to control error behavior
$ErrorActionPreference = "Stop"  # Treat all errors as terminating

# 3. Use Try-Catch with specific exception types
try {
    Get-ADUser "nonexistent" -ErrorAction Stop
}
catch [Microsoft.ActiveDirectory.Management.ADIdentityNotFoundException] {
    Write-Warning "User not found"
}
catch {
    Write-Error "Unexpected error: $($_.Exception.Message)"
}

# 4. Validate parameters before processing
function Get-UserSafe {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Username
    )
    
    try {
        return Get-ADUser $Username -ErrorAction Stop
    }
    catch [Microsoft.ActiveDirectory.Management.ADIdentityNotFoundException] {
        Write-Error "User '$Username' not found"
        return $null
    }
}

# 5. Use -ErrorVariable to capture errors without terminating
Get-Process "nonexistent" -ErrorVariable procError -ErrorAction SilentlyContinue
if ($procError) {
    Write-Warning "Process not found: $($procError[0].Exception.Message)"
}
```

### Python Error Handling Best Practices

```python
# 1. Use specific exception types
try:
    process_file(file_path)
except FileNotFoundError:
    logger.error(f"File not found: {file_path}")
    raise BusinessException(f"File not found: {file_path}", "FILE_NOT_FOUND")
except PermissionError:
    logger.error(f"Permission denied: {file_path}")
    raise BusinessException(f"Permission denied: {file_path}", "ACCESS_DENIED")

# 2. Use context managers for resource management
try:
    with open(file_path, 'r') as file:
        content = file.read()
        process_content(content)
except IOError as e:
    logger.error(f"IO error: {e}")
    raise

# 3. Use custom exceptions for business logic
class BusinessException(Exception):
    def __init__(self, message, error_code):
        super().__init__(message)
        self.error_code = error_code

# 4. Use finally for cleanup
try:
    resource = acquire_resource()
    process_with_resource(resource)
finally:
    release_resource(resource)

# 5. Don't catch bare Exception unless absolutely necessary
try:
    risky_operation()
except Exception as e:
    # Only catch Exception at the top level for logging
    logger.critical(f"Unhandled exception: {e}", exc_info=True)
    raise

# 6. Use exception chaining
try:
    process_data(data)
except ValueError as e:
    raise BusinessException("Invalid data format", "INVALID_DATA") from e
```

## Error Handling in UDEO

### UDEO Error Handling Strategy

The UDEO system implements a comprehensive error handling strategy that ensures reliability and maintainability:

1. **Layered Error Handling**: Each layer handles errors appropriate to its level
2. **Standardized Error Codes**: Consistent error codes across the system
3. **Centralized Logging**: All errors are logged with context
4. **Graceful Degradation**: System continues operating at reduced capacity when possible
5. **Error Recovery**: Automatic recovery mechanisms for common failure scenarios

### UDEO Error Classification

```csharp
namespace UDEO.Core.Errors
{
    // Error severity levels
    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    // Error categories
    public enum ErrorCategory
    {
        Validation,
        Business,
        System,
        Integration,
        Security
    }
    
    // Standard error response
    public class ErrorResponse
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public ErrorSeverity Severity { get; set; }
        public ErrorCategory Category { get; set; }
        public Dictionary<string, object> Context { get; set; }
        public string CorrelationId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
```

### UDEO Error Handling Implementation

```csharp
// Expert error handling
public class ExpertBase
{
    protected ILogger Logger { get; }
    protected IErrorHandler ErrorHandler { get; }
    
    protected async Task<ExpertResponse> ExecuteWithErrorHandlingAsync(
        Func<Task<ExpertResponse>> operation,
        ExpertRequest request)
    {
        try
        {
            return await operation();
        }
        catch (ValidationException ex)
        {
            return HandleValidationException(ex, request);
        }
        catch (BusinessException ex)
        {
            return HandleBusinessException(ex, request);
        }
        catch (SystemException ex)
        {
            return HandleSystemException(ex, request);
        }
        catch (Exception ex)
        {
            return HandleUnexpectedException(ex, request);
        }
    }
    
    private ExpertResponse HandleValidationException(
        ValidationException ex, 
        ExpertRequest request)
    {
        Logger.LogWarning(ex, "Validation error in expert {ExpertId}", GetType().Name);
        
        return new ExpertResponse
        {
            IsSuccess = false,
            ErrorCode = "VALIDATION_ERROR",
            Message = ex.Message,
            CorrelationId = request.CorrelationId
        };
    }
}
```

This comprehensive guide to programming errors covers all types of errors across C#, PowerShell, and Python, providing practical examples and best practices for error detection, prevention, and handling in the UDEO system.