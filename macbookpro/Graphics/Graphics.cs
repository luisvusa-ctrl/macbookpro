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
        
        using var d3d = new Direct3D();

        var parameters = new PresentParameters
        {
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            DeviceWindowHandle = WindowOverlay.Window.Handle,
            BackBufferFormat = Format.A8R8G8B8,
            BackBufferWidth = (int)_res.X,
            BackBufferHeight = (int)_res.Y,
            PresentationInterval = PresentInterval.Immediate,
            EnableAutoDepthStencil = false,         
            MultiSampleType = 0,
            MultiSampleQuality = 0
        };

        _device = new Device(new Direct3D(), 0, DeviceType.Hardware, WindowOverlay.Window.Handle,
            CreateFlags.HardwareVertexProcessing, parameters);

        _device.SetRenderState(RenderState.AntialiasedLineEnable, true);
        _device.SetRenderState(RenderState.MultisampleAntialias, true);
        _device.SetRenderState(RenderState.SeparateAlphaBlendEnable, true);
        _device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        _device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

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
                Vector2 namePos = new Vector2(centerX, topLeft.Y - 15);
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

            if (features.SkeletonEsp)
            {
                SkeletonEsp.Draw(this, entity, _viewMatrix);
            }
            if (features.EspSnaplines) EspSnaplines.Draw(this, entity);
        }
    }

    
    public void DrawLine(Color color, Vector2 start, Vector2 end)
    {
        DrawThickLine(color, start, end, 1.2f);
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

    public void DrawHDLine(Vector2 start, Vector2 end, float width, Color color)
    {
        if (_device == null || start == end) return;

        
        Vector2 s = new Vector2(start.X + 0.5f, start.Y + 0.5f);
        Vector2 e = new Vector2(end.X + 0.5f, end.Y + 0.5f);

        Vector2 dir = e - s;
        float len = dir.Length();
        if (len < 0.1f) return;

        Vector2 norm = new Vector2(-dir.Y, dir.X) / len * (width * 0.5f);

        
        Vertex[] verts = new Vertex[4];
        uint c = (uint)color.ToBgra();

        verts[0] = new Vertex { Position = new Vector4(s.X - norm.X, s.Y - norm.Y, 0, 1.0f), Color = new ColorBGRA(c) };
        verts[1] = new Vertex { Position = new Vector4(s.X + norm.X, s.Y + norm.Y, 0, 1.0f), Color = new ColorBGRA(c) };
        verts[2] = new Vertex { Position = new Vector4(e.X - norm.X, e.Y - norm.Y, 0, 1.0f), Color = new ColorBGRA(c) };
        verts[3] = new Vertex { Position = new Vector4(e.X + norm.X, e.Y + norm.Y, 0, 1.0f), Color = new ColorBGRA(c) };

        _device.SetRenderState(RenderState.AntialiasedLineEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, verts);
    }
    public void DrawHitboxBuffer(Vector3 min, Vector3 max, Matrix matrix, Matrix viewMatrix, Color color)
    {
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(min.X, min.Y, min.Y);
        corners[1] = new Vector3(max.X, min.Y, min.Z);
        corners[2] = new Vector3(max.X, max.Y, min.Z);
        corners[3] = new Vector3(min.X, max.Y, min.Z);
        corners[4] = new Vector3(min.X, min.Y, max.Z);
        corners[5] = new Vector3(max.X, min.Y, max.Z);
        corners[6] = new Vector3(max.X, max.Y, max.Z);
        corners[7] = new Vector3(min.X, max.Y, max.Z);

        Vector2[] screenPoints = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            
            var worldPoint = Vector3.TransformCoordinate(corners[i], matrix);
            screenPoints[i] = WorldToScreen(worldPoint, viewMatrix);
            if (screenPoints[i] == Vector2.Zero) return; 
        }

        // Desenha as faces (Isso dá o efeito de preenchimento do Chams)
        DrawFilledPolygon(color, new[] { screenPoints[0], screenPoints[1], screenPoints[2], screenPoints[3] }); 
        DrawFilledPolygon(color, new[] { screenPoints[4], screenPoints[5], screenPoints[6], screenPoints[7] }); 
        DrawFilledPolygon(color, new[] { screenPoints[0], screenPoints[1], screenPoints[5], screenPoints[4] }); 
                                                                                                                
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

    public void DrawFilledRotatedRectangle(Color color, Vector2 center, float length, float width, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);

        Vector2 halfLen = new Vector2(length / 2f * cos, length / 2f * sin);
        Vector2 halfWid = new Vector2(width / 2f * -sin, width / 2f * cos);

        Vector2[] points = new Vector2[]
        {
        center - halfLen - halfWid,
        center + halfLen - halfWid,
        center + halfLen + halfWid,
        center - halfLen + halfWid
        };

        DrawFilledPolygon(color, points);
    }

    public void DrawSmoothLine(Vector2 start, Vector2 end, float width, Color color)
    {
        if (_device == null || _line == null) return;

        
        var p1 = new SharpDX.Mathematics.Interop.RawVector2(start.X + 0.5f, start.Y + 0.5f);
        var p2 = new SharpDX.Mathematics.Interop.RawVector2(end.X + 0.5f, end.Y + 0.5f);

        _line.Width = width;
        _line.Antialias = true; 
        _line.Begin();
        _line.Draw(new[] { p1, p2 }, new SharpDX.Mathematics.Interop.RawColorBGRA(color.B, color.G, color.R, color.A));
        _line.End();
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
        if (length < 0.1f) return;

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

    public void DrawFilledPolygon(Color color, Vector2[] points)
    {
        if (points == null || points.Length < 3 || _device == null) return;

        
        int rawColor = color.ToBgra();
        Vertex[] vertices = new Vertex[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vertex
            {
                Position = new Vector4(points[i].X, points[i].Y, 0.0f, 1.0f),
                
                Color = new ColorBGRA(rawColor)
            };
        }

        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        _device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

        _device.DrawUserPrimitives(PrimitiveType.TriangleFan, points.Length - 2, vertices);
    }
    public void DrawPolygon(Color color, Vector2[] points)
    {
        if (points == null || points.Length < 2) return;

        
        for (int i = 0; i < points.Length - 1; i++)
        {
            DrawLine(color, points[i], points[i + 1]); 
        }
        
        DrawLine(color, points[points.Length - 1], points[0]);
    }

    private void RenderVertices()
    {
        if (_vertexCount == 0 || _device == null) return;
        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.LineList, _vertexCount / 2, _vertexArray);
    }

    public void DrawCircle(Color color, Vector2 centralPoint, float radius, int segments = 16)
{
    if (_device == null) return;

    Vector2[] points = new Vector2[segments + 1];
    for (int i = 0; i <= segments; i++)
    {
        float theta = (float)(i * 2 * Math.PI / segments);
        points[i] = new Vector2(
            centralPoint.X + (float)Math.Cos(theta) * radius,
            centralPoint.Y + (float)Math.Sin(theta) * radius
        );
    }

    
    for (int i = 0; i < segments; i++)
    {
        DrawLine(color, points[i], points[i + 1]);
    }
}

    public Vector2 WorldToScreen(Vector3 worldPos, Matrix viewMatrix)
    {
        Vector3 transform = viewMatrix.Transform(worldPos);
        if (transform.Z >= 1.0f) return Vector2.Zero;

        return new Vector2(transform.X, transform.Y);
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