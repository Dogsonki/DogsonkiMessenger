namespace Client.Networking.Commands;

[AttributeUsage(AttributeTargets.Property)]
public class CommandProperty : Attribute
{
    public Type PropertyType;

    public CommandProperty(Type type)
    {
        PropertyType = type;
    }
}