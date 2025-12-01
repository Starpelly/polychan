using Imageboard;
using Polychan.App.Widgets.Main;
using Polychan.GUI;
using Polychan.Resources;
// using Polychan.App.Database;

namespace Polychan.App;

public static class ChanApp
{
    public static Settings Settings { get; private set; } = null!;

    public static MainWindow MainWindow { get; private set; } = null!;
    public static Imageboard.Client ImageboardClient { get; private set; } = null!;

    private static readonly string DbPath =
        Path.Combine(GetAppFolder(), "4chan.db");

    // public static readonly ThreadHistoryDatabase HistoryDb = new(Path.Combine(GetAppFolder(), DbPath));
    // public static readonly DownloadedDatabase DownloadedDb = new(Path.Combine(GetAppFolder(), DbPath));

    private static void init()
    {
        // Load settings first
        Settings = Settings.Load();

        ImageboardClient = new Imageboard.Client(Settings.Cookies.CloudflareClearance, Settings.Cookies.FourchanPasskey);
        // Client.Boards = Client.GetBoardsAsync().GetAwaiter().GetResult();
        
        // HistoryDb.Initialize();
        // DownloadedDb.Initialize();
        
        foreach (var name in PolychanResources.ResourceAssembly.GetManifestResourceNames())
        {
            Console.WriteLine(name);
        }
    }

    public static void Start()
    {
        init();

        using var app = new Application();

        MainWindow = new MainWindow();

        MainWindow.Title = "Polychan";
        MainWindow.Resize(1600, 900);

        // Main Window Icon
        /*
        using var iconStream =
            PolychanResources.ResourceAssembly.GetManifestResourceStream("Polychan.Resources.Images.Client.4channy.ico");
        MainWindow.SetIconFromStream(iconStream!);
*/

        var testCatalog = ChanApp.ImageboardClient
            .GetCatalogAsync(ChanApp.ImageboardClient.FourChanBoards[new BoardId("g")]).GetAwaiter().GetResult();
        var tab = MainWindow.CreateNewTabWithCatalog(testCatalog);
        
        // LoadCatalog("g");
        // LoadThread("714085510");
        MainWindow.Show();

        // LoadCatalog("g");
        // LoadThread("105756382");

        app.Run();
    }

    /*
    public static void LoadCatalog(string board)
    {
        // Client.Catalog = Client.GetCatalogAsync().GetAwaiter().GetResult();

        MainWindow.NewTab(board);
        MainWindow.Title = $"Polychan - /{board}/";
    }

    public static void LoadThread(FChan.Models.CatalogThread thread)
    {
        // MainWindow.LoadThreadPosts(Client.CurrentThread, thread, thread.No);
        // MainWindow.Title = $"Polychan - /{Client.CurrentBoard}/{thread.No}/ - {Client.CurrentThread.Posts[0].Sub}";
    }
    */

    public static string GetAppFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "Polychan");

        return appFolder;
    }
}