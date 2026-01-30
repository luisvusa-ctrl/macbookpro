using CS2Cheat.Data.Entity;
using SharpDX;
using System;

namespace CS2Cheat.Features
{
    public static class EspHealthBar
    {
        
        private static readonly SharpDX.Color BackgroundColor = new SharpDX.Color(0, 0, 0, 180);

        public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
        {
            if (entity.Health <= 0) return;

           
            float health = Math.Clamp(entity.Health, 0, 100);
            float healthPercentage = health / 100f;

            
            byte r = (byte)(255 * Math.Min(2.0f - 2.0f * healthPercentage, 1.0f));
            byte g = (byte)(255 * Math.Min(2.0f * healthPercentage, 1.0f));
            SharpDX.Color healthColor = new SharpDX.Color(r, g, (byte)0, (byte)255);

            
            float barWidth = 2.0f;
            float gap = -2.5f;

            
            float xLeft = topLeft.X - gap - barWidth;
            float xRight = topLeft.X - gap;
            float yTop = topLeft.Y;
            float yBottom = bottomRight.Y;
            float totalHeight = yBottom - yTop;

            
            graphics.DrawFilledRectangle(BackgroundColor,
                new Vector2(xLeft - 1, yTop - 1),
                new Vector2(xRight + 1, yBottom + 1));

            
            float filledHeight = totalHeight * healthPercentage;

            
            Vector2 filledTop = new Vector2(xLeft, yBottom - filledHeight);
            Vector2 filledBottom = new Vector2(xRight, yBottom);

            
            if (filledHeight > 0)
            {
                graphics.DrawFilledRectangle(healthColor, filledTop, filledBottom);
            }
        }
    }
}