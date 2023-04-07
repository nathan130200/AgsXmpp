using System.Security.Cryptography;

namespace AgsXmpp;

public abstract class IdGenerator
{
    public static IdGenerator Default { get; } = new DefaultIdGenerator();
    public static IdGenerator Sequential { get; } = new SequentialIdGenerator();
    public static IdGenerator Guid { get; } = new GuidIdGenerator();

    public abstract string NextId();

    /* ---- Implementations ---- */

    class DefaultIdGenerator : IdGenerator
    {
        internal DefaultIdGenerator()
        {

        }

        public override string NextId()
        {
            Span<byte> buf = stackalloc byte[8];
            RandomNumberGenerator.Fill(buf);
            return Convert.ToHexString(buf).ToLowerInvariant();
        }
    }

    class SequentialIdGenerator : IdGenerator
    {
        public string Prefix { get; set; } = "uid";
        public string Format { get; set; } = "X8";

        private volatile uint _value;

        internal SequentialIdGenerator()
        {

        }

        public override string NextId()
        {
            lock (this)
            {
                if (_value == uint.MaxValue - 1)
                    _value = 0;

                return $"{Prefix}{_value++.ToString(Format)}";
            }
        }
    }

    class GuidIdGenerator : IdGenerator
    {
        public string Format { get; set; } = "D";

        internal GuidIdGenerator()
        {

        }

        public override string NextId()
            => System.Guid.NewGuid().ToString(Format);
    }
}
