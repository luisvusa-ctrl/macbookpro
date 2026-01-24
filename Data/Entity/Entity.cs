using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Utils;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CS2Cheat.Data.Entity;

public class Entity : EntityBase
{
    private readonly ConcurrentDictionary<string, Vector3> _bonePositions;
    private bool _dormant = true;

    public Entity(int index)
    {
        Id = index;
        _bonePositions = new ConcurrentDictionary<string, Vector3>(Offsets.Bones.ToDictionary(
            bone => bone.Key,
            _ => Vector3.Zero
        ));
    }

    // --- PROPRIEDADES PARA O ESP / FLAGS ---
    public int Id { get; }
    public IReadOnlyDictionary<string, Vector3> BonePos => _bonePositions;
    public string Name { get; private set; } = string.Empty;
    public int Money { get; private set; }
    public int ArmorValue { get; private set; }
    public bool HasHelmet { get; private set; }
    public int ScopedValue { get; private set; }
    public bool IsScoped { get; private set; }
    public float FlashAlpha { get; private set; }
    public bool IsSpotted { get; private set; }

    // Propriedades da Arma
    public int WeaponId { get; private set; }
    public string CurrentWeaponName { get; private set; } = string.Empty;

    public override bool IsAlive()
    {
        return base.IsAlive() && !_dormant;
    }

    protected override IntPtr ReadControllerBase(GameProcess gameProcess)
    {
        var entryIndex = (Id & 0x7FFF) >> 9;
        if (gameProcess?.Process == null) return IntPtr.Zero;

        var listEntry = gameProcess.Process.Read<IntPtr>(EntityList + 8 * entryIndex + 16);
        return listEntry != IntPtr.Zero
            ? gameProcess.Process.Read<IntPtr>(listEntry + 112 * (Id & 0x1FF))
            : IntPtr.Zero;
    }

    protected override IntPtr ReadAddressBase(GameProcess gameProcess)
    {
        if (gameProcess?.Process == null) return IntPtr.Zero;

        var playerPawn = gameProcess.Process.Read<int>(ControllerBase + Offsets.m_hPawn);
        var pawnIndex = (playerPawn & 0x7FFF) >> 9;
        var listEntry = gameProcess.Process.Read<IntPtr>(EntityList + 0x8 * pawnIndex + 16);

        return listEntry != IntPtr.Zero
            ? gameProcess.Process.Read<IntPtr>(listEntry + 112 * (playerPawn & 0x1FF))
            : IntPtr.Zero;
    }

    public override bool Update(GameProcess gameProcess)
    {
        if (!base.Update(gameProcess) || gameProcess.Process == null) return false;

        IntPtr weaponBase = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_pClippingWeapon);

        if (weaponBase != IntPtr.Zero)
        {
            // 2. A corrente de ponteiros que o DragonBurn usa:
            // WeaponBase -> m_AttributeManager (0x10B0) -> m_Item (0x50) -> m_iItemDefinitionIndex (0x1BA)

            // No CS2 externo, somar os offsets e ler o valor final de uma vez é o mais seguro:
            int idOffset = Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex;

            // Lemos o ID como ushort (2 bytes positivos)
            ushort weaponId = gameProcess.Process.Read<ushort>(weaponBase + idOffset);

            this.WeaponId = weaponId;
            this.CurrentWeaponName = EspWeapon.GetNameById(weaponId);
        }
        else
        {
            this.WeaponId = 0;
            this.CurrentWeaponName = "knife";
        }

        // --- RESTANTE DAS LEITURAS (NOME, MONEY, ETC) ---
        Name = gameProcess.Process.ReadString(ControllerBase + Offsets.m_iszPlayerName);

        var moneyService = gameProcess.Process.Read<IntPtr>(ControllerBase + Offsets.m_pInGameMoneyServices);
        Money = (moneyService != IntPtr.Zero) ? gameProcess.Process.Read<int>(moneyService + Offsets.m_iAccount) : 0;

        ArmorValue = gameProcess.Process.Read<int>(AddressBase + Offsets.m_ArmorValue);
        HasHelmet = gameProcess.Process.Read<byte>(AddressBase + Offsets.m_bHasHelmet) != 0;

        ScopedValue = gameProcess.Process.Read<byte>(AddressBase + Offsets.m_bIsScoped);
        IsScoped = (ScopedValue > 0 && ScopedValue < 3);

        _dormant = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bDormant);
        FlashAlpha = gameProcess.Process.Read<float>(AddressBase + Offsets.m_flFlashDuration);

        return !IsAlive() || UpdateBonePositions(gameProcess);
    
    }

    private bool UpdateBonePositions(GameProcess gameProcess)
    {
        try
        {
            if (gameProcess?.Process == null) return false;

            var gameSceneNode = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_pGameSceneNode);
            var boneArray = gameProcess.Process.Read<IntPtr>(gameSceneNode + Offsets.m_modelState + 128);

            foreach (var (boneName, boneIndex) in Offsets.Bones)
            {
                var bonePos = gameProcess.Process.Read<Vector3>(boneArray + boneIndex * 32);
                _bonePositions.AddOrUpdate(boneName, bonePos, (_, _) => bonePos);
            }

            return true;
        }
        catch { return false; }
    }
}