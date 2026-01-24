using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public class EspPlayerName
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        var name = entity.Name ?? "UNKNOWN";
        var font = graphics.FontConsolas32;
        if (font == null) return;

        var textSize = font.MeasureText(null, name, FontDrawFlags.Center);
        int textWidth = textSize.Right - textSize.Left;

        int nameX = (int)((topLeft.X + bottomRight.X) / 2 - (textWidth / 2));
        int nameY = (int)(topLeft.Y - 12f);

        // --- CONTORNO SUAVE (Para fonte lisa) ---
        // Desenhamos apenas uma vez com um pequeno deslocamento em preto
        // Isso cria uma sombra que destaca a fonte sem deixá-la "suja"
        font.DrawText(null, name, nameX + 1, nameY + 1, new Color(0, 0, 0, 200));

        // --- TEXTO PRINCIPAL (BRANCO) ---
        font.DrawText(null, name, nameX, nameY, Color.White);
    }
}