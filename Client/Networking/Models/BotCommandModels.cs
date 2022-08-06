using Client.Networking.Core;
using Newtonsoft.Json;
using System.Reflection;

namespace Client.Networking.Bot.Models;

public interface IBotCommand
{
    public string Command { get; set; }

    /// <summary>
    /// Checks if parameters from user has the same count as command properties
    /// </summary>
    public static bool HasAgrs(Type command,int ProvidedArgs) => command.GetProperties().Length == ProvidedArgs;

    //For now it doesn't do much but will be useful when command will need Classes
    /// <summary>
    /// Checks if values of properties type have assigned types
    /// </summary>
    /// <param name="command">Runtime command class</param>
    /// <param name="error">Error string</param>
    public static bool CheckProperties(object command, out string error)
    {
        foreach(var prop in command.GetType().GetProperties())
        {
            object? PropValue = prop.GetValue(command);
            if(PropValue is null)
            {
                error = $"{prop.Name} has no value";
                return false;
            }

            Type PropType = PropValue.GetType();
            CommandProperty? AssignedType = prop.GetCustomAttribute<CommandProperty>();

            if(PropType is not null && AssignedType is not null)
            {
                if(PropType != AssignedType.PropertyType)
                {
                    if(AssignedType.PropertyType == typeof(int))
                    {
                        int _;
                        if(!int.TryParse((string)PropValue,out _))
                        {
                            error = $"{prop.Name}:{PropType} value: {PropValue} is not a {AssignedType.PropertyType.ToString()}";
                            return false;
                        }
                    }
                }
            }
        }
        error = String.Empty;
        return true;
    }

    public static bool PrepareAndSend(IBotCommand command, out string error) 
    { 
        if(CheckProperties(command, out error))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class CommandProperty : Attribute
{
    public Type PropertyType;

    public CommandProperty(Type type)
    {
        PropertyType = type;
    }
}

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

    public Bet(string command,object betMoney,object winPercent)
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