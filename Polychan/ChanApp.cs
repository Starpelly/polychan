using Polychan.GUI;
using Polychan.Resources;

namespace Polychan;

public static class ChanApp
{
    public static Settings Settings { get; private set; }

    public static PolychanWindow MainWindow { get; private set; }
    public static ChanClient ChanClient { get; private set; }

    private static void init()
    {
        // Load settings first
        Settings = Settings.Load();

        ChanClient = new();
        ChanClient.Boards = ChanClient.GetBoardsAsync().GetAwaiter().GetResult();
    }

    public static void Start()
    {
        init();

        using var app = new Application();

        MainWindow = new PolychanWindow();

        MainWindow.Title = "Polychan";
        MainWindow.Resize(1600, 900);

        // Main Window Icon
        using var iconStream = PolychanResources.ResourceAssembly.GetManifestResourceStream("Polychan.Resources.Images.4channy.ico");
        MainWindow.SetIconFromStream(iconStream!);

        LoadCatalog("g");
        // LoadThread("714085510");
        MainWindow.Show();
        MainWindow.T();

        // LoadCatalog("g");
        // LoadThread("105756382");

        app.Run();
    }

    public static void LoadCatalog(string board)
    {
        ChanClient.CurrentBoard = board;
        ChanClient.Catalog = ChanClient.GetCatalogAsync().GetAwaiter().GetResult();

        MainWindow.LoadBoardCatalog(board);
        MainWindow.Title = $"Polychan - /{board}/";

        MainWindow.T();
    }

    public static void LoadThread(string threadID)
    {
        ChanClient.CurrentThread = ChanClient.GetThreadPostsAsync(threadID).GetAwaiter().GetResult();

        MainWindow.LoadThreadPosts(threadID);
        MainWindow.Title = $"Polychan - /{ChanClient.CurrentBoard}/{threadID}/ - {ChanClient.CurrentThread.Posts[0].Sub}";
    }
}