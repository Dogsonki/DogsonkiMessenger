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

    private List<IViewBindable> SearchResult { get; } = new List<IViewBindable>();

    /* Results that being displayed on page after querying by SearchOption*/
    private List<IViewBindable> SearchFound { get; } = new List<IViewBindable>();

    private SearchOption SearchFilterOption { get; set; }

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

    private void OnSearchOptionSelect(SearchOption filterOption) 
    {
        SearchFilterOption = filterOption;

        if(SearchResult.Count > 0) 
        {
            QuerySearchList();
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

        if (SearchFilterOption == SearchOption.All)
        {
            JS.InvokeVoidAsync("SearchPageOptionSelector", new object[] { "search-option-all" });

            SearchFound.AddRange(SearchResult);
        }
        else if (SearchFilterOption == SearchOption.Users)
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

    public void InviteUser(IViewBindable view) 
    {
        ((User)view).InviteAsFriend();
        StateHasChanged();
    }

    private bool ShouldRenderInvite(IViewBindable view) 
    {
        if (view.IsUser()) {
            UserProperties properties = ((User)view).UserProperties;

            return properties.IsFriend != FriendStatus.Friend && properties.IsFriend != FriendStatus.Invited;
        }

        return false;
    }

    private bool ShouldRenderCancelInvite(IViewBindable view) 
    {
        if (view.IsUser()) {
            return ((User)view).UserProperties.IsFriend == FriendStatus.Invited;
        }

        return false;
    }

    private void FoundClicked(IViewBindable view) 
    {
        Conversation.OpenChat(view, naviagtion);
    }

    public enum SearchOption
    {
        All,
        Users,
        Groups
    }
}