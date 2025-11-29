using MaterialDesign;
using Polychan.GUI;
using Polychan.GUI.Layouts;
using Polychan.GUI.Widgets;

namespace Polychan.App.Widgets;

public class SettingsDialog : DialogWindow
{
    public SettingsDialog(Widget? parent = null) : base(parent)
    {
        Resize(642, 506);

        Layout = new VBoxLayout
        {
        };

        var menubar = new MenuBar(this)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed)
        };
        var file = menubar.AddMenu("File");
        file.AddAction(new(MaterialIcons.Folder, "Open Settings & Data Path", () =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = ChanApp.GetAppFolder(),
                UseShellExecute = true,
                Verb = "open"
            });
        }));
        file.AddAction(new(MaterialIcons.ContentCopy, "Copy Settings & Data Path to Clipboard", () =>
        {
            Application.Clipboard.SetText(ChanApp.GetAppFolder().Replace("\\", "/"));
        }));
        var window = menubar.AddMenu("Window");
        window.AddAction(new(MaterialIcons.Close, "Close", delegate(){
            this.Dispose();
        }));

        new NullWidget(this)
        {
            Fitting = FitPolicy.ExpandingPolicy
        };

        var buttons = new NullWidget(this)
        {
            Fitting = new(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            Height = 42,
            Layout = new HBoxLayout
            {
                Padding = new(9),
                Spacing = 6
            },
            Name = "buttons"
        };
        new NullWidget(buttons)
        {
            Fitting = FitPolicy.ExpandingPolicy,
        };
        new PushButton("OK", buttons)
        {
            X = 16,
            Y = 16,
            Width = 80,
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
        new PushButton("Cancel", buttons)
        {
            X = 16,
            Y = 16,
            Width = 80,
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
        new PushButton("Apply", buttons)
        {
            X = 16,
            Y = 16,
            Width = 80,
            Fitting = new(FitPolicy.Policy.Fixed, FitPolicy.Policy.Expanding)
        };
    }

    public override void OnShown()
    {
        Title = "Preferences";
    }
}