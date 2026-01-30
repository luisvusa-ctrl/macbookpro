using CS2Cheat.Core.Data;
using CS2Cheat.Data.Entity;
using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Utils;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

namespace CS2Cheat.Graphics;

public class Graphics : ThreadedServiceBase
{
    private readonly object _deviceLock = new();
    private readonly Vertex[] _vertexArray = new Vertex[32768];
    private int _vertexCount = 0;

    private Matrix _viewMatrix;
    private Vector2 _res;
    private Device? _device;
    private SharpDX.Direct3D9.Line? _line;
    private bool _isDisposed;

    public Graphics(GameProcess gameProcess, GameData gameData, WindowOverlay windowOverlay)
    {
        WindowOverlay = windowOverlay;
        GameProcess = gameProcess;
        GameData = gameData;
        _res = new Vector2(WindowOverlay.Window.Width, WindowOverlay.Window.Height);
        InitializeDevice();
    }

    protected override string ThreadName => nameof(Graphics);
    private WindowOverlay WindowOverlay { get; }
    public GameProcess GameProcess { get; }
    public GameData GameData { get; }

    public Font? FontAzonix64 { get; private set; }
    public Font? FontConsolas32 { get; private set; }
    public Font? Undefeated { get; private set; }
    public Font? Gamesense { get; private set; }
    public Font? WeaponIcons { get; private set; }
    public Font? BombTime { get; private set; }

    public SharpDX.Direct3D9.Device Device => _device;
    public override void Dispose()
    {
        if (_isDisposed) return;
        base.Dispose();
        lock (_deviceLock) { DisposeResources(); _isDisposed = true; }
    }

    private void InitializeDevice()
    {
        var parameters = new PresentParameters
        {
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            DeviceWindowHandle = WindowOverlay.Window.Handle,
            BackBufferFormat = Format.A8R8G8B8,
            BackBufferWidth = (int)_res.X,
            BackBufferHeight = (int)_res.Y,
            PresentationInterval = PresentInterval.Immediate, 
            EnableAutoDepthStencil = false
        };

        _device = new Device(new Direct3D(), 0, DeviceType.Hardware, WindowOverlay.Window.Handle,
            CreateFlags.HardwareVertexProcessing, parameters);

        FontAzonix64 = new Font(_device, new FontDescription { Height = 32, FaceName = "Tahoma" });
        Gamesense = new Font(_device, new FontDescription { Height = 11, FaceName = "Smallest Pixel-7", Quality = FontQuality.Antialiased });
        FontConsolas32 = new Font(_device, new FontDescription { Height = 12, FaceName = "Tahoma", Weight = FontWeight.Bold, Quality = FontQuality.ClearType });
        WeaponIcons = new Font(_device, new FontDescription { Height = 15, FaceName = "csgo_icons", Quality = FontQuality.Antialiased });
        BombTime = new Font(_device, new FontDescription { Height = 30, FaceName = "Tahoma", Weight = FontWeight.Bold, Quality = FontQuality.ClearType });
        Undefeated = new Font(_device, new FontDescription { Height = 12, FaceName = "undefeated" });

        try { _line = new SharpDX.Direct3D9.Line(_device) { Width = 2.0f, Antialias = true }; }
        catch { _line = null; }
    }

    
    public Device? GetDevice() => _device;

    private void RenderFrame()
    {
        if (_isDisposed) return;
        lock (_deviceLock)
        {
            if (_device == null) return;
            _device.Clear(ClearFlags.Target, Color.FromAbgr(0), 1, 0);
            _device.BeginScene();
            _vertexCount = 0;

            
            GameData.UpdateViewMatrix();

            if (GameData.Player != null)
            {
                _viewMatrix = GameData.Player.MatrixViewProjectionViewport;
                DrawFeatures();
                RenderVertices();
            }

            _device.EndScene();
            _device.Present();
        }
    }

    private void DrawFeatures()
    {
        WindowOverlay.Draw(GameProcess, this);
        var features = ConfigManager.Load();
        if (features.EspAimCrosshair) EspAimCrosshair.Draw(this);
        if (features.BombTimer) BombTimer.Draw(this);
        DrawPlayerEsp(features);
    }

