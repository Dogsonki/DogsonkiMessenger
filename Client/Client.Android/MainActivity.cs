using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views.InputMethods;
using System.IO;
using System.Threading.Tasks;

namespace Client.Droid
{
    [Activity(Label = "Client", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public const int PickImageID = 1000;
        public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PickImageID)
            {
                if ((resultCode == Result.Ok) && (data != null))
                {
                    Android.Net.Uri uri = data.Data;
                    Stream stream = ContentResolver.OpenInputStream(uri);

                    // Set the Stream as the completion of the Task
                    PickImageTaskCompletionSource.SetResult(stream);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public InputMethodManager GetInputManager() => (InputMethodManager)this.GetSystemService(Context.InputMethodService);

    }
}