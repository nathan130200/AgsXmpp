using System.Diagnostics;
using System.Security;
using System.Text;

namespace AgsXmpp;

[DebuggerDisplay("{ToString(),nq}")]
public class Jid : IEquatable<Jid>
{
    private string _user, _server, _resource;

    Jid()
    {

    }

    public Jid(string jid)
    {
        var at = jid.IndexOf('@');

        User = jid[0..at];

        var temp = jid[(at + 1)..];

        var slash = temp.IndexOf('/');

        if (slash == -1)
            Server = temp;
        else
        {
            Server = temp[0..slash];
            Resource = temp[(slash + 1)..];
        }
    }

    public static Jid Empty => new();

    public bool IsBare => string.IsNullOrWhiteSpace(_resource);
    public bool IsFull => !IsBare;

    public Jid(string user, string server, string resource)
    {
        User = user;
        Server = server;
        Resource = resource;
    }

    public Jid(string user, string server) : this(user, server, string.Empty)
    {
        User = user;
        Server = server;
    }

    public Jid Bare => new(_user, _server);

    public string User
    {
        get => _user;
        set => _user = SecurityElement.Escape(value);
    }

    public string Server
    {
        get => _server;
        set => _server = SecurityElement.Escape(value);
    }

    public string Resource
    {
        get => _resource;
        set => _resource = SecurityElement.Escape(value);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_user))
            sb.Append(_user).Append('@');

        sb.Append(_server);

        if (!string.IsNullOrWhiteSpace(_resource))
            sb.Append('/').Append(_resource);

        return sb.ToString();
    }

    public override int GetHashCode()
        => HashCode.Combine(_user, _server, _resource);

    public override bool Equals(object obj)
        => obj is Jid other && Equals(other);

    public bool Equals(Jid other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(other, this))
            return true;

        return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
