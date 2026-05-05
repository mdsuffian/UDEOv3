
# PowerShell Syntax Reference

This document provides a comprehensive reference for PowerShell syntax, covering all language constructs from basic to advanced features.

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Variables and Data Types](#variables-and-data-types)
3. [Operators](#operators)
4. [Arrays and Collections](#arrays-and-collections)
5. [Control Flow](#control-flow)
6. [Functions and Scripts](#functions-and-scripts)
7. [Objects and Properties](#objects-and-properties)
8. [Error Handling](#error-handling)
9. [Modules and Snap-ins](#modules-and-snap-ins)
10. [Pipeline](#pipeline)
11. [Regular Expressions](#regular-expressions)
12. [File System Operations](#file-system-operations)
13. [WMI and CIM](#wmi-and-cim)
14. [XML and JSON Handling](#xml-and-json-handling)
15. [Classes (PowerShell 5.0+)](#classes-powershell-50)
16. [DSC (Desired State Configuration)](#dsc-desired-state-configuration)
17. [Advanced Features](#advanced-features)

## Basic Syntax

### Command Structure

```powershell
# Basic command syntax
Get-Process -Name "notepad" -ErrorAction Stop

# Command with parameters
Get-ChildItem -Path "C:\Temp" -Recurse -Filter "*.txt"

# Using aliases
ls -Path "C:\Temp" -Recurse -Filter "*.txt"
dir -Path "C:\Temp" -Recurse -Filter "*.txt"
```

### Comments

```powershell
# Single-line comment

<#
Multi-line comment
that spans multiple lines
#>

# Comment-based help
<#
.SYNOPSIS
    Brief description of the function
.DESCRIPTION
    Detailed description of the function
.PARAMETER ParameterName
    Description of the parameter
.EXAMPLE
    Example of how to use the function
#>
```

### Case Sensitivity

```powershell
# PowerShell is generally case-insensitive
$variable = "value"
$VARIABLE = "value"  # Same variable

# Some operations are case-sensitive
"Hello" -eq "hello"  # True (case-insensitive comparison)
"Hello" -ceq "hello" # False (case-sensitive comparison)
```

## Variables and Data Types

### Variable Declaration

```powershell
# Basic variable assignment
$name = "John"
$age = 30
$isStudent = $false

# Variable names can include special characters
${special-variable} = "value"
${variable with spaces} = "value"

# Using Set-Variable cmdlet
Set-Variable -Name "city" -Value "New York" -Option ReadOnly

# Variable scopes
$global:globalVar = "Global scope"
$script:scriptVar = "Script scope"
$local:localVar = "Local scope"  # Default scope
$private:privateVar = "Private scope"
```

### Data Types

```powershell
# Strings
$string1 = "Hello World"
$string2 = 'Hello World'
$string3 = @"
Multiline string
that spans multiple lines
"@

# Numbers
$integer = 42
$double = 3.14
$scientific = 1.23e-4

# Arrays
$array = 1, 2, 3, 4, 5
$array2 = @(1, 2, 3, 4, 5)
$emptyArray = @()

# Hashtables
$hashtable = @{
    Name = "John"
    Age = 30
    City = "New York"
}

# Booleans
$true
$false

# Null
$null

# Type casting
$stringNumber = "123"
$number = [int]$stringNumber
$datetime = [datetime]"2023-01-01"
```

### Type Accelerators

```powershell
# Common type accelerators
[string]$text = "Hello"
[int]$number = 42
[datetime]$date = Get-Date
[xml]$xml = Get-Content "file.xml"
[regex]$pattern = "\d+"
[scriptblock]$script = { Get-Process }

# Custom types
[adsi]$adsi = "LDAP://CN=Users,DC=domain,DC=com"
[wmi]$wmi = Get-WmiObject -Class Win32_OperatingSystem
```

## Operators

### Arithmetic Operators

```powershell
$a = 10
$b = 3

$result = $a + $b   # 13 (Addition)
$result = $a - $b   # 7 (Subtraction)
$result = $a * $b   # 30 (Multiplication)
$result = $a / $b   # 3.333... (Division)
$result = $a % $b   # 1 (Modulus)
$result = $a ** $b  # 1000 (Exponentiation)
```

### Assignment Operators

```powershell
$a = 10      # Assignment
$a += 5      # $a = $a + 5 (15)
$a -= 3      # $a = $a - 3 (12)
$a *= 2      # $a = $a * 2 (24)
$a /= 4      # $a = $a / 4 (6)
$a %= 4      # $a = $a % 4 (2)
$a++, ++$a   # Increment (returns original value, then increments)
$a--, --$a   # Decrement (returns original value, then decrements)
```

### Comparison Operators

```powershell
$a = 10
$b = "10"

$a -eq $b     # True (Equal)
$a -ne $b     # False (Not equal)
$a -gt $b     # False (Greater than)
$a -ge $b     # True (Greater than or equal)
$a -lt $b     # False (Less than)
$a -le $b     # True (Less than or equal)

# Case-sensitive versions
$a -ceq $b    # False (Case-sensitive equal)
$a -cne $b    # True (Case-sensitive not equal)
$a -cgt $b    # False (Case-sensitive greater than)
$a -cge $b    # False (Case-sensitive greater than or equal)
$a -clt $b    # False (Case-sensitive less than)
$a -cle $b    # False (Case-sensitive less than or equal)
```

### Logical Operators

```powershell
$a = $true
$b = $false

$a -and $b    # False (Logical AND)
$a -or $b     # True (Logical OR)
-a $a         # False (Logical NOT)
$a -xor $b    # True (Logical XOR)
```

### String Operators

```powershell
$string1 = "Hello"
$string2 = "World"

# Concatenation
$result = $string1 + " " + $string2  # "Hello World"

# Multiplication
$result = "=" * 20  # "===================="

# Contains
$string1 -contains "H"      # True
$string1 -notcontains "X"   # True

# StartsWith/EndsWith
$string1 -like "H*"         # True (Starts with H)
$string1 -like "*o"         # True (Ends with o)
$string1 -notlike "*X*"     # True

# Match (regular expression)
$string1 -match "\w+"        # True (Matches word characters)
$string1 -notmatch "\d+"     # True (Doesn't match digits)
```

### Redirection Operators

```powershell
# Output redirection
Get-Process > "processes.txt"           # Redirect to file (overwrite)
Get-Process >> "processes.txt"          # Append to file
Get-Process 2> "errors.txt"             # Redirect errors
Get-Process 2>&1 > "output.txt"         # Redirect both output and errors

# Input redirection
Get-Content "input.txt" | ForEach-Object { $_ }

# Pipeline to null
Get-Process | Out-Null
```

## Arrays and Collections

### Arrays

```powershell
# Creating arrays
$array1 = 1, 2, 3, 4, 5
$array2 = @(1, 2, 3, 4, 5)
$array3 = @()
$array4 = @("apple", "banana", "cherry")

# Accessing elements
$first = $array1[0]      # First element
$last = $array1[-1]      # Last element
$range = $array1[0..2]   # Elements 0, 1, 2
$skip = $array1[2..$array1.Length-1]  # Skip first 2 elements

# Array operations
$array1 += 6             # Add element (creates new array)
$array1[0] = 10          # Change element

# Counting elements
$count = $array1.Count
$count = $array1.Length

# Checking if array contains value
$contains = $array1 -contains 3

# Iterating through array
foreach ($item in $array1) {
    Write-Host $item
}

for ($i = 0; $i -lt $array1.Count; $i++) {
    Write-Host $array1[$i]
}
```

### Multidimensional Arrays

```powershell
# 2D array
$matrix = @(
    @(1, 2, 3),
    @(4, 5, 6),
    @(7, 8, 9)
)

# Accessing elements
$value = $matrix[1][2]  # 6 (second row, third column)

# Iterating through 2D array
for ($i = 0; $i -lt $matrix.Count; $i++) {
    for ($j = 0; $j -lt $matrix[$i].Count; $j++) {
        Write-Host $matrix[$i][$j]
    }
}
```

### ArrayLists

```powershell
# Creating ArrayList
$list = New-Object System.Collections.ArrayList
$list = [System.Collections.ArrayList]@()

# Adding elements
$list.Add("Item 1")
$list.AddRange(@("Item 2", "Item 3"))
$list.Insert(1, "Inserted Item")

# Removing elements
$list.Remove("Item 2")
$list.RemoveAt(0)
$list.RemoveRange(0, 2)

# Sorting
$list.Sort()
```

### Generic Lists

```powershell
# Creating generic list
$list = New-Object System.Collections.Generic.List[string]
$list = [System.Collections.Generic.List[string]]::new()

# Adding elements
$list.Add("Item 1")
$list.Add("Item 2")

# Checking if contains
$contains = $list.Contains("Item 1")

# Finding items
$item = $list.Find({ param($x) $x -eq "Item 1" })
$items = $list.FindAll({ param($x) $x.StartsWith("Item") })
```

## Control Flow

### If Statements

```powershell
# Basic if
if ($condition) {
    # Code to execute if condition is true
}

# If-else
if ($condition) {
    # Code to execute if condition is true
} else {
    # Code to execute if condition is false
}

# If-elseif-else
if ($condition1) {
    # Code to execute if condition1 is true
} elseif ($condition2) {
    # Code to execute if condition2 is true
} else {
    # Code to execute if all conditions are false
}

# Ternary operator (PowerShell 7.0+)
$result = $condition ? "Value if true" : "Value if false"
```

### Switch Statements

```powershell
# Basic switch
switch ($value) {
    1 { "One" }
    2 { "Two" }
    3 { "Three" }
    default { "Other" }
}

# Switch with conditions
switch ($value) {
    { $_ -lt 10 } { "Less than 10" }
    { $_ -lt 100 } { "Less than 100" }
    default { "100 or more" }
}

# Switch with wildcard matching
switch -Wildcard ($string) {
    "a*" { "Starts with a" }
    "*z" { "Ends with z" }
    default { "Other" }
}

# Switch with regex matching
switch -Regex ($string) {
    "^\d+$" { "All digits" }
    "^[a-zA-Z]+$" { "All letters" }
    default { "Mixed or special characters" }
}

# Switch with file content
switch -File "config.txt" {
    "debug=true" { $debugMode = $true }
    "version=" { $version = $_.Split("=")[1] }
}
```

### Loops

```powershell
# For loop
for ($i = 0; $i -lt 10; $i++) {
    Write-Host $i
}

# ForEach loop (collection)
$array = 1, 2, 3, 4, 5
foreach ($item in $array) {
    Write-Host $item
}

# ForEach-Object (pipeline)
$array | ForEach-Object {
    Write-Host $_
}

# While loop
$count = 0
while ($count -lt 10) {
    Write-Host $count
    $count++
}

# Do-While loop
$count = 0
do {
    Write-Host $count
    $count++
} while ($count -lt 10)

# Do-Until loop
$count = 0
do {
    Write-Host $count
    $count++
} until ($count -ge 10)
```

### Loop Control

```powershell
# Break
for ($i = 0; $i -lt 10; $i++) {
    if ($i -eq 5) {
        break  # Exit loop
    }
    Write-Host $i
}

# Continue
for ($i = 0; $i -lt 10; $i++) {
    if ($i % 2 -eq 0) {
        continue  # Skip to next iteration
    }
    Write-Host $i
}

# Return (exit function/script)
function Test-Return {
    for ($i = 0; $i -lt 10; $i++) {
        if ($i -eq 5) {
            return $i  # Exit function and return value
        }
        Write-Host $i
    }
}
```

## Functions and Scripts

### Function Declaration

```powershell
# Basic function
function Write-Hello {
    Write-Host "Hello, World!"
}

# Function with parameters
function Write-Greeting {
    param (
        [string]$Name,
        [int]$Age
    )
    
    Write-Host "Hello, $Name! You are $Age years old."
}

# Function with parameter attributes
function Get-UserInfo {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$UserName,
        
        [Parameter(Mandatory=$false)]
        [int]$Age,
        
        [Parameter(Mandatory=$false)]
        [switch]$IncludeDetails
    )
    
    if ($IncludeDetails) {
        return @{
            Name = $UserName
            Age = $Age
            Timestamp = Get-Date
        }
    } else {
        return $UserName
    }
}
```

### Advanced Function Parameters

```powershell
function Advanced-Function {
    [CmdletBinding()]
    param (
        # Mandatory parameter
        [Parameter(Mandatory=$true)]
        [string]$RequiredParam,
        
        # Parameter from pipeline
        [Parameter(ValueFromPipeline=$true)]
        [string]$PipelineParam,
        
        # Parameter from pipeline property name
        [Parameter(ValueFromPipelineByPropertyName=$true)]
        [string]$NameParam,
        
        # Parameter set
        [Parameter(ParameterSetName="Set1")]
        [string]$Set1Param,
        
        [Parameter(ParameterSetName="Set2")]
        [string]$Set2Param,
        
        # Validate set
        [Parameter(Mandatory=$false)]
        [ValidateSet("Option1", "Option2", "Option3")]
        [string]$Option,
        
        # Validate range
        [Parameter(Mandatory=$false)]
        [ValidateRange(1, 100)]
        [int]$Number,
        
        # Validate pattern
        [Parameter(Mandatory=$false)]
        [ValidatePattern("\d{3}-\d{2}-\d{4}")]
        [string]$SSN
    )
    
    # Function implementation
}
```

### Return Values

```powershell
# Single return value
function Get-Square {
    param ([int]$Number)
    return $Number * $Number
}

# Multiple return values (array)
function Get-UserInfo {
    param ([string]$UserName)
    
    $user = Get-ADUser $UserName
    $groups = Get-ADPrincipalGroupMembership $user
    
    return $user, $groups
}

# Multiple return values (hashtable)
function Get-SystemInfo {
    return @{
        OS = (Get-WmiObject -Class Win32_OperatingSystem).Caption
        Memory = (Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory
        Disk = Get-WmiObject -Class Win32_LogicalDisk
    }
}

# Pipeline output
function Get-ProcessInfo {
    Get-Process | ForEach-Object {
        [PSCustomObject]@{
            Name = $_.Name
            ID = $_.ID
            Memory = $_.WorkingSet
        }
    }
}
```

### Script Blocks

```powershell
# Creating script block
$scriptBlock = {
    param ([string]$Name)
    Write-Host "Hello, $Name!"
}

# Executing script block
& $scriptBlock "World"

# Using Invoke-Command
Invoke-Command -ScriptBlock $scriptBlock -ArgumentList "PowerShell"

# Creating delayed script block
$delayed = [scriptblock]::Create("Get-Date")
& $delayed

# Script block with scope
$scriptBlock = {
    $localVar = "Local"
    $script:scriptVar = "Script"
    $global:globalVar = "Global"
}
```

## Objects and Properties

### Creating Objects

```powershell
# Creating custom object with Select-Object
$object = Get-Process | Select-Object -First 1 | Select-Object Name, ID, WorkingSet

# Creating PSCustomObject
$person = [PSCustomObject]@{
    Name = "John Doe"
    Age = 30
    City = "New York"
}

# Creating object with New-Object
$process = New-Object System.Diagnostics.Process
$process.StartInfo.FileName = "notepad.exe"
$process.Start()

# Creating object with Add-Member
$object = New-Object PSObject
$object | Add-Member -MemberType NoteProperty -Name "Name" -Value "John"
$object | Add-Member -MemberType ScriptMethod -Name "ToString" -Value { return $this.Name }
```

### Working with Objects

```powershell
# Accessing properties
$name = $person.Name
$age = $person.Age

# Setting properties
$person.Age = 31

# Calling methods
$process.Start()

# Getting object members
$members = $object | Get-Member
$properties = $object | Get-Member -MemberType Properties
$methods = $object | Get-Member -MemberType Methods

# Checking if property exists
if ($object.PSObject.Properties.Name -contains "Name") {
    $name = $object.Name
}

# Adding custom properties
$object | Add-Member -MemberType NoteProperty -Name "CustomProperty" -Value "Custom Value"
```

### Object Comparison

```powershell
# Comparing objects
$obj1 = [PSCustomObject]@{ Name = "John"; Age = 30 }
$obj2 = [PSCustomObject]@{ Name = "John"; Age = 30 }
$obj3 = [PSCustomObject]@{ Name = "John"; Age = 31 }

# Compare-Object
$diff = Compare-Object $obj1 $obj2 -Property Name, Age
$diff = Compare-Object $obj1 $obj3 -Property Name, Age

# Custom comparison
function Compare-Objects {
    param (
        [psobject]$Object1,
        [psobject]$Object2,
        [string[]]$Properties
    )
    
    $result = @()
    
    foreach ($prop in $Properties) {
        $value1 = $Object1.$prop
        $value2 = $Object2.$prop
        
        if ($value1 -ne $value2) {
            $result += [PSCustomObject]@{
                Property = $prop
                Object1Value = $value1
                Object2Value = $value2
            }
        }
    }
    
    return $result
}
```

## Error Handling

### Error Types

```powershell
# Terminating errors
throw "This is a terminating error"

# Non-terminating errors
Write-Error "This is a non-terminating error"

# ErrorAction preference
$ErrorActionPreference = "Stop"    # Stop on errors
$ErrorActionPreference = "Continue" # Continue on errors (default)
$ErrorActionPreference = "SilentlyContinue" # Don't display errors
$ErrorActionPreference = "Inquire"    # Prompt for action

# Error variable
if ($error) {
    Write-Host "Last error: $($error[0])"
}

# Error handling with -ErrorAction
Get-Process "NonExistentProcess" -ErrorAction Stop
Get-Process "NonExistentProcess" -ErrorAction SilentlyContinue
```

### Try-Catch-Finally

```powershell
# Basic try-catch
try {
    # Code that might throw an error
    Get-Process "NonExistentProcess" -ErrorAction Stop
}
catch {
    # Code to handle the error
    Write-Host "Error occurred: $($_.Exception.Message)"
}

# Try-catch with specific exception types
try {
    # Code that might throw specific errors
    [int]("invalid")
}
catch [System.Management.Automation.PSInvalidOperationException] {
    Write-Host "Invalid operation"
}
catch [System.Management.Automation.RuntimeException] {
    Write-Host "Runtime exception"
}
catch {
    Write-Host "Other error: $($_.Exception.GetType().FullName)"
}

# Try-catch-finally
try {
    # Code that might throw an error
    $file = [System.IO.File]::Open("C:\Temp\test.txt", "Open")
    # Process file
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
}
finally {
    if ($file) {
        $file.Close()
    }
}
```

### Error Variables

```powershell
# $Error automatic variable
$Error.Clear()  # Clear error list
$Error.Count    # Number of errors
$Error[0]       # Most recent error
$Error[-1]      # First error in list

# $LASTEXITCODE
& "somecommand.exe"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Command failed with exit code $LASTEXITCODE"
}

# $?
if ($?) {
    Write-Host "Last command succeeded"
} else {
    Write-Host "Last command failed"
}
```

### Custom Error Handling

```powershell
# Write-Error with custom error record
function Test-Error {
    [CmdletBinding()]
    param (
        [string]$Message
    )
    
    $errorRecord = [System.Management.Automation.ErrorRecord]::new(
        [System.Exception]::new($Message),
        "CustomErrorId",
        [System.Management.Automation.ErrorCategory]::InvalidOperation,
        $null
    )
    
    $PSCmdlet.WriteError($errorRecord)
}

# Throw with custom exception
function Throw-CustomError {
    param (
        [string]$Message,
        [string]$ErrorId,
        [System.Management.Automation.ErrorCategory]$Category
    )
    
    $exception = [System.Exception]::new($Message)
    $errorRecord = [System.Management.Automation.ErrorRecord]::new(
        $exception,
        $ErrorId,
        $Category,
        $null
    )
    
    throw $errorRecord
}
```

## Modules and Snap-ins

### Modules

```powershell
# Importing modules
Import-Module "ActiveDirectory"
Import-Module "C:\Modules\MyModule.psm1"
Import-Module "MyModule" -Force  # Reload module

# Listing modules
Get-Module
Get-Module -ListAvailable

# Removing modules
Remove-Module "ActiveDirectory"

# Creating modules (MyModule.psm1)
function Get-MyFunction {
    [CmdletBinding()]
    param (
        [string]$Parameter
    )
    
    return "Result: $Parameter"
}

# Exporting functions
Export-ModuleMember -Function Get-MyFunction
```

### Module Manifests

```powershell
# Creating module manifest (MyModule.psd1)
@{
    RootModule = 'MyModule.psm1'
    ModuleVersion = '2.0.0'
    GUID = '12345678-1234-1234-1234-123456789012'
    Author = 'Author Name'
    CompanyName = 'Company Name'
    Copyright = '(c) 2023 Author Name'
    Description = 'Module description'
    PowerShellVersion = '5.1'
    FunctionsToExport = @('Get-MyFunction')
    CmdletsToExport = ''
    VariablesToExport = ''
    AliasesToExport = ''
}
```

### Snap-ins (Legacy)

```powershell
# Listing snap-ins
Get-PSSnapin
Get-PSSnapin -Registered

# Adding snap-ins
Add-PSSnapin "Microsoft.Exchange.Management.PowerShell.E2010"

# Removing snap-ins
Remove-PSSnapin "Microsoft.Exchange.Management.PowerShell.E2010"
```

## Pipeline

### Basic Pipeline

```powershell
# Simple pipeline
Get-Process | Where-Object { $_.CPU -gt 10 } | Sort-Object CPU -Descending | Select-Object -First 5

# Pipeline with script blocks
1..10 | ForEach-Object {
    $_ * 2
}

# Pipeline with filter
function Filter-Even {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline=$true)]
        [int]$Number
    )
    
    process {
        if ($Number % 2 -eq 0) {
            Write-Output $Number
        }
    }
}

1..10 | Filter-Even
```

### Advanced Pipeline

```powershell
# Begin, Process, End blocks
function Pipeline-Advanced {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline=$true)]
        [psobject]$InputObject
    )
    
    begin {
        Write-Host "Pipeline started"
        $total = 0
        $count = 0
    }
    
    process {
        Write-Host "Processing: $InputObject"
        $total += $InputObject
        $count++
    }
    
    end {
        Write-Host "Pipeline completed"
        Write-Host "Total: $total, Count: $count, Average: $($total/$count)"
    }
}

1..10 | Pipeline-Advanced
```

### Pipeline Parameters

```powershell
# Accepting pipeline input
function Get-FileInfo {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
        [string[]]$Path
    )
    
    process {
        foreach ($p in $Path) {
            $info = Get-Item $p -ErrorAction SilentlyContinue
            if ($info) {
                [PSCustomObject]@{
                    Name = $info.Name
                    Size = $info.Length
                    LastModified = $info.LastWriteTime
                }
            }
        }
    }
}

# Usage
Get-ChildItem | Get-FileInfo
"file1.txt", "file2.txt" | Get-FileInfo
```

## Regular Expressions

### Basic Regex

```powershell
# Match operator
"Hello World" -match "\w+"      # True
$Matches                         # Contains match groups

# Not match
"Hello World" -notmatch "\d+"    # True

# Replace operator
"Hello World" -replace "World", "PowerShell"

# Split operator
"a,b,c,d" -split ","             # Returns array
```

### Advanced Regex

```powershell
# Using regex class
$regex = [regex]"\d+"
$text = "There are 123 numbers in 456 this text"
$matches = $regex.Matches($text)

# Groups and captures
$pattern = "(?<word>\w+):(?<number>\d+)"
$text = "count:123, size:456, weight:789"
$matches = [regex]::Matches($text, $pattern)

foreach ($match in $matches) {
    $word = $match.Groups["word"].Value
    $number = $match.Groups["number"].Value
    Write-Host "$word = $number"
}

# Regex options
$text = "Hello WORLD"
$text -match "(?i)hello"  # Case-insensitive match

$text -match "(?m)^hello" # Multiline mode
```

### Regex in Select-String

```powershell
# Search files with regex
Select-String -Path "*.log" -Pattern "ERROR|FATAL" -CaseSensitive

# Get context lines
Select-String -Path "*.log" -Pattern "ERROR" -Context 2

# Regex groups in Select-String
Select-String -Path "*.log" -Pattern "(?<timestamp>\d{4}-\d{2}-\d{2}) (?<level>\w+)" |
ForEach-Object {
    [PSCustomObject]@{
        Timestamp = $_.Matches[0].Groups["timestamp"].Value
        Level = $_.Matches[0].Groups["level"].Value
        Line = $_.Line
    }
}
```

## File System Operations

### File Operations

```powershell
# Reading files
$content = Get-Content "file.txt"
$content = Get-Content "file.txt" -Raw  # As single string
$lines = Get-Content "file.txt" -TotalCount 10  # First 10 lines

# Writing files
"Hello World" | Out-File "file.txt"
"Hello World" | Set-Content "file.txt"
"Hello World" | Add-Content "file.txt"

# File properties
$file = Get-Item "file.txt"
$name = $file.Name
$size = $file.Length
$created = $file.CreationTime
$modified = $file.LastWriteTime

# Test paths
if (Test-Path "file.txt") {
    Write-Host "File exists"
}

# Copy, move, remove
Copy-Item "source.txt" "destination.txt"
Move-Item "source.txt" "destination.txt"
Remove-Item "file.txt"
```

### Directory Operations

```powershell
# Creating directories
New-Item -Path "C:\Temp\NewFolder" -ItemType Directory
mkdir "C:\Temp\NewFolder"  # Alias

# Listing directories
Get-ChildItem -Path "C:\Temp" -Directory
Get-ChildItem -Path "C:\Temp" -File

# Recursive operations
Get-ChildItem -Path "C:\Temp" -Recurse
Get-ChildItem -Path "C:\Temp" -Recurse -Filter "*.txt"

# Directory properties
$dir = Get-Item "C:\Temp"
$files = $dir.GetFiles()
$subdirs = $dir.GetDirectories()
```

### File System Provider

```powershell
# Navigating file system
Set-Location "C:\Temp"
cd "C:\Temp"  # Alias

# Provider-specific commands
Get-PSProvider
Get-PSDrive

# Alternative data streams
Set-Content -Path "file.txt:stream" -Value "Hidden data"
Get-Content -Path "file.txt:stream"
```

## WMI and CIM

### WMI Operations

```powershell
# Getting WMI objects
$os = Get-WmiObject -Class Win32_OperatingSystem
$services = Get-WmiObject -Class Win32_Service

# Querying WMI
$processes = Get-WmiObject -Query "SELECT * FROM Win32_Process WHERE Name = 'notepad.exe'"

# WMI methods
$os | Get-Member
$os.Reboot()  # Reboot computer

# Creating WMI objects
$class = Get-WmiObject -Class Win32_Process -List
$class.Create("notepad.exe")
```

### CIM Operations

```powershell
# Getting CIM instances
$os = Get-CimInstance -ClassName Win32_OperatingSystem
$services = Get-CimInstance -ClassName Win32_Service

# Querying CIM
$processes = Get-CimInstance -Query "SELECT * FROM Win32_Process WHERE Name = 'notepad.exe'"

# CIM sessions
$session = New-CimSession -ComputerName "RemoteComputer"
$os = Get-CimInstance -ClassName Win32_OperatingSystem -CimSession $session
Remove-CimSession -CimSession $session

# CIM methods
$os | Get-CimClass
$os | Invoke-CimMethod -MethodName "Reboot"
```

## XML and JSON Handling

### XML Operations

```powershell
# Loading XML
[xml]$xml = Get-Content "file.xml"

# Creating XML
$xml = [xml]@"
<root>
    <person>
        <name>John</name>
        <age>30</age>
    </person>
</root>
"@

# Querying XML
$names = $xml.root.person.name
$age = $xml.SelectSingleNode("//person[@name='John']/age").InnerText

# Modifying XML
$newPerson = $xml.CreateElement("person")
$nameElement = $xml.CreateElement("name")
$nameElement.InnerText = "Jane"
$newPerson.AppendChild($nameElement) | Out-Null
$xml.root.AppendChild($newPerson) | Out-Null

# Saving XML
$xml.Save("output.xml")
```

### JSON Operations

```powershell
# Converting to JSON
$object = [PSCustomObject]@{
    Name = "John"
    Age = 30
    City = "New York"
}
$json = $object | ConvertTo-Json -Depth 3

# Converting from JSON
$json = '{"Name": "John", "Age": 30, "City": "New York"}'
$object = $json | ConvertFrom-Json

# Working with JSON files
Get-Content "data.json" | ConvertFrom-Json
$object | ConvertTo-Json | Set-Content "data.json"

# Complex JSON
$complex = @{
    People = @(
        @{ Name = "John"; Age = 30 },
        @{ Name = "Jane"; Age = 25 }
    )
    Settings = @{
        Theme = "Dark"
        Notifications = $true
    }
}
$json = $complex | ConvertTo-Json -Depth 4
```

## Classes (PowerShell 5.0+)

### Class Declaration

```powershell
# Basic class
class Person {
    [string]$Name
    [int]$Age
    
    Person([string]$name, [int]$age) {
        $this.Name = $name
        $this.Age = $age
    }
    
    [string]GetInfo() {
        return "$($this.Name) is $($this.Age) years old"
    }
}

# Using class
$person = [Person]::new("John", 30)
$info = $person.GetInfo()
```

### Advanced Classes

```powershell
# Class with inheritance
class Employee : Person {
    [string]$Title
    [decimal]$Salary
    
    Employee([string]$name, [int]$age, [string]$title, [decimal]$salary) : base($name, $age) {
        $this.Title = $title
        $this.Salary = $salary
    }
    
    [string]GetFullInfo() {
        return "$($this.GetInfo()), Title: $($this.Title), Salary: $($this.Salary)"
    }
}

# Static properties and methods
class MathHelper {
    static [double]$PI = 3.14159
    
    static [double]CalculateArea([double]$radius) {
        return [MathHelper]::PI * $radius * $radius
    }
}

# Using static members
$area = [MathHelper]::CalculateArea(5)
$pi = [MathHelper]::PI
```

### Class Properties

```powershell
class Product {
    hidden [int]$_id
    [string]$Name
    
    [int]$Id {
        get { return $this._id }
        set { 
            if ($value -gt 0) {
                $this._id = $value
            } else {
                throw "ID must be positive"
            }
        }
    }
    
    [string]$Description {
        get { return "$($this.Name) - Product ID: $($this.Id)" }
    }
    
    [ValidateSet("Electronics", "Clothing", "Books")]
    [string]$Category
}
```

### Class Methods

```powershell
class Calculator {
    [int]Add([int]$a, [int]$b) {
        return $a + $b
    }
    
    [int]Multiply([int]$a, [int]$b) {
        return $a * $b
    }
    
