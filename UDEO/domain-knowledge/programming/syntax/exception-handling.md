# Comprehensive Exception Handling Guide

This document provides a comprehensive guide to exception handling across multiple programming languages, covering all scenarios and best practices for robust error management.

For detailed information about different types of programming errors (syntax errors, compile errors, semantic errors, etc.), please refer to the [Error Types Guide](error-types.md).

For a comprehensive reference of specific error codes and their solutions across C#, PowerShell, and Python, please refer to the [Error Reference Guide](error-reference.md).

## Table of Contents

1. [Exception Handling Fundamentals](#exception-handling-fundamentals)
2. [C# Exception Handling](#c-exception-handling)
3. [PowerShell Exception Handling](#powershell-exception-handling)
4. [Python Exception Handling](#python-exception-handling)
5. [Cross-Language Best Practices](#cross-language-best-practices)
6. [Exception Handling Patterns](#exception-handling-patterns)
7. [Logging and Monitoring](#logging-and-monitoring)
8. [Error Recovery Strategies](#error-recovery-strategies)
9. [Exception Handling in UDEO](#exception-handling-in-udeo)

## Exception Handling Fundamentals

### What is Exception Handling?

Exception handling is a mechanism for managing errors and exceptional situations that occur during program execution. It allows programs to detect, report, and recover from errors in a controlled manner.

### Key Concepts

1. **Exception**: An object that represents an error or unusual condition
2. **Throw/Raise**: Initiating an exception when an error is detected
3. **Catch/Handle**: Responding to an exception with appropriate error handling code
4. **Propagation**: The process of an exception moving up the call stack until caught
5. **Resource Management**: Ensuring resources are properly released even when exceptions occur

### Exception Handling Principles

1. **Fail Fast**: Detect and report errors as early as possible
2. **Fail Safe**: Maintain system stability even when errors occur
3. **Graceful Degradation**: Continue operating at reduced capacity when possible
4. **Clear Error Messages**: Provide meaningful information about what went wrong
5. **Proper Cleanup**: Always release resources and maintain invariants

## C# Exception Handling

### Basic Exception Handling

```csharp
try
{
    // Code that might throw an exception
    int result = Divide(10, 0);
}
catch (DivideByZeroException ex)
{
    // Handle specific exception
    Console.WriteLine($"Cannot divide by zero: {ex.Message}");
}
catch (Exception ex)
{
    // Handle any other exception
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    // Code that always executes, regardless of exceptions
    Console.WriteLine("Cleanup code");
}
```

### Exception Hierarchy

```csharp
// System.Exception is the base class for all exceptions
// Common exception types:
// - SystemException: Base class for predefined exceptions
//   - ArgumentException: Invalid argument
//     - ArgumentNullException: Null argument
//     - ArgumentOutOfRangeException: Out of range argument
//   - InvalidOperationException: Invalid operation
//   - NotSupportedException: Unsupported operation
//   - NotImplementedException: Feature not implemented
//   - FormatException: Invalid format
//   - OverflowException: Arithmetic overflow
//   - IndexOutOfRangeException: Array index out of range
//   - NullReferenceException: Null reference
//   - FileNotFoundException: File not found
//   - IOException: I/O error
//   - UnauthorizedAccessException: Access denied
// - ApplicationException: Base class for application exceptions
```

### Custom Exceptions

```csharp
// Create custom exception
public class BusinessException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
    
    public BusinessException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
    
    // For serialization support
    protected BusinessException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) 
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode));
    }
}

// Throwing custom exceptions
public void ProcessOrder(Order order)
{
    if (order == null)
    {
        throw new ArgumentNullException(nameof(order));
    }
    
    if (order.Total > order.MaximumAllowed)
    {
        throw new BusinessException(
            "Order total exceeds maximum allowed",
            "ORDER_TOTAL_EXCEEDED"
        );
    }
}
```

### Exception Filters (C# 6.0+)

```csharp
try
{
    ProcessData(data);
}
catch (ArgumentException ex) when (ex.ParamName == "data")
{
    // Handle only when parameter name is "data"
    Console.WriteLine("Invalid data parameter");
}
catch (Exception ex) when (IsCriticalError(ex))
{
    // Handle only critical errors
    LogCriticalError(ex);
    throw; // Re-throw
}
```

### Async Exception Handling

```csharp
// Basic async exception handling
public async Task ProcessAsync()
{
    try
    {
        await ProcessDataAsync();
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP request failed: {ex.Message}");
    }
}

// Handling exceptions in Task.WhenAll
public async Task ProcessMultipleAsync()
{
    var tasks = new List<Task>
    {
        ProcessItemAsync(1),
        ProcessItemAsync(2),
        ProcessItemAsync(3)
    };
    
    try
    {
        await Task.WhenAll(tasks);
    }
    catch (Exception ex)
    {
        // First exception is caught, but others might be in inner exceptions
        Console.WriteLine($"One or more tasks failed: {ex.Message}");
        
        // Check all tasks for exceptions
        foreach (var task in tasks)
        {
            if (task.IsFaulted)
            {
                foreach (var innerEx in task.Exception.InnerExceptions)
                {
                    Console.WriteLine($"Task failed: {innerEx.Message}");
                }
            }
        }
    }
}

// Exception handling with ConfigureAwait
public async Task ProcessWithConfigureAwait()
{
    try
    {
        // ConfigureAwait(false) avoids deadlock in certain contexts
        await ProcessDataAsync().ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        // Exception handling context might be different
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

### Resource Management

```csharp
// Using statement for automatic resource disposal
public void ProcessFile(string filePath)
{
    try
    {
        using (var fileStream = new FileStream(filePath, FileMode.Open))
        using (var reader = new StreamReader(fileStream))
        {
            string content = reader.ReadToEnd();
            ProcessContent(content);
        }
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"File not found: {filePath}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"IO error: {ex.Message}");
    }
}

// Custom IDisposable implementation
public class DatabaseConnection : IDisposable
{
    private SqlConnection _connection;
    private bool _disposed = false;
    
    public DatabaseConnection(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }
    
    public void ExecuteQuery(string query)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DatabaseConnection));
        
        using (var command = new SqlCommand(query, _connection))
        {
            command.ExecuteNonQuery();
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
    
    ~DatabaseConnection()
    {
        Dispose(false);
    }
}
```

## PowerShell Exception Handling

### Basic Exception Handling

```powershell
# Basic try-catch
try {
    # Code that might throw an exception
    Get-Process "NonExistentProcess" -ErrorAction Stop
}
catch [System.Management.Automation.ItemNotFoundException] {
    Write-Host "Process not found"
}
catch {
    Write-Host "An error occurred: $($_.Exception.Message)"
}
finally {
    Write-Host "Cleanup code"
}

# ErrorAction parameter
Get-Process "NonExistentProcess" -ErrorAction Stop    # Terminate on error
Get-Process "NonExistentProcess" -ErrorAction Continue # Continue on error
Get-Process "NonExistentProcess" -ErrorAction SilentlyContinue # No error message
Get-Process "NonExistentProcess" -ErrorAction Inquire   # Prompt for action
```

### Error Types in PowerShell

```powershell
# Terminating errors (stop execution)
throw "This is a terminating error"
Write-Error "This is a non-terminating error" -ErrorAction Stop

# Non-terminating errors (continue execution)
Write-Error "This is a non-terminating error"

# Error variable
if ($error) {
    Write-Host "Last error: $($error[0].Exception.Message)"
}

# $LASTEXITCODE for external commands
& "somecommand.exe"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Command failed with exit code $LASTEXITCODE"
}

# $? automatic variable
if ($?) {
    Write-Host "Last command succeeded"
} else {
    Write-Host "Last command failed"
}
```

### Custom Error Records

```powershell
# Create custom error record
function Write-CustomError {
    param (
        [string]$Message,
        [string]$ErrorId,
        [System.Management.Automation.ErrorCategory]$Category = "InvalidOperation"
    )
    
    $exception = New-Object System.Exception $Message
    $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, $ErrorId, $Category, $null
    $PSCmdlet.WriteError($errorRecord)
}

# Throw with custom exception
function Throw-CustomError {
    param (
        [string]$Message,
        [string]$ErrorId,
        [System.Management.Automation.ErrorCategory]$Category = "InvalidOperation"
    )
    
    $exception = New-Object System.Exception $Message
    $errorRecord = New-Object System.Management.Automation.ErrorRecord $exception, $ErrorId, $Category, $null
    throw $errorRecord
}
```

### Function Error Handling

```powershell
# Advanced function with error handling
function Get-User {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$UserName,
        
        [Parameter(Mandatory=$false)]
        [string]$Domain = "localhost"
    )
    
    try {
        # Validate parameters
        if ([string]::IsNullOrEmpty($UserName)) {
            throw "UserName cannot be empty"
        }
        
        # Attempt to get user
        $user = Get-ADUser -Identity $UserName -Server $Domain -ErrorAction Stop
        return $user
    }
    catch [Microsoft.ActiveDirectory.Management.ADIdentityNotFoundException] {
        Write-Error "User '$UserName' not found in domain '$Domain'" -ErrorAction Stop
    }
    catch [Microsoft.ActiveDirectory.Management.ADServerDownException] {
        Write-Error "Cannot connect to domain controller for domain '$Domain'" -ErrorAction Stop
    }
    catch {
        Write-Error "Unexpected error getting user '$UserName': $($_.Exception.Message)" -ErrorAction Stop
    }
}

# Error handling with ShouldProcess
function Remove-UserSafely {
    [CmdletBinding(SupportsShouldProcess=$true)]
    param (
        [Parameter(Mandatory=$true)]
        [string]$UserName
    )
    
    try {
        $user = Get-User -UserName $UserName
        
        if ($PSCmdlet.ShouldProcess($user.DistinguishedName, "Remove")) {
            Remove-ADUser -Identity $user -Confirm:$false -ErrorAction Stop
            Write-Host "User '$UserName' removed successfully"
        }
    }
    catch {
        Write-Error "Failed to remove user '$UserName': $($_.Exception.Message)"
    }
}
```

### Pipeline Error Handling

```powershell
# Error handling in pipeline
function Process-Items {
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline=$true)]
        [object[]]$InputObject
    )
    
    begin {
        $processedCount = 0
        $errorCount = 0
    }
    
    process {
        foreach ($item in $InputObject) {
            try {
                # Process item
                Process-Item -Item $item -ErrorAction Stop
                $processedCount++
            }
            catch {
                $errorCount++
                Write-Error "Failed to process item '$($item.Name)': $($_.Exception.Message)"
                
                # Decide whether to continue or stop
                if ($errorCount -gt 10) {
                    Write-Error "Too many errors, stopping processing"
                    break
                }
            }
        }
    }
    
    end {
        Write-Host "Processed $processedCount items with $errorCount errors"
    }
}
```

## Python Exception Handling

### Basic Exception Handling

```python
# Basic try-except
try:
    result = 10 / 0
except ZeroDivisionError:
    print("Cannot divide by zero")
except Exception as e:
    print(f"An error occurred: {e}")
finally:
    print("Cleanup code")

# Try-except-else
try:
    result = int("123")
except ValueError:
    print("Invalid number")
else:
    print(f"Success: {result}")
finally:
    print("Cleanup code")
```

### Exception Hierarchy

```python
# BaseException
# ├── SystemExit
# ├── KeyboardInterrupt
# ├── GeneratorExit
# └── Exception
#     ├── StopIteration
#     ├── StopAsyncIteration
#     ├── ArithmeticError
#     │   ├── FloatingPointError
#     │   ├── OverflowError
#     │   └── ZeroDivisionError
#     ├── AssertionError
#     ├── AttributeError
#     ├── BufferError
#     ├── EOFError
#     ├── ImportError
#     │   └── ModuleNotFoundError
#     ├── LookupError
#     │   ├── IndexError
#     │   └── KeyError
#     ├── MemoryError
#     ├── NameError
#     │   └── UnboundLocalError
#     ├── OSError
#     │   ├── BlockingIOError
#     │   ├── ChildProcessError
#     │   ├── ConnectionError
#     │   │   ├── BrokenPipeError
#     │   │   ├── ConnectionAbortedError
#     │   │   ├── ConnectionRefusedError
#     │   │   └── ConnectionResetError
#     │   ├── FileExistsError
#     │   ├── FileNotFoundError
#     │   ├── InterruptedError
#     │   ├── IsADirectoryError
#     │   ├── NotADirectoryError
#     │   ├── PermissionError
#     │   └── ProcessLookupError
#     ├── ReferenceError
#     ├── RuntimeError
#     │   ├── NotImplementedError
#     │   └── RecursionError
#     ├── SyntaxError
#     │   ├── IndentationError
#     │   │   └── TabError
#     │   └── UnicodeDecodeError
#     ├── SystemError
#     ├── TypeError
#     ├── ValueError
#     │   └── UnicodeError
#     │       ├── UnicodeDecodeError
#     │       ├── UnicodeEncodeError
#     │       └── UnicodeTranslateError
#     └── Warning
```

### Custom Exceptions

```python
# Create custom exception
class ValidationError(Exception):
    """Custom validation error."""
    
    def __init__(self, message, field=None, value=None):
        super().__init__(message)
        self.field = field
        self.value = value

class BusinessError(Exception):
    """Business logic error."""
    
    def __init__(self, message, error_code=None):
        super().__init__(message)
        self.error_code = error_code

# Using custom exceptions
def validate_user_data(data):
    """Validate user data."""
    if not data.get('name'):
        raise ValidationError("Name is required", field="name")
    
    if not isinstance(data.get('age'), int) or data['age'] < 0:
        raise ValidationError("Age must be a positive integer", field="age", value=data.get('age'))

def process_order(order):
    """Process an order."""
    if order['total'] > order['limit']:
        raise BusinessError(
            "Order total exceeds limit",
            error_code="ORDER_LIMIT_EXCEEDED"
        )
```

### Context Managers for Exception Handling

```python
# Custom context manager
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
        
        # Return True to suppress exception, False to re-raise
        if exc_type is not None:
            print(f"Exception occurred: {exc_val}")
            return False
        return True

# Using context manager
with DatabaseConnection("mydb") as conn:
    print(f"Using connection: {conn}")
    # If an exception occurs here, __exit__ will be called

# Contextlib module
from contextlib import contextmanager

@contextmanager
def file_manager(filename, mode):
    """Simple file manager context manager."""
    file = open(filename, mode)
    try:
        yield file
    finally:
        file.close()
```

### Async Exception Handling

```python
import asyncio

# Async exception handling
async def fetch_data(url):
    """Fetch data from URL asynchronously."""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(url) as response:
                return await response.text()
    except aiohttp.ClientError as e:
        print(f"HTTP error: {e}")
        raise
    except asyncio.TimeoutError:
        print("Request timed out")
        raise

# Handling multiple async exceptions
async def process_multiple_urls(urls):
    """Process multiple URLs."""
    tasks = [fetch_data(url) for url in urls]
    
    try:
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        for i, result in enumerate(results):
            if isinstance(result, Exception):
                print(f"Error processing {urls[i]}: {result}")
            else:
                print(f"Success: {urls[i]}")
    except Exception as e:
        print(f"Unexpected error: {e}")
```

## Cross-Language Best Practices

### Universal Exception Handling Principles

1. **Be Specific**: Catch specific exceptions, not generic ones
2. **Don't Swallow Exceptions**: Always handle or log exceptions appropriately
3. **Provide Context**: Include relevant information when re-throwing exceptions
4. **Resource Cleanup**: Always release resources, even when exceptions occur
5. **Fail Fast**: Detect and report errors as early as possible
6. **Meaningful Messages**: Provide clear, actionable error messages

### Exception Handling Anti-Patterns

```csharp
// ANTI-PATTERN: Catching all exceptions
try {
    // Some code
}
catch (Exception ex) {
    // This hides the specific error type
    Console.WriteLine("Something went wrong");
}

// ANTI-PATTERN: Empty catch blocks
try {
    // Some code
}
catch {
    // Completely ignores the error
}

// ANTI-PATTERN: Catching and re-throwing incorrectly
try {
    // Some code
}
catch (Exception ex) {
    throw ex; // Loses stack trace
}

// CORRECT: Use throw; to preserve stack trace
try {
    // Some code
}
catch (Exception ex) {
    // Handle and re-throw
    throw;
}
```

### Language-Specific Considerations

#### C#
- Use `using` statements for resource management
- Implement `IDisposable` for custom resources
- Use exception filters for conditional handling
- Consider `async/await` exception propagation

#### PowerShell
- Understand the difference between terminating and non-terminating errors
- Use `-ErrorAction` parameter appropriately
- Implement proper error handling in advanced functions
- Use `$ErrorActionPreference` for global error handling

#### Python
- Use context managers (`with` statement) for resource management
- Understand exception chaining with `raise ... from ...`
- Use specific exception types from the hierarchy
- Implement proper async exception handling

## Exception Handling Patterns

### Retry Pattern

```csharp
// C# Retry Pattern
public async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxAttempts = 3, TimeSpan delay = null)
{
    delay = delay ?? TimeSpan.FromSeconds(1);
    
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {delay.TotalSeconds}s...");
            await Task.Delay(delay);
        }
    }
    
    throw new InvalidOperationException($"Operation failed after {maxAttempts} attempts");
}

