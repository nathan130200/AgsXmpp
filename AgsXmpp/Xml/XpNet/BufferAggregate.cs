using System.Text;

namespace AgsXmpp.Xml.XpNet;

public class BufferAggregate : IDisposable
{
    class BufNode
    {
        internal byte[] buffer;
        internal BufNode next;
    };

    volatile bool disposed;

    ~BufferAggregate()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        BufNode temp;

        for (temp = _head; temp != null; temp = temp.next)
            temp.buffer = null;

        _stream.Dispose();
        _stream = null;

        GC.SuppressFinalize(this);
    }

    MemoryStream _stream;
    BufNode _head, _tail;

    public BufferAggregate()
    {
        _stream = new MemoryStream();
    }

    public void Write(byte[] buf)
    {
        _stream.Write(buf, 0, buf.Length);
        if (_tail == null)
        {
            _head = _tail = new BufNode();
            _head.buffer = buf;
        }
        else
        {
            var n = new BufNode
            {
                buffer = buf
            };

            _tail.next = n;
            _tail = n;
        }
    }

    public byte[] GetBuffer()
    {
        return _stream.ToArray();
    }

    public void Clear(int offset)
    {
        int s = 0;
        int nbytes = -1;

        BufNode bn = null;

        for (bn = _head; bn != null; bn = bn.next)
        {
            if (s + bn.buffer.Length <= offset)
            {
                if (s + bn.buffer.Length == offset)
                {
                    bn = bn.next;
                    break;
                }
                s += bn.buffer.Length;
            }
            else
            {
                nbytes = s + bn.buffer.Length - offset;
                break;
            }
        }

        _head = bn;
        if (_head == null)
            _tail = null;

        if (nbytes > 0)
        {
            byte[] buf = new byte[nbytes];

            Buffer.BlockCopy(_head.buffer,
                                    _head.buffer.Length - nbytes,
                                    buf, 0, nbytes);
            _head.buffer = buf;
        }

        _stream.SetLength(0);
        for (bn = _head; bn != null; bn = bn.next)
        {
            _stream.Write(bn.buffer, 0, bn.buffer.Length);
        }
    }

    public override string ToString()
    {
        var buf = GetBuffer();
        return Encoding.UTF8.GetString(buf, 0, buf.Length);
    }
}
