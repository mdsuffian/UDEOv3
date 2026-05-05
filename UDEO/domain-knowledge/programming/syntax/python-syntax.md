# Python Syntax Reference

This document provides a comprehensive reference for Python syntax, covering all language constructs from basic to advanced features.

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Variables and Data Types](#variables-and-data-types)
3. [Operators](#operators)
4. [Control Flow](#control-flow)
5. [Functions and Modules](#functions-and-modules)
6. [Classes and Objects](#classes-and-objects)
7. [Exception Handling](#exception-handling)
8. [File Operations](#file-operations)
9. [Collections](#collections)
10. [Comprehensions and Generators](#comprehensions-and-generators)
11. [Decorators](#decorators)
12. [Context Managers](#context-managers)
13. [Metaclasses](#metaclasses)
14. [Async Programming](#async-programming)
15. [Type Hints](#type-hints)
16. [Data Classes](#data-classes)
17. [Advanced Features](#advanced-features)

## Basic Syntax

### Program Structure

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
This is a module docstring.
It provides information about the module.
"""

import os
import sys
from typing import List, Dict

# Global constants
PI = 3.14159
VERSION = "2.0.0"

def main():
    """Main function entry point."""
    print("Hello, World!")

if __name__ == "__main__":
    main()
```

### Comments and Docstrings

```python
# Single-line comment

"""
Multi-line string
that can be used as a docstring
"""

def function():
    """
    Function docstring.
    
    Args:
        param1: Description of parameter 1
        param2: Description of parameter 2
    
    Returns:
        Description of return value
    
    Raises:
        ValueError: When invalid input is provided
    """
    pass

class MyClass:
    """Class docstring."""
    
    def method(self):
        """Method docstring."""
        pass
```

### Indentation

```python
# Python uses indentation to define code blocks
def function():
    if condition:
        # 4 spaces indentation
        statement1()
        statement2()
    else:
        statement3()
        if nested_condition:
            # 8 spaces indentation
            statement4()

# Consistent indentation is required
# Use 4 spaces per level (PEP 8 recommendation)
```

## Variables and Data Types

### Variable Assignment

```python
# Basic assignment
name = "John"
age = 30
height = 5.9
is_student = False

# Multiple assignment
x, y, z = 1, 2, 3

# Chain assignment
a = b = c = 0

# Unpacking
values = [1, 2, 3]
first, second, third = values

# Extended unpacking (Python 3.0+)
first, *middle, last = [1, 2, 3, 4, 5]

# Swapping variables
a, b = b, a
```

### Basic Data Types

```python
# Numeric types
integer = 42
float_num = 3.14
complex_num = 2 + 3j

# Boolean
bool_true = True
bool_false = False

# None
none_value = None

# Strings
single_quote = 'Hello'
double_quote = "World"
multiline = """This is a
multiline string"""
raw_string = r"C:\path\to\file"
f_string = f"Hello, {name}!"  # f-string (Python 3.6+)

# Type checking
type(variable)  # Returns the type
isinstance(variable, int)  # Check if instance of type
```

### Type Conversion

```python
# Explicit conversion
int_value = int("123")
float_value = float("3.14")
str_value = str(42)
bool_value = bool(1)  # True

# Base conversion
binary = bin(42)  # '0b101010'
hexadecimal = hex(42)  # '0x2a'
octal = oct(42)  # '0o52'

# String to number with base
binary_to_int = int("1010", 2)  # 10
hex_to_int = int("2a", 16)  # 42
```

## Operators

### Arithmetic Operators

```python
a = 10
b = 3

addition = a + b      # 13
subtraction = a - b   # 7
multiplication = a * b  # 30
division = a / b      # 3.333...
floor_division = a // b  # 3
modulus = a % b       # 1
exponent = a ** b     # 1000

# Unary operators
positive = +a         # 10
negative = -a         # -10
```

### Comparison Operators

```python
a = 10
b = 20

equal = a == b        # False
not_equal = a != b    # True
greater = a > b       # False
less = a < b          # True
greater_equal = a >= b  # False
less_equal = a <= b   # True

# Chaining comparisons
result = 0 < a < 20   # True
```

### Logical Operators

```python
a = True
b = False

and_result = a and b  # False
or_result = a or b    # True
not_result = not a    # False

# Short-circuit evaluation
result = True or expensive_function()  # expensive_function not called
result = False and expensive_function()  # expensive_function not called
```

### Bitwise Operators

```python
a = 5  # 0b0101
b = 3  # 0b0011

bitwise_and = a & b    # 1 (0b0001)
bitwise_or = a | b     # 7 (0b0111)
bitwise_xor = a ^ b    # 6 (0b0110)
bitwise_not = ~a       # -6 (ones' complement)
left_shift = a << 1    # 10 (0b1010)
right_shift = a >> 1   # 2 (0b0010)
```

### Membership and Identity Operators

```python
# Membership operators
list_item = [1, 2, 3, 4, 5]
in_result = 3 in list_item       # True
not_in_result = 6 not in list_item  # True

# Identity operators
a = [1, 2, 3]
b = [1, 2, 3]
c = a

is_same = a is c      # True (same object)
is_not_same = a is not b  # True (different objects)
```

## Control Flow

### Conditional Statements

```python
# if statement
if condition:
    # Code to execute if condition is true
    pass

# if-else statement
if condition:
    # Code if condition is true
    pass
else:
    # Code if condition is false
    pass

# if-elif-else statement
if condition1:
    # Code if condition1 is true
    pass
elif condition2:
    # Code if condition2 is true
    pass
else:
    # Code if all conditions are false
    pass

# Conditional expression (ternary)
result = value_if_true if condition else value_if_false

# Nested conditions
if condition1:
    if condition2:
        # Nested code
        pass
```

### Loops

```python
# for loop
for item in iterable:
    # Code to execute for each item
    pass

# for loop with range
for i in range(5):  # 0, 1, 2, 3, 4
    print(i)

for i in range(1, 6):  # 1, 2, 3, 4, 5
    print(i)

for i in range(0, 10, 2):  # 0, 2, 4, 6, 8
    print(i)

# while loop
while condition:
    # Code to execute while condition is true
    pass

# break and continue
for i in range(10):
    if i == 5:
        break  # Exit loop
    if i % 2 == 0:
        continue  # Skip to next iteration
    print(i)

# else clause with loops
for i in range(5):
    print(i)
else:
    print("Loop completed normally")  # Not executed if loop is broken

while condition:
    # Loop code
    pass
else:
    print("Loop completed normally")
```

### Loop Control

```python
# Nested loops with break labels (using exceptions)
class BreakLoop(Exception):
    pass

try:
    for i in range(5):
        for j in range(5):
            if i == 2 and j == 3:
                raise BreakLoop()
            print(i, j)
except BreakLoop:
    print("Broke out of nested loops")

# Using enumerate with loops
for index, value in enumerate(["a", "b", "c"]):
    print(f"{index}: {value}")

# Using zip with multiple iterables
names = ["Alice", "Bob", "Charlie"]
ages = [25, 30, 35]
for name, age in zip(names, ages):
    print(f"{name} is {age} years old")
```

## Functions and Modules

### Function Definition

```python
# Basic function
def greet(name):
    """Greet someone."""
    return f"Hello, {name}!"

# Function with default parameters
def greet(name, greeting="Hello"):
    """Greet someone with a custom greeting."""
    return f"{greeting}, {name}!"

# Function with variable number of arguments
def sum_all(*args):
    """Sum all arguments."""
    return sum(args)

# Function with keyword arguments
def create_person(**kwargs):
    """Create a person from keyword arguments."""
    return kwargs

# Function with both positional and keyword arguments
def complex_function(required, optional="default", *args, **kwargs):
    """Complex function with various parameter types."""
    pass

# Function annotations (type hints)
def add(a: int, b: int) -> int:
    """Add two integers."""
    return a + b
```

### Advanced Function Features

```python
# Lambda functions
square = lambda x: x * x
add = lambda x, y: x + y

# Higher-order functions
def apply_operation(func, *args):
    """Apply a function to arguments."""
    return func(*args)

result = apply_operation(lambda x, y: x + y, 5, 3)  # 8

# Closures
def make_multiplier(factor):
    """Create a multiplier function."""
    def multiplier(number):
        return number * factor
    return multiplier

times_three = make_multiplier(3)
result = times_three(5)  # 15

# Decorators (see Decorators section)
```

### Modules

```python
# Importing modules
import math
import os, sys
from math import pi, sqrt
from math import *  # Import all (not recommended)

# Importing with alias
import numpy as np
import pandas as pd

# Relative imports (from within a package)
from . import sibling_module
from .. import parent_module
from ..subpackage import module

# Module structure
# mymodule.py
"""
My module documentation.
"""

__version__ = "2.0.0"
__all__ = ["function1", "class1"]  # Controls what `from module import *` imports

def function1():
    """Function 1 documentation."""
    pass

class Class1:
    """Class 1 documentation."""
    pass
```

## Classes and Objects

### Class Definition

```python
# Basic class
class Person:
    """A simple person class."""
    
    # Class attribute
    species = "Homo sapiens"
    
    def __init__(self, name, age):
        """Initialize a person."""
        self.name = name  # Instance attribute
        self.age = age
    
    def greet(self):
        """Greet the person."""
        return f"Hello, I'm {self.name} and I'm {self.age} years old."
    
    def __str__(self):
        """String representation."""
        return f"Person(name={self.name}, age={self.age})"
    
    def __repr__(self):
        """Official string representation."""
        return f"Person('{self.name}', {self.age})"

# Using the class
person = Person("John", 30)
print(person.greet())
```

### Inheritance

```python
# Base class
class Animal:
    """Base animal class."""
    
    def __init__(self, name):
        self.name = name
    
    def speak(self):
        """Make a sound."""
        raise NotImplementedError("Subclasses must implement this method")

# Derived class
class Dog(Animal):
    """Dog class."""
    
    def __init__(self, name, breed):
        super().__init__(name)  # Call parent constructor
        self.breed = breed
    
    def speak(self):
        """Bark."""
        return f"{self.name} says Woof!"
    
    def fetch(self):
        """Fetch a ball."""
        return f"{self.name} is fetching the ball!"

# Multiple inheritance
class FlyingDog(Dog, FlyingCreature):
    """A dog that can fly (fictional)."""
    
    def __init__(self, name, breed, wingspan):
        Dog.__init__(self, name, breed)
        FlyingCreature.__init__(self, wingspan)
```

### Special Methods

```python
class Vector:
    """A 2D vector class."""
    
    def __init__(self, x, y):
        self.x = x
        self.y = y
    
    def __str__(self):
        return f"Vector({self.x}, {self.y})"
    
    def __repr__(self):
        return f"Vector({self.x}, {self.y})"
    
    def __add__(self, other):
        """Vector addition."""
        return Vector(self.x + other.x, self.y + other.y)
    
    def __sub__(self, other):
        """Vector subtraction."""
        return Vector(self.x - other.x, self.y - other.y)
    
    def __mul__(self, scalar):
        """Scalar multiplication."""
        return Vector(self.x * scalar, self.y * scalar)
    
    def __eq__(self, other):
        """Equality comparison."""
        return self.x == other.x and self.y == other.y
    
    def __len__(self):
        """Return magnitude."""
        return int((self.x ** 2 + self.y ** 2) ** 0.5)
    
    def __getitem__(self, index):
        """Index access."""
        if index == 0:
            return self.x
        elif index == 1:
            return self.y
        else:
            raise IndexError("Index out of range")
```

### Properties and Descriptors

```python
class Person:
    """Person class with properties."""
    
    def __init__(self, name):
        self._name = name  # "Private" attribute
    
    @property
    def name(self):
        """Get name (read-only property)."""
        return self._name
    
    @name.setter
    def name(self, value):
        """Set name with validation."""
        if not value:
            raise ValueError("Name cannot be empty")
        self._name = value
    
    @property
    def name_upper(self):
        """Get name in uppercase (computed property)."""
        return self._name.upper()

# Descriptor
class ValidatedAttribute:
    """Descriptor for validated attributes."""
    
    def __init__(self, name, validator):
        self.name = name
        self.validator = validator
    
    def __get__(self, obj, objtype=None):
        if obj is None:
            return self
        return obj.__dict__[self.name]
    
    def __set__(self, obj, value):
        if not self.validator(value):
            raise ValueError(f"Invalid value for {self.name}")
        obj.__dict__[self.name] = value

class Person:
    """Person class using descriptors."""
    
    age = ValidatedAttribute("age", lambda x: isinstance(x, int) and x >= 0)
    
    def __init__(self, name, age):
        self.name = name
        self.age = age
```

## Exception Handling

### Basic Exception Handling

```python
# Basic try-except
try:
    result = 10 / 0
except ZeroDivisionError:
    print("Cannot divide by zero")

# Multiple exception types
try:
    # Code that might raise different exceptions
    pass
except ValueError as e:
    print(f"Value error: {e}")
except TypeError as e:
    print(f"Type error: {e}")
except Exception as e:
    print(f"Unexpected error: {e}")

# Try-except-else-finally
try:
    result = risky_operation()
except ValueError as e:
    print(f"Error: {e}")
else:
    print("Operation successful")
finally:
    print("Cleanup code (always executed)")
```

### Custom Exceptions

```python
# Custom exception class
class CustomError(Exception):
    """Custom exception."""
    
    def __init__(self, message, code=None):
        super().__init__(message)
        self.code = code

# Raising exceptions
def divide(a, b):
    """Divide two numbers."""
    if b == 0:
        raise ValueError("Cannot divide by zero")
    return a / b

# Re-raising exceptions
try:
    result = risky_operation()
except ValueError as e:
    print(f"Handling error: {e}")
    raise  # Re-raise the same exception

# Exception chaining
try:
    result = risky_operation()
except ValueError as e:
    raise CustomError("Custom error message") from e
```

### Context Managers for Exception Handling

```python
# Using with statement for resource management
with open("file.txt", "r") as file:
    content = file.read()
    # File automatically closed when exiting with block

# Custom context manager
class DatabaseConnection:
    """Database connection context manager."""
    
    def __enter__(self):
        print("Connecting to database")
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        print("Closing database connection")
        if exc_type is not None:
            print(f"Exception occurred: {exc_val}")
        return True  # Suppress exception

# Using custom context manager
with DatabaseConnection() as conn:
    # Use connection
    pass
```

## File Operations

### Basic File Operations

```python
# Reading files
with open("file.txt", "r") as file:
    content = file.read()  # Read entire file

with open("file.txt", "r") as file:
    lines = file.readlines()  # Read all lines as list

with open("file.txt", "r") as file:
    for line in file:  # Iterate over lines
        print(line.strip())

# Writing files
with open("output.txt", "w") as file:
    file.write("Hello, World!")

with open("output.txt", "a") as file:  # Append mode
    file.write("\nAppending to file")

# Using pathlib (Python 3.4+)
from pathlib import Path

path = Path("file.txt")
content = path.read_text()
path.write_text("Hello, World!")

# Check if file exists
if path.exists():
    print("File exists")

# Create directories
Path("new_directory").mkdir(exist_ok=True)
```

### Advanced File Operations

```python
# Binary files
with open("image.jpg", "rb") as file:
    image_data = file.read()

with open("output.jpg", "wb") as file:
    file.write(image_data)

# File positioning
with open("file.txt", "r") as file:
    file.seek(10)  # Move to position 10
    content = file.read(20)  # Read 20 bytes
    position = file.tell()  # Get current position

# Temporary files
import tempfile

with tempfile.NamedTemporaryFile(delete=False) as temp:
    temp.write(b"Temporary data")
    temp_path = temp.name

# File and directory operations
import os
import shutil

# Copy, move, delete
shutil.copy("source.txt", "destination.txt")
shutil.move("source.txt", "new_location.txt")
os.remove("file.txt")

# Directory operations
os.listdir("directory")  # List files
os.mkdir("new_directory")  # Create directory
os.rmdir("empty_directory")  # Remove empty directory
shutil.rmtree("directory")  # Remove directory and contents
```

## Collections

### Lists

```python
# Creating lists
empty_list = []
numbers = [1, 2, 3, 4, 5]
mixed = [1, "hello", 3.14, True]

# List operations
numbers.append(6)  # Add to end
numbers.insert(0, 0)  # Insert at index
numbers.extend([7, 8, 9])  # Extend with another list

# Removing elements
numbers.pop()  # Remove and return last element
numbers.pop(0)  # Remove and return element at index
numbers.remove(5)  # Remove first occurrence
del numbers[0]  # Delete element at index

# List comprehension
squares = [x ** 2 for x in range(10)]
even_squares = [x ** 2 for x in range(10) if x % 2 == 0]

# List methods
numbers.sort()  # Sort in place
sorted_numbers = sorted(numbers)  # Return sorted copy
numbers.reverse()  # Reverse in place
count = numbers.count(5)  # Count occurrences
index = numbers.index(5)  # Find index of element
```

### Tuples

```python
# Creating tuples
empty_tuple = ()
single_tuple = (1,)  # Note the comma
coordinates = (3, 4)
mixed_tuple = (1, "hello", 3.14)

# Tuple operations
x, y = coordinates  # Unpacking
x, y, z = mixed_tuple

# Named tuples
from collections import namedtuple

Point = namedtuple("Point", ["x", "y"])
p = Point(3, 4)
print(p.x, p.y)  # 3 4

# Tuple methods
index = coordinates.index(4)  # Find index
count = coordinates.count(3)  # Count occurrences
```

### Dictionaries

```python
# Creating dictionaries
empty_dict = {}
person = {"name": "John", "age": 30}
person2 = dict(name="Jane", age=25)

# Dictionary operations
person["city"] = "New York"  # Add/modify key
age = person.get("age", 0)  # Get with default
del person["age"]  # Remove key

# Dictionary methods
keys = person.keys()  # Get keys
values = person.values()  # Get values
items = person.items()  # Get key-value pairs

# Dictionary comprehension
squares = {x: x ** 2 for x in range(10)}
even_squares = {x: x ** 2 for x in range(10) if x % 2 == 0}

# Merging dictionaries (Python 3.9+)
merged = {**dict1, **dict2}
merged = dict1 | dict2  # Union operator
```

### Sets

```python
# Creating sets
empty_set = set()
numbers = {1, 2, 3, 4, 5}

# Set operations
numbers.add(6)  # Add element
numbers.remove(3)  # Remove element (raises error if not found)
numbers.discard(3)  # Remove element (no error if not found)

# Set operations
set1 = {1, 2, 3, 4}
set2 = {3, 4, 5, 6}

union = set1 | set2  # Union
intersection = set1 & set2  # Intersection
difference = set1 - set2  # Difference
symmetric_difference = set1 ^ set2  # Symmetric difference

# Set comprehension
squares = {x ** 2 for x in range(10)}
```

## Comprehensions and Generators

### List Comprehensions

```python
# Basic list comprehension
squares = [x ** 2 for x in range(10)]

# With condition
even_squares = [x ** 2 for x in range(10) if x % 2 == 0]

# Nested list comprehension
matrix = [[i * j for j in range(3)] for i in range(3)]

# List comprehension with multiple conditions
result = [x for x in range(100) if x % 2 == 0 and x % 3 == 0]
```

### Dictionary and Set Comprehensions

```python
# Dictionary comprehension
word_lengths = {word: len(word) for word in ["hello", "world", "python"]}

# Set comprehension
unique_squares = {x ** 2 for x in range(10) if x % 2 == 0}
```

### Generator Expressions

```python
# Generator expression
squares_gen = (x ** 2 for x in range(10))

# Using generator
for square in squares_gen:
    print(square)

# Generator expression with function
sum_of_squares = sum(x ** 2 for x in range(10))
```

### Generator Functions

```python
# Generator function
def fibonacci():
    """Generate Fibonacci numbers."""
    a, b = 0, 1
    while True:
        yield a
        a, b = b, a + b

# Using generator
fib = fibonacci()
for _ in range(10):
    print(next(fib))

# Generator with return value
def countdown(n):
    """Countdown from n to 0."""
    while n > 0:
        yield n
        n -= 1
    return "Done"

# Using generator with return value
gen = countdown(5)
for value in gen:
    print(value)
print(gen.return_value)  # "Done"
```

## Decorators

### Function Decorators

```python
# Basic decorator
def timing_decorator(func):
    """Decorator to time function execution."""
    import time
    
    def wrapper(*args, **kwargs):
        start = time.time()
        result = func(*args, **kwargs)
        end = time.time()
        print(f"{func.__name__} took {end - start:.4f} seconds")
        return result
    
    return wrapper

@timing_decorator
def slow_function():
    """A slow function."""
    import time
    time.sleep(1)
    return "Done"

# Decorator with parameters
def repeat(n):
    """Decorator to repeat function n times."""
    def decorator(func):
        def wrapper(*args, **kwargs):
            results = []
            for _ in range(n):
                results.append(func(*args, **kwargs))
            return results
        return wrapper
    return decorator

@repeat(3)
def greet(name):
    """Greet someone."""
    return f"Hello, {name}!"
```

### Class Decorators

```python
# Class decorator
def add_class_attribute(cls):
    """Add an attribute to a class."""
    cls.added_attribute = "Added by decorator"
    return cls

@add_class_attribute
class MyClass:
    pass

print(MyClass.added_attribute)  # "Added by decorator"

# Decorator for methods
def method_decorator(func):
    """Decorator for instance methods."""
    def wrapper(self, *args, **kwargs):
        print(f"Calling {func.__name__} on {self}")
        return func(self, *args, **kwargs)
    return wrapper

class Person:
    @method_decorator
    def greet(self):
        return "Hello!"
```

### Property Decorators

```python
class Person:
    def __init__(self, name):
        self._name = name
    
    @property
    def name(self):
        """Get name."""
        return self._name
    
    @name.setter
    def name(self, value):
        """Set name with validation."""
        if not value:
            raise ValueError("Name cannot be empty")
        self._name = value
    
    @name.deleter
    def name(self):
        """Delete name."""
        del self._name
```

## Context Managers

### Context Manager Classes

```python
class DatabaseConnection:
    """Database connection context manager."""
    
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.connection = None
    
    def __enter__(self):
        print("Opening database connection")
        # Simulate opening connection
        self.connection = f"Connection to {self.connection_string}"
        return self.connection
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        print("Closing database connection")
        # Simulate closing connection
        self.connection = None
        if exc_type is not None:
            print(f"Exception occurred: {exc_val}")
        return False  # Don't suppress exceptions

# Using context manager
with DatabaseConnection("mydb") as conn:
    print(f"Using connection: {conn}")
```

### Contextlib Module

```python
from contextlib import contextmanager

@contextmanager
def file_manager(filename, mode):
    """Simple file manager context manager."""
    file = open(filename, mode)
    try:
        yield file
    finally:
        file.close()

# Using context manager
with file_manager("test.txt", "w") as f:
    f.write("Hello, World!")

# Nested context managers
from contextlib import nested

with nested(file_manager("input.txt", "r"), file_manager("output.txt", "w")) as (inp, out):
    content = inp.read()
    out.write(content.upper())
```

## Metaclasses

### Basic Metaclass

```python
class SingletonMeta(type):
    """Metaclass that implements the Singleton pattern."""
    
    _instances = {}
    
    def __call__(cls, *args, **kwargs):
        if cls not in cls._instances:
            instance = super().__call__(*args, **kwargs)
            cls._instances[cls] = instance
        return cls._instances[cls]

class Singleton(metaclass=SingletonMeta):
    """Singleton class."""
    
    def __init__(self):
        self.value = 0

# All instances are the same
s1 = Singleton()
s2 = Singleton()
print(s1 is s2)  # True
```

### Advanced Metaclass

```python
class ValidationMeta(type):
    """Metaclass that validates class attributes."""
    
    def __new__(cls, name, bases, namespace):
        # Validate that all classes have a 'required_attr'
        if 'required_attr' not in namespace:
            raise TypeError(f"Class {name} must have 'required_attr'")
        
        # Add a timestamp to all classes
        namespace['created_at'] = datetime.datetime.now()
        
        return super().__new__(cls, name, bases, namespace)

class MyClass(metaclass=ValidationMeta):
    required_attr = "I'm required"
    
    def __init__(self):
        pass
```

## Async Programming

### Async/Await Basics

```python
import asyncio

# Async function
async def fetch_data(url):
    """Fetch data from URL asynchronously."""
    print(f"Fetching {url}")
    await asyncio.sleep(1)  # Simulate async operation
    return f"Data from {url}"

# Running async function
async def main():
    """Main async function."""
    tasks = [
        fetch_data("https://api.example.com/1"),
        fetch_data("https://api.example.com/2"),
        fetch_data("https://api.example.com/3")
    ]
    
    results = await asyncio.gather(*tasks)
    for result in results:
        print(result)

# Run async function
asyncio.run(main())
```

### Async Context Managers

```python
class AsyncDatabaseConnection:
    """Async database connection context manager."""
    
    async def __aenter__(self):
        print("Opening async database connection")
        # Simulate async connection
        await asyncio.sleep(0.1)
        return self
    
    async def __aexit__(self, exc_type, exc_val, exc_tb):
        print("Closing async database connection")
        # Simulate async cleanup
        await asyncio.sleep(0.1)
        return False

# Using async context manager
async def use_async_connection():
    async with AsyncDatabaseConnection() as conn:
        print("Using async connection")
        await asyncio.sleep(0.5)
```

## Type Hints

### Basic Type Hints

```python
from typing import List, Dict, Tuple, Optional, Union

# Variable type hints
name: str = "John"
age: int = 30
height: float = 5.9
is_student: bool = False

# Function type hints
def add(a: int, b: int) -> int:
    """Add two integers."""
    return a + b

def process_items(items: List[str]) -> Dict[str, int]:
    """Process list of items."""
    return {item: len(item) for item in items}

# Optional types
def find_user(user_id: int) -> Optional[str]:
    """Find user by ID."""
    # Returns string if found, None if not found
    pass

# Union types
def process_value(value: Union[int, str]) -> str:
    """Process integer or string value."""
    return str(value)
```

### Advanced Type Hints

```python
from typing import TypeVar, Generic, Callable, Any

# Generic types
T = TypeVar('T')

class Stack(Generic[T]):
    """Generic stack."""
    
    def __init__(self):
        self._items: List[T] = []
    
    def push(self, item: T) -> None:
        self._items.append(item)
    
    def pop(self) -> T:
        return self._items.pop()

# Callable types
def apply_operation(func: Callable[[int, int], int], a: int, b: int) -> int:
    """Apply operation to two integers."""
    return func(a, b)

# Protocol types (Python 3.8+)
from typing import Protocol

class Drawable(Protocol):
    """Protocol for drawable objects."""
    
    def draw(self) -> None:
        ...

def draw_all(objects: List[Drawable]) -> None:
    """Draw all drawable objects."""
    for obj in objects:
        obj.draw()
```

## Data Classes

### Basic Data Classes

```python
from dataclasses import dataclass
from datetime import datetime

@dataclass
class Person:
    """Simple person data class."""
    name: str
    age: int
    email: str = "default@example.com"  # Default value
    
    def greet(self) -> str:
        """Greet the person."""
        return f"Hello, I'm {self.name}!"

# Using data class
person = Person("John", 30)
print(person)  # Person(name='John', age=30, email='default@example.com')
```

### Advanced Data Classes

```python
from dataclasses import dataclass, field
from typing import List

@dataclass
class Department:
    """Department data class."""
    name: str
    employees: List['Person'] = field(default_factory=list)
    created_at: datetime = field(default_factory=datetime.now)
    
    def add_employee(self, person: 'Person') -> None:
        """Add employee to department."""
        self.employees.append(person)

# Data class with post-init processing
@dataclass
class Circle:
    """Circle data class."""
    radius: float
    
    def __post_init__(self):
        """Calculate area after initialization."""
        self.area = 3.14159 * self.radius ** 2
```

## Advanced Features

### Iterators and Iterables

```python
# Custom iterator
class Countdown:
    """Countdown iterator."""
    
    def __init__(self, start):
        self.current = start
    
    def __iter__(self):
        return self
    
    def __next__(self):
        if self.current <= 0:
            raise StopIteration
        self.current -= 1
        return self.current + 1

# Using iterator
for i in Countdown(5):
    print(i)
```

### Magic Methods

```python
class MagicMethods:
    """Class demonstrating magic methods."""
    
    def __init__(self, value):
        self.value = value
    
    def __str__(self):
        return f"MagicMethods({self.value})"
    
    def __repr__(self):
        return f"MagicMethods({self.value!r})"
    
    def __add__(self, other):
        return MagicMethods(self.value + other.value)
    
    def __eq__(self, other):
        return self.value == other.value
    
    def __hash__(self):
        return hash(self.value)
    
    def __bool__(self):
        return bool(self.value)
    
    def __call__(self, multiplier):
        return self.value * multiplier
```

### Reflection and Introspection

```python
import inspect

class MyClass:
    """Example class for reflection."""
    
    def __init__(self, value):
        self.value = value
    
    def method(self):
        """Example method."""
        pass

# Get class information
cls = MyClass
print(cls.__name__)  # "MyClass"
print(cls.__doc__)   # "Example class for reflection"

# Get methods and attributes
members = inspect.getmembers(cls)
print(members)

# Get method signature
sig = inspect.signature(cls.method)
print(sig)  # (self)

# Check if object has attribute
obj = MyClass(42)
print(hasattr(obj, 'value'))  # True
print(getattr(obj, 'value'))  # 42
```

This comprehensive Python syntax reference covers all major language features from basic syntax to advanced concepts like metaclasses and async programming. It serves as a complete guide for developers working with Python in the UDEO system.