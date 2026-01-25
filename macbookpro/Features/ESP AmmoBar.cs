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
        // Só desenha se a munição máxima for válida
        if (entity.MaxWeaponAmmo <= 0) return;

        // 1. Definições de Estética
        float boxWidth = bottomRight.X - topLeft.X;
        // O (float) garante que a divisão funcione e a barra diminua suavemente
        float ammoPercentage = Math.Clamp((float)entity.CurrentWeaponAmmo / entity.MaxWeaponAmmo, 0f, 1f);

        // Posição: 2px abaixo da box
        // Altura: 6px (de +2 até +8)
        Vector2 barTopLeft = new Vector2(topLeft.X, bottomRight.Y + 2);
        Vector2 barBottomRight = new Vector2(bottomRight.X, bottomRight.Y + 5);

        // 2. Desenha o Fundo (Sombra/Background)
        // Isso faz com que, quando a munição baixar, você veja o "vazio" em preto
        graphics.DrawFilledRectangle(new Color(0, 0, 0, 180), barTopLeft, barBottomRight);

        // 3. Desenha o Preenchimento Azul Azulado (Dinâmico)
        float currentBarWidth = boxWidth * ammoPercentage;
        Vector2 ammoFillRight = new Vector2(barTopLeft.X + currentBarWidth, barBottomRight.Y);

        // Cor Azul Vibrante que você escolheu
        Color azulAzulado = new Color(0, 120, 255, 255);
        graphics.DrawFilledRectangle(azulAzulado, barTopLeft, ammoFillRight);

        // 4. Contorno Preto (Outline)
        // O contorno deve envolver a barra INTEIRA para manter o shape da box
        graphics.DrawRectangle(Color.Black, barTopLeft, barBottomRight);
    }
}