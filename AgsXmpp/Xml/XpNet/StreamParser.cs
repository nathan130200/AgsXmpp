using AgsXmpp.Factory;
using AgsXmpp.Xml.XpNet.Exceptions;
using AgsXmpp.Xml.XpNet.Tokenizer;

namespace AgsXmpp.Xml.XpNet;

public class StreamParser
{
    public event EventHandler<Exception> OnError;
    public event EventHandler<Element> OnStreamStart;
    public event EventHandler OnStreamEnd;
    public event EventHandler<Element> OnStreamElement;

    private long m_Depth = 0;
    private Element m_root = default;
    private Element current = default;

    static readonly System.Text.UTF8Encoding utf = new();
    readonly Encoding m_enc = new UTF8Encoding();
    readonly NS m_ns = new();

    private BufferAggregate m_buf = new();
    private bool m_cdata = false;


    public StreamParser()
    {

    }

    public void Reset()
    {
        m_Depth = 0;
        m_root = null;
        current = null;
        m_cdata = false;

        m_buf?.Dispose();
        m_buf = new BufferAggregate();

        m_ns.Clear();
    }

    public long Depth
    {
        get { return m_Depth; }
    }

    /// <summary>
    /// Put bytes into the parser.
    /// </summary>
    /// <param name="buf">The bytes to put into the parse stream</param>
    /// <param name="offset">Offset into buf to start at</param>
    /// <param name="length">Number of bytes to write</param>
    public void Push(byte[] buf, int offset, int length)
    {

        // or assert, really, but this is a little nicer.
        if (length == 0)
            return;

        // No locking is required.  Read() won't get called again
        // until this method returns.

        // TODO: only do this copy if we have a partial token at the
        // end of parsing.
        byte[] copy = new byte[length];
        Buffer.BlockCopy(buf, offset, copy, 0, length);
        m_buf.Write(copy);

        byte[] b = m_buf.GetBuffer();
        int off = 0;
        TOK tok = TOK.END_TAG;
        ContentToken ct = new();
        try
        {
            while (off < b.Length)
            {
                if (m_cdata)
                    tok = m_enc.tokenizeCdataSection(b, off, b.Length, ct);
                else
                    tok = m_enc.tokenizeContent(b, off, b.Length, ct);

                switch (tok)
                {
                    case TOK.EMPTY_ELEMENT_NO_ATTS:
                    case TOK.EMPTY_ELEMENT_WITH_ATTS:
                        StartTag(b, off, ct, tok);
                        EndTag(b, off, ct, tok);
                        break;
                    case TOK.START_TAG_NO_ATTS:
                    case TOK.START_TAG_WITH_ATTS:
                        StartTag(b, off, ct, tok);
                        break;
                    case TOK.END_TAG:
                        EndTag(b, off, ct, tok);
                        break;
                    case TOK.DATA_CHARS:
                    case TOK.DATA_NEWLINE:
                        AddText(utf.GetString(b, off, ct.TokenEnd - off));
                        break;
                    case TOK.CHAR_REF:
                    case TOK.MAGIC_ENTITY_REF:
                        AddText(new string(new char[] { ct.RefChar1 }));
                        break;
                    case TOK.CHAR_PAIR_REF:
                        AddText(new string(new char[] {ct.RefChar1,
                                                            ct.RefChar2}));
                        break;
                    /*case TOK.COMMENT:
                        if (current != null)
                        {
                            // <!-- 4
                            //  --> 3
                            int start = off + 4 * m_enc.MinBytesPerChar;
                            int end = ct.TokenEnd - off -
                                7 * m_enc.MinBytesPerChar;
                            string text = utf.GetString(b, start, end);

                            // 
                            // 6 April 2023 ~ NOFIX: Comments are not allowed in XMPP. We can just opt to just ignore.
                            // In strict xmpp client/server WILL fire <restricted-xml/>
                            //   description: the entity has attempted to send restricted XML features such as a comment, processing instruction, DTD, entity reference, or unescaped character
                            // my guess CDATA is allowed maybe 🤔
                            // In other words TOK.DATA_CHARS and TOK.DATA_NEWLINE will get fired instead when processing CDATA
                            // So i assume its safe 
                            //
                            // REMOVED: current.AddChild(new Comment(text));
                            // 
                            // Based on XMPP RFC-3920 (a.k.a XMPP-CORE)
                            // Are <b>NOT</b> allowed the following types:
                            // - comments
                            // - processing instruction (such as <?xml version... ?>)
                            // - internal/external DTD declarations/subsets
                            // - internal/external entity reference.
                            // - character data or attribute values containing unescaped chars.
                            //
                            // also XMPP consider if we  receives such restricted XML data, we MUST ignore the data.
                            // instead of raising errors and breaking communication.
                            // its fine for now 😁
                        }
                        break;*/
                    case TOK.CDATA_SECT_OPEN:
                        m_cdata = true;
                        break;
                    case TOK.CDATA_SECT_CLOSE:
                        m_cdata = false;
                        break;
                    case TOK.XML_DECL:
                        // thou shalt use UTF8, and XML version 1.
                        // i shall ignore evidence to the contrary...
                        // TODO: Throw an exception if these assuptions are wrong
                        break;
                    case TOK.ENTITY_REF:
                    case TOK.PI:
                    default:
                        throw new NotImplementedException("Token type not implemented: " + tok);
                }
                off = ct.TokenEnd;
            }
        }
        catch (PartialTokenException)
        {
            // ignored;
        }
        catch (ExtensibleTokenException)
        {
            // ignored;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
        }
        finally
        {
            m_buf.Clear(off);
        }
    }