// Usage
var result = await RetryAsync(() => ApiService.GetDataAsync());
```

```python
# Python Retry Pattern
import time
from functools import wraps

def retry(max_attempts=3, delay=1, exceptions=(Exception,)):
    """Retry decorator."""
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            for attempt in range(1, max_attempts + 1):
                try:
                    return func(*args, **kwargs)
                except exceptions as e:
                    if attempt == max_attempts:
                        raise
                    print(f"Attempt {attempt} failed: {e}. Retrying in {delay}s...")
                    time.sleep(delay)
        return wrapper
    return decorator

# Usage
@retry(max_attempts=3, delay=2, exceptions=(ConnectionError, TimeoutError))
def fetch_data(url):
    # Function that might fail
    pass
```

### Circuit Breaker Pattern

```csharp
// C# Circuit Breaker Pattern
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _recoveryTimeout;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    
    public CircuitBreaker(int failureThreshold = 3, TimeSpan? recoveryTimeout = null)
    {
        _failureThreshold = failureThreshold;
        _recoveryTimeout = recoveryTimeout ?? TimeSpan.FromMinutes(1);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _recoveryTimeout)
            {
                _state = CircuitBreakerState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
        }
        
        try
        {
            var result = await operation();
            
            if (_state == CircuitBreakerState.HalfOpen)
            {
                Reset();
            }
            
            return result;
        }
        catch (Exception ex)
        {
            RecordFailure();
            throw;
        }
    }
    
    private void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
        
        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitBreakerState.Open;
        }
    }
    
    private void Reset()
    {
        _failureCount = 0;
        _state = CircuitBreakerState.Closed;
    }
    
    enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }
}
```

### Result Pattern (Exception-Free Error Handling)

```python
# Python Result Pattern
from typing import Generic, TypeVar, Union
from dataclasses import dataclass

