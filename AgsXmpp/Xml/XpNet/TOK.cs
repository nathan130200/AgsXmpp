namespace AgsXmpp.Xml.XpNet;

/// <summary>
/// Tokens that might have been found
/// </summary>
public enum TOK
{
    /**
     * Represents one or more characters of data.
     */
    DATA_CHARS,

    /**
     * Represents a newline (CR, LF or CR followed by LF) in data.
     */
    DATA_NEWLINE,

    /**
     * Represents a complete start-tag <code>&lt;name&gt;</code>,
     * that doesn't have any attribute specifications.
     */
    START_TAG_NO_ATTS,

    /**
     * Represents a complete start-tag <code>&lt;name
     * att="val"&gt;</code>, that contains one or more
     * attribute specifications.
     */
    START_TAG_WITH_ATTS,

    /**
     * Represents an empty element tag <code>&lt;name/&gt;</code>,
     * that doesn't have any attribute specifications.
     */
    EMPTY_ELEMENT_NO_ATTS,

    /**
     * Represents an empty element tag <code>&lt;name
     * att="val"/&gt;</code>, that contains one or more
     * attribute specifications.
     */
    EMPTY_ELEMENT_WITH_ATTS,

    /**
     * Represents a complete end-tag <code>&lt;/name&gt;</code>.
     */
    END_TAG,

    /**
     * Represents the start of a CDATA section
     * <code>&lt;![CDATA[</code>.
     */
    CDATA_SECT_OPEN,

    /**
     * Represents the end of a CDATA section <code>]]&gt;</code>.
     */
    CDATA_SECT_CLOSE,

    /**
     * Represents a general entity reference.
     */
    ENTITY_REF,

    /**
     * Represents a general entity reference to a one of the 5
     * predefined entities <code>amp</code>, <code>lt</code>,
     * <code>gt</code>, <code>quot</code>, <code>apos</code>.
     */
    MAGIC_ENTITY_REF,

    /**
     * Represents a numeric character reference (decimal or
     * hexadecimal), when the referenced character is less
     * than or equal to 0xFFFF and so is represented by a
     * single char.
     */
    CHAR_REF,

    /**
     * Represents a numeric character reference (decimal or
     * hexadecimal), when the referenced character is greater
     * than 0xFFFF and so is represented by a pair of chars.
     */
    CHAR_PAIR_REF,

    /**
     * Represents a processing instruction.
     */
    PI,

    /**
     * Represents an XML declaration or text declaration (a
     * processing instruction whose target is
     * <code>xml</code>).
     */
    XML_DECL,

    /**
     * Represents a comment <code>&lt;!-- comment --&gt;</code>.
     * This can occur both in the prolog and in content.
     */
    COMMENT,

    /**
     * Represents a white space character in an attribute
     * value, excluding white space characters that are part
     * of line boundaries.
     */
    ATTRIBUTE_VALUE_S,

    /**
     * Represents a parameter entity reference in the prolog.
     */
    PARAM_ENTITY_REF,

    /**
     * Represents whitespace in the prolog.
     * The token contains one or more whitespace characters.
     */
    PROLOG_S,

    /**
     * Represents <code>&lt;!NAME</code> in the prolog.
     */
    DECL_OPEN,

    /**
     * Represents <code>&gt;</code> in the prolog.
     */
    DECL_CLOSE,

    /**
     * Represents a name in the prolog.
     */
    NAME,

    /**
     * Represents a name token in the prolog that is not a name.
     */
    NMTOKEN,

    /**
     * Represents <code>#NAME</code> in the prolog.
     */
    POUND_NAME,

    /**
     * Represents <code>|</code> in the prolog.
     */
    OR,

    /**
     * Represents a <code>%</code> in the prolog that does not start
     * a parameter entity reference.
     * This can occur in an entity declaration.
     */
    PERCENT,

    /**
     * Represents a <code>(</code> in the prolog.
     */
    OPEN_PAREN,

    /**
     * Represents a <code>)</code> in the prolog that is not
     * followed immediately by any of
     *  <code>*</code>, <code>+</code> or <code>?</code>.
     */
    CLOSE_PAREN,

    /**
     * Represents <code>[</code> in the prolog.
     */
    OPEN_BRACKET,

    /**
     * Represents <code>]</code> in the prolog.
     */
    CLOSE_BRACKET,

    /**
     * Represents a literal (EntityValue, AttValue, SystemLiteral or
     * PubidLiteral).
     */
    LITERAL,

    /**
     * Represents a name followed immediately by <code>?</code>.
     */
    NAME_QUESTION,

    /**
     * Represents a name followed immediately by <code>*</code>.
     */
    NAME_ASTERISK,

    /**
     * Represents a name followed immediately by <code>+</code>.
     */
    NAME_PLUS,

    /**
     * Represents <code>&lt;![</code> in the prolog.
     */
    COND_SECT_OPEN,

    /**
     * Represents <code>]]&gt;</code> in the prolog.
     */
    COND_SECT_CLOSE,

    /**
     * Represents <code>)?</code> in the prolog.
     */
    CLOSE_PAREN_QUESTION,

    /**
     * Represents <code>)*</code> in the prolog.
     */
    CLOSE_PAREN_ASTERISK,

    /**
     * Represents <code>)+</code> in the prolog.
     */
    CLOSE_PAREN_PLUS,

    /**
     * Represents <code>,</code> in the prolog.
     */
    COMMA,
};