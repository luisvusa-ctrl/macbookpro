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

    public static void Draw(Graphics.Graphics graphics, Entity entity, Vector3 localPos)
    {
        if (entity == null || !entity.IsAlive()) return;

        float distance = Vector3.Distance(localPos, entity.Position);

        
        bool isOnGround = (entity.Flags & 1) != 0;
        bool isMoving = entity.Velocity > 135f;

        if (isOnGround && isMoving && distance < 1100f)
        {
            AddRipple(entity.Position, Color.White);
        }

        
        if (entity.JustLanded && distance < 1600f)
        {
            AddRipple(entity.Position, Color.Orange);
        }

        UpdateAndDrawRipples(graphics);
    }

    private static void AddRipple(Vector3 worldPos, Color color)
    {
        
        if (Ripples.Count > 0 && (DateTime.Now - Ripples[^1].CreationTime).TotalMilliseconds < 200) return;

        Ripples.Add(new SoundRipple
        {
            WorldPosition = worldPos,
            Radius = 0f,
            MaxRadius = 30f,
            Alpha = 160, 
            Color = color,
            CreationTime = DateTime.Now
        });
    }

    private static void UpdateAndDrawRipples(Graphics.Graphics graphics)
    {
        for (int i = Ripples.Count - 1; i >= 0; i--)
        {
            var ripple = Ripples[i];
            float lifeTime = (float)(DateTime.Now - ripple.CreationTime).TotalSeconds;
            float duration = 0.7f; 

            if (lifeTime >= duration)
            {
                Ripples.RemoveAt(i);
                continue;
            }

            
            float progress = lifeTime / duration;
            
            float easeOut = 1f - MathF.Pow(1f - progress, 3);

            float currentRadius = easeOut * ripple.MaxRadius;
            int currentAlpha = (int)(ripple.Alpha * (1f - progress));

            Draw3DStepClean(graphics, ripple.WorldPosition, currentRadius,
                           new Color(ripple.Color.R, ripple.Color.G, ripple.Color.B, currentAlpha));
        }
    }

    private static void Draw3DStepClean(Graphics.Graphics graphics, Vector3 center, float radius, Color color)
    {
        const int segments = 80; 
        double step = 2 * Math.PI / segments;
        Vector2 lastPoint = Vector2.Zero;

        
        Color softColor = new Color(color.R, color.G, color.B, color.A / 2);

        for (int i = 0; i <= segments; i++)
        {
            float theta = (float)(i * step);
            Vector3 worldPoint = new Vector3(
                center.X + (radius * MathF.Cos(theta)),
                center.Y + (radius * MathF.Sin(theta)),
                center.Z - 1.5f // 
            );

            
            Vector3 screenPos = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(worldPoint);

            if (screenPos.Z < 1)
            {
                Vector2 currentPoint = new Vector2(screenPos.X, screenPos.Y);

                if (i > 0)
                {
                    
                    graphics.DrawLine(color, lastPoint, currentPoint);

                    
                    graphics.DrawLine(softColor,
                        new Vector2(lastPoint.X + 0.5f, lastPoint.Y + 0.5f),
                        new Vector2(currentPoint.X + 0.5f, currentPoint.Y + 0.5f));
                }
                lastPoint = currentPoint;
            }
        }
    }

    private class SoundRipple
    {
        public Vector3 WorldPosition;
        public float Radius, MaxRadius;
        public int Alpha;
        public Color Color;
        public DateTime CreationTime;
    }
}