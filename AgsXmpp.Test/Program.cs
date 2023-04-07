using AgsXmpp;
using AgsXmpp.Xml;

internal class Program
{
    private static void Main(string[] args)
    {
        var el = new AgsXmpp.Protocol.Base.Stream();
        el.GenerateId();
        el.From = new("foo.bar");
        el.To = new("bar.baz");

        var features = new Element("stream:features");
        features.SetNamespace("stream", Xmlns.STREAM);
        features.AddChild(new Element("starttls", Xmlns.TLS)
            .AddChild(new Element("required")));

        var mech = new Element("mechanisms", Xmlns.SASL);
        mech.AddChild(new Element("mechanism") { Value = "PLAIN" });
        mech.AddChild(new Element("mechanism") { Value = "DIGEST-MD5" });
        features.AddChild(mech);

        el.AddChild(features);

        Console.WriteLine("tag [1]: \n" + el.ToString() + "\n");
        Console.WriteLine("tag [2]: \n" + el.ToString(true) + "\n");
        Console.ReadLine();
    }
}