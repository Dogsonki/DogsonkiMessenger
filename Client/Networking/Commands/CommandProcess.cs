using Client.Utility;
using System.Reflection;

namespace Client.Networking.Commands;

internal static class CommandProcess
{
    private static Dictionary<string, Type> Commands = new Dictionary<string, Type>();

    static CommandProcess() 
    {
        GetReflectionCommands();
    }

    /// <summary>
    /// Returns true if command is successfully prepared and sent
    /// </summary>
    public static bool Invoke(string commandName, string[] args, out string error)
    {
        Commands.TryGetValue(commandName, out Type? command);

        try
        {
            if (command is null)
            {
                error = "Command dose not exists";
                return false;
            }

            if (!ICommand.HasAgrs(command, args.Length))
            {
                error = "Count of provided arguments is wrong";
                return false;
            }

            object? instance = Activator.CreateInstance(command, args);

            if (instance is null)
            {
                error = "Something went wrong when creating instance of command";
                return false;
            }

            if (((ICommand)instance).Sendable)
            {
                return ICommand.PrepareAndSend((ICommand)instance, out error);
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }

        error = string.Empty;
        return false;
    }

    private static void GetReflectionCommands()
    {
        IEnumerable<Type> coms = from asm in Assembly.GetExecutingAssembly().GetTypes() where asm.Namespace == "Client.Networking.Commands" select asm;

        foreach (Type com in coms)
        {
            if (typeof(ICommand).IsAssignableFrom(com))
            {
                CommandAliasAttribute? alias = com.GetCustomAttribute<CommandAliasAttribute>();

                if (alias is null) continue;

                Commands.Add(alias.Alias, com);
            }
        }
    }
}