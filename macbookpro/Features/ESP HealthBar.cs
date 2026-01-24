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

        // Cores
        int r = (int)(255 * (1.0f - healthPercentage));
        int g = (int)(255 * healthPercentage);
        Color healthColor = new Color((byte)r, (byte)g, (byte)0);

        // CONFIGURAÇÃO DA POSIÇÃO (4 pixels à esquerda do box)
        float barWidth = 2.0f;
        float gap = 3.0f; // Espaço entre o box e a barra

        float xLeft = topLeft.X - gap - barWidth;
        float xRight = topLeft.X - gap;

        Vector2 barTop = new Vector2(xLeft, topLeft.Y);
        Vector2 barBottom = new Vector2(xRight, bottomRight.Y);

        // 1. DESENHA O FUNDO/BORDA PRETA (Outline)
        // Expandimos 1 pixel para cada lado para criar a borda perfeita
        graphics.DrawFilledRectangle(new Color(0, 0, 0, 255),
            new Vector2(barTop.X - 1, barTop.Y - 1),
            new Vector2(barBottom.X + 1, barBottom.Y + 1));

        // 2. CALCULA O PREENCHIMENTO (De baixo para cima)
        float height = barBottom.Y - barTop.Y;
        float filledHeight = height * healthPercentage;
        Vector2 filledTop = new Vector2(barTop.X, barBottom.Y - filledHeight);

        // 3. DESENHA A PARTE COLORIDA
        graphics.DrawFilledRectangle(healthColor, filledTop, barBottom);
    }
}