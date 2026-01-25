using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspSound
{
    private static readonly List<SoundRipple> Ripples = new();

    public static void Draw(Graphics.Graphics graphics, Entity entity, Vector2 screenPos, Vector3 localPos)
    {
        if (entity == null || !entity.IsAlive() || entity.AddressBase == IntPtr.Zero) return;

        float distance = Vector3.Distance(localPos, entity.Position);

        // --- AJUSTE DE DISTÂNCIA REALISTA ---
        bool isOnGround = (entity.Flags & 1) != 0;
        bool isMoving = entity.Velocity > 138f;
        bool isDucking = (entity.Flags & 4) != 0;

        // 1. Passos: Só avisa se estiver a menos de 950 unidades (limite auditivo do CS2)
        if (isOnGround && !isDucking && isMoving && distance < 950f)
        {
            AddRipple(entity.Position, localPos, Color.Yellow);
        }

        // 2. Quedas: Mais barulhentas, limite de 1400 unidades
        if (entity.JustLanded && distance < 1400f)
        {
            AddRipple(entity.Position, localPos, Color.Orange, true);
        }

        UpdateAndDrawRipples(graphics, localPos);
    }

    private static void AddRipple(Vector3 worldPos, Vector3 localPos, Color color, bool isLanding = false)
    {
        if (Ripples.Count > 15) Ripples.RemoveAt(0);
        float dist = Vector3.Distance(localPos, worldPos);

        Ripples.Add(new SoundRipple
        {
            WorldPosition = worldPos,
            Radius = 2f,
            Alpha = (int)MathUtil.Clamp(255 - (dist / 6), 30, 255), // Alpha mais agressivo com a distância
            MaxRadius = isLanding ? 45f : 20f,
            Color = color,
            Speed = isLanding ? 2.5f : 1.5f
        });
    }

    private static void UpdateAndDrawRipples(Graphics.Graphics graphics, Vector3 localPos)
    {
        for (int i = Ripples.Count - 1; i >= 0; i--)
        {
            var ripple = Ripples[i];
            ripple.Radius += ripple.Speed;
            ripple.Alpha -= 5;

            if (ripple.Alpha <= 0) { Ripples.RemoveAt(i); continue; }

            Vector3 screenPos3D = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(ripple.WorldPosition);
            if (screenPos3D.Z < 1)
            {
                graphics.DrawCircle(new Color(ripple.Color.R, ripple.Color.G, ripple.Color.B, ripple.Alpha),
                                   new Vector2(screenPos3D.X, screenPos3D.Y), ripple.Radius);
            }
        }
    }

    private class SoundRipple { public Vector3 WorldPosition; public float Radius, MaxRadius, Speed; public int Alpha; public Color Color; }
}