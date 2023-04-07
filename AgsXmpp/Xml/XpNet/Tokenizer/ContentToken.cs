using AgsXmpp.Xml.XpNet.Exceptions;

namespace AgsXmpp.Xml.XpNet.Tokenizer;

public class ContentToken : Token
{
    private const int InitialAttributeCount = 8;
    private int attCount = 0;
    private int[] attNameStart = new int[InitialAttributeCount];
    private int[] attNameEnd = new int[InitialAttributeCount];
    private int[] attValueStart = new int[InitialAttributeCount];
    private int[] attValueEnd = new int[InitialAttributeCount];
    private bool[] attNormalized = new bool[InitialAttributeCount];

    /// <summary>
    /// Returns the number of attributes specified in the start-tag or empty element tag.
    /// </summary>
    public int getAttributeSpecifiedCount()
    {
        return attCount;
    }

    /// <summary>
    /// Returns the index of the first character of the name of the
    /// attribute index <paramref name="i"/>
    /// </summary>
    public int getAttributeNameStart(int i)
    {
        if (i >= attCount)
            throw new IndexOutOfRangeException();

        return attNameStart[i];
    }

    /// <summary>
    /// Returns the index following the last character of the name of the attribute index <paramref name="i"/>
    /// </summary>
    public int getAttributeNameEnd(int i)
    {
        if (i >= attCount)
            throw new IndexOutOfRangeException();
        return attNameEnd[i];
    }

    /// <summary>
    /// Returns the index of the character following the opening quote of 
    /// attribute index <paramref name="i"/>
    /// </summary>
    public int getAttributeValueStart(int i)
    {
        if (i >= attCount)
            throw new IndexOutOfRangeException();
        return attValueStart[i];
    }

    /// <summary>
    /// Returns the index of the closing quote attribute index <paramref name="i"/>
    /// </summary>
    public int getAttributeValueEnd(int i)
    {
        if (i >= attCount)
            throw new IndexOutOfRangeException();
        return attValueEnd[i];
    }

    ///<summary>
    /// Returns true if attribute index <paramref name="i"/> does not need to
	/// be normalized. This is an optimization that allows further processing
	/// of the attribute to be avoided when it is known that normalization
	/// cannot change the value of the attribute.
    /// </summary>
    public bool isAttributeNormalized(int i)
    {
        if (i >= attCount)
            throw new IndexOutOfRangeException();
        return attNormalized[i];
    }

    /// <summary>
    /// Clear out all of the current attributes
    /// </summary>
    public void clearAttributes()
    {
        attCount = 0;
        attNameStart = new int[InitialAttributeCount];
        attNameEnd = new int[InitialAttributeCount];
        attValueStart = new int[InitialAttributeCount];
        attValueEnd = new int[InitialAttributeCount];
        attNormalized = new bool[InitialAttributeCount];
    }

    /// <summary>
    /// Add a new attribute
    /// </summary>
    public void appendAttribute(int nameStart, int nameEnd,
        int valueStart, int valueEnd,
        bool normalized)
    {
        if (attCount == attNameStart.Length)
        {
            attNameStart = grow(attNameStart);
            attNameEnd = grow(attNameEnd);
            attValueStart = grow(attValueStart);
            attValueEnd = grow(attValueEnd);
            attNormalized = grow(attNormalized);
        }
        attNameStart[attCount] = nameStart;
        attNameEnd[attCount] = nameEnd;
        attValueStart[attCount] = valueStart;
        attValueEnd[attCount] = valueEnd;
        attNormalized[attCount] = normalized;
        ++attCount;
    }

    // <summary>
    /// Is the current attribute unique?
    /// </summary>
    public void checkAttributeUniqueness(byte[] buf)
    {
        for (int i = 1; i < attCount; i++)
        {
            int len = attNameEnd[i] - attNameStart[i];
            for (int j = 0; j < i; j++)
            {
                if (attNameEnd[j] - attNameStart[j] == len)
                {
                    int n = len;
                    int s1 = attNameStart[i];
                    int s2 = attNameStart[j];
                    do
                    {
                        if (--n < 0)
                            throw new InvalidTokenException(attNameStart[i],
                                InvalidTokenType.DuplicateAttribute);
                    } while (buf[s1++] == buf[s2++]);
                }
            }
        }
    }

    private static int[] grow(int[] v)
    {
        int[] tem = v;
        v = new int[tem.Length << 1];
        Array.Copy(tem, 0, v, 0, tem.Length);
        return v;
    }

    private static bool[] grow(bool[] v)
    {
        bool[] tem = v;
        v = new bool[tem.Length << 1];
        Array.Copy(tem, 0, v, 0, tem.Length);
        return v;
    }
}
