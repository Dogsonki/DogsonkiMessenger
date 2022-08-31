using Newtonsoft.Json;

namespace Client.Commands;

[Serializable]
public class Daily : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Daily(string command)
    {
        Command = command;
    }

    public bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Daily), ProvidedArgs);
}

[Serializable]
public class Bet : ICommand
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

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Bet), ProvidedArgs);
}

public class JackpotBuy : ICommand
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

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(JackpotBuy), ProvidedArgs);
}

public class Scratchcard : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Scratchcard(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Scratchcard), ProvidedArgs);
}

public class Shop : ICommand
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

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Shop), ProvidedArgs);
}

public class Slots : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    public Slots(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Slots), ProvidedArgs);
}