using CS2Cheat.Data.Entity;
using CS2Cheat.Graphics;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;

namespace CS2Cheat.Features;

public static class EspSnaplines
{
    public static void Draw(Graphics.Graphics graphics, Entity entity)
    {
        
        if (entity == null || !entity.IsAlive() || entity.BonePos == null) return;

        
        if (entity.BonePos.TryGetValue("pelvis", out var pelvisW))
        {
            Vector3 screenPos = graphics.GameData.Player.MatrixViewProjectionViewport.Transform(pelvisW);

            
            if (screenPos.Z < 1)
            {
                
                var device = graphics.GetDevice();
                var viewport = device.Viewport;

                
                using (var line = new SharpDX.Direct3D9.Line(device))
                {
                    
                    line.Width = 2.5f;       
                    line.Antialias = true;   

                    
                    Vector2 startPoint = new Vector2(viewport.Width / 2f, viewport.Height);
                    Vector2 endPoint = new Vector2(screenPos.X, screenPos.Y);

                    
                    Color color = entity.IsSpotted
                        ? new Color(255, 255, 255, 255)
                        : new Color(255, 255, 255, 255);

                    
                    line.Begin();
                    line.Draw(new[] { startPoint, endPoint }, new ColorBGRA(color.R, color.G, color.B, color.A));
                    line.End();
                }
            }
        }
    }
}