using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.Networking.Models;
using Client.Networking.Packets.Models;
using Client.Networking.Packets;
using Client.Networking.Core;
using Client.Utility;
using Microsoft.JSInterop;

namespace Client.Pages;

public partial class SearchPage
{
    [Parameter]
    public string? SearchInput { get; set; }

    /* All search results that being found*/
    private List<IViewBindable> SearchResult { get; } = new List<IViewBindable>();

    /* Results that being displayed on page after quering by SearchOption*/
    private List<IViewBindable> SearchFound { get; } = new List<IViewBindable>();

    private SearchOption Option { get; set; }

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
            SearchPacket packet = new SearchPacket(searchInput, true);
            SocketCore.SendCallback(packet, Token.SEARCH_USER, ParseFound, false);
        }
    }

    private void OpenChat(IViewBindable view)
    {
        Conversation.OpenChat(view, naviagtion);
    }

    private void ParseFound(SocketPacket packet)
    {
        SearchResult.Clear();
        SearchFound.Clear();

        SearchModel[]? founds = packet.Deserialize<SearchModel[]?>();

        if (founds is null || founds.Length == 0)
        {
            InvokeAsync(StateHasChanged);
            return;
        }

        foreach (var found in founds)
        {
            IViewBindable createdFound = IViewBindable.CreateOrGet(found.Username, found.Id, found.isGroup);
            createdFound.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
            SearchResult.Add(createdFound);
        }

        QuerySearchList();

        InvokeAsync(StateHasChanged);
    }

    private void OnSearchOptionSelect(SearchOption option) 
    {
        Option = option;

        if(SearchResult.Count > 0) 
        {
            QuerySearchList();
        }
    }

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

    public enum SearchOption
    {
        All,
        Users,
        Groups
    }
}