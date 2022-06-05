﻿using Client.Models;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Client.Views
{
    public class MainAfterLoginPageView
    {
        public static ObservableCollection<PeronFoundModel> LastChats { get; set; } = new ObservableCollection<PeronFoundModel>();

        public static void Clear() => Device.BeginInvokeOnMainThread(() => LastChats.Clear());

        public static void ParseQuery(JArray array)
        {
            JArray ar = array;

            foreach (var user in ar)
            {
                LastChats.Add(new PeronFoundModel(user.ToString()));
            }
        }
    }
}