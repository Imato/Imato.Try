# Imato.Try
Simple try-retry strategy 

#### Try

```csharp
using Imato.Try;

// Execution options
var options = new TryOptions
{
    RetryCount = 3,
    Delay = 20,
    Timeout = 2000
};

// Function to execute
var testFunc1 = (int id) =>
{
    if (id % 2 == 0)
    {
        return Task.FromResult(id);
    }
    throw new ArgumentException(nameof(id));
};

// Default behavior
var result = await Try
    .Function(() => testFunc1(2))
    .GetResultAsync();

// Add exception handler
result = await Try
    .Function(() => testFunc1(2))
    .OnError((ex) => Console.WriteLine(ex.Message))
    .GetResultAsync();

// With retry and over options
result = await Try
    .Function(() => testFunc1(2))
    .Setup(options)
    .OnError((ex) => Console.WriteLine(ex.Message))
    .GetResultAsync();

// Without result
var errorCount = 0;
await Try
    .Function(() => testFunc2(1))
    .Function(() => testFunc2(2))
    .Function(() => testFunc2(3))
    .OnError((ex) => errorCount++)
    .OnError((ex) => Console.WriteLine(ex.Message))
    .Setup(options)
    .ExecuteAsync();

```