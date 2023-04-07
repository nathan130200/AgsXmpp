using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;

namespace AgsXmpp.Xml;

[DebuggerDisplay("{StartTag(),nq}")]
public class Element : ICloneable
{
    private string _tagName;
    private string _prefix;
    private string _value;
    private List<Element> _children;
    private Dictionary<string, string> _attributes;
    private Element _parent;

    public Element(Element other)
    {
        _tagName = other.TagName;
        _prefix = other.Prefix;
        _value = other.Value;
        _children = new(other.Children);
        _attributes = new(other.Attributes);
    }

    Element()
    {
        _children = new List<Element>();
        _attributes = new Dictionary<string, string>();
    }

    public Element(string tag, object attrs = default) : this()
    {
        var off = tag.IndexOf(':');

        if (off == -1)
            TagName = tag;
        else
        {
            Prefix = tag[0..off];
            TagName = tag[(off + 1)..];
        }

        SetAttributes(attrs);
    }

    public Element(string tag, string xmlns, object attrs = default) : this(tag, attrs)
    {
        SetNamespace(xmlns);
    }

    public string TagName
    {
        get => _tagName;
        set => _tagName = XmlConvert.EncodeName(value);
    }

    public string Prefix
    {
        get => _prefix;
        set => _prefix = XmlConvert.EncodeName(value);
    }

    public string Value
    {
        get => _value;
        set => _value = SecurityElement.Escape(value);
    }

    public IReadOnlyList<Element> Children
    {
        get
        {
            Element[] result;

            lock (_children)
                result = _children.ToArray();

            return result.ToList();
        }
    }

