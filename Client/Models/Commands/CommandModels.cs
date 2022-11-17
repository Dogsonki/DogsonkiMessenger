using Client.Models.Commands;
using Newtonsoft.Json;

namespace Client.Commands;

[Serializable]
[CommandAlias("!mem")]
public class Mem : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonIgnore] 
    public bool Sendable { get; set; } = true;

    public Mem(string command)
    {
        Command = command;
    }
    public bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Mem), ProvidedArgs);
}

[Serializable]
[CommandAlias("!daily")]
public class Daily : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonIgnore]
    public bool Sendable { get; set; } = true;

    public Daily(string command)
    {
        Command = command;
    }

    public bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Daily), ProvidedArgs);
}

[Serializable]
[CommandAlias("!bet")]
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

    [JsonIgnore]
    public bool Sendable { get; set; } = true;

    public Bet(string command, object betMoney, object winPercent)
    {
        Command = command;
        BetMoney = betMoney;
        WinPercent = winPercent;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Bet), ProvidedArgs);
}

[Serializable]
[CommandAlias("!jackpotbuy")]
public class JackpotBuy : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonProperty("tickets")]
    [CommandProperty(typeof(int))]
    public object Count { get; set; }

    [JsonIgnore] 
    public bool Sendable { get; set; } = true;

    public JackpotBuy(string command, object count)
    {
        Command = command;
        Count = count;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(JackpotBuy), ProvidedArgs);
}

[Serializable]
[CommandAlias("!zdrapka")]
public class Scratchcard : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonIgnore]
    public bool Sendable { get; set; } = true;

    public Scratchcard(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Scratchcard), ProvidedArgs);
}

[Serializable]
[CommandAlias("!sklep")]
public class Shop : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonProperty("item_id")]
    [CommandProperty(typeof(int))]
    public object ItemId { get; set; }

    [JsonIgnore]
    public bool Sendable { get; set; } = true;

    public Shop(string command, object itemId)
    {
        Command = command;
        ItemId = itemId;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Shop), ProvidedArgs);
}

[Serializable]
[CommandAlias("!slots")]
public class Slots : ICommand
{
    [JsonProperty("command")]
    [CommandProperty(typeof(string))]
    public string Command { get; set; }

    [JsonIgnore]
    public bool Sendable { get; set; } = true;

    public Slots(string command)
    {
        Command = command;
    }

    public static bool HasArgs(int ProvidedArgs) => ICommand.HasAgrs(typeof(Slots), ProvidedArgs);
}