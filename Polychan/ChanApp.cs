using Polychan.GUI;
using Polychan.Resources;
using Polychan.App.Database;

namespace Polychan.App;

public static class ChanApp
{
    public static Settings Settings { get; private set; } = null!;

    public static MainWindow MainWindow { get; private set; } = null!;
    public static FourChanClient Client { get; private set; } = null!;

    private static readonly string DbPath =
        Path.Combine(GetAppFolder(), "4chan.db");

    public static readonly ThreadHistoryDatabase HistoryDb = new(Path.Combine(GetAppFolder(), DbPath));

    private static void init()
    {
        // Load settings first
        Settings = Settings.Load();

        Client = new FourChanClient();
        Client.Boards = Client.GetBoardsAsync().GetAwaiter().GetResult();
        
        HistoryDb.Initialize();
    }

    public static void Start()
    {
        init();

        using var app = new Application();

        MainWindow = new MainWindow();

        MainWindow.Title = "Polychan";
        MainWindow.Resize(1600, 900);

        // Main Window Icon
        using var iconStream =
            PolychanResources.ResourceAssembly.GetManifestResourceStream("Polychan.Resources.Images.4channy.ico");
        MainWindow.SetIconFromStream(iconStream!);

        LoadCatalog("g");
        // LoadThread("714085510");
        MainWindow.Show();

        // LoadCatalog("g");
        // LoadThread("105756382");

        app.Run();
    }

    public static void LoadCatalog(string board)
    {
        Client.CurrentBoard = board;
        Client.Catalog = Client.GetCatalogAsync().GetAwaiter().GetResult();

        MainWindow.LoadBoardCatalog(board);
        MainWindow.Title = $"Polychan - /{board}/";
    }

    public static void LoadThread(FChan.Models.CatalogThread thread)
    {
        Client.CurrentThread = Client.GetThreadPostsAsync(thread.No).GetAwaiter().GetResult();

        MainWindow.LoadThreadPosts(Client.CurrentThread, thread, thread.No);
        MainWindow.Title = $"Polychan - /{Client.CurrentBoard}/{thread.No}/ - {Client.CurrentThread.Posts[0].Sub}";
    }

    public static string GetAppFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "Polychan");

        return appFolder;
    }
}