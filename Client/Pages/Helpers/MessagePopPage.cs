using Client.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Logger.Push("InfoLevel is null", TraceType.Func, LogLevel.Warning);
            }

            if (ErrorLevel is null)
            {
                Logger.Push("ErrorLevel is null", TraceType.Func, LogLevel.Warning);
            }
        }

        public void ShowInfo(string info)
        {
            InfoText.Text = info;
            if (!InfoLevel.Contains(InfoText))
                InfoLevel.Children.Add(InfoText);
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
            ErrorText.Text = error;
            if (!ErrorLevel.Children.Contains(ErrorText))
                ErrorLevel.Children.Add(ErrorText);
        }
    }

    public enum PopType
    {
        Error,
        Info
    }
}
