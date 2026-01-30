using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using System.Collections.Generic;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class SkeletonEsp
{
    private static readonly (string s, string e)[] BoneConnections =
    {
        ("head", "neck_0"), ("neck_0", "spine_1"), ("spine_1", "spine_2"), ("spine_2", "pelvis"),
        ("spine_1", "arm_upper_L"), ("arm_upper_L", "arm_lower_L"), ("arm_lower_L", "hand_L"),
        ("spine_1", "arm_upper_R"), ("arm_upper_R", "arm_lower_R"), ("arm_lower_R", "hand_R"),
        ("pelvis", "leg_upper_L"), ("leg_upper_L", "leg_lower_L"), ("leg_lower_L", "ankle_L"),
        ("pelvis", "leg_upper_R"), ("leg_upper_R", "leg_lower_R"), ("leg_lower_R", "ankle_R")
    };

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity)
    {
        if (entity.BonePos == null || entity.BonePos.Count == 0) return;

        
        Color skeletonColor = new Color(255, 170, 255, 255);
        float thickness = 2.0f; 
        var viewMatrix = graphics.GameData.Player.MatrixViewProjectionViewport;

        foreach (var connection in BoneConnections)
        {
            if (entity.BonePos.TryGetValue(connection.s, out Vector3 startWorld) &&
                entity.BonePos.TryGetValue(connection.e, out Vector3 endWorld))
            {
                Vector3 startScreen = viewMatrix.Transform(startWorld);
                Vector3 endScreen = viewMatrix.Transform(endWorld);

                
                if (startScreen.Z < 1.0f && endScreen.Z < 1.0f)
                {
                    Vector2 s = new Vector2(startScreen.X, startScreen.Y);
                    Vector2 e = new Vector2(endScreen.X, endScreen.Y);

                    // 1. Desenha a linha sólida
                    graphics.DrawThickLine(skeletonColor, s, e, thickness);

                    
                    graphics.DrawFilledRectangle(skeletonColor, s - new Vector2(1, 1), s + new Vector2(1, 1));
                }
            }
        }
    } 
} 