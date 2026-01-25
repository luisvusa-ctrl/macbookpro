using CS2Cheat.Core.Data;
using CS2Cheat.Data.Entity;
using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Utils;
using GameOverlay.Drawing;
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

    // Garante que usamos a Line do DirectX para evitar conflito com GameOverlay
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

        Gamesense = new Font(_device, new FontDescription
        {
            Height = 11,
            FaceName = "Smallest Pixel-7",
            Weight = FontWeight.Normal,
            MipLevels = 1,
            Italic = false,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.Antialiased
        });

        FontConsolas32 = new Font(_device, new FontDescription
        {
            Height = 12,
            FaceName = "Tahoma",
            Weight = FontWeight.Bold,
            Quality = FontQuality.ClearType
        });

        WeaponIcons = new Font(_device, new FontDescription
        {
            Height = 15,
            FaceName = "csgo_icons",
            Weight = FontWeight.Normal,
            Quality = FontQuality.Antialiased
        });

        Undefeated = new Font(_device, new FontDescription { Height = 12, FaceName = "undefeated" });

        try
        {
            _line = new SharpDX.Direct3D9.Line(_device)
            {
                Width = 2.0f,
                Antialias = true
            };
        }
        catch { _line = null; }
    }

    private void RenderFrame()
    {
        lock (_deviceLock)
        {
            if (_device == null) return;

            _device.Clear(ClearFlags.Target, Color.FromAbgr(0), 1, 0);
            _device.BeginScene();

            _vertexCount = 0;

            GameData.UpdateViewMatrix();
            if (GameData.Player != null)
                _viewMatrix = GameData.Player.MatrixViewProjectionViewport;

            DrawFeatures();
            RenderVertices();

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

    // --- CORREÇÃO PRINCIPAL AQUI ---
    private void DrawPlayerEsp(ConfigManager features)
    {
        var player = GameData.Player;
        if (player == null || GameData.Entities == null) return;

        // Lemos a posição do LocalPlayer direto da memória para não depender da classe Player
        // Isso resolve o erro de "Player does not contain definition for Position"
        Vector3 localPos = GameProcess.Process.Read<Vector3>(player.AddressBase + Offsets.m_vOldOrigin);

        foreach (var entity in GameData.Entities)
        {
            if (!entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;
            if (features.TeamCheck && entity.Team == player.Team) continue;

            var boundingBox = GetEntityBoundingBox(entity);
            if (boundingBox == null) continue;

            Vector2 topLeft = boundingBox.Value.Item1;
            Vector2 bottomRight = boundingBox.Value.Item2;

            if (features.EspBox) EspBox.Draw(this, entity, topLeft, bottomRight);
            if (features.EspHealthBar) EspHealthBar.Draw(this, entity, topLeft, bottomRight);
            if (features.EspPlayerName) EspPlayerName.Draw(this, entity, topLeft, bottomRight);
            if (features.EspAmmoBar) EspAmmoBar.Draw(this, entity, topLeft, bottomRight);
            if (features.SkeletonEsp) SkeletonEsp.Draw(this, entity);
            if (features.EspFlags) EspFlags.Draw(this, entity, topLeft, bottomRight);
            if (features.EspWeapon) EspWeapon.Draw(this, entity, topLeft, bottomRight);

            if (features.EspSound)
            {
                Vector2 footPos = new Vector2((topLeft.X + bottomRight.X) / 2.0f, bottomRight.Y);
                                
                EspSound.Draw(this, entity, footPos, localPos);
            }
        }
    }

    private (Vector2, Vector2)? GetEntityBoundingBox(Entity entity)
    {
        if (entity.BonePos == null || !entity.BonePos.TryGetValue("head", out var head3D) ||
            !entity.BonePos.TryGetValue("pelvis", out var pelvis3D)) return null;

        Vector3 headPos = _viewMatrix.Transform(head3D + new Vector3(0, 0, 8f));
        Vector3 feetPos = _viewMatrix.Transform(pelvis3D - new Vector3(0, 0, 38f));

        if (headPos.Z >= 1 || feetPos.Z >= 1) return null;

        float height = Math.Abs(feetPos.Y - headPos.Y);
        float width = height / 1.8f;

        return (new Vector2(feetPos.X - (width / 2), headPos.Y), new Vector2(feetPos.X + (width / 2), feetPos.Y));
    }

    public void DrawLineWorld(Color color, params Vector3[] verticesWorld)
    {
        if (verticesWorld == null || verticesWorld.Length < 2 || _device == null) return;

        if (_line != null)
        {
            var screenPoints = new List<Vector2>();
            foreach (var v in verticesWorld)
            {
                Vector3 p = _viewMatrix.Transform(v);
                if (p.Z < 1f) screenPoints.Add(new Vector2(p.X, p.Y));
            }

            if (screenPoints.Count < 2) return;

            _line.Begin();
            _line.Draw(screenPoints.ToArray(), color);
            _line.End();
        }
        else
        {
            DrawLineWorldRaw(color, verticesWorld);
        }
    }

    private void DrawLineWorldRaw(Color color, params Vector3[] verticesWorld)
    {
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);
        for (int i = 0; i < verticesWorld.Length - 1; i++)
        {
            Vector3 start = _viewMatrix.Transform(verticesWorld[i]);
            Vector3 end = _viewMatrix.Transform(verticesWorld[i + 1]);
            if (start.Z < 1 && end.Z < 1)
            {
                if (_vertexCount + 2 >= _vertexArray.Length) break;
                _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(start.X, start.Y, 0, 1.0f), Color = d3dColor };
                _vertexArray[_vertexCount++] = new Vertex { Position = new Vector4(end.X, end.Y, 0, 1.0f), Color = d3dColor };
            }
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

    // Métodos de Círculo para o Sound ESP
    public void DrawCircle(Color color, Vector2 center, float radius, int segments = 24)
    {
        if (_device == null) return;
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);

        Vertex[] vertices = new Vertex[segments + 1];
        float step = MathUtil.TwoPi / segments;

        for (int i = 0; i <= segments; i++)
        {
            float theta = i * step;
            float x = center.X + radius * (float)Math.Cos(theta);
            float y = center.Y + radius * (float)Math.Sin(theta);
            vertices[i] = new Vertex { Position = new Vector4(x, y, 0, 1.0f), Color = d3dColor };
        }

        if (_vertexCount + vertices.Length >= _vertexArray.Length)
        {
            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
            _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            _device.DrawUserPrimitives(PrimitiveType.LineStrip, segments, vertices);
            return;
        }

        for (int i = 0; i < segments; i++)
        {
            _vertexArray[_vertexCount++] = vertices[i];
            _vertexArray[_vertexCount++] = vertices[i + 1];
        }
    }

    public void DrawFilledCircle(Color color, Vector2 center, float radius, int segments = 24)
    {
        if (_device == null) return;
        var d3dColor = new ColorBGRA(color.R, color.G, color.B, color.A);

        Vertex[] vertices = new Vertex[segments + 2];
        vertices[0] = new Vertex { Position = new Vector4(center.X, center.Y, 0, 1.0f), Color = d3dColor };

        float step = MathUtil.TwoPi / segments;
        for (int i = 0; i <= segments; i++)
        {
            float theta = i * step;
            float x = center.X + radius * (float)Math.Cos(theta);
            float y = center.Y + radius * (float)Math.Sin(theta);
            vertices[i + 1] = new Vertex { Position = new Vector4(x, y, 0, 1.0f), Color = d3dColor };
        }

        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.TriangleFan, segments, vertices);
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

    private void RenderVertices()
    {
        if (_vertexCount == 0 || _device == null) return;
        _device.SetRenderState(RenderState.AlphaBlendEnable, true);
        _device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        _device.DrawUserPrimitives(PrimitiveType.LineList, _vertexCount / 2, _vertexArray);
    }

    private void DisposeResources()
    {
        FontAzonix64?.Dispose();
        FontConsolas32?.Dispose();
        Undefeated?.Dispose();
        _line?.Dispose();
        _device?.Dispose();
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