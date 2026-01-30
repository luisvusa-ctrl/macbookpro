using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

namespace CS2Cheat.Features;

public class EspFlags
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 position, Vector2 bottomRight)
    {
        var font = graphics.Gamesense;
        if (font == null) return;

        float x = position.X;
        float y = position.Y;
        float spacing = 10f;

        if (entity.Money > 0)
            DrawFlag(font, $"${entity.Money}", x, ref y, spacing, Color.LightGreen);

        if (entity.ArmorValue > 0)
            DrawFlag(font, entity.HasHelmet ? "HK" : "K", x, ref y, spacing, Color.White);

        if (entity.IsScoped)
            DrawFlag(font, "ZOOM", x, ref y, spacing, Color.Cyan);
    }

    private static void DrawFlag(Font font, string text, float x, ref float y, float spacing, Color color)
    {
        
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; 
                font.DrawText(null, text, (int)x + i, (int)y + j, Color.Black);
            }
        }

        
        font.DrawText(null, text, (int)x, (int)y, color);
        y += spacing;
    }
}