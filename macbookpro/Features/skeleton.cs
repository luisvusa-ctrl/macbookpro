using CS2Cheat.Data.Entity;
using SharpDX;
using System.Collections.Generic;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class SkeletonEsp
{
    
    private static readonly string[][] BoneConnections = new string[][]
    {
        new[] { "head", "neck_0" },
        new[] { "neck_0", "spine_2" },
        new[] { "spine_2", "pelvis" },
        new[] { "neck_0", "arm_upper_L" },
        new[] { "arm_upper_L", "arm_lower_L" },
        new[] { "arm_lower_L", "hand_L" },
        new[] { "neck_0", "arm_upper_R" },
        new[] { "arm_upper_R", "arm_lower_R" },
        new[] { "arm_lower_R", "hand_R" },
        new[] { "pelvis", "leg_upper_L" },
        new[] { "leg_upper_L", "leg_lower_L" },
        new[] { "leg_lower_L", "ankle_L" },
        new[] { "pelvis", "leg_upper_R" },
        new[] { "leg_upper_R", "leg_lower_R" },
        new[] { "leg_lower_R", "ankle_R" }
    };

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Matrix viewMatrix)
    {
        if (entity.Health <= 0 || entity.BonePos == null) return;

        Color skeletonColor = Color.White; 
        float thickness = 2.0f; 

        foreach (var connection in BoneConnections)
        {
            if (entity.BonePos.TryGetValue(connection[0], out var start3D) &&
                entity.BonePos.TryGetValue(connection[1], out var end3D))
            {
                Vector2 b1 = graphics.WorldToScreen(start3D, viewMatrix);
                Vector2 b2 = graphics.WorldToScreen(end3D, viewMatrix);

                if (b1 != Vector2.Zero && b2 != Vector2.Zero)
                {
                    
                    graphics.DrawSmoothLine(b1, b2, thickness, skeletonColor);
                }
            }
        }
    }
}