using Client.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.Views
{
    public class SearchPageView
    {
        public static ObservableCollection<PeronFoundModel> PeopleFound { get; set; } = new ObservableCollection<PeronFoundModel>();

        public static void AddFound(UserModel user) => PeopleFound.Add(new PeronFoundModel(user));

        public static void ClearList() => PeopleFound.Clear();

        public static void ParseQuery(object req)
        {
            List<SearchModel> users = ((JArray)req).ToObject<List<SearchModel>>();
            foreach(var user in users)
            {
                UserModel u = UserModel.CreateOrGet(user.Username, user.ID);
                AddFound(u);
            }
           
        }
    }
}