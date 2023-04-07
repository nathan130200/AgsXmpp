using AgsXmpp.Xml;

namespace AgsXmpp.Protocol.Base;

public class DirectionalElement : Element
{
    public DirectionalElement(string tag) : base(tag)
    {
    }

    public DirectionalElement(string tag, string xmlns) : base(tag, xmlns)
    {

    }

    public Jid From
    {
        get
        {
            if (HasAttribute("from"))
                return new(GetAttribute("from"));

            return null;
        }
        set
        {
            if (value is null)
                RemoveAttribute("from");
            else
                SetAttribute("from", value.ToString());
        }
    }

    public Jid To
    {
        get
        {
            if (HasAttribute("to"))
                return new(GetAttribute("to"));

            return null;
        }
        set
        {
            if (value is null)
                RemoveAttribute("to");
            else
                SetAttribute("to", value.ToString());
        }
    }

    public void SwitchDirection()
    {
        var from = From;
        var to = To;

        RemoveAttribute("from");
        RemoveAttribute("to");

        (to, from) = (from, to);

        From = from;
        To = to;
    }
}

public class Stream : Stanza
{
    public Stream() : base("stream:stream")
    {
        SetNamespace("stream", Xmlns.STREAM);
    }
}