namespace Client
{
    public class LocalUser
    {
        public static string Username;
        public static bool IsChatting { get; set; } = false;
        public static bool IsLoggedIn { get; set; } = false;

        public static void Logout()
        {
            Username = null;
            IsLoggedIn = false;
        }
    }
}
