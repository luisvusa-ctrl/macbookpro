using CS2Cheat.Data.Entity;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public class EspPlayerName
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 position, Vector2 bottomRight)
    {
        var name = entity.Name ?? "UNKNOWN";
        var font = graphics.FontConsolas32;
        if (font == null) return;

        
        var textSize = font.MeasureText(null, name, FontDrawFlags.Center);
        int textWidth = textSize.Right - textSize.Left;

     
        int nameX = (int)(position.X - (textWidth / 2));
        int nameY = (int)position.Y;

        
        font.DrawText(null, name, nameX + 1, nameY + 1, new Color(0, 0, 0, 255));

        
        font.DrawText(null, name, nameX, nameY, Color.White);
    }
}