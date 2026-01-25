using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspChams
{
    // Conexões do Esqueleto para desenhar por cima do Chams
    private static readonly (string, string)[] SkeletonConnections = new[]
    {
        ("head", "neck_0"), ("neck_0", "spine_2"), ("spine_2", "spine_1"), ("spine_1", "pelvis"),
        ("neck_0", "arm_upper_L"), ("arm_upper_L", "arm_lower_L"), ("arm_lower_L", "hand_L"),
        ("neck_0", "arm_upper_R"), ("arm_upper_R", "arm_lower_R"), ("arm_lower_R", "hand_R"),
        ("pelvis", "leg_upper_L"), ("leg_upper_L", "leg_lower_L"), ("leg_lower_L", "ankle_L"),
        ("pelvis", "leg_upper_R"), ("leg_upper_R", "leg_lower_R"), ("leg_lower_R", "ankle_R")
    };

    // Malha do Chams (Volumes)
    private static readonly (string S, string E, float rS, float rE)[] ModelMesh = new[]
    {
        ("head", "neck_0", 7.8f, 6.5f), ("neck_0", "spine_2", 6.5f, 11.0f),
        ("spine_2", "spine_1", 11.0f, 12.0f), ("spine_1", "pelvis", 12.0f, 13.0f),
        ("neck_0", "arm_upper_L", 6.5f, 6.8f), ("neck_0", "arm_upper_R", 6.5f, 6.8f),
        ("arm_upper_L", "spine_2", 6.8f, 11.0f), ("arm_upper_R", "spine_2", 6.8f, 11.0f),
        ("arm_upper_L", "arm_lower_L", 6.0f, 4.8f), ("arm_lower_L", "hand_L", 4.8f, 3.5f),
        ("arm_upper_R", "arm_lower_R", 6.0f, 4.8f), ("arm_lower_R", "hand_R", 4.8f, 3.5f),
        ("pelvis", "leg_upper_L", 13.0f, 9.0f), ("pelvis", "leg_upper_R", 13.0f, 9.0f),
        ("leg_upper_L", "leg_lower_L", 9.0f, 7.0f), ("leg_lower_L", "ankle_L", 7.0f, 5.5f),
        ("leg_upper_R", "leg_lower_R", 9.0f, 7.0f), ("leg_lower_R", "ankle_R", 7.0f, 5.5f)
    };

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity)
    {
        // 1. FILTRO ANTI-BORRÃO (O segredo para não aparecer manchas na tela)
        if (entity == null || !entity.IsAlive() || entity.BonePos == null) return;

        if (!entity.BonePos.TryGetValue("head", out var headW) ||
            !entity.BonePos.TryGetValue("pelvis", out var pelvisW)) return;

        // Se as coordenadas de mundo forem 0,0,0, é um inimigo inválido. Aborta.
        if (headW == Vector3.Zero || pelvisW == Vector3.Zero) return;

        Vector3 headS = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(headW);
        Vector3 pelvisS = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(pelvisW);

        if (headS.Z >= 1 || pelvisS.Z >= 1) return;

        float dist2D = Vector2.Distance(new Vector2(headS.X, headS.Y), new Vector2(pelvisS.X, pelvisS.Y));
        float masterScale = dist2D / 38.0f;

        // Cores
        Color chamsColor = entity.IsSpotted ? new Color(154, 255, 0, 170) : new Color(120, 0, 255, 70);
        Color skeletonColor = new Color(255, 255, 255, 150); // Branco suave

        // --- PASSO 1: DESENHAR O CHAMS (VOLUME) ---
        foreach (var seg in ModelMesh)
        {
            if (entity.BonePos.TryGetValue(seg.S, out var startW) && entity.BonePos.TryGetValue(seg.E, out var endW))
            {
                if (startW == Vector3.Zero || endW == Vector3.Zero) continue;

                Vector3 sS = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(startW);
                Vector3 eS = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(endW);

                DrawPhantomSegment(graphics, new Vector2(sS.X, sS.Y), new Vector2(eS.X, eS.Y), seg.rS * masterScale, seg.rE * masterScale, chamsColor);
            }
        }

        // --- PASSO 2: DESENHAR O ESQUELETO (DEFINIÇÃO) ---
        // Desenhamos por último para ele ficar Visível por cima da cor do Chams
        foreach (var (b1, b2) in SkeletonConnections)
        {
            if (entity.BonePos.TryGetValue(b1, out var w1) && entity.BonePos.TryGetValue(b2, out var w2))
            {
                if (w1 == Vector3.Zero || w2 == Vector3.Zero) continue;

                Vector3 s1 = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(w1);
                Vector3 s2 = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(w2);

                graphics.DrawLine(skeletonColor, new Vector2(s1.X, s1.Y), new Vector2(s2.X, s2.Y));
            }
        }
    }

    private static void DrawPhantomSegment(CS2Cheat.Graphics.Graphics graphics, Vector2 start, Vector2 end, float rStart, float rEnd, Color color)
    {
        float distance = Vector2.Distance(start, end);
        float stepSize = Math.Max(0.4f, ((rStart + rEnd) / 2f) * 0.1f);
        int steps = (int)(distance / stepSize);
        steps = Math.Min(steps, 80);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            graphics.DrawFilledCircle(color, Vector2.Lerp(start, end, t), MathUtil.Lerp(rStart, rEnd, t));
        }
    }
}