namespace AgsXmpp.Xml.XpNet.Exceptions;

/// <summary>
/// Base class for other exceptions
/// </summary>
public class TokenException : Exception
{

}

/// <summary>
/// An empty token was detected.  This only happens with a buffer of length 0 is passed in
/// to the parser.
/// </summary>
public class EmptyTokenException : TokenException
{

}

/// <summary>
/// End of prolog.
/// </summary>
public class EndOfPrologException : TokenException
{

}

/// <summary>
/// Thrown to indicate that the byte subarray being tokenized is a legal XML token, but that subsequent bytes in the same entity could be part of the token.
/// <para>
/// For example <see cref="Encoding.tokenizeProlog"/> would throw this if the byte subarray consists of a legal XML name.
/// </para>
/// </summary>
public class ExtensibleTokenException : TokenException
{
    private TOK _token;

    public ExtensibleTokenException(TOK token)
    {
        _token = token;
    }

    /// <summary>
    /// Returns the type of token in the byte subarrary.
    /// </summary>
    public TOK TokenType
    {
        get { return _token; }
    }
}

/// <summary>
/// Type of token in the byte subarrary.
/// </summary>
public enum InvalidTokenType
{
    /// <summary>
    /// An illegal character
    /// </summary>
    IllegalChar,

    /// <summary>
    /// Doc prefix wasn't XML
    /// </summary>
    XmlTarget,

    /// <summary>
    /// More than one attribute with the same name on the same element.
    /// </summary>
    DuplicateAttribute
}

/// <summary>
/// Several kinds of token problems.
/// </summary>
public class InvalidTokenException : TokenException
{
    private readonly int _offset;
    private readonly InvalidTokenType _type;

    /// <summary>
    /// Some other type of bad token detected
    /// </summary>
    public InvalidTokenException(int offset, InvalidTokenType type)
    {
        _offset = offset;
        _type = type;
    }

    /// <summary>
    /// Illegal character detected
    /// </summary>
    /// <param name="offset"></param>
    public InvalidTokenException(int offset)
    {
        _offset = offset;
        _type = InvalidTokenType.IllegalChar;
    }

    /// <summary>
    /// Offset into the buffer where the problem ocurred.
    /// </summary>
    public int Offset
    {
        get { return _offset; }
    }

    /// <summary>
    /// Type of exception
    /// </summary>
    public InvalidTokenType Type
    {
        get { return _type; }
    }
}

/// <summary>
/// Thrown to indicate that the subarray being tokenized is not the 
/// complete encoding of one or more characters, but might be if 
/// more bytes were added.
/// </summary>
public class PartialCharException : PartialTokenException
{
    private int _leadByteIndex;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="leadByteIndex"></param>
    public PartialCharException(int leadByteIndex)
    {
        _leadByteIndex = leadByteIndex;
    }

    /**
     * Returns the index of the first byte that is not part of the complete
     * encoding of a character.
     */
    public int LeadByteIndex
    {
        get { return _leadByteIndex; }
    }
}

/// <summary>
/// A partial token was received.  Try again, after you add more bytes to the buffer.
/// </summary>
public class PartialTokenException : TokenException
{

}