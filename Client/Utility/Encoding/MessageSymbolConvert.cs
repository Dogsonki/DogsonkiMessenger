namespace Client.MessageEncoding;

public static class MessageSymbolConvert
{
    private static MessageSymbolModel sgDollar = new MessageSymbolModel("$", @"\sg01");

    public static void EncodeMessage(ref string input)
    {
        foreach (MessageSymbolModel symbol in MessageSymbolModel.Symbols)
        {
            input.Replace(symbol.Encoded, symbol.Converted);
        }
    }

    public static void ConvertMessage(ref string input)
    {
        foreach (MessageSymbolModel symbol in MessageSymbolModel.Symbols)
        {
            input.Replace(symbol.Converted, symbol.Encoded);
        }
    }
}