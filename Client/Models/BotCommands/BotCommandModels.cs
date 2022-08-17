using Newtonsoft.Json;

namespace Client.Networking.Models.BotCommands;

[Serializable]
public class Daily : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Daily(string command)
    {
        Command = command;
    }

    public bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(Daily), ProvidedArgs);
}

[Serializable]
public class Bet : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonProperty("bet_money")]
    [CommandProperty(typeof(int))]
    public object BetMoney { get; set; }

    [JsonProperty("percent_to_win")]
    [CommandProperty(typeof(int))]
    public object WinPercent { get; set; }

    public Bet(string command, object betMoney, object winPercent)
    {
        Command = command;
        BetMoney = betMoney;
        WinPercent = winPercent;
    }

    public static bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(Bet), ProvidedArgs);
}

public class JackpotBuy : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonProperty("tickets")]
    [CommandProperty(typeof(int))]
    public object Count { get; set; }

    public JackpotBuy(string command, object count)
    {
        Command = command;
        Count = count;
    }

    public static bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(JackpotBuy), ProvidedArgs);
}

public class Scratchcard : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Scratchcard(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(Scratchcard), ProvidedArgs);
}

public class Shop : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonProperty("item_id")]
    [CommandProperty(typeof(int))]
    public object ItemId { get; set; }

    public Shop(string command, object itemId)
    {
        Command = command;
        ItemId = itemId;
    }

    public static bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(Shop), ProvidedArgs);
}

public class Slots : IBotCommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Slots(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => IBotCommand.HasAgrs(typeof(Slots), ProvidedArgs);
}