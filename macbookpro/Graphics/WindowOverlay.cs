using System;
using System.Threading;
using System.Windows.Threading;
using CS2Cheat.Data.Game;
using CS2Cheat.Utils;
using GameOverlay.Windows;
using SharpDX;
using Color = SharpDX.Color;
using Rectangle = System.Drawing.Rectangle;

namespace CS2Cheat.Graphics;

public class WindowOverlay : ThreadedServiceBase
{
    private const int TargetFps = 144; // Ajuste para o seu monitor (ex: 60, 144, 240)
    private const int FrameDelay = 1000 / TargetFps;

    public WindowOverlay(GameProcess gameProcess)
    {
        GameProcess = gameProcess;

        Window = new OverlayWindow
        {
            Title = "Overlay",
            IsTopmost = true,
            IsVisible = true,
            X = -32000,
            Y = -32000,
            Width = 16,
            Height = 16
        };

        Window.Create();
    }

    protected override string ThreadName => nameof(WindowOverlay);
    private GameProcess GameProcess { get; set; }
    public OverlayWindow Window { get; private set; }

    public override void Dispose()
    {
        base.Dispose();
        Window?.Dispose();
        Window = null;
        GameProcess = null;
    }

    protected override void FrameAction()
    {
        if (GameProcess == null) return;

        try
        {
            // Atualiza a posição da janela de forma segura
            Update(GameProcess.WindowRectangleClient);

            // Mantém a fluidez constante sem sobrecarregar a CPU
            Thread.Sleep(FrameDelay);
        }
        catch (Exception)
        {
            // Impede que o app feche sozinho em caso de erro de leitura
        }
    }

    private DateTime _nextCheck = DateTime.MinValue;

    private void Update(Rectangle windowRectangle)
    {
        // Só verifica a cada 500ms se a janela mudou para poupar recursos
        if (DateTime.Now < _nextCheck) return;
        _nextCheck = DateTime.Now.AddMilliseconds(10);

        // Se a janela do jogo não mudou, não faz nada
        if (Window.X == windowRectangle.Location.X &&
            Window.Y == windowRectangle.Location.Y &&
            Window.Width == windowRectangle.Size.Width &&
            Window.Height == windowRectangle.Size.Height)
            return;

        // Atualiza as dimensões do overlay para "colar" no jogo
        if (windowRectangle.Width > 0 && windowRectangle.Height > 0)
        {
            Window.X = windowRectangle.Location.X;
            Window.Y = windowRectangle.Location.Y;
            Window.Width = windowRectangle.Size.Width;
            Window.Height = windowRectangle.Size.Height;
        }
    }


    public static void Draw(GameProcess gameProcess, Graphics graphics)
    {
        // Desenha apenas se o jogo estiver ativo para economizar FPS
        if (gameProcess == null || graphics == null) return;

        // Borda do overlay (opcional, útil para debug)
        graphics.DrawLine(Color.DarkGray,
            new Vector2(0, 0),
            new Vector2(gameProcess.WindowRectangleClient.Width - 1, 0),
            new Vector2(gameProcess.WindowRectangleClient.Width - 1, 0),
            new Vector2(gameProcess.WindowRectangleClient.Width - 1, gameProcess.WindowRectangleClient.Height - 1),
            new Vector2(gameProcess.WindowRectangleClient.Width - 1, gameProcess.WindowRectangleClient.Height - 1),
            new Vector2(0, gameProcess.WindowRectangleClient.Height - 1),
            new Vector2(0, gameProcess.WindowRectangleClient.Height - 1),
            new Vector2(0, 0)
        );
    }
}