    private void StartTag(byte[] buf, int offset,
            ContentToken ct, TOK tok)
    {
        m_Depth++;
        int colon;
        string name;
        string prefix;

        var dict = new Dictionary<string, string>();

        m_ns.PushScope();

        // if i have attributes
        if ((tok == TOK.START_TAG_WITH_ATTS) ||
            (tok == TOK.EMPTY_ELEMENT_WITH_ATTS))
        {
            int start;
            int end;
            string val;

            for (int i = 0; i < ct.getAttributeSpecifiedCount(); i++)
            {
                start = ct.getAttributeNameStart(i);
                end = ct.getAttributeNameEnd(i);
                name = utf.GetString(buf, start, end - start);

                start = ct.getAttributeValueStart(i);
                end = ct.getAttributeValueEnd(i);
                val = utf.GetString(buf, start, end - start);

                val = NormalizeAttributeValue(buf, start, end - start);

                if (name.StartsWith("xmlns:"))
                {
                    colon = name.IndexOf(':');
                    prefix = name.Substring(colon + 1);
                    m_ns.AddNamespace(prefix, val);
                }
                else if (name == "xmlns")
                    m_ns.AddNamespace(string.Empty, val);
                else
                    dict[name] = val;
            }
        }

        name = utf.GetString(buf,
            offset + m_enc.MinBytesPerChar,
            ct.NameEnd - offset - m_enc.MinBytesPerChar);

        colon = name.IndexOf(':');
        prefix = null;

        string ns;

        if (colon > 0)
        {
            prefix = name.Substring(0, colon);
            name = name.Substring(colon + 1);
            ns = m_ns.LookupNamespace(prefix);
        }
        else
        {
            ns = m_ns.DefaultNamespace;
        }

        Element newel = ElementFactory.GetElement(prefix, name, ns);

        foreach (var (key, value) in dict)
            newel.SetAttribute(key, value);

        if (m_root == null)
        {
            m_root = newel;
            OnStreamStart?.Invoke(this, m_root);
        }
        else
        {
            current?.AddChild(newel);
            current = newel;
        }
    }

    private void EndTag(byte[] buf, int offset, ContentToken ct, TOK tok)
    {
        m_Depth--;
        m_ns.PopScope();

        if (current == null)
        {
            OnStreamEnd?.Invoke(this, EventArgs.Empty);
            return;
        }

        string name = null;

        if ((tok == TOK.EMPTY_ELEMENT_WITH_ATTS) ||
            (tok == TOK.EMPTY_ELEMENT_NO_ATTS))
            name = utf.GetString(buf,
                offset + m_enc.MinBytesPerChar,
                ct.NameEnd - offset -
                m_enc.MinBytesPerChar);
        else
            name = utf.GetString(buf,
                offset + m_enc.MinBytesPerChar * 2,
                ct.NameEnd - offset -
                m_enc.MinBytesPerChar * 2);

        var parent = current.Parent;

        if (parent is null)
            DoRaiseOnStreamElement(current);

        current = parent;
    }

    void DoRaiseOnStreamElement(Element el)
    {
        try
        {
            OnStreamElement?.Invoke(this, el);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
        }
    }

    private string NormalizeAttributeValue(byte[] buf, int offset, int length)
    {
        if (length == 0)
            return null;

        string val = null;
        BufferAggregate buffer = new();
        byte[] copy = new byte[length];
        Buffer.BlockCopy(buf, offset, copy, 0, length);
        buffer.Write(copy);
        byte[] b = buffer.GetBuffer();
        int off = 0;
        TOK tok = TOK.END_TAG;
        ContentToken ct = new();
        try
        {
            while (off < b.Length)
            {
                tok = m_enc.tokenizeAttributeValue(b, off, b.Length, ct);

                switch (tok)
                {
                    case TOK.ATTRIBUTE_VALUE_S:
                    case TOK.DATA_CHARS:
                    case TOK.DATA_NEWLINE:
                        val += (utf.GetString(b, off, ct.TokenEnd - off));
                        break;
                    case TOK.CHAR_REF:
                    case TOK.MAGIC_ENTITY_REF:
                        val += new string(new char[] { ct.RefChar1 });
                        break;
                    case TOK.CHAR_PAIR_REF:
                        val += new string(new char[] { ct.RefChar1, ct.RefChar2 });
                        break;
                    case TOK.ENTITY_REF:
                        throw new NotImplementedException("Token type not implemented: " + tok);
                }

                off = ct.TokenEnd;
            }
        }
        catch (PartialTokenException)
        {
            // ignored;
        }
        catch (ExtensibleTokenException)
        {
            // ignored;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
        }
        finally
        {
            buffer.Clear(off);
        }

        return val;
    }

    private void AddText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (current != null)
        {
            //XmlNode last = current.LastChild;

            //if (last != null && last.NodeType == XmlNodeType.Text)
            //    last.Value = last.Value + text;
            //else
            //    current.Value = text;

            if (string.IsNullOrEmpty(current.Value))
                current.Value = text;
            else
                current.Value += text;
        }
    }
}
