namespace AgsXmpp.Protocol.Base;

public class Stanza : DirectionalElement
{
    public Stanza(string tag) : base(tag)
    {
    }

    public Stanza(string tag, string xmlns) : base(tag, xmlns)
    {
    }

    public string Id
    {
        get
        {
            if (HasAttribute("id"))
                return GetAttribute("id");

            return string.Empty;
        }
        set
        {
            if (value is null)
                value = string.Empty;

            SetAttribute("id", value);
        }
    }

    /// <summary>
    /// Generates a automatic id for the packet and overwrite existing.
    /// </summary>
    public void GenerateId()
        => Id = IdGenerator.Guid.NextId();

    public string Language
    {
        get => GetAttribute("xml:lang");
        set
        {
            if (string.IsNullOrEmpty(value))
                RemoveAttribute("xml:lang");
            else
                SetAttribute("xml:lang", value);
        }
    }
}