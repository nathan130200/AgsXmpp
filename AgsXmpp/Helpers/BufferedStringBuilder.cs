using System.Buffers;
using System.Text;

namespace AgsXmpp.Helpers;

public class BufferedStringBuilder : IDisposable
{
    private Encoding _encoding;
    private ArrayBufferWriter<byte> _writer;

    public BufferedStringBuilder(Encoding encoding = default)
        => Init(encoding);

    void Init(Encoding enc)
    {
        _encoding = enc ?? Encoding.UTF8;
        _writer ??= new ArrayBufferWriter<byte>(256);
    }

    ~BufferedStringBuilder()
    {
        Reset();
    }

    void Reset()
    {
        _writer?.Clear();
        _writer = default;
    }

    public void Dispose()
    {
        Reset();
        GC.SuppressFinalize(this);
    }

    public BufferedStringBuilder AppendLine()
        => Append('\n');

    public BufferedStringBuilder AppendLine(string str)
        => Append('\n').Append(str);

    public BufferedStringBuilder Append(char ch)
        => AppendChars(ch);

    public BufferedStringBuilder Append(string str)
        => AppendChars(str);

    public BufferedStringBuilder Append(StringBuilder sb)
        => AppendChars(sb.ToString());

    BufferedStringBuilder AppendChars(params char[] chars)
    {
        _encoding.GetBytes(chars, _writer);
        return this;
    }

    BufferedStringBuilder AppendChars(ReadOnlySpan<char> chars)
    {
        _encoding.GetBytes(chars, _writer);
        return this;
    }

    public override string ToString()
        => _encoding.GetString(_writer.WrittenSpan);

    public override int GetHashCode()
        => _writer.GetHashCode();
}