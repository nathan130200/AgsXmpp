using System.Diagnostics;

namespace AgsXmpp.Xml.XpNet;

/// <summary>
/// Namespace stack.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public class NS
{
    readonly Stack<Dictionary<string, string>> _stack = new();

    /// <summary>
    /// Create a new stack, primed with xmlns and xml as prefixes.
    /// </summary>
    public NS()
    {
        PushScope();
        AddNamespace("xmlns", "http://www.w3.org/2000/xmlns/");
        AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
    }

    /// <summary>
    /// Declare a new scope, typically at the start of each element
    /// </summary>
    public void PushScope()
    {
        _stack.Push(new());
    }

    /// <summary>
    /// Pop the current scope off the stack.  Typically at the end of each element.
    /// </summary>
    public void PopScope()
    {
        if (_stack.TryPop(out var dict))
            dict.Clear();
    }

    /// <summary>
    /// Add a namespace to the current scope.
    /// </summary>
    public void AddNamespace(string prefix, string uri)
    {

        _stack.Peek().Add(prefix, uri);
    }

    /// <summary>
    /// Lookup a prefix to find a namespace.  Searches down the stack, starting at the current scope.
    /// </summary>
    public string LookupNamespace(string prefix)
    {
        foreach (var dict in _stack)
        {
            if (dict.Count > 0 && dict.ContainsKey(prefix))
                return dict[prefix];
        }

        return string.Empty;
    }

    /// <summary>
    /// The current default namespace.
    /// </summary>
    public string DefaultNamespace
    {
        get { return LookupNamespace(string.Empty); }
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        foreach (var dict in _stack)
        {
            sb.Append("---\n");

            foreach (var (key, value) in dict)
                sb.Append(string.Format("{0}={1}\n", key, value));
        }
        return sb.ToString();
    }

    public void Clear()
        => _stack.Clear();
}