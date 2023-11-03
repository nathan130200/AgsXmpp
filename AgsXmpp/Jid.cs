using AgsXmpp.Helpers;

namespace AgsXmpp;

public sealed class Jid : IEquatable<Jid>
{
    public string User
    {
        get;
        init;
    }

    public string Server
    {
        get;
        init;
    }

    public string Resource
    {
        get;
        init;
    }

    public Jid()
    {

    }

    public Jid(string jid)
    {
        ArgumentException.ThrowIfNullOrEmpty(jid);

        var ofs = jid.IndexOf('@');

        if (ofs != -1)
        {
            User = jid[0..ofs];
            jid = jid[1..];
        }

        ofs = jid.IndexOf('/');

        if (ofs == -1)
            Server = jid;
        else
        {
            Server = jid[0..ofs];
            Resource = jid[(ofs + 1)..];
        }
    }

    public Jid(string user = default, string server = default, string resource = default)
    {
        User = user;
        Server = server;
        Resource = resource;
    }

    public override int GetHashCode()
        => HashCode.Combine(User ?? string.Empty,
            Server ?? string.Empty,
            Resource ?? string.Empty);

    const StringComparison strCompareMethod = StringComparison.Ordinal;

    public bool Equals(Jid other)
    {
        if (ReferenceEquals(other, null))
            return false;

        if (ReferenceEquals(other, this))
            return true;

        bool result =
            string.Compare(User, other.User, strCompareMethod) == 0 &&
            string.Compare(Server, other.Server, strCompareMethod) == 0;

        if (IsBare)
            return result;

        return result && string.Compare(Resource, other.Resource, strCompareMethod) == 0;
    }

    public bool IsBare
        => string.IsNullOrEmpty(Resource);

    public bool IsFull
        => !IsBare;

    public Jid Bare
        => new(user: User, server: Server);

    public override string ToString()
    {
        using (var sb = new BufferedStringBuilder())
        {
            if (User is not null)
                sb.Append(User).Append('@');

            if (Server is not null)
                sb.Append(Server);

            if (Resource is not null)
                sb.Append('/').Append(Resource);

            return sb.ToString();
        }
    }
}