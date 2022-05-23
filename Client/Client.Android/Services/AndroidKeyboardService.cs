using Client.Utility.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Client.Droid.AndroidKeybaordService))]
namespace Client.Droid
{
    public class AndroidKeybaordService : IKeyboardService
    {
        public bool IsKeyboardVisible() => MainActivity.MainActivityInstance.GetInputManager().IsAcceptingText;
    }
}