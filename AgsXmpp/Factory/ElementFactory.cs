using AgsXmpp.Xml;

namespace AgsXmpp.Factory;

public class ElementFactory
{
    static ElementFactory()
    {

    }

    public static Element GetElement(string prefix, string name, string ns)
    {
        // TODO: Lookup for XML in table.

        string tagname = name;

        if (!string.IsNullOrEmpty(prefix))
            tagname = $"{prefix}:{tagname}";

        return new Element(tagname, ns);
    }
}
