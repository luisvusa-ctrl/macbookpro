using CS2Cheat.DTO.ClientDllDTO;
using CS2Cheat.Utils.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CS2Cheat.Utils;

public abstract class Offsets
{
    #region Static Fields
    public const float WeaponRecoilScale = 2f;
    public const nint m_nCurrentTickThisFrame = 0x34;

    // Endereços Base (dw)
    public static int dwLocalPlayerPawn;
    public static int dwLocalPlayerController;
    public static int dwEntityList;
    public static int dwViewMatrix;
    public static int dwViewAngles;
    public static int dwPlantedC4;
    public static int dwGlobalVars;

    // Propriedades da Entidade
    public static int m_vOldOrigin;
    public static int m_vecViewOffset;
    public static int m_AimPunchAngle;
    public static int m_modelState;
    public static int m_pGameSceneNode;
    public static int m_fFlags;
    public static int m_iIDEntIndex;
    public static int m_lifeState;
    public static int m_iHealth;
    public static int m_iTeamNum;
    public static int m_bDormant;
    public static int m_iShotsFired;
    public static int m_hPawn;
    public static int m_entitySpottedState;
    public static int m_iszPlayerName;
    public static int m_vecAbsVelocity;

    // Armas e Itens
    public static int m_pClippingWeapon;
    public static int m_AttributeManager;
    public static int m_Item;
    public static int m_iItemDefinitionIndex;
    public static int m_iClip1;

    // ESP, Flags e Combate
    public static int m_bIsScoped;
    public static int m_flFlashDuration;
    public static int m_ArmorValue;
    public static int m_bHasHelmet;
    public static int m_pInGameMoneyServices;
    public static int m_iAccount;

    // Bomb Timer
    public static int m_flDefuseCountDown;
    public static int m_flC4Blow;
    public static int m_bBeingDefused;
    public static int m_nBombSite;
    public static int m_bBombDefused;

    public static readonly Dictionary<string, int> Bones = new()
    {
        { "head", 6 }, { "neck_0", 5 }, { "spine_1", 4 }, { "spine_2", 2 },
        { "pelvis", 0 }, { "arm_upper_L", 8 }, { "arm_lower_L", 9 },
        { "hand_L", 10 }, { "arm_upper_R", 13 }, { "arm_lower_R", 14 },
        { "hand_R", 15 }, { "leg_upper_L", 22 }, { "leg_lower_L", 23 },
        { "ankle_L", 24 }, { "leg_upper_R", 25 }, { "leg_lower_R", 26 },
        { "ankle_R", 27 }
    };
    #endregion

