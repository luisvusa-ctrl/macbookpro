using CS2Cheat.Utils;
using System;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

internal class BombTimer(Graphics.Graphics graphics) : ThreadedServiceBase
{
    private static string _bombPlanted = string.Empty;
    private static string _bombSite = string.Empty;
    private static bool _isBombPlanted;
    private static float _defuseLeft;
    private static float _timeLeft;
    private float _currentTime;

    protected override void FrameAction()
    {
        if (!graphics.GameProcess.IsValid) return;

        
        IntPtr globalVars = graphics.GameProcess.ModuleClient.Read<IntPtr>(Offsets.dwGlobalVars);

        
        _currentTime = graphics.GameProcess.Process.Read<float>(globalVars + 0x30);

        
        _isBombPlanted = graphics.GameProcess.ModuleClient.Read<bool>(Offsets.dwPlantedC4 - 0x8);

        if (!_isBombPlanted)
        {
            _timeLeft = 0;
            _defuseLeft = 0;
            _bombPlanted = string.Empty;
            return;
        }

        IntPtr tempC4 = graphics.GameProcess.ModuleClient.Read<IntPtr>(Offsets.dwPlantedC4);
        IntPtr plantedC4 = graphics.GameProcess.Process.Read<IntPtr>(tempC4);

        if (plantedC4 != IntPtr.Zero)
        {
            
            float c4Blow = graphics.GameProcess.Process.Read<float>(plantedC4 + Offsets.m_flC4Blow);
            float defuseCountDown = graphics.GameProcess.Process.Read<float>(plantedC4 + Offsets.m_flDefuseCountDown);
            bool beingDefused = graphics.GameProcess.Process.Read<bool>(plantedC4 + Offsets.m_bBeingDefused);
            int siteInt = graphics.GameProcess.Process.Read<int>(plantedC4 + Offsets.m_nBombSite);

            
            _timeLeft = Math.Max(0, c4Blow - _currentTime);
            _defuseLeft = beingDefused ? Math.Max(0, defuseCountDown - _currentTime) : 0;

            _bombSite = siteInt == 1 ? "B" : "A";
            _bombPlanted = $"Bomb is planted on site: {_bombSite}";

            
            if (_timeLeft <= 0) _isBombPlanted = false;
        }
    }

    public static void Draw(Graphics.Graphics graphics)
    {
        if (!_isBombPlanted || _timeLeft <= 0) return;

        var timerText = $"Time left: {_timeLeft:0.0} seconds";
        var defuseText = _defuseLeft > 0 ? $"Defuse time: {_defuseLeft:0.0} seconds" : "Not being defused";

        
        graphics.BombTime?.DrawText(default,
            $"{_bombPlanted}{Environment.NewLine}{timerText}{Environment.NewLine}{defuseText}",
            20, 500, Color.WhiteSmoke);
    }
}