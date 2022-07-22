using Client.Models;
using Client.Models.UserType.Bindable;
using Client.Networking.Core;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Pages.TemporaryPages.GroupChat;

public partial class GroupChatCreator : ContentPage
{
    public static ObservableCollection<AnyListBindable> Invited { get; set; } = new ObservableCollection<AnyListBindable>();
    public static ObservableCollection<AnyListBindable> UsersFound { get; set; } = new ObservableCollection<AnyListBindable>();

    public GroupChatCreator()
    {
        Invited.Clear();
        UsersFound.Clear();

        Invited.Add(new AnyListBindable(LocalUser.UserRef));
        LocalUser.isCreatingGroup = true;

        InitializeComponent();

        GroupName.Text = LocalUser.UserRef.Username + "'s Chat Group";

        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void Search(object sender, EventArgs e)
    {
        UsersFound.Clear();

        string input = ((Entry)sender).Text;

        if (!string.IsNullOrEmpty(input))
        {
            SocketCore.Send(input, Token.SEARCH_USER);
        }
    }

    public static void ParseFound(object req)
    {
        List<SearchModel> users = ((JArray)req).ToObject<List<SearchModel>>();

        foreach (var user in users)
        {
            AnyListBindable br = new AnyListBindable(User.CreateOrGet(user.Username, user.ID));
            br.Input = new Command(AddToInvited);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UsersFound.Add(br);
            });
        }
    }

    public static void AddToInvited(object user)
    {
        AnyListBindable AddedUser = (AnyListBindable)user;

        foreach(AnyListBindable u in Invited)
        {
            if(u.Id == AddedUser.Id)
            {
                return;
            }
        }
        Invited.Add(AddedUser);
    }

    private void CreateGroup(object sender, EventArgs e)
    {
        List<int> users = new List<int>();

        for(int i = 0; i < Invited.Count; i++)
        {
            int id = (int)Invited[i].Id;

            if (id != LocalUser.Id)
            {
                users.Add(id);
            }
           
        }

        SocketCore.Send(new GroupChatCreateModel(GroupName.Text, int.Parse(LocalUser.id), users.ToArray()),Token.GROUP_CHAT_CREATE);
    }
}