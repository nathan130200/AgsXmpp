namespace AgsXmpp.Collections;

public class FullJidComparer : IComparer<Jid>
{
    public static FullJidComparer Instance { get; } = new();

    public int Compare(Jid x, Jid y)
    {
        if (!x.IsFull)
            return -1;

        if (!y.IsFull)
            return 1;

        var cmp = StringComparison.OrdinalIgnoreCase;

        bool result = x.User?.Equals(y.User, cmp) == true
            && x.Server?.Equals(y.Server, cmp) == true
            && x.Resource?.Equals(y.Resource, cmp) == true;

        return result ? 0 : -1;
    }
}
