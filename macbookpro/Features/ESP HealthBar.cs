using CS2Cheat.Data.Entity;
using SharpDX;
using System;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspHealthBar
{
    public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        float health = Math.Clamp(entity.Health, 0, 100);
        float healthPercentage = health / 100f;

        int r = (int)(255 * (1.0f - healthPercentage));
        int g = (int)(255 * healthPercentage);
        Color healthColor = new Color((byte)r, (byte)g, (byte)0);

        
        float barWidth = 2.0f;
        float gap = 3.0f; 

        float xLeft = topLeft.X - gap - barWidth;
        float xRight = topLeft.X - gap;

        Vector2 barTop = new Vector2(xLeft, topLeft.Y);
        Vector2 barBottom = new Vector2(xRight, bottomRight.Y);

       
        graphics.DrawFilledRectangle(new Color(0, 0, 0, 255),
            new Vector2(barTop.X - 1, barTop.Y - 1),
            new Vector2(barBottom.X + 1, barBottom.Y + 1));

        
        float height = barBottom.Y - barTop.Y;
        float filledHeight = height * healthPercentage;
        Vector2 filledTop = new Vector2(barTop.X, barBottom.Y - filledHeight);

        
        graphics.DrawFilledRectangle(healthColor, filledTop, barBottom);
    }
}