namespace AgsXmpp.Collections;

public class BareJidComparer : IComparer<Jid>
{
    public static BareJidComparer Instance { get; } = new();

    public int Compare(Jid x, Jid y)
    {
        if (!x.IsBare)
            x = x.Bare;

        if (!y.IsBare)
            y = y.Bare;

        bool result = x.User?.Equals(y.User) == true
            && x.Server?.Equals(y.Server) == true;
        return result ? 0 : -1;
    }
}