T = TypeVar('T')
E = TypeVar('E')

@dataclass
class Result(Generic[T, E]):
    """Result type for exception-free error handling."""
    value: Union[T, None] = None
    error: Union[E, None] = None
    
    @property
    def is_success(self) -> bool:
        return self.error is None
    
    @property
    def is_failure(self) -> bool:
        return self.error is not None
    
    @classmethod
    def success(cls, value: T) -> 'Result[T, E]':
        return cls(value=value, error=None)
    
    @classmethod
    def failure(cls, error: E) -> 'Result[T, E]':
        return cls(value=None, error=error)
    
    def map(self, func):
        """Map success value through function."""
        if self.is_success:
            try:
                return Result.success(func(self.value))
            except Exception as e:
                return Result.failure(e)
        return self
    
    def flat_map(self, func):
        """Flat map for chaining Result operations."""
        if self.is_success:
            return func(self.value)
        return self

# Usage
def divide(a: float, b: float) -> Result[float, str]:
    """Divide two numbers, returning Result instead of raising exception."""
    if b == 0:
        return Result.failure("Cannot divide by zero")
    return Result.success(a / b)

# Chain operations
result = divide(10, 2).map(lambda x: x * 2).flat_map(lambda x: divide(x, 4))
```

## Logging and Monitoring

### Structured Logging

```csharp
// C# Structured Logging with Serilog
using Serilog;

