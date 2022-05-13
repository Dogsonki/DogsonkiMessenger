using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Client.Models;
using Xamarin.Forms;

namespace Client.Views
{
    public class PeopleFinderViewModel
    {
        public static ObservableCollection<PeronFoundModel> PeopleFound { get; set; } = new ObservableCollection<PeronFoundModel>();

        public PeopleFinderViewModel()
        {
        }//TODO: Fix buttons that opens chat room

        private void OpenChat(string req)
        {
            Console.WriteLine(req);
        }

        public static void AddFound(string username)
        {
            PeopleFound.Add(new PeronFoundModel(username));
        }

        public static void ClearList()
        {
            PeopleFound.Clear();
        }
    }
}
