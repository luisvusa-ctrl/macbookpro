using CS2Cheat.Data.Entity;
using SharpDX;
using System;

namespace CS2Cheat.Features
{
    public static class EspAmmoBar
    {
        // Cores em cache para evitar erros de ambiguidade e alocação
        private static readonly SharpDX.Color BackgroundColor = new SharpDX.Color(0, 0, 0, 180);
        private static readonly SharpDX.Color AmmoColor = new SharpDX.Color(0, 120, 255, 255); // Azul Azulado

        public static void Draw(CS2Cheat.Graphics.Graphics graphics, Entity entity, Vector2 topLeft, Vector2 bottomRight)
        {
            // 1. Verificação de Sanidade: Se a munição for 0 ou negativa, pode ser que o offset esteja errado.
            // Para testar, você pode comentar a linha abaixo. Se a barra aparecer vazia, o problema é a leitura da memória.
            if (entity.MaxWeaponAmmo <= 0) return;

            // 2. Cálculo da largura e proporção
            float boxWidth = bottomRight.X - topLeft.X;

            // Usamos float para a divisão ser precisa e não retornar sempre 0 ou 1
            float ammoPercentage = Math.Clamp((float)entity.CurrentWeaponAmmo / (float)entity.MaxWeaponAmmo, 0f, 1f);

            // 3. Posicionamento milimétrico
            float gap = 3.0f;       // Espaço abaixo da box
            float barHeight = 2.0f;  // Altura da barra

            float xLeft = topLeft.X;
            float xRight = bottomRight.X;
            float yTop = bottomRight.Y + gap;
            float yBottom = yTop + barHeight;

            // 4. Desenho do Background (Sombra/Outline)
            // O segredo do alinhamento: o background deve ter o mesmo X da Box
            graphics.DrawFilledRectangle(BackgroundColor,
                new Vector2(xLeft - 1, yTop - 1),
                new Vector2(xRight + 1, yBottom + 1));

            // 5. Cálculo do preenchimento
            float filledWidth = boxWidth * ammoPercentage;

            // 6. Renderização da barra colorida
            // Se filledWidth for 0 (vazio), desenhamos apenas o fundo preto
            if (filledWidth > 0)
            {
                graphics.DrawFilledRectangle(AmmoColor,
                    new Vector2(xLeft, yTop),
                    new Vector2(xLeft + filledWidth, yBottom));
            }
        }
    }
}