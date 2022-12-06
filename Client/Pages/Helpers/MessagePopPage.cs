using Client.Utility;


namespace Client.Pages.Helpers
{
    public class MessagePopPage
    {
        StackLayout InfoLevel;
        StackLayout ErrorLevel;

        private Label InfoText = new Label()
        {
            TextColor = Color.FromRgb(255, 255, 255),
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            HorizontalTextAlignment = TextAlignment.Center
        };

        private Label ErrorText = new Label()
        {
            TextColor = Color.FromRgb(255, 0, 0),
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            HorizontalTextAlignment = TextAlignment.Center
        };

        public MessagePopPage(ContentPage page)
        {
            InfoLevel = (StackLayout)page.FindByName("InfoLevel");
            ErrorLevel = (StackLayout)page.FindByName("ErrorLevel");

            if (InfoLevel is null)
            {
                Logger.Push("InfoLevel is null", LogLevel.Warning);
            }

            if (ErrorLevel is null)
            {
                Logger.Push("ErrorLevel is null", LogLevel.Warning);
            }
        }

        public void ShowInfo(string info)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                InfoLevel.IsVisible = true;
                InfoText.Text = info;
                if (!InfoLevel.Contains(InfoText))
                    InfoLevel.Children.Add(InfoText);
            });
        }

        public void Clear()
        {
            Clear(PopType.Error);
            Clear(PopType.Info);
        }

        public void Clear(PopType type)
        {
            switch (type)
            {
                case PopType.Info:
                    {
                        if (InfoLevel is null) return;
                        if (InfoLevel.Contains(InfoText)) InfoLevel.Remove(InfoText);
                        break;
                    }
                case PopType.Error:
                    {
                        if (ErrorLevel is null) return;
                        if (ErrorLevel.Contains(ErrorText)) ErrorLevel.Remove(ErrorText);
                        break;
                    }
            }
        }

        public void ShowError(string error)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorLevel.IsVisible = true;
                ErrorText.Text = error;

                if (!ErrorLevel.Children.Contains(ErrorText))
                    ErrorLevel.Children.Add(ErrorText);
            });
        }
    }

    public enum PopType
    {
        Error,
        Info
    }
}
