using Microsoft.AspNetCore.Components;
using Client.Models;
using Client.Networking.Models;
using Client.Networking.Packets.Models;
using Client.Networking.Packets;
using Client.Networking.Core;
using Microsoft.JSInterop;
using Client.Utility;
using Client.Pages.Components;

namespace Client.Pages;

public partial class SearchPage
{
    [Parameter]
    public string? SearchInput { get; set; }

    private bool ShouldShowResult { get; set; } = false;

    public IViewBindable? SelectedContextView { get; set; }

    /* All search results that being found*/
    private List<IViewBindable> SearchResult { get; } = new List<IViewBindable>();

    /* Results that being displayed on page after querying by SearchOption*/
    private List<IViewBindable> SearchFound { get; } = new List<IViewBindable>();

    private SearchOption Option { get; set; }

    private LoadingComponentController SearchLoadingComponent { get; } = new LoadingComponentController(); 

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
            SocketCore.SendCallback(packet, Token.SEARCH_USER, ParseFound, false);
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
            IViewBindable createdFound = IViewBindable.CreateOrGet(found.Username, found.Id, found.isGroup);
            createdFound.PropertyChanged += async (sender, e) => { await InvokeAsync(StateHasChanged); };
            SearchResult.Add(createdFound);
        }

        ShouldShowResult = true;

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

    private bool ShouldShowLoading()
    {
        return !ShouldShowResult && SearchResult.Count == 0;
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

    public void SetContextMenu(IViewBindable view)
    {
        SelectedContextView = view;
        JS.InvokeVoidAsync("ShowContextMenu", new object[] { true, "context-frame" });
    }

    public void CloseContextMenu()
    {
        SelectedContextView = null;
        JS.InvokeVoidAsync("ShowContextMenu", new object[] { false, "context-frame" });
    }

    public enum SearchOption
    {
        All,
        Users,
        Groups
    }
}