public class OrderProcessor
{
    private readonly ILogger _logger = Log.ForContext<OrderProcessor>();
    
    public async Task ProcessOrderAsync(Order order)
    {
        _logger.Information("Processing order {OrderId} for customer {CustomerId}", 
            order.Id, order.CustomerId);
        
        try
        {
            var result = await ProcessPaymentAsync(order);
            
            _logger.Information("Order {OrderId} processed successfully", order.Id);
        }
        catch (PaymentException ex)
        {
            _logger.Error(ex, "Payment failed for order {OrderId}", order.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error processing order {OrderId}", order.Id);
            throw;
        }
    }
}
```

```python
# Python Structured Logging with structlog
import structlog

logger = structlog.get_logger()

class OrderProcessor:
    def process_order(self, order):
        logger.info("Processing order", order_id=order.id, customer_id=order.customer_id)
        
        try:
            result = self.process_payment(order)
            logger.info("Order processed successfully", order_id=order.id)
            return result
        except PaymentException as e:
            logger.error("Payment failed", order_id=order.id, error=str(e))
            raise
        except Exception as e:
            logger.error("Unexpected error", order_id=order.id, error=str(e), exc_info=True)
            raise
```

### Exception Metrics

```python
# Python Exception Metrics
from collections import defaultdict
import time

class ExceptionTracker:
    """Track exception occurrences and rates."""
    
