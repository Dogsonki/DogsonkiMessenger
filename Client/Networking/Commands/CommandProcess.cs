using Client.Utility;
using System.Reflection;

namespace Client.Networking.Commands;

internal class CommandProcess
{
    private static Dictionary<string, Type> Commands = new Dictionary<string, Type>();

    public static void Invoke(string commandName, string[] args, out string error)
    {
        if (Commands.Count == 0)
        {
            GetReflectionCommands();
        }

        Commands.TryGetValue(commandName, out Type? command);

        try
        {
            if (command is null)
            {
                error = "Command dose not exists";
                return;
            }

            if (!ICommand.HasAgrs(command, args.Length))
            {
                error = "Count of provided arguments is wrong";
                return;
            }

            object? instance = Activator.CreateInstance(command, args);

            if (instance is null)
            {
                Logger.Push($"Something went wrong when creating instance of command: {command.Name} args: {args.Length}", LogLevel.Error);
                error = "Something went wrong when creating instance of command";
                return;
            }

            if (((ICommand)instance).Sendable)
            {
                ICommand.PrepareAndSend((ICommand)instance, out error);
            }
        }
        catch (Exception ex)
        {
            Logger.Push(ex, LogLevel.Error);
        }

        error = string.Empty;
    }

    public static void GetReflectionCommands()
    {
        IEnumerable<Type> coms = from asm in Assembly.GetExecutingAssembly().GetTypes() where asm.Namespace == "Client.Commands" select asm;

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