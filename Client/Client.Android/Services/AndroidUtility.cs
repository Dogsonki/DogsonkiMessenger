using Client.Utility.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Client.Droid.AndroidUtility))]

namespace Client.Droid
{
    public class AndroidUtility : IKeyboardService
    {
        public AndroidUtility() 
        {
        
        }

        public bool IsKeyboardVisible() => MainActivity.MainActivityInstance.GetInputManager().IsAcceptingText;
    }
}