    def __init__(self):
        self._counts = defaultdict(int)
        self._last_occurrence = {}
        self._rates = defaultdict(float)
    
    def record_exception(self, exception_type: str):
        """Record an exception occurrence."""
        self._counts[exception_type] += 1
        self._last_occurrence[exception_type] = time.time()
        
        # Calculate rate (exceptions per minute)
        if exception_type in self._rates:
            time_diff = time.time() - self._rates[exception_type + "_time"]
            if time_diff > 60:
                self._rates[exception_type] = self._counts[exception_type] / (time_diff / 60)
                self._rates[exception_type + "_time"] = time.time()
        else:
            self._rates[exception_type] = 1.0
            self._rates[exception_type + "_time"] = time.time()
    
    def get_stats(self):
        """Get exception statistics."""
        return {
            "counts": dict(self._counts),
            "last_occurrence": dict(self._last_occurrence),
            "rates": {k: v for k, v in self._rates.items() if not k.endswith("_time")}
        }
```

## Error Recovery Strategies

### Graceful Degradation

```csharp
// C# Graceful Degradation
public class DataProvider
{
    private readonly IPrimaryDataSource _primary;
    private readonly ISecondaryDataSource _secondary;
    private readonly ILogger _logger;
    
    public async Task<Data> GetDataAsync()
    {
        try
        {
            // Try primary source
            return await _primary.GetDataAsync();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Primary data source failed, trying secondary");
            
            try
            {
                // Fall back to secondary source
                return await _secondary.GetDataAsync();
            }
            catch (Exception ex2)
            {
                _logger.Error(ex2, "All data sources failed");
                
                // Return cached/default data
                return GetDefaultData();
            }
        }
    }
    
