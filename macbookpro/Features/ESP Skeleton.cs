using CS2Cheat.Data.Entity;
using SharpDX;
using System.Collections.Generic;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class SkeletonEsp
{

    private static readonly (string s, string e)[] Bones =
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

        Color color = entity.Team == CS2Cheat.Core.Data.Team.Terrorists ? Color.White : Color.White;

        foreach (var connection in Bones)
        {
            if (entity.BonePos.TryGetValue(connection.s, out Vector3 start) &&
                entity.BonePos.TryGetValue(connection.e, out Vector3 end))
            {

                graphics.DrawLineWorld(color, start, end);
            }
        }
    }
}