﻿using System.Diagnostics;
using CS2Cheat.Core;
using CS2Cheat.Utils;

namespace CS2Cheat.Data.Game;

public class GameProcess : ThreadedServiceBase
{
    #region constants

    private const string NameProcess = "cs2";

    private const string NameModule = "client.dll";

    private const string NameWindow = "Counter-Strike 2";

    #endregion

    #region properties

    protected override string ThreadName => nameof(GameProcess);

    protected override TimeSpan ThreadFrameSleep { get; set; } = new(0, 0, 0, 0, 500);

    public System.Diagnostics.Process? Process { get; private set; }

    public Module? ModuleClient { get; private set; }

    private IntPtr WindowHwnd { get; set; }

    public Rectangle WindowRectangleClient { get; private set; }

    private bool WindowActive { get; set; }

    public bool IsValid => WindowActive;

    #endregion

    #region routines

    public override void Dispose()
    {
        InvalidateWindow();
        InvalidateModules();
        base.Dispose();
    }


    protected override async void FrameAction()
    {
        if (!EnsureProcessAndModules()) InvalidateModules();

        if (!EnsureWindow()) InvalidateWindow();

        await Task.Delay(ThreadFrameSleep);
    }


    private void InvalidateModules()
    {
        ModuleClient?.Dispose();
        ModuleClient = default;

        Process?.Dispose();
        Process = default;
    }

    private void InvalidateWindow()
    {
        WindowHwnd = IntPtr.Zero;
        WindowRectangleClient = Rectangle.Empty;
        WindowActive = false;
    }

    private bool EnsureProcessAndModules()
    {
        Process ??= System.Diagnostics.Process.GetProcessesByName(NameProcess).FirstOrDefault()!;
        if (Process == null || Process.HasExited) return false;

        if (ModuleClient == null && Process != null)
        {
            var processModule = Process.Modules
                .OfType<ProcessModule>()
                .FirstOrDefault(m => m.ModuleName.Equals(NameModule, StringComparison.OrdinalIgnoreCase));
            if (processModule != null)
                ModuleClient = new Module(Process, processModule); // Pass both Process and ProcessModule as required
        }

        return ModuleClient != null;
    }


    private bool EnsureWindow()
    {
        WindowHwnd = User32.FindWindow(null!, NameWindow);
        if (WindowHwnd == IntPtr.Zero) return false;

        WindowRectangleClient = Utility.GetClientRectangle(WindowHwnd);
        if (WindowRectangleClient.Width <= 0 || WindowRectangleClient.Height <= 0) return false;

        WindowActive = WindowHwnd == User32.GetForegroundWindow();

        return WindowActive;
    }

    #endregion
}