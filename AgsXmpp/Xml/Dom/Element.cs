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
        lock (_attributes)
        {
            if (_attributes.TryGetValue(name, out var value))
                return value;

            return null;
        }
    }

    public void RemoveAttribute(string name)
    {
        lock (_attributes)
            _attributes.Remove(name);
    }

    public void SetAttribute(string name, string value)
    {
        lock (_attributes)
            _attributes[name] = value;
    }

    public void SetNamespace(string prefix, string xmlns)
    {
        var key = nameof(xmlns);

        if (!string.IsNullOrWhiteSpace(prefix))
            key += $":{prefix}";

        lock (_attributes)
            _attributes[key] = xmlns;
    }

    public void SetNamespace(string xmlns)
    {
        lock (_attributes)
        {
            if (string.IsNullOrWhiteSpace(xmlns))
                _attributes.Remove(nameof(xmlns));
            else
                _attributes[nameof(xmlns)] = xmlns;
        }
    }

    public bool IsRootElement => _parent is null;

    public Element Parent
    {
        get => _parent;
        set
        {
            _parent?.RemoveChild(this);
            _parent = value;
        }
    }

    public void AddChild(Element e)
    {
        if (ReferenceEquals(e.Parent, this))
            return;

        e.Parent?.RemoveChild(e);

        lock (_children)
            _children.Add(e);

        e.Parent = this;
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
}
