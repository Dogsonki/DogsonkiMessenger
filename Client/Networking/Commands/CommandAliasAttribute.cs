namespace Client.Networking.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandAliasAttribute : Attribute
{
    public string Alias { get; set; }

    public CommandAliasAttribute(string alias)
    {
        Alias = alias;
    }
}