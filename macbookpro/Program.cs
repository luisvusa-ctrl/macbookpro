using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Graphics;
using CS2Cheat.Utils;
using static CS2Cheat.Core.User32;
using Application = System.Windows.Application;

namespace CS2Cheat;

public class Program :
    Application,
    IDisposable
{
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    public static extern uint TimeEndPeriod(uint uMilliseconds);

    private Program()
    {
        OptimizePerformance();

        Offsets.UpdateOffsets();
        Startup += (_, _) => InitializeComponent();
        Exit += (_, _) => Dispose();
    }

    private GameProcess GameProcess { get; set; } = null!;
    private GameData GameData { get; set; } = null!;
    private WindowOverlay WindowOverlay { get; set; } = null!;
    private Graphics.Graphics Graphics { get; set; } = null!;
    private BombTimer BombTimer { get; set; } = null!;

    private static void OptimizePerformance()
    {
        
        TimeBeginPeriod(1);

        
        System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

        
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
    }

    public void Dispose()
    {
        GameProcess?.Dispose();
        GameData?.Dispose();
        WindowOverlay?.Dispose();
        Graphics?.Dispose();
        BombTimer?.Dispose();

        TimeEndPeriod(1);
    }

    public static void Main()
    {
        new Program().Run();
    }

    private void InitializeComponent()
    {
        var features = ConfigManager.Load();
        GameProcess = new GameProcess();
        GameProcess.Start();

        GameData = new GameData(GameProcess);
        GameData.Start();

        WindowOverlay = new WindowOverlay(GameProcess);
        WindowOverlay.Start();

        Graphics = new Graphics.Graphics(GameProcess, GameData, WindowOverlay);
        Graphics.Start();

        BombTimer = new BombTimer(Graphics);
        if (features.BombTimer) BombTimer.Start();

        if (WindowOverlay?.Window?.Handle != IntPtr.Zero)
        {
            SetWindowDisplayAffinity(WindowOverlay.Window.Handle, 0x00000011);
        }
    }
}