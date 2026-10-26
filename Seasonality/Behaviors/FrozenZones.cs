using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Seasonality;

public class FrozenZones : MonoBehaviour
{
    public MeshCollider m_surfaceCollider = null!;
    public MeshRenderer m_surfaceRenderer = null!;
    public WaterVolume m_waterVolume = null!;
    public Material m_originalMaterial = null!;

    private static readonly List<FrozenZones> Instances = [];
    public bool m_frozen;

    private static PhysicsMaterial? _slipMat;
    private static PhysicsMaterial? m_slipperyMat
    {
        get
        {
            if (_slipMat != null) return _slipMat;
            _slipMat = Resources.FindObjectsOfTypeAll<PhysicsMaterial>()
                .FirstOrDefault(m => m.name == "SlipperyIce");
            return _slipMat;
        }
    }

    private static Material m_frozenMat;
    
    public static void UpdateAll()
    {
        foreach (FrozenZones instance in Instances)
        {
            if (Configs.m_waterFreezes.Value is Toggle.Off)
            {
                instance.ThawWater();
            }
            else
            {
                if (SeasonTime.GetCurrentSeason() is Season.Winter) 
                    instance.FreezeWater();
                else instance.ThawWater();
            }
        }
    }
    public void Awake()
    {
        Transform water = transform.Find("Water");
        Transform surface = water.Find("WaterSurface");
        Transform volume = water.Find("WaterVolume");

        MeshFilter filter = surface.GetComponent<MeshFilter>();
        m_surfaceRenderer = surface.GetComponent<MeshRenderer>();
        m_waterVolume = volume.GetComponent<WaterVolume>();
        m_surfaceCollider = surface.gameObject.AddComponent<MeshCollider>();
        m_originalMaterial = m_surfaceRenderer.material;
        if (m_frozenMat == null)
        {
            m_frozenMat = new Material(m_surfaceRenderer.material);
            SetupFrozenMat();
        }        
        
        m_surfaceCollider.sharedMesh = filter.sharedMesh;
        m_surfaceCollider.material = m_slipperyMat;

        SetInitialValues();
        if (Instances.Contains(this)) return;
        Instances.Add(this);
    }
    public void OnDestroy()
    {
        Instances.Remove(this);
    }

    [HarmonyPatch(typeof(Fish), nameof(Fish.Awake))]
    private static class Fish_Awake_Patch
    {
        private static void Postfix(Fish __instance)
        {
            if (SeasonTime.GetCurrentSeason() is Season.Winter && Configs.m_waterFreezes.Value is Toggle.On)
            {
                if (__instance.transform.position.y >= ZoneSystem.instance.m_waterLevel - 1f)
                {
                    __instance.transform.position += new Vector3(0f, -2f, 0f);
                }
            }
        }
    }

    public static void SetupFrozenMat()
    {
        var normal = SeasonalityPlugin.FrozenWaterMat1.GetTexture("_IceNormal");
        m_frozenMat.SetTexture("_Normal", normal);
        m_frozenMat.SetFloat("_NormalScale", 1f);
    }

    private void SetInitialValues()
    {
        if (SeasonTime.GetCurrentSeason() is Season.Winter && Configs.m_waterFreezes.Value is Toggle.On)
        {
            m_surfaceRenderer.material = m_frozenMat;
            m_surfaceCollider.enabled = true;
            m_waterVolume.m_useGlobalWind = false;
            m_frozen = true;
        }
        else
        {
            m_surfaceRenderer.material = m_originalMaterial;
            m_surfaceCollider.enabled = false;
            m_waterVolume.m_useGlobalWind = true;
            m_frozen = false;
        }
    }
    
    [HarmonyPatch(typeof(Character), nameof(Character.UpdateWalking))]
    private static class Character_UpdateWalking_Patch
    {
        private static readonly FieldInfo m_slippingField = AccessTools.Field(typeof(Character), "m_slipping");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Stfld &&
                    code[i].operand is FieldInfo fi &&
                    fi == m_slippingField)
                {
                    var insert = new List<CodeInstruction>
                    {
                        new(OpCodes.Ldarg_0),                
                        new(OpCodes.Ldarg_0),                 
                        new(OpCodes.Ldarg_0),         
                        new(OpCodes.Ldfld, m_slippingField), 
                        new(OpCodes.Call,
                            AccessTools.Method(typeof(Character_UpdateWalking_Patch), nameof(ForceSlipOnWater))),                  
                        new(OpCodes.Stfld, m_slippingField),  
                    };

                    code.InsertRange(i + 1, insert);
                    break;
                }
            }

            return code;
        }

        private static bool ForceSlipOnWater(Character instance, bool originalValue)
        {
            if (SeasonTime.GetCurrentSeason() is not Season.Winter || Configs.m_waterFreezes.Value is Toggle.Off) return originalValue;
            
            var collider = instance.m_lastGroundCollider;
            if (collider != null && collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                return true;
            }
            return originalValue;
        }
    }

    public void FreezeWater()
    {
        if (!m_surfaceCollider || !m_surfaceCollider || !m_waterVolume) return;
        if (m_frozen) return;

        m_surfaceRenderer.material = m_frozenMat;
        m_surfaceCollider.enabled = true;
        m_waterVolume.m_useGlobalWind = false;
        m_frozen = true;
    }

    public void ThawWater()
    {
        if (!m_surfaceCollider || !m_surfaceCollider || !m_waterVolume) return;
        if (!m_frozen) return;
        
        m_surfaceRenderer.material = m_originalMaterial;
        m_surfaceCollider.enabled = false;
        m_waterVolume.m_useGlobalWind = true;
        
        m_waterVolume.Start();
        
        m_frozen = false;
    }
    
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    private static class ZoneSystem_Start_Patch
    {
        private static void Postfix(ZoneSystem __instance)
        {
            if (!__instance) return;
            __instance.m_zonePrefab.AddComponent<FrozenZones>();
        }
    }
}