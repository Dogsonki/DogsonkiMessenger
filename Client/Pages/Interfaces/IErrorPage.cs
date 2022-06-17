namespace Client.Pages;

internal interface IErrorPage
{
    StackLayout ErrorLevel { get; }
    Label ErrorText { get; }

    void ShowError(string ex);
    void ClearError();
}
