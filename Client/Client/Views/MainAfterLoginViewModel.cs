﻿using Client.Models;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Client.Views
{
    public class MainAfterLoginViewModel
    {
        public static ObservableCollection<PeronFoundModel> LastChats { get; set; } = new ObservableCollection<PeronFoundModel>();

        public static void ParseQuery(string rev)
        {
            JArray ar = JArray.Parse(rev);

            foreach(var user in ar)
            {
                LastChats.Add(new PeronFoundModel(user.ToString()));
            }

            if(ar.Count > 0)
            {
                //No last chats ...
            }
        }
    }
}