    private Data GetDefaultData()
    {
        // Return minimal default data
        return new Data { /* default values */ };
    }
}
```

### Bulkhead Pattern

```python
# Python Bulkhead Pattern
import asyncio
from concurrent.futures import ThreadPoolExecutor

class Bulkhead:
    """Isolate resources to prevent cascading failures."""
    
    def __init__(self, max_workers=5):
        self._executor = ThreadPoolExecutor(max_workers=max_workers)
        self._semaphore = asyncio.Semaphore(max_workers)
    
    async def execute(self, func, *args, **kwargs):
        """Execute function with bulkhead protection."""
        async with self._semaphore:
            loop = asyncio.get_event_loop()
            return await loop.run_in_executor(self._executor, func, *args, **kwargs)
    
    def shutdown(self):
        """Shutdown the bulkhead."""
        self._executor.shutdown(wait=True)

# Usage
bulkhead = Bulkhead(max_workers=3)

async def process_requests(requests):
    tasks = [bulkhead.execute(process_request, req) for req in requests]
    results = await asyncio.gather(*tasks, return_exceptions=True)
    return results
```

## Exception Handling in UDEO

### UDEO Exception Handling Strategy

The UDEO system implements a comprehensive exception handling strategy that ensures reliability and maintainability across all components:

1. **Layered Exception Handling**: Each layer handles exceptions appropriate to its level
2. **Standardized Error Codes**: Consistent error codes across the system
3. **Centralized Logging**: All exceptions are logged with context
4. **Graceful Degradation**: System continues operating at reduced capacity when possible
5. **Error Recovery**: Automatic recovery mechanisms for common failure scenarios

### UDEO Exception Classes

```csharp
// UDEO-specific exception hierarchy
namespace UDEO.Core.Exceptions
{
    // Base UDEO exception
    public class UDEOException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, object> Context { get; }
        
