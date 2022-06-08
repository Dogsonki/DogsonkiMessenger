using Client.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Client.Views
{
    public class MainAfterLoginPageView
    {
        public static ObservableCollection<PeronFoundModel> LastChats { get; set; } = new ObservableCollection<PeronFoundModel>();
        public LocalUser user { get; set; } = LocalUser.Current;

        public static void Clear() => Device.BeginInvokeOnMainThread(() => LastChats.Clear());

        public static void AddLastUser(SearchModel user) => LastChats.Add(new PeronFoundModel(UserModel.CreateOrGet(user.Username, user.ID)));
    }
}
