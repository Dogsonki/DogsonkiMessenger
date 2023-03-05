using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.Networking.Models;
using Client.Networking.Packets.Models;
using Client.Networking.Packets;
using Client.Networking.Core;
using Microsoft.JSInterop;
using Client.Pages.Components;

namespace Client.Pages;

public partial class SearchPage
{
    [Parameter]
    public string? SearchInput { get; set; }

    private bool ShouldShowResult { get; set; } = false;

    public IViewBindable? SelectedContextView { get; set; }

    private List<IViewBindable> SearchResult { get; } = new List<IViewBindable>();

    /* Results that being displayed on page after querying by SearchOption*/
    private List<IViewBindable> SearchFound { get; } = new List<IViewBindable>();

    private SearchOption Option { get; set; }

    private StateComponentController SearchLoadingComponent { get; } = new StateComponentController(); 

    protected override void OnParametersSet()
    {
        SendSearch(SearchInput);
    }

    private void SearchBarSubmit()
    {
        SendSearch(SearchInput);
    }

    private void SendSearch(string? searchInput)
    {
        if (!string.IsNullOrEmpty(searchInput))
        {
            ShouldShowResult = false;

            SearchPacket packet = new SearchPacket(searchInput, true);
            SocketCore.SendCallback(packet, Token.SEARCH_USER, (packet) => Task.Run(() => ParseFound(packet)), false);
        }
    }

    private void OpenChat(IViewBindable? view)
    {
        if(view is null)
        {
            return;
        }

        Conversation.OpenChat(view, naviagtion);
    }

    /// <summary>
    /// Parses all views from packet and adds them to SearchResult.
    /// </summary>
    private void ParseFound(SocketPacket packet)
    {
        SearchResult.Clear();
        SearchFound.Clear();

        SearchModel[]? founds = packet.Deserialize<SearchModel[]?>();

        if (founds is null || founds.Length == 0)
        {
            InvokeAsync(StateHasChanged);

            ShouldShowResult = true;

            return;
        }

        foreach (var found in founds)
        {
            IViewBindable createdFound = IViewBindable.CreateOrGet(found.Name, found.Id, found.isGroup);
            createdFound.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
            SearchResult.Add(createdFound);
        }

        ShouldShowResult = true;

        if(SearchResult.Count > 0)
        {
            SearchFound.AddRange(SearchResult);

            QuerySearchList();
        }
    }

    private void OnSearchOptionSelect(SearchOption option) 
    {
        Option = option;

        if(SearchResult.Count > 0) 
        {
            QuerySearchList();
        }
    }

    private bool ShouldShowInviteOption()
    {
        if(SelectedContextView is not null && SelectedContextView.BindType == BindableType.User)
        {
            User asUser = (User)SelectedContextView;
            return asUser.UserProperties.IsFriend == FriendStatus.None;
        }

        return false;
    }

    private void TryInvite()
    {
        if(SelectedContextView is not null)
        {
            User asUser = (User)SelectedContextView;
            asUser.InviteAsFriend();
            StateHasChanged();
        }
    }

    private bool ShouldShowLoading()
    {
        return !ShouldShowResult && SearchResult.Count == 0;
    }

    /// <summary>
    /// Queries SearchResult and displays SearchFound on the page. Invoke only after parse or when SearchResult has values.
    /// </summary>
    private void QuerySearchList()
    {
        SearchFound.Clear();

        if (Option == SearchOption.All)
        {
            JS.InvokeVoidAsync("SearchPageOptionSelector", new object[] { "search-option-all" });

            SearchFound.AddRange(SearchResult);
        }
        else if (Option == SearchOption.Users)
        {
            JS.InvokeVoidAsync("SearchPageOptionSelector", new object[] { "search-option-users" });

            SearchFound.AddRange(SearchResult.FindAll(x => x.BindType == BindableType.User || x.BindType == BindableType.LocalUser));
        }
        else
        {
            JS.InvokeVoidAsync("SearchPageOptionSelector", new object[] { "search-option-groups" });

            SearchFound.AddRange(SearchResult.FindAll(x => x.BindType == BindableType.Group));
        }

        InvokeAsync(StateHasChanged);
    }

    public void DisplayContextMenu(bool display, IViewBindable? view)
    {
        SelectedContextView = view;

        JS.InvokeVoidAsync("ShowContextMenu", new object[] { display, "context-frame" });
    }

    public enum SearchOption
    {
        All,
        Users,
        Groups
    }
}