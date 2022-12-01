using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Client.Networking.Commands;

public struct CommandModel
{
    public string CommandName { get; set; }
    public string CommandDescription { get; set; }

    public CommandModel(string commandName, string commandDescription)
    {
        CommandName = commandName;
        CommandDescription = commandDescription;
    }
}

public static class CommandList
{
    public static ObservableCommandList<CommandModel> Commands { get; set; } = new ObservableCommandList<CommandModel>();

    public static Collection<CommandModel> AllCommands { get; set; } = new Collection<CommandModel>()
    {
        new CommandModel("!bet","Usage: !bet {money} {percentWin}"),
        new CommandModel("!daily","Gives you a free coins every day!"),
        new CommandModel("!sklep","Usage: !sklep {itemId}"),
        new CommandModel("!jackpotbuy","Usage: !jackpotbuy {count}"),
        new CommandModel("!zdrapka","Gives you a scratchcard!"),
        new CommandModel("!slots","Try your luck at slots!"),
        new CommandModel("!clear","Clears all visible messages"),
        new CommandModel("!invite","Usage: !invite {User Id}"),
        new CommandModel("!remove","Usage: !remove {User Id}"),
        new CommandModel("!mem","Sends funny meme to chat"),
    };
}

public class ObservableCommandList<T> : ObservableCollection<T>
{
    public void ReplaceRange(IEnumerable<T> collection)
    {
        Items.Clear();
        foreach (var item in collection) Items.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}