    public static async Task UpdateOffsets()
    {
        try
        {
            var sourceDataDw = JsonConvert.DeserializeObject<OffsetsDTO>(await FetchJson("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.json"));
            var sourceDataClient = JsonConvert.DeserializeObject<ClientDllDTO>(await FetchJson("https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.json"));
            var classes = sourceDataClient.clientdll.classes;

            dynamic d = new ExpandoObject();

            
            d.dwLocalPlayerController = sourceDataDw.clientdll.dwLocalPlayerController;
            d.dwEntityList = sourceDataDw.clientdll.dwEntityList;
            d.dwViewMatrix = sourceDataDw.clientdll.dwViewMatrix;
            d.dwLocalPlayerPawn = sourceDataDw.clientdll.dwLocalPlayerPawn;
            d.dwViewAngles = sourceDataDw.clientdll.dwViewAngles;
            d.dwPlantedC4 = sourceDataDw.clientdll.dwPlantedC4;
            d.dwGlobalVars = sourceDataDw.clientdll.dwGlobalVars;

            
            d.m_iHealth = classes.C_BaseEntity.fields.m_iHealth;
            d.m_iTeamNum = classes.C_BaseEntity.fields.m_iTeamNum;
            d.m_lifeState = classes.C_BaseEntity.fields.m_lifeState;
            d.m_fFlags = classes.C_BaseEntity.fields.m_fFlags;
            d.m_vecAbsVelocity = classes.C_BaseEntity.fields.m_vecAbsVelocity;
            d.m_hPawn = classes.CBasePlayerController.fields.m_hPawn;
            d.m_iszPlayerName = classes.CBasePlayerController.fields.m_iszPlayerName;
            d.m_pGameSceneNode = classes.C_BaseEntity.fields.m_pGameSceneNode;
            d.m_modelState = classes.CSkeletonInstance.fields.m_modelState;
            d.m_bDormant = classes.CGameSceneNode.fields.m_bDormant;
            d.m_vOldOrigin = classes.C_BasePlayerPawn.fields.m_vOldOrigin;
            d.m_vecViewOffset = classes.C_BaseModelEntity.fields.m_vecViewOffset;
            d.m_AimPunchAngle = classes.C_CSPlayerPawn.fields.m_aimPunchAngle;
            d.m_iShotsFired = classes.C_CSPlayerPawn.fields.m_iShotsFired;

            
            d.m_flFlashDuration = classes.C_CSPlayerPawnBase.fields.m_flFlashDuration;
            d.m_ArmorValue = classes.C_CSPlayerPawn.fields.m_ArmorValue;
            d.m_bHasHelmet = classes.C_CSPlayerPawn.fields.m_bHasHelmet;
            d.m_pInGameMoneyServices = classes.CCSPlayerController.fields.m_pInGameMoneyServices;
            d.m_iAccount = classes.CCSPlayerController_InGameMoneyServices.fields.m_iAccount;

            
            try { d.m_bIsScoped = classes.C_CSPlayerPawn.fields.m_bIsScoped; }
            catch { try { d.m_bIsScoped = classes.C_CSPlayerPawnBase.fields.m_bIsScoped; } catch { d.m_bIsScoped = 0x26F8; } }

            
            d.m_pClippingWeapon = classes.C_CSPlayerPawnBase.fields.m_pClippingWeapon;
            d.m_AttributeManager = classes.C_EconEntity.fields.m_AttributeManager;
            d.m_Item = classes.C_AttributeContainer.fields.m_Item;
            d.m_iItemDefinitionIndex = classes.C_EconItemView.fields.m_iItemDefinitionIndex;

            
            d.m_flDefuseCountDown = classes.C_PlantedC4.fields.m_flDefuseCountDown;
            d.m_flC4Blow = classes.C_PlantedC4.fields.m_flC4Blow;
            d.m_bBeingDefused = classes.C_PlantedC4.fields.m_bBeingDefused;
            d.m_nBombSite = classes.C_PlantedC4.fields.m_nBombSite;
            d.m_bBombDefused = classes.C_PlantedC4.fields.m_bBombDefused;

            UpdateStaticFields(d);
        }
        catch (Exception ex) { Console.WriteLine("Erro crítico ao carregar offsets: " + ex.Message); }
    }

    private static void UpdateStaticFields(dynamic data)
    {
        dwLocalPlayerController = data.dwLocalPlayerController;
        dwEntityList = data.dwEntityList;
        dwViewMatrix = data.dwViewMatrix;
        dwLocalPlayerPawn = data.dwLocalPlayerPawn;
        dwViewAngles = data.dwViewAngles;
        dwPlantedC4 = data.dwPlantedC4;
        dwGlobalVars = data.dwGlobalVars;

        m_iHealth = data.m_iHealth;
        m_iTeamNum = data.m_iTeamNum;
        m_lifeState = data.m_lifeState;
        m_fFlags = data.m_fFlags;
        m_hPawn = data.m_hPawn;
        m_iszPlayerName = data.m_iszPlayerName;
        m_pGameSceneNode = data.m_pGameSceneNode;
        m_modelState = data.m_modelState;
        m_bDormant = data.m_bDormant;
        m_vOldOrigin = data.m_vOldOrigin;
        m_vecViewOffset = data.m_vecViewOffset;
        m_AimPunchAngle = data.m_AimPunchAngle;
        m_iShotsFired = data.m_iShotsFired;
        m_vecAbsVelocity = data.m_vecAbsVelocity;

        m_bIsScoped = data.m_bIsScoped;
        m_flFlashDuration = data.m_flFlashDuration;
        m_ArmorValue = data.m_ArmorValue;
        m_bHasHelmet = data.m_bHasHelmet;
        m_pInGameMoneyServices = data.m_pInGameMoneyServices;
        m_iAccount = data.m_iAccount;

        m_pClippingWeapon = 0x3DC0;
        m_AttributeManager = data.m_AttributeManager;
        m_Item = data.m_Item;
        m_iItemDefinitionIndex = data.m_iItemDefinitionIndex;

        m_flDefuseCountDown = data.m_flDefuseCountDown;
        m_flC4Blow = data.m_flC4Blow;
        m_bBeingDefused = data.m_bBeingDefused;
        m_nBombSite = data.m_nBombSite;
        m_bBombDefused = data.m_bBombDefused;
        m_iClip1 = 0x18D0;
    }

    private static async Task<string> FetchJson(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }
}