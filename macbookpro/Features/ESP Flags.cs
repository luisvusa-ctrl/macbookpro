using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

namespace CS2Cheat.Features;

public class EspFlags
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        var font = graphics.Gamesense;
        if (font == null) return;

        float xOffset = bottomRight.X + 4f;
        float yOffset = topLeft.Y;
        float spacing = 9f;

        // --- FLAG: DINHEIRO ($) ---
        if (entity.Money > 0)
            DrawFlag(font, $"${entity.Money}", xOffset, ref yOffset, spacing, Color.LightGreen);

        // --- FLAG: HK (Armor/Capacete) ---
        if (entity.ArmorValue > 0)
            DrawFlag(font, entity.HasHelmet ? "HK" : "K", xOffset, ref yOffset, spacing, Color.White);

        // --- FLAG: ZOOM (Scoped) ---
        if (entity.IsScoped)
            DrawFlag(font, "ZOOM", xOffset, ref yOffset, spacing, Color.Cyan);

    }

    private static void DrawFlag(Font font, string text, float x, ref float y, float spacing, Color color)
    {
        var black = Color.Black;

        // --- DESENHAR O CONTORNO "PESADO" (8 DIREÇÕES) ---
        // Isso preenche os cantos diagonais, deixando a borda visualmente mais grossa
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Pula o centro (onde vai a cor principal)
                if (i == 0 && j == 0) continue;

                font.DrawText(null, text, (int)x + i, (int)y + j, black);
            }
        }

        // --- TEXTO PRINCIPAL ---
        font.DrawText(null, text, (int)x, (int)y, color);

        // Pula para a próxima linha
        y += spacing;
    }
}