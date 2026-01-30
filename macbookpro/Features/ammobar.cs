using CS2Cheat.Data.Entity;
using SharpDX;
using System;

namespace CS2Cheat.Features
{
    public static class EspAmmoBar
    {
        private static readonly SharpDX.Color BackgroundColor = new SharpDX.Color(0, 0, 0, 200);
        private static readonly SharpDX.Color AmmoColor = new SharpDX.Color(0, 120, 255, 255); // Azul

        public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
        {
            
            if (entity.MaxWeaponAmmo <= 0) return;

            
            
            float ammoPercentage = Math.Clamp((float)entity.CurrentWeaponAmmo / (float)entity.MaxWeaponAmmo, 0f, 1f);

            
            float xLeft = topLeft.X;
            float xRight = bottomRight.X;
            float yTop = topLeft.Y;
            float yBottom = bottomRight.Y;

            float totalWidth = xRight - xLeft;

            
            graphics.DrawFilledRectangle(BackgroundColor,
                new Vector2(xLeft - 1, yTop - 1),
                new Vector2(xRight + 1, yBottom + 1));

            
            float filledWidth = totalWidth * ammoPercentage;

            
            if (filledWidth > 0)
            {
                graphics.DrawFilledRectangle(AmmoColor,
                    new Vector2(xLeft, yTop),
                    new Vector2(xLeft + filledWidth, yBottom));
            }
        }
    }
}