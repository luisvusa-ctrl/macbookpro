using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using System.Collections.Generic;
using Color = SharpDX.Color;

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
        { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" },
        { 7, "W" }, { 8, "U" }, { 9, "Z" }, { 10, "R" },
        { 11, "X" }, { 13, "Q" }, { 14, "g" }, { 16, "S" },
        { 17, "K" }, { 19, "P" }, { 23, "N" }, { 24, "L" },
        { 25, "b" }, { 26, "M" }, { 31, "h" }, { 32, "E" },
        { 34, "T" }, { 35, "e" }, { 36, "F" }, { 38, "Y" },
        { 39, "V" }, { 40, "a" }, { 42, "]" }, { 59, "[" },
        { 60, "T" }, { 61, "G" }, { 63, "I" }, { 64, "J" },
        { 49, "o" }, { 43, "i" }, { 44, "j" }, { 45, "k" }, { 46, "l" }
    };

    public static string GetNameById(int id) => WeaponNames.TryGetValue(id, out var name) ? name : "knife";

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        var fontText = graphics.FontConsolas32;
        var fontIcons = graphics.Undefeated;
        if (fontText == null || fontIcons == null || string.IsNullOrEmpty(entity.CurrentWeaponName)) return;

        // --- 1. DESENHAR O NOME (TEXTO) ---
        string nameStr = entity.CurrentWeaponName.ToLower();
        var textSize = fontText.MeasureText(null, nameStr, FontDrawFlags.Center);
        int textWidth = textSize.Right - textSize.Left;

        int textX = (int)((topLeft.X + bottomRight.X) / 2 - (textWidth / 2));
        int textY = (int)(bottomRight.Y + 2f); // Logo abaixo da box

        fontText.DrawText(null, nameStr, textX + 1, textY + 1, Color.Black);
        fontText.DrawText(null, nameStr, textX, textY, Color.White);

        // --- 2. DESENHAR O ÍCONE ---
        if (WeaponIcons.TryGetValue(entity.WeaponId, out var iconChar))
        {
            var iconSize = fontIcons.MeasureText(null, iconChar, FontDrawFlags.Center);
            int iconWidth = iconSize.Right - iconSize.Left;

            int iconX = (int)((topLeft.X + bottomRight.X) / 2 - (iconWidth / 2));
            // O Y do ícone é o Y do texto + a altura da fonte do texto (geralmente 10-12px)
            int iconY = textY + 11;

            fontIcons.DrawText(null, iconChar, iconX + 1, iconY + 1, Color.Black);
            fontIcons.DrawText(null, iconChar, iconX, iconY, Color.White);
        }
    }
}