    public IReadOnlyDictionary<string, string> Attributes
    {
        get
        {
            KeyValuePair<string, string>[] result;

            lock (_attributes)
                result = _attributes.ToArray();

            return result.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public string StartTag()
    {
        var sb = new StringBuilder()
            .Append('<')
            .Append(GetElementKey(this))
            .Append(' ');

        foreach (var (key, value) in Attributes)
            sb.AppendFormat("{0}=\"{1}\" ", key, value);

        return sb.Append('>').ToString();
    }

    public void SetAttributes(object attrs)
    {
        if (attrs is null)
            return;

        lock (_attributes)
        {
            foreach (var p in attrs.GetType().GetTypeInfo().DeclaredProperties)
            {
                string attrVal;

                var value = p.GetValue(attrs);

                if (value is null)
                    attrVal = string.Empty;
                else
                {
                    if (value is IFormattable fmt)
                        attrVal = fmt.ToString(null, CultureInfo.InvariantCulture);
                    else
                        attrVal = value.ToString();
                }

                _attributes[p.Name] = attrVal;
            }
        }
    }

    public bool HasAttribute(string name)
    {
        lock (_attributes)
            return _attributes.ContainsKey(name);
    }

    public string GetAttribute(string name)
    {
        string value = null;

        lock (_attributes)
            _attributes.TryGetValue(name, out value);

        return value;
    }

    public bool RemoveAttribute(string name)
    {
        lock (_attributes)
            return _attributes.Remove(name);
    }

    public void SetAttribute(string name, string value)
    {
        lock (_attributes)
        {
            if (value is null)
                _attributes.Remove(name);
            else
                _attributes[name] = value;
        }
    }

    public void SetNamespace(string prefix, string ns)
    {
        var sb = new StringBuilder("xmlns");

        if (!string.IsNullOrWhiteSpace(prefix))
            sb.Append(':').Append(prefix);

        SetAttribute(sb.ToString(), ns);
    }

    public void SetNamespace(string ns)
        => SetNamespace(default, ns);

    public string Namespace
    {
        get => GetNamespace(Prefix);
        set => SetAttribute(GetNamespaceKey(Prefix), value);
    }

    public string GetNamespace(string prefix, bool expandSearch = true)
    {
        var temp = GetAttribute(GetNamespaceKey(prefix));

        if (string.IsNullOrEmpty(temp) && expandSearch)
            return Parent?.GetNamespace(prefix);

        return temp;
    }

    public bool IsRootElement
        => _parent is null;

    public Element Parent
    {
        get => _parent;
        set
        {
            _parent?.RemoveChild(this);
            _parent = value;
        }
    }

    public Element GetRoot()
    {
        var temp = this;

        while (!temp.IsRootElement)
            temp = temp.Parent;

        return temp;
    }

    public Element AddChild(Element e)
    {
        if (ReferenceEquals(e.Parent, this))
            return this;

        e.Parent?.RemoveChild(e);

        lock (_children)
            _children.Add(e);

        e.Parent = this;

        return this;
    }

    public void RemoveChild(Element e)
    {
        if (!ReferenceEquals(e.Parent, this))
            return;

        lock (_children)
            _children.Remove(e);

        e.Parent = null;
    }

    public void Remove()
        => Parent?.RemoveChild(this);

    public Element Clone()
    {
        var el = new Element
        {
            TagName = _tagName,
            Prefix = _prefix,
            Value = _value
        };

        foreach (var (key, value) in Attributes)
            el.SetAttribute(key, value);

        foreach (var child in Children)
            el.AddChild(child.Clone());

        return el;
    }

    public string StarTag()
    {
        var sb = new StringBuilder()
            .Append('<');

        if (!string.IsNullOrEmpty(Prefix))
            sb.Append(Prefix).Append(':');

        sb.Append(TagName).Append(' ');

        foreach (var (key, value) in Attributes)
            sb.AppendFormat("{0}=\"{1}\" ", key, value);

        return sb.Append('>').ToString();
    }

    public string EndTag()
    {
        var sb = new StringBuilder("</");

        if (!string.IsNullOrEmpty(Prefix))
            sb.Append(Prefix).Append(':');

        return sb.Append(TagName).Append('>').ToString();
    }

    object ICloneable.Clone()
        => Clone();

    public override string ToString()
    {
        return ToString(false);
    }

    public string ToString(bool indent, int indentSize = 1)
    {
        var sb = new StringBuilder();

        using var xw = XmlWriter.Create(sb, new XmlWriterSettings
        {
            Indent = indent,
            IndentChars = indent ? new string(' ', Math.Max(1, indentSize)) : string.Empty,
            CheckCharacters = true,
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
            NamespaceHandling = NamespaceHandling.OmitDuplicates
        });

        ToString(this, xw);
        xw.Flush();

        return sb.ToString();
    }

    static string GetNamespaceKey(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return "xmlns";

        return $"xmlns:{prefix}";
    }

    static string GetElementKey(Element el)
    {
        if (string.IsNullOrEmpty(el.Prefix))
            return el.TagName;

        return string.Concat(el.Prefix, ':', el.TagName);
    }

    static void ExtractFromQName(string key, out string localName, out string prefix)
    {
        prefix = default;

        var off = key.IndexOf(':');

        if (off == -1)
            localName = key;
        else
        {
            prefix = key[0..off];
            localName = key[(off + 1)..];
        }
    }

    static void ToString(Element el, XmlWriter xw)
    {
        if (string.IsNullOrEmpty(el.Prefix))
            xw.WriteStartElement(el.TagName, el.Namespace ?? string.Empty);
        else
            xw.WriteStartElement(el.Prefix, el.TagName, el.Namespace ?? string.Empty);

        foreach (var (key, value) in el.Attributes)
        {
            ExtractFromQName(key, out var localName, out var prefix);

            if (prefix == "xmlns" && localName == el.Prefix)
                continue;

            if (!string.IsNullOrEmpty(prefix))
                xw.WriteAttributeString(localName, el.GetAttribute(prefix), value);
            else
                xw.WriteAttributeString(localName, value);
        }

        foreach (var child in el.Children)
            ToString(child, xw);

        if (!string.IsNullOrWhiteSpace(el.Value))
            xw.WriteString(el.Value);

        xw.WriteEndElement();
    }
}
