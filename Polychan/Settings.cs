using FChan.Models;
using Newtonsoft.Json;

namespace Polychan.App;

public class Settings : Model
{
    public class CookieData
    {
        public string CloudflareClearance = string.Empty;
        public string FourchanPasskey = string.Empty;
    }

    public CookieData Cookies = new();

    public static Settings Load()
    {
        var appFolder = ChanApp.GetAppFolder();
        var jsonFilePath = Path.Combine(appFolder, "settings.json");

        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        if (File.Exists(jsonFilePath))
        {
            var json = File.ReadAllText(jsonFilePath);

            // @INVESTIGATE
            // This shouldn't fail but idk maybe there should be an error if it does?
            return JsonConvert.DeserializeObject<Settings>(json)!;
        }
        else
        {
            // Just create an empty settings file, who cares?
            var newS = new Settings();
            var json = JsonConvert.SerializeObject(newS, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
            return newS;
        }
    }
}