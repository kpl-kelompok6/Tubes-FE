using KPL_FE.Controllers;
using KPL_FE.Views;
using System.Windows.Controls;
using Xunit;

namespace KPL_FE.Tests;

public class ShortcutHelpOverlayTests
{
    private static void RunOnSta(Action action)
    {
        Exception? ex = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception e) { ex = e; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (ex != null) throw ex;
    }

    [Fact]
    public void Overlay_Create_ShouldNotThrow()
    {
        RunOnSta(() =>
        {
            var ex = Record.Exception(() => _ = new ShortcutHelpOverlay());
            Assert.Null(ex);
        });
    }

    [Fact]
    public void Controller_HandleShowHelp_ShouldNotThrow_WhenFrameHasNoWindow()
    {
        RunOnSta(() =>
        {
            var frame = new Frame();
            var controller = new KeyboardShortcutController(frame, () => null);
            var ex = Record.Exception(() => controller.HandleShowHelp());
            Assert.Null(ex);
        });
    }

    [Fact]
    public void Controller_ShouldNotThrow_OnAnyRegisteredShortcut()
    {
        RunOnSta(() =>
        {
            var frame = new Frame();
            var controller = new KeyboardShortcutController(frame, () => null);

            var ex1 = Record.Exception(() => controller.HandleNewTransaction());
            var ex2 = Record.Exception(() => controller.HandleFocusSearch());
            var ex3 = Record.Exception(() => controller.HandleRefresh());
            var ex4 = Record.Exception(() => controller.HandleShowHelp());

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
        });
    }
}
