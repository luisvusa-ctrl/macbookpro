using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using System.Collections.Generic;
using Color = SharpDX.Color;
using CS2Cheat.Utils;

namespace CS2Cheat.Features;

public class EspWeapon
{
    private static readonly Dictionary<int, string> WeaponNames = new()
    {
        { 1, "deagle" }, { 2, "dualies" }, { 3, "five-seven" }, { 4, "glock" },
        { 7, "ak-47" }, { 8, "aug" }, { 9, "awp" }, { 10, "famas" },
        { 11, "g3sg1" }, { 13, "galil" }, { 14, "m249" }, { 16, "m4a4" },
        { 17, "mac-10" }, { 19, "p90" }, { 23, "mp5-sd" }, { 24, "ump-45" },
        { 25, "xm1014" }, { 26, "bizon" }, { 31, "zeus" }, { 32, "p2000" },
        { 34, "mp9" }, { 35, "nova" }, { 36, "p250" }, { 38, "scar-20" },
        { 39, "sg 553" }, { 40, "ssg 08" }, { 42, "knife" }, { 59, "knife" },
        { 60, "m4a1-s" }, { 61, "usp-s" }, { 63, "cz75-a" }, { 64, "r8" },
        { 49, "c4" }, { 43, "flash" }, { 44, "he grenade" }, { 45, "smoke" }, { 46, "molotov" }
    };

    private static readonly Dictionary<int, string> WeaponIcons = new()
    {
        { 1, "\uE001" }, { 2, "\uE002" }, { 3, "\uE003" }, { 4, "\uE004" },
        { 32, "\uE020" }, { 36, "\uE024" }, { 61, "\uE03D" }, { 63, "\uE03F" }, { 64, "\uE040" },
        { 7, "\uE007" }, { 8, "\uE008" }, { 10, "\uE00A" }, { 13, "\uE00D" },
        { 16, "\uE00E" }, { 60, "\uE010" }, { 39, "\uE027" }, { 9, "\uE009" },
        { 11, "\uE00B" }, { 38, "\uE026" }, { 40, "\uE028" }, { 17, "\uE011" },
        { 19, "\uE013" }, { 23, "\uE017" }, { 24, "\uE018" }, { 26, "\uE01A" },
        { 34, "\uE022" }, { 14, "\uE00E" }, { 25, "\uE019" }, { 35, "\uE023" },
        { 31, "\uE01F" }, { 42, "\uE02A" }, { 59, "\uE03B" }, { 49, "\uE031" },
        { 43, "\uE02B" }, { 44, "\uE02C" }, { 45, "\uE02D" }, { 46, "\uE02E" }
    };

    public static string GetNameById(int id) => WeaponNames.TryGetValue(id, out var name) ? name : "knife";

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight, ConfigManager config)
    {
        if (config == null) return;

        int currentYOffset = 2;


        if (config.EspAmmoBar)
        {
            currentYOffset += 5; 
        }

        if (config.DrawWeaponText)
        {
            DrawWeaponTextAt(graphics, entity, topLeft, bottomRight, currentYOffset);

            currentYOffset += config.EspAmmoBar ? 7 : 9;
        }
        else
        {
            if (config.EspAmmoBar) currentYOffset -= 1;
        }

        if (config.DrawWeaponIcon)
        {
            DrawWeaponIconAt(graphics, entity, topLeft, bottomRight, currentYOffset);
        }
    }

    private static void DrawWeaponTextAt(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight, int yOffset)
    {
        var fontText = graphics.Gamesense;
        if (fontText == null || string.IsNullOrEmpty(entity.CurrentWeaponName)) return;

        string nameStr = entity.CurrentWeaponName.ToLower();
        var textSize = fontText.MeasureText(null, nameStr, FontDrawFlags.Center);
        int textWidth = textSize.Right - textSize.Left;

        int textX = (int)((topLeft.X + bottomRight.X) / 2 - (textWidth / 2));
        int textY = (int)(bottomRight.Y + yOffset);

        // Outline
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                fontText.DrawText(null, nameStr, textX + i, textY + j, Color.Black);
            }
        }
        fontText.DrawText(null, nameStr, textX, textY, Color.White);
    }

    private static void DrawWeaponIconAt(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight, int yOffset)
    {
        var fontIcons = graphics.WeaponIcons;
        if (fontIcons == null) return;

        if (WeaponIcons.TryGetValue(entity.WeaponId, out var iconChar))
        {
            var iconSize = fontIcons.MeasureText(null, iconChar, FontDrawFlags.Center);
            int iconWidth = iconSize.Right - iconSize.Left;

            int iconX = (int)((topLeft.X + bottomRight.X) / 2 - (iconWidth / 2));
            int iconY = (int)(bottomRight.Y + yOffset);

            // Shadow
            fontIcons.DrawText(null, iconChar, iconX + 1, iconY + 1, Color.Black);
            // Icon
            fontIcons.DrawText(null, iconChar, iconX, iconY, Color.White);
        }
    }
}