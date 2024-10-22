// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace PaymentGateway.Lib.Functional;

/*
 * TODO: disclaimer
 * This code is from one of my libraries that I haven't made public yet. I thought I'd bring it in here to show
 * some of the coding style I'm exploring. Keen to hear thoughts.
 */

public class Val<T>
{
    [MemberNotNullWhen(false, nameof(IsValid))]
    private string? Error { get; }
    
    [MemberNotNullWhen(true, nameof(IsValid))]
    private T? Value { get; }
    
    public bool IsValid { get; }
    
    public bool IsInvalid([NotNullWhen(true)] out string? error)
    {
        error = Error;
        return !IsValid;
    }

    private Val(T result) => (Value, Error, IsValid) = (result, null, true);
    private Val(string error) => (Value, Error, IsValid) = (default, error, false);
    
    public static Val<T> Valid(T result) => new(result);
    public static Val<T> Invalid(string error) => new(error);

    public static implicit operator Val<T>(T value) => new(value);
    public static implicit operator Val<T>(string error) => new(error);
    
    public bool TryGet([NotNullWhen(true)] out T? result)
    {
        result = Value;
        return IsValid;
    }

    public void Match(Action<string> invalid, Action<T> valid)
    {
        if (IsValid)
            valid(Value!);
        else
            invalid(Error!);
    }

    public R Match<R>(Func<string, R> invalid, Func<T, R> valid) 
        =>
        IsValid
            ? valid(Value!)
            : invalid(Error!);

    public Val<R> Map<R>(Func<T, R> mapper) 
        =>
        Match(
            err => new Val<R>(err),
            value => new Val<R>(mapper(value))
        );

    public Task<Val<R>> Map<R>(Func<T, Task<R>> mapper) 
        =>
        Match(
            err => Task.FromResult(new Val<R>(err)), 
            async value => new Val<R>(await mapper(value))
        );
}

public static class ValidationExt
{
    /// <summary>
    /// If both <b>left</b> and <b>right</b> are valid, applies the combinator function to produce the result.
    /// </summary>
    public static Val<R> Combine<T, T1, R>(
        this Val<T> left,
        Val<T1> right,
        Func<T, T1, R> combinator)
        =>
            left.Match(
                Val<R>.Invalid,
                tVal =>
                    right.Match(
                        Val<R>.Invalid,
                        t1Val => Val<R>.Valid(combinator(tVal, t1Val))));
}

public static partial class Prelude
{
    public static Val<T> Valid<T>(T value) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return Val<T>.Valid(value);
    }

    public static Val<T> Invalid<T>(string error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return Val<T>.Invalid(error);
    }
}
