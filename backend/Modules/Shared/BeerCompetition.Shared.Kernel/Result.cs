namespace BeerCompetition.Shared.Kernel;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Provides a railway-oriented programming approach to error handling without exceptions.
/// </summary>
/// <remarks>
/// Use Result pattern instead of throwing exceptions for business logic errors.
/// Exceptions should only be used for technical/infrastructure failures.
/// 
/// Example usage:
/// <code>
/// public Result ValidateRegistrationDeadline(DateTime deadline)
/// {
///     if (deadline < DateTime.UtcNow)
///         return Result.Failure("Registration deadline must be in the future");
///     
///     return Result.Success();
/// }
/// </code>
/// </remarks>
public record Result
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error message if the operation failed.
    /// Empty string if operation succeeded.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Creates a new Result instance.
    /// Use factory methods Success() or Failure() instead of calling constructor directly.
    /// </summary>
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Successful result cannot have an error message");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failed result must have an error message");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }

    /// <summary>
    /// Creates a successful result with no return value.
    /// </summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="error">Error message describing why the operation failed.</param>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Implicit conversion from Result to bool for convenient success checking.
    /// </summary>
    public static implicit operator bool(Result result) => result.IsSuccess;
}

/// <summary>
/// Represents the result of an operation that returns a value if successful.
/// Provides type-safe error handling without exceptions.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// public Result&lt;Competition&gt; CreateCompetition(string name, DateTime deadline)
/// {
///     if (string.IsNullOrWhiteSpace(name))
///         return Result&lt;Competition&gt;.Failure("Competition name is required");
///     
///     var competition = new Competition { Name = name, RegistrationDeadline = deadline };
///     return Result&lt;Competition&gt;.Success(competition);
/// }
/// </code>
/// </remarks>
public record Result<T> : Result
{
    /// <summary>
    /// The value returned by the operation if successful.
    /// Will be default(T) if operation failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a new Result&lt;T&gt; instance.
    /// Use factory methods Success() or Failure() instead of calling constructor directly.
    /// </summary>
    private Result(bool isSuccess, T? value, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with a return value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    public static Result<T> Success(T value) => new(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message and no value.
    /// </summary>
    /// <param name="error">Error message describing why the operation failed.</param>
    public new static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Implicit conversion from Result&lt;T&gt; to T for convenient value access.
    /// Throws if result is a failure - always check IsSuccess before accessing Value.
    /// </summary>
    public static implicit operator T?(Result<T> result) => result.Value;
}
