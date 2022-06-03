using Client.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace Client.Views
{
    public class SearchPageView
    {
        public static ObservableCollection<PeronFoundModel> PeopleFound { get; set; } = new ObservableCollection<PeronFoundModel>();

        public static void AddFound(string username)
        {
            PeopleFound.Add(new PeronFoundModel(username));
        }

        public static void ClearList()
        {
            PeopleFound.Clear();
        }

        public static void ParseQuery(object req)
        {
            JArray users;
            try
            {
                users = (JArray)req;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot parse usernames: {req} : " + ex);
                return;
            }

            foreach (var a in users)
            {
                AddFound(a.ToString());
            }
        }
    }
}
