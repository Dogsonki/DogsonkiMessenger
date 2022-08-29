namespace Client.Networking.Models.BotCommands;

public class CommandProperty : Attribute
{
    public Type PropertyType;

    public CommandProperty(Type type)
    {
        PropertyType = type;
    }
}