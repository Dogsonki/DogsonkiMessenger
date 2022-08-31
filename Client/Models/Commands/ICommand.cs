using System.Reflection;
using Client.Networking.Core;

namespace Client.Commands;

public interface ICommand
{
    public string Command { get; set; }

    /// <summary>
    /// Checks if parameters from user has the same count as command properties
    /// </summary>
    public static bool HasAgrs(Type command, int ProvidedArgs) => command.GetProperties().Length == ProvidedArgs;

    //For now it doesn't do much but will be useful when command will need Classes
    /// <summary>
    /// Checks if values of properties type have assigned types
    /// </summary>
    /// <param name="command">Runtime command class</param>
    /// <param name="error">Error string</param>
    public static bool CheckProperties(object command, out string error)
    {
        foreach (var prop in command.GetType().GetProperties())
        {
            object? PropValue = prop.GetValue(command);
            if (PropValue is null)
            {
                error = $"{prop.Name} has no value";
                return false;
            }

            Type PropType = PropValue.GetType();
            CommandProperty? AssignedType = prop.GetCustomAttribute<CommandProperty>();

            if (PropType is not null && AssignedType is not null)
            {
                if (PropType != AssignedType.PropertyType)
                {
                    if (AssignedType.PropertyType == typeof(int))
                    {
                        int _;
                        if (!int.TryParse((string)PropValue, out _))
                        {
                            error = $"{prop.Name}:{PropType} value: {PropValue} is not a {AssignedType.PropertyType.Name}";
                            return false;
                        }
                    }
                }
            }
        }
        error = string.Empty;
        return true;
    }

    public static bool PrepareAndSend(ICommand command, out string error)
    {
        if (CheckProperties(command, out error))
        {
            return SocketCore.SendCommand(command);
        }
        else
        {
            return false;
        }
    }
}