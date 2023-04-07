using System.Diagnostics;

namespace AgsXmpp.Xml.XpNet.Tokenizer;

[DebuggerDisplay("Line={LineNumber}; Column={ColumnNumber}")]
public class Position : ICloneable
{
    private int _lineNumber;
    private int _columnNumber;

    public Position()
    {
        _lineNumber = 1;
        _columnNumber = 0;
    }

    /// <summary>
    /// Returns the line number.
    /// The first line number is 1.
    /// </summary>
    public int LineNumber
    {
        get { return _lineNumber; }
        set { _lineNumber = value; }
    }

    /// <summary>
    /// Returns the column number.
    /// The first column number is 0.
    /// A tab character is not treated specially.
    /// </summary>
    public int ColumnNumber
    {
        get { return _columnNumber; }
        set { _columnNumber = value; }
    }

    public Position Clone() => new()
    {
        ColumnNumber = _columnNumber,
        LineNumber = _lineNumber
    };

    object ICloneable.Clone() => Clone();
}

/// <summary>
/// A token that was parsed.
/// </summary>
[DebuggerDisplay("TokenEnd={TokenEnd}; NameEnd={NameEnd}")]
public class Token
{
    private int tokenEnd = -1;
    private int nameEnd = -1;
    private char refChar1 = (char)0;
    private char refChar2 = (char)0;

    /// <summary>
    /// The end of the current token, in relation to the beginning of the buffer.
    /// </summary>
    public int TokenEnd
    {
        get { return tokenEnd; }
        set { tokenEnd = value; }
    }

    /// <summary>
    /// The end of the current token's name, in relation to the beginning of the buffer.
    /// </summary>
    public int NameEnd
    {
        get { return nameEnd; }
        set { nameEnd = value; }
    }

    public char RefChar
    {
        get { return refChar1; }
    }

    /// <summary>
    /// The parsed-out character. &amp; for &amp;amp;
    /// </summary>
    public char RefChar1
    {
        get { return refChar1; }
        set { refChar1 = value; }
    }
    /// <summary>
    /// The second of two parsed-out characters.  TODO: find example.
    /// </summary>
    public char RefChar2
    {
        get { return refChar2; }
        set { refChar2 = value; }
    }

    public void getRefCharPair(char[] ch, int off)
    {
        ch[off] = refChar1;
        ch[off + 1] = refChar2;
    }
}