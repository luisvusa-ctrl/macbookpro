using CS2Cheat.Data.Entity;
using SharpDX;
using System;

namespace CS2Cheat.Features
{
    public static class EspHealthBar
    {
        // Cache de cores estáticas para evitar alocações de memória
        private static readonly SharpDX.Color BackgroundColor = new SharpDX.Color(0, 0, 0, 180);

        public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
        {
            if (entity.Health <= 0) return;

            // 1. Cálculo da porcentagem de vida
            float health = Math.Clamp(entity.Health, 0, 100);
            float healthPercentage = health / 100f;

            // 2. CORREÇÃO DA AMBIGUIDADE (Cast para byte)
            // O uso de (byte) resolve o erro CS0104 e a ambiguidade do construtor.
            byte r = (byte)(255 * Math.Min(2.0f - 2.0f * healthPercentage, 1.0f));
            byte g = (byte)(255 * Math.Min(2.0f * healthPercentage, 1.0f));
            SharpDX.Color healthColor = new SharpDX.Color(r, g, (byte)0, (byte)255);

            // 3. Layout Estabilizado
            // A largura da barra e o gap (espaço) devem ser pequenos para não "vibrar"
            float barWidth = 2.0f;
            float gap = 3.0f;

            // Definimos as coordenadas baseadas exatamente nos limites da Box recebida
            float xLeft = topLeft.X - gap - barWidth;
            float xRight = topLeft.X - gap;
            float yTop = topLeft.Y;
            float yBottom = bottomRight.Y;
            float totalHeight = yBottom - yTop;

            // 4. Desenho do Background (Moldura)
            // Desenhamos 1 pixel a mais para criar um contorno que estabiliza visualmente a barra
            graphics.DrawFilledRectangle(BackgroundColor,
                new Vector2(xLeft - 1, yTop - 1),
                new Vector2(xRight + 1, yBottom + 1));

            // 5. Cálculo do Preenchimento (Vida sobe de baixo para cima)
            float filledHeight = totalHeight * healthPercentage;

            // O topo da barra de vida é o fundo menos a altura preenchida
            Vector2 filledTop = new Vector2(xLeft, yBottom - filledHeight);
            Vector2 filledBottom = new Vector2(xRight, yBottom);

            // 6. Renderização da parte colorida
            if (filledHeight > 0)
            {
                graphics.DrawFilledRectangle(healthColor, filledTop, filledBottom);
            }
        }
    }
}