namespace AgsXmpp.Protocol;

public readonly struct IqType : IParsable<IqType>
{
    readonly string value;
    IqType(string value) => this.value = value;

    public static IqType Get { get; } = new("get");
    public static IqType Set { get; } = new("set");
    public static IqType Result { get; } = new("result");
    public static IqType Error { get; } = new("error");

    public static IEnumerable<IqType> Values
    {
        get
        {
            yield return Get;
            yield return Set;
            yield return Result;
            yield return Error;
        }
    }

    public override int GetHashCode()
        => value.GetHashCode();

    public override bool Equals(object obj)
    {
        return obj is IqType other
            && other.value.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(IqType lhs, IqType rhs) => lhs.Equals(rhs);
    public static bool operator !=(IqType lhs, IqType rhs) => !(lhs == rhs);
    public override string ToString() => value;

    public static IqType? Parse(string s)
    {
        foreach (var it in Values)
        {
            if (it.value.Equals(s, StringComparison.OrdinalIgnoreCase))
                return it;
        }

        return default;
    }

    static IqType IParsable<IqType>.Parse(string s, IFormatProvider _)
        => Parse(s) ?? Get;

    static bool IParsable<IqType>.TryParse(string s, IFormatProvider _, out IqType result)
    {
        var it = Parse(s);
        result = it.GetValueOrDefault(Get);
        return it.HasValue;
    }
}
