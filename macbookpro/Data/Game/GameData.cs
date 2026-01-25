using System.Linq;
using CS2Cheat.Data.Entity;
using CS2Cheat.Utils;

namespace CS2Cheat.Data.Game;

public class GameData : ThreadedServiceBase
{
    #region properties

    protected override string ThreadName => nameof(GameData);

    private GameProcess? GameProcess { get; set; }

    public Player? Player { get; private set; }

    public Entity.Entity[]? Entities { get; private set; }

    #endregion

    #region methods

    /// <inheritdoc />
    public GameData(GameProcess gameProcess)
    {
        GameProcess = gameProcess;
        Player = new Player();
        
        Entities = Enumerable.Range(0, 64).Select(index => new Entity.Entity(index)).ToArray();
    }

    public override void Dispose()
    {
        base.Dispose();

        Entities = null;
        Player = null;
        GameProcess = null;
    }

    /// <summary>
    /// Método chamado pelo Graphics.cs para atualizar a câmera imediatamente antes de desenhar.
    /// Isso é o que faz o ESP ficar "colado" no boneco sem tremer.
    /// </summary>
    public void UpdateViewMatrix()
    {
        if (GameProcess != null && GameProcess.IsValid)
        {
            
            Player?.Update(GameProcess);
        }
    }

    protected override void FrameAction()
    {
        if (GameProcess == null || !GameProcess.IsValid) return;

        
        Player?.Update(GameProcess);

        
        if (Entities != null)
        {
            foreach (var entity in Entities)
            {
                entity.Update(GameProcess);
            }
        }
    }

    #endregion
}