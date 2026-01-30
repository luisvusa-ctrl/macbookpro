using CS2Cheat.Data.Entity;
using SharpDX;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public class EspBox
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        Color outlineColor = new Color(0, 0, 0, 255);
        Color boxColor = Color.White;

        
        graphics.DrawRectangle(outlineColor,
            new Vector2(topLeft.X - 1, topLeft.Y - 1),
            new Vector2(bottomRight.X + 1, bottomRight.Y + 1));

       
        graphics.DrawRectangle(outlineColor,
            new Vector2(topLeft.X + 1, topLeft.Y + 1),
            new Vector2(bottomRight.X - 1, bottomRight.Y - 1));

        
        graphics.DrawRectangle(boxColor, topLeft, bottomRight);
    }
}