        public UDEOException(string message, string errorCode, Dictionary<string, object> context = null)
            : base(message)
        {
            ErrorCode = errorCode;
            Context = context ?? new Dictionary<string, object>();
        }
    }
    
    // Expert system exceptions
    public class ExpertException : UDEOException
    {
        public string ExpertId { get; }
        
        public ExpertException(string expertId, string message, string errorCode, Dictionary<string, object> context = null)
            : base(message, errorCode, context)
        {
            ExpertId = expertId;
        }
    }
    
    // Workflow exceptions
    public class WorkflowException : UDEOException
    {
        public string WorkflowId { get; }
        public string ActivityId { get; }
        
        public WorkflowException(string workflowId, string activityId, string message, string errorCode, Dictionary<string, object> context = null)
            : base(message, errorCode, context)
        {
            WorkflowId = workflowId;
            ActivityId = activityId;
        }
    }
    
    // Data bus exceptions
    public class DataBusException : UDEOException
    {
        public string Operation { get; }
        
        public DataBusException(string operation, string message, string errorCode, Dictionary<string, object> context = null)
            : base(message, errorCode, context)
        {
            Operation = operation;
        }
    }
}
```

### UDEO Error Handling Guidelines

1. **Use UDEO Exception Classes**: Always use the appropriate UDEO exception class
2. **Include Context**: Provide relevant context information with exceptions
3. **Log Everything**: Ensure all exceptions are properly logged
4. **Don't Expose Internal Details**: Use user-friendly error messages
5. **Implement Recovery**: Where possible, implement automatic recovery mechanisms

This comprehensive exception handling guide provides a complete reference for handling errors across multiple programming languages, ensuring robust and reliable error management in the UDEO system.