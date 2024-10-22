// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;

namespace PaymentGateway.Lib.Functional;

/*
 * TODO: disclaimer
 * This code is from one of my libraries that I haven't made public yet. I thought I'd bring it in here to show
 * some of the coding style I'm exploring. Keen to hear thoughts.
 */

public struct NoneType;

public readonly struct Opt<T> : IEquatable<Opt<T>>, IEquatable<NoneType> where T : notnull
{
    private readonly T? Value;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSome { get; }

    public static Opt<T> None => new();
    public bool IsNone => !IsSome;

    internal Opt(T value) => (Value, IsSome) = (value, true);

    public Opt()
    {
        IsSome = false;
        Value = default;
    }

    public R Match<R>(Func<R> none, Func<T, R> some)
        => IsSome ? some(Value) : none();

    public void Deconstruct(out T? value, out bool isSome) => (value, isSome) = (Value, IsSome);

    public bool Equals(NoneType _) => IsNone;

    public bool Equals(Opt<T> other) =>
        IsSome == other.IsSome
        && (IsNone || Value!.Equals(other.Value));

    public override bool Equals(object? other)
        => other switch
        {
            NoneType none => Equals(none),
            Opt<T> opt => Equals(opt),
            _ => false
        };

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, IsSome);
    }

    public static implicit operator Opt<T>(T? value) => value == null ? None : new Opt<T>(value);
    public static implicit operator Opt<T>(NoneType _) => new();
    public static bool operator true(Opt<T> @this) => @this.IsSome;
    public static bool operator false(Opt<T> @this) => @this.IsNone;
    public static Opt<T> operator |(Opt<T> l, Opt<T> r) => l.IsSome ? l : r;
    public static bool operator ==(Opt<T> left, Opt<T> right) => Equals(left, right);
    public static bool operator !=(Opt<T> left, Opt<T> right) => !(left == right);
    public static implicit operator string(Opt<T> opt) => opt.ToString();

    public override string ToString() => IsSome ? $"Some({Value})" : "None";
}

public static partial class Prelude
{
    private static NoneType _None => new();
    public static NoneType None => _None;

    public static Opt<T> Some<T>(T value) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return new Opt<T>(value);
    }
}
