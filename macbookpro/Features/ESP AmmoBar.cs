using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspAmmoBar
{
    public static void Draw(Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
    {
        // Define a largura baseada na box do player
        float boxWidth = bottomRight.X - topLeft.X;

        // --- AJUSTE DE GROSSURA AQUI ---
        // barTopLeft: Começa 2px abaixo da box
        // barBottomRight: Agora termina 8px abaixo da box (8 - 2 = 6px de altura/grossura)
        Vector2 barTopLeft = new Vector2(topLeft.X, bottomRight.Y + 2);
        Vector2 barBottomRight = new Vector2(bottomRight.X, bottomRight.Y + 5);

        // --- COR AZULADA (R: 0, G: 120, B: 255) ---
        Color azulado = new Color(0, 120, 255, 255);

        // Desenha a barra preenchida
        graphics.DrawFilledRectangle(azulado, barTopLeft, barBottomRight);

        // Desenha o contorno preto (Outline)
        graphics.DrawRectangle(Color.Black, barTopLeft, barBottomRight);
    }
}