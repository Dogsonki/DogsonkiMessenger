namespace Client.Models.Settings;

internal class UserPreferences
{
    public static readonly PreferenceModel[] DefaultPreferences = new[]
    {
        new PreferenceModel("first_logging",false)
    };

    public static void Create()
    {
        foreach (var pref in DefaultPreferences)
        {
            Type t = pref.DefaultValue.GetType();

            if (t == typeof(string))
            {
                Preferences.Set(pref.Name, (string)pref.DefaultValue);
            }
            else if (t == typeof(int))
            {
                Preferences.Set(pref.Name, (int)pref.DefaultValue);
            }
            else if (t == typeof(bool))
            {
                Preferences.Set(pref.Name, (bool)pref.DefaultValue);
            }
        }
    }
}

struct PreferenceModel
{
    public string Name;
    public object DefaultValue;

    public PreferenceModel(string name, object defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }
}