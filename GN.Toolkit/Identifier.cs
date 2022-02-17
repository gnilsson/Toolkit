namespace GN.Toolkit;

public struct Identifier
{
    public static readonly Identifier Empty = default;

    private readonly Guid _guidValue;
    private readonly string _base64Value;

    public Identifier(Guid guidValue)
    {
        _guidValue = guidValue;
        _base64Value = ToIdentifierString(guidValue);
    }

    public Identifier(string base64Value)
    {
        _guidValue = ToIdentifierGuid(base64Value);
        _base64Value = base64Value;
    }

    public static Identifier New() => new(Guid.NewGuid());

    private static string ToIdentifierString(Guid guid)
    {
        var base64Guid = Convert.ToBase64String(guid.ToByteArray());

        base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

        return base64Guid[..22];
    }

    private static Guid ToIdentifierGuid(string str)
    {
        str = str.Replace('_', '/').Replace('-', '+');

        var byteArray = Convert.FromBase64String(str + "==");

        return new Guid(byteArray);
    }

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string ToString() => _base64Value;

    public static implicit operator Identifier(Guid guidValue) => new(guidValue);
    public static implicit operator Identifier(string base64Value) => new(base64Value);
    public static implicit operator string(Identifier identifier) => identifier._base64Value;

    public static bool operator ==(Identifier id1, Identifier id2) => id1._guidValue.Equals(id2._guidValue);
    public static bool operator !=(Identifier id1, Identifier id2) => !id1._guidValue.Equals(id2._guidValue);
}
