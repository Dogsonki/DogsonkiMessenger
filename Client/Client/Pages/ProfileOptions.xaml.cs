using Client.IO;
using Client.Models;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProfileOptions : ContentPage
	{
		public ProfileOptions ()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent ();
		}

        private async void ChangeAvatarClicked(object sender, System.EventArgs e)
        {
            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick new avatar"
            });

            if (image == null)
                return;

            Stream stream = await image.OpenReadAsync();
            lock (stream)
            {
                LocalUser.Current.Avatar = ImageSource.FromStream(() => new MemoryStream(Essential.StreamToBuffer(stream, stream.Length)));
            }
            await stream.DisposeAsync();
        }
    }
}