using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AfterLoginPageFlyout : ContentPage
    {
        public ListView ListView;

        public AfterLoginPageFlyout()
        {
            InitializeComponent();

            BindingContext = new AfterLoginPageFlyoutViewModel();
            ListView = MenuItemsListView;
        }

        class AfterLoginPageFlyoutViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<AfterLoginPageFlyoutMenuItem> MenuItems { get; set; }

            public AfterLoginPageFlyoutViewModel()
            {
                MenuItems = new ObservableCollection<AfterLoginPageFlyoutMenuItem>(new[]
                {
                    new AfterLoginPageFlyoutMenuItem { Id = 0, Title = "Page 1" },
                    new AfterLoginPageFlyoutMenuItem { Id = 1, Title = "Page 2" },
                    new AfterLoginPageFlyoutMenuItem { Id = 2, Title = "Page 3" },
                    new AfterLoginPageFlyoutMenuItem { Id = 3, Title = "Page 4" },
                    new AfterLoginPageFlyoutMenuItem { Id = 4, Title = "Page 5" },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}