    private void DrawPlayerEsp(ConfigManager features)
    {
        var player = GameData.Player;
        if (player == null || GameData.Entities == null) return;

        Vector3 localPos = GameProcess.Process.Read<Vector3>(player.AddressBase + Offsets.m_vOldOrigin);

        foreach (var entity in GameData.Entities)
        {
            if (!entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;
            if (features.TeamCheck && entity.Team == player.Team) continue;

           
            if (entity.BonePos == null || !entity.BonePos.TryGetValue("head", out var head3D)) continue;

            Vector3 footPos3D = entity.Position; 

            
            Vector3 headTransform = _viewMatrix.Transform(head3D + new Vector3(0, 0, 7f)); 
            Vector3 footTransform = _viewMatrix.Transform(footPos3D);

            
            if (headTransform.Z >= 1 || footTransform.Z >= 1) continue;

            Vector2 screenHead = new Vector2(headTransform.X, headTransform.Y);
            Vector2 screenFoot = new Vector2(footTransform.X, footTransform.Y);

            
            float height = Math.Abs(screenFoot.Y - screenHead.Y);
            float width = height / 1.9f;

            Vector2 topLeft = new Vector2(screenFoot.X - (width / 2), screenHead.Y);
            Vector2 bottomRight = new Vector2(screenFoot.X + (width / 2), screenFoot.Y);
            float centerX = screenFoot.X;

            

            
            if (features.EspBox)
                EspBox.Draw(this, entity, topLeft, bottomRight);

            
            if (features.EspPlayerName)
            {
                Vector2 namePos = new Vector2(centerX, topLeft.Y - 14);
                EspPlayerName.Draw(this, entity, namePos, bottomRight);
            }

            
            if (features.EspHealthBar)
            {
                
                Vector2 hTopLeft = new Vector2(topLeft.X - 6, topLeft.Y);
                Vector2 hBottomRight = new Vector2(topLeft.X - 4, bottomRight.Y);
                EspHealthBar.Draw(this, entity, hTopLeft, hBottomRight);
            }

            
            if (features.EspFlags)
            {
                Vector2 flagsPos = new Vector2(bottomRight.X + 4, topLeft.Y);
                EspFlags.Draw(this, entity, flagsPos, bottomRight);
            }

            
            float currentBottomY = bottomRight.Y + 3;

            
            if (features.EspAmmoBar)
            {
                Vector2 aTopLeft = new Vector2(topLeft.X, currentBottomY);
                Vector2 aBottomRight = new Vector2(bottomRight.X, currentBottomY + 2);
                EspAmmoBar.Draw(this, entity, aTopLeft, aBottomRight);
                currentBottomY += 5;
            }

            
            if (features.DrawWeaponIcon || features.DrawWeaponText)
            {
                Vector2 weaponPos = new Vector2(centerX, currentBottomY);
                
                EspWeapon.Draw(this, entity, weaponPos, bottomRight, features);
            }

            
            if (features.EspSound) EspSound.Draw(this, entity, localPos);
            if (features.SkeletonEsp) SkeletonEsp.Draw(this, entity);
            if (features.EspSnaplines) EspSnaplines.Draw(this, entity);
        }
    }

    
    public void DrawLine(Color color, Vector2 start, Vector2 end)
    {
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
        if (_vertexCount + 2 >= _vertexArray.Length) return;
        _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(start.X, start.Y, 0, 1.0f), Color = d3dColor };
        _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(end.X, end.Y, 0, 1.0f), Color = d3dColor };
    }

    public void DrawLine(Color color, params Vector2[] verts)
    {
        if (verts == null || verts.Length < 2) return;
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
        for (int i = 0; i < verts.Length - 1; i++)
        {
            if (_vertexCount + 2 >= _vertexArray.Length) break;
            _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(verts[i].X, verts[i].Y, 0, 1.0f), Color = d3dColor };
            _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(verts[i + 1].X, verts[i + 1].Y, 0, 1.0f), Color = d3dColor };
        }
    }

    public void DrawFilledRectangle(Color color, Vector2 topLeft, Vector2 bottomRight)
    {
        if (_device == null) return;
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
        Vertex[] vertices = new Vertex[4];
        vertices[0] = new Vertex { Position = new Vector4(topLeft.X, topLeft.Y, 0, 1.0f), Color = d3dColor };
        vertices[1] = new Vertex { Position = new Vector4(bottomRight.X, topLeft.Y, 0, 1.0f), Color = d3dColor };
        vertices[2] = new Vertex { Position = new Vector4(bottomRight.X, bottomRight.Y, 0, 1.0f), Color = d3dColor };
        vertices[3] = new Vertex { Position = new Vector4(topLeft.X, bottomRight.Y, 0, 1.0f), Color = d3dColor };

        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, vertices);
    }

    public void DrawRectangle(Color color, Vector2 topLeft, Vector2 bottomRight)
    {
        Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);
        Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
        DrawLine(color, topLeft, topRight);
        DrawLine(color, topRight, bottomRight);
        DrawLine(color, bottomRight, bottomLeft);
        DrawLine(color, bottomLeft, topLeft);
    }

    public void DrawLineWorld(Color color, params Vector3[] verticesWorld)
    {
        if (verticesWorld == null || verticesWorld.Length < 2) return;
        foreach (var v in verticesWorld)
        {
            Vector3 p = _viewMatrix.Transform(v);
            if (p.Z < 1f) DrawLine(color, new Vector2(p.X, p.Y));
        }
    }

    public void DrawThickLine(Color color, Vector2 start, Vector2 end, float thickness)
{
    if (_device == null || start == end) return;

    var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
    Vector2 dir = end - start;
    float length = dir.Length();
    Vector2 norm = new Vector2(-dir.Y, dir.X) / length * (thickness * 0.5f);

    Vertex[] vertices = new Vertex[4];
    
    vertices[0] = new Vertex { Position = new Vector4(start.X - norm.X, start.Y - norm.Y, 0, 1.0f), Color = d3dColor };
    vertices[1] = new Vertex { Position = new Vector4(start.X + norm.X, start.Y + norm.Y, 0, 1.0f), Color = d3dColor };
    vertices[2] = new Vertex { Position = new Vector4(end.X - norm.X, end.Y - norm.Y, 0, 1.0f), Color = d3dColor };
    vertices[3] = new Vertex { Position = new Vector4(end.X + norm.X, end.Y + norm.Y, 0, 1.0f), Color = d3dColor };

    
    _device.SetRenderState(RenderState.AlphaBlendEnable, true);
    _device.SetRenderState(RenderState.CullMode, Cull.None); 
    _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
    _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertices);
}

    public void DrawFilledCircle(Color color, Vector2 center, float radius, int segments = 24)
    {
        if (_device == null) return;
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
        Vertex[] vertices = new Vertex[segments + 2];
        vertices[0] = new Vertex { Position = new Vector4(center.X, center.Y, 0, 1.0f), Color = d3dColor };
        for (int i = 0; i <= segments; i++)
        {
            float theta = i * MathUtil.TwoPi / segments;
            vertices[i + 1] = new Vertex { Position = new Vector4(center.X + radius * (float)Math.Cos(theta), center.Y + radius * (float)Math.Sin(theta), 0, 1.0f), Color = d3dColor };
        }
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.TriangleFan, segments, vertices);
    }

    private void RenderVertices()
    {
        if (_vertexCount == 0 || _device == null) return;
        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.LineList, _vertexCount / 2, _vertexArray);
    }

    private (Vector2, Vector2)? GetEntityBoundingBox(Entity entity)
    {
        if (entity.BonePos == null || !entity.BonePos.TryGetValue("head", out var head3D) ||
            !entity.BonePos.TryGetValue("pelvis", out var pelvis3D)) return null;
        Vector3 headPos = _viewMatrix.Transform(head3D + new Vector3(0, 0, 8f));
        Vector3 feetPos = _viewMatrix.Transform(pelvis3D - new Vector3(0, 0, 38f));
        if (headPos.Z >= 1 || feetPos.Z >= 1) return null;
        float height = Math.Abs(feetPos.Y - headPos.Y);
        return (new Vector2(feetPos.X - (height / 3.6f), headPos.Y), new Vector2(feetPos.X + (height / 3.6f), feetPos.Y));
    }

    private void DisposeResources()
    {
        FontAzonix64?.Dispose(); FontConsolas32?.Dispose(); Gamesense?.Dispose();
        WeaponIcons?.Dispose(); BombTime?.Dispose(); Undefeated?.Dispose();
        _line?.Dispose(); _device?.Dispose();
    }

    protected override void FrameAction()
    {
        if (!GameProcess.IsValid) return;
        if ((int)_res.X != WindowOverlay.Window.Width || (int)_res.Y != WindowOverlay.Window.Height)
        {
            _res = new Vector2(WindowOverlay.Window.Width, WindowOverlay.Window.Height);
            lock (_deviceLock) { DisposeResources(); InitializeDevice(); }
        }
        RenderFrame();
    }
}