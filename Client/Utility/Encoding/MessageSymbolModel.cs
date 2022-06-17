namespace Client.MessageEncoding;

public class MessageSymbolModel
{
    public static List<MessageSymbolModel> Symbols = new List<MessageSymbolModel>();

    public string Converted { get; }
    public string Encoded { get; }

    public MessageSymbolModel(string converted, string encoded)
    {
        Converted = converted;
        Encoded = encoded;
        Symbols.Add(this);
    }

}