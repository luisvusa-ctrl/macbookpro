using CS2Cheat.Data.Game;
using CS2Cheat.Features;
using CS2Cheat.Utils;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic; // Adicionado para IReadOnlyDictionary
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

    public Vector3 Position { get; private set; }
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
    public float Velocity { get; private set; }
    public float VerticalVelocity { get; private set; }
    public int Flags { get; private set; }
    public bool JustLanded { get; private set; }
    public int WeaponId { get; private set; }
    public string CurrentWeaponName { get; private set; } = string.Empty;
    public int CurrentWeaponAmmo { get; private set; }
    public int MaxWeaponAmmo { get; private set; }

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

        // --- Leitura de Arma ---
        IntPtr weaponBase = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_pClippingWeapon);
        if (weaponBase != IntPtr.Zero)
        {
            int idOffset = Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex;
            ushort weaponId = gameProcess.Process.Read<ushort>(weaponBase + idOffset);
            this.WeaponId = weaponId;
            this.CurrentWeaponName = EspWeapon.GetNameById(weaponId);
            this.CurrentWeaponAmmo = gameProcess.Process.Read<int>(weaponBase + Offsets.m_iClip1);
            this.MaxWeaponAmmo = GetMaxAmmo(weaponId);
        }
        else
        {
            this.WeaponId = 0;
            this.CurrentWeaponName = "knife";
            this.CurrentWeaponAmmo = 0;
            this.MaxWeaponAmmo = 0;
        }

        // --- Dados Básicos ---
        Name = gameProcess.Process.ReadString(ControllerBase + Offsets.m_iszPlayerName);
        var moneyService = gameProcess.Process.Read<IntPtr>(ControllerBase + Offsets.m_pInGameMoneyServices);
        Money = (moneyService != IntPtr.Zero) ? gameProcess.Process.Read<int>(moneyService + Offsets.m_iAccount) : 0;
        ArmorValue = gameProcess.Process.Read<int>(AddressBase + Offsets.m_ArmorValue);
        HasHelmet = gameProcess.Process.Read<byte>(AddressBase + Offsets.m_bHasHelmet) != 0;
        this.Position = gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_vOldOrigin);

        // --- Velocidade e Flags ---
        Vector3 velocityVec = gameProcess.Process.Read<Vector3>(AddressBase + Offsets.m_vecVelocity);
        this.Velocity = (float)Math.Sqrt(velocityVec.X * velocityVec.X + velocityVec.Y * velocityVec.Y);
        float lastVerticalVelocity = this.VerticalVelocity;
        this.VerticalVelocity = velocityVec.Z;
        this.Flags = gameProcess.Process.Read<int>(AddressBase + Offsets.m_fFlags);
        this.JustLanded = ((this.Flags & 1) != 0) && lastVerticalVelocity < -400f;

        // --- Estados ---
        ScopedValue = gameProcess.Process.Read<byte>(AddressBase + Offsets.m_bIsScoped);
        IsScoped = (ScopedValue > 0 && ScopedValue < 3);
        _dormant = gameProcess.Process.Read<bool>(AddressBase + Offsets.m_bDormant);
        FlashAlpha = gameProcess.Process.Read<float>(AddressBase + Offsets.m_flFlashDuration);

        // Atualizar Ossos apenas se estiver vivo
        return UpdateBonePositions(gameProcess);
    }

    private bool UpdateBonePositions(GameProcess gameProcess)
    {
        try
        {
            if (gameProcess?.Process == null || AddressBase == IntPtr.Zero) return false;

            // Acesso ao GameSceneNode
            var gameSceneNode = gameProcess.Process.Read<IntPtr>(AddressBase + Offsets.m_pGameSceneNode);
            if (gameSceneNode == IntPtr.Zero) return false;

            // Acesso ao BoneArray via ModelState
            var boneArray = gameProcess.Process.Read<IntPtr>(gameSceneNode + Offsets.m_modelState + 128);
            if (boneArray == IntPtr.Zero) return false;

            foreach (var bone in Offsets.Bones)
            {
                // Leitura direta com offset de 32 bytes (padrão CS2)
                var bonePos = gameProcess.Process.Read<Vector3>(boneArray + (bone.Value * 32));

                // Filtro para evitar ossos inválidos "bugando" o desenho
                if (bonePos != Vector3.Zero)
                {
                    _bonePositions[bone.Key] = bonePos;
                }
            }
            return true;
        }
        catch { return false; }
    }

    private static int GetMaxAmmo(int weaponId)
    {
        return weaponId switch
        {
            // Pistolas
            61 => 12, // USP-S
            32 => 13, // P2000
            4 => 20,  // Glock-18
            2 => 30,  // Elite
            36 => 13, // P250
            30 => 18, // Tec-9
            3 => 20,  // Five-Seven
            63 => 12, // CZ75-Auto
            1 => 7,   // Desert Eagle
            64 => 8,  // R8 Revolver

            // Rifles
            7 => 30,  // AK-47
            16 => 30, // M4A4
            60 => 20, // M4A1-S
            13 => 35, // Galil AR
            10 => 25, // FAMAS
            8 => 30,  // AUG
            39 => 30, // SG 553

            // Snipers
            9 => 5,   // AWP (CS2 agora são 5 balas, não 10)
            40 => 10, // SSG 08 (Scout)
            38 => 20, // SCAR-20
            11 => 20, // G3SG1

            // SMGs
            17 => 30, // MAC-10
            33 => 30, // MP7
            34 => 30, // MP9
            23 => 30, // MP5-SD
            24 => 25, // UMP-45
            19 => 50, // P90
            26 => 64, // PP-Bizon

            // Shotguns & Heavy
            35 => 8,  // Nova
            25 => 7,  // XM1014
            29 => 7,  // Sawed-Off
            27 => 5,  // MAG-7
            14 => 100,// M249
            28 => 150,// Negev

            _ => 30 // Padrão para casos desconhecidos
        };
    }
}