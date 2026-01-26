using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using System;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspAmmoBar
{
    public static void Draw(Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        
        if (entity.MaxWeaponAmmo <= 0) return;

        
        float boxWidth = bottomRight.X - topLeft.X;
        
        float ammoPercentage = Math.Clamp((float)entity.CurrentWeaponAmmo / entity.MaxWeaponAmmo, 0f, 1f);

        
        Vector2 barTopLeft = new Vector2(topLeft.X, bottomRight.Y + 2);
        Vector2 barBottomRight = new Vector2(bottomRight.X, bottomRight.Y + 5);

       
        graphics.DrawFilledRectangle(new Color(0, 0, 0, 180), barTopLeft, barBottomRight);

        
        float currentBarWidth = boxWidth * ammoPercentage;
        Vector2 ammoFillRight = new Vector2(barTopLeft.X + currentBarWidth, barBottomRight.Y);

       
        Color azulAzulado = new Color(0, 120, 255, 255);
        graphics.DrawFilledRectangle(azulAzulado, barTopLeft, ammoFillRight);

        
        graphics.DrawRectangle(Color.Black, barTopLeft, barBottomRight);
    }
}