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
    
    { 1, "\uE001" }, // deagle
    { 2, "\uE002" }, // dualies
    { 3, "\uE003" }, // five-seven
    { 4, "\uE004" }, // glock
    { 32, "\uE020" }, // p2000
    { 36, "\uE024" }, // p250
    { 61, "\uE03D" }, // usp-s
    { 63, "\uE03F" }, // cz75-a
    { 64, "\uE040" }, // r8

   
    { 7, "\uE007" }, // ak-47
    { 8, "\uE008" }, // aug
    { 10, "\uE00A" }, // famas (o ícone selecionado no seu print)
    { 13, "\uE00D" }, // galil
    { 16, "\uE00E" }, // m4a4
    { 60, "\uE010" }, // m4a1-s
    { 39, "\uE027" }, // sg 553

    
    { 9, "\uE009" },  // awp
    { 11, "\uE00B" }, // g3sg1
    { 38, "\uE026" }, // scar-20
    { 40, "\uE028" }, // ssg 08

   
    { 17, "\uE011" }, // mac-10
    { 19, "\uE013" }, // p90
    { 23, "\uE017" }, // mp5-sd
    { 24, "\uE018" }, // ump-45
    { 26, "\uE01A" }, // bizon
    { 34, "\uE022" }, // mp9
    { 14, "\uE00E" }, // m249
    { 25, "\uE019" }, // xm1014
    { 35, "\uE023" }, // nova

    
    { 31, "\uE01F" }, // zeus
    { 42, "\uE02A" }, // knife
    { 59, "\uE03B" }, // knife t
    { 49, "\uE031" }, // c4
    { 43, "\uE02B" }, // flash
    { 44, "\uE02C" }, // he grenade
    { 45, "\uE02D" }, // smoke
    { 46, "\uE02E" }  // molotov
};

    public static string GetNameById(int id) => WeaponNames.TryGetValue(id, out var name) ? name : "knife";

    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        var fontText = graphics.Gamesense;
        var fontIcons = graphics.WeaponIcons;
        if (fontText == null || fontIcons == null || string.IsNullOrEmpty(entity.CurrentWeaponName)) return;


        string nameStr = entity.CurrentWeaponName.ToLower();
        var textSize = fontText.MeasureText(null, nameStr, FontDrawFlags.Center);
        int textWidth = textSize.Right - textSize.Left;

        int textX = (int)((topLeft.X + bottomRight.X) / 2 - (textWidth / 2));
        int textY = (int)(bottomRight.Y + 6f);


        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                fontText.DrawText(null, nameStr, textX + i, textY + j, Color.Black);
            }
        }
        fontText.DrawText(null, nameStr, textX, textY, Color.White);


        if (WeaponIcons.TryGetValue(entity.WeaponId, out var iconChar))
        {
            var iconSize = fontIcons.MeasureText(null, iconChar, FontDrawFlags.Center);
            int iconWidth = iconSize.Right - iconSize.Left;

            int iconX = (int)((topLeft.X + bottomRight.X) / 2 - (iconWidth / 2));
            int iconY = textY + 8;

            fontIcons.DrawText(null, iconChar, iconX + 1, iconY + 1, Color.Black);

            fontIcons.DrawText(null, iconChar, iconX, iconY, Color.White);
        }
    }
}