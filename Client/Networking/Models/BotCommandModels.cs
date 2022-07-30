using Newtonsoft.Json;

namespace Client.Networking.Bot.Models;

public interface IBotCommand
{
    public string Command { get; set; }
    /// <summary>
    /// Count of serializable proporties
    /// </summary>
    public static int ArgsCount { get; set; }
}

[Serializable]
public class Daily : IBotCommand
{
    [JsonProperty("command")]
    public string Command { get; set; }

    public Daily(string command)
    {
       Command = command;
    }
}

[Serializable]
public class Bet : IBotCommand
{
    [JsonIgnore]
    public static int ArgsCount { get; set; } = 3;

    [JsonProperty("command")]
    public string Command { get; set; }
    [JsonProperty("bet_money")]
    public object BetMoney { get; set; } //int
    [JsonProperty("percent_to_win")]
    public object WinPercent { get; set; } //int

    public Bet(string command,object betMoney,object winPercent)
    {
        Command = command;
        BetMoney = betMoney;
        WinPercent = winPercent;
    }
}

public class JackpotBuy : IBotCommand
{
    [JsonIgnore]
    public static int ArgsCount { get; set; } = 2;

    [JsonProperty("command")]
    public string Command { get; set; }
    [JsonProperty("tickets")]
    public object Count { get; set; } //int

    public JackpotBuy(string command, object count)
    {
        Command = command;
        Count = count;
    }
}

public class Scratchcard : IBotCommand
{
    [JsonProperty("command")]
    public string Command { get; set; }
    
    public Scratchcard(string command)
    {
        Command = command;
    }
}

public class Shop : IBotCommand
{
    [JsonIgnore]
    public static int ArgsCount { get; set; } = 2;

    [JsonProperty("command")]
    public string Command { get; set; }
    [JsonProperty("item_id")]
    public object ItemId { get; set; }

    public Shop(string command, object itemId)
    {
        Command = command;
        ItemId = itemId;
    }
}

public class Slots : IBotCommand
{
    [JsonProperty("command")]
    public string Command { get; set; }

    public Slots(string command)
    {
        Command = command;
    }
}