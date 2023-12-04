using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

// <<<<<<< status-effect-refinements
// =======
// static class SeasonalCompendium
// {
//     public static string customTooltip = null!;
//     [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.AddActiveEffects))]
//     static class CompendiumAddActiveEffectsPatch
//     {
//         private static void Postfix(TextsDialog __instance)
//         {
//             if (_StatusEffectVisible.Value is Toggle.On) return;
//             if (!Player.m_localPlayer) return;
            
//             string texts = __instance.m_texts[0].m_text;

//             string appendedText = $"{customTooltip} \n\n" + texts;

//             __instance.m_texts[0].m_text = appendedText;
//         }
//     }
// }
// >>>>>>> main
public enum Modifier
{
    None,
    Attack,
    HealthRegen,
    StaminaRegen,
    RaiseSkills,
    Speed,
    Noise,
    MaxCarryWeight,
    Stealth,
    RunStaminaDrain,
    DamageReduction,
    FallDamage,
    EitrRegen
}

public class SeasonalEffect
{
    public string effectName = null!;
    public string displayName = "";
    public int duration = 0;
    public Sprite? sprite;
    public string? spriteName;
    public string[]? startEffectNames;
    public string[]? stopEffectNames;
    public string? startMsg = "";
    public string? stopMsg = "";
    public string? effectTooltip = "";
    public string? damageMod;
    public Modifier Modifier = Modifier.None;
    public float m_newValue = 0f;
    public string? activationAnimation = "gpower";
    public Dictionary<Modifier, float> Modifiers = new()
    {
        { Modifier.Attack, 1f },
        { Modifier.HealthRegen , 1f },
        { Modifier.StaminaRegen , 1f },
        { Modifier.RaiseSkills , 1f },
        { Modifier.Speed , 1f },
        { Modifier.Noise , 1f },
        { Modifier.MaxCarryWeight , 0f },
        { Modifier.Stealth , 1f },
        { Modifier.RunStaminaDrain , 1f },
        { Modifier.DamageReduction , 0f },
        { Modifier.FallDamage , 1f },
        { Modifier.EitrRegen , 1f }
    };

    public readonly List<HitData.DamageModPair> damageMods = new();

    public StatusEffect? Init()
    {
        ObjectDB obd = ObjectDB.instance;

        // Make sure new effects have unique names
        if (obd.m_StatusEffects.Find(effect => effect.name == effectName)) return null;

        string appendedTooltip = effectTooltip ?? "";
        string modName = Modifier.ToString();
        string normalizedString = "";
        for (int i = 0; i < modName.Length; ++i)
        {
            if (i == 0) normalizedString += modName[i];
            else
            {
                if (char.IsUpper(modName[i]))
                {
                    normalizedString += " ";
                }
                normalizedString += modName[i];
            }
        }
        Modifiers[Modifier] = m_newValue;
        switch (Modifier)
        {
            case Modifier.None:
                break;
            case Modifier.DamageReduction:
                float reductionValue = Mathf.Clamp01(m_newValue);
                string damageString = $"\n{modName} -<color=orange>{reductionValue * 100}</color>%";
                appendedTooltip += damageString;

                break;
            case Modifier.MaxCarryWeight:
                if (m_newValue > 0)
                {
                    string maxCarryString = $"\n{modName} +<color=orange>{m_newValue}</color>";
                    appendedTooltip += maxCarryString;
                }
                if (m_newValue < 0)
                {
                    string maxCarryString = $"\n{modName} -<color=orange>{m_newValue.ToString().Replace("-", "")}</color>";
                    appendedTooltip += maxCarryString;
                }
                break;
            default:
                if (m_newValue > 1)
                {
                    string defaultString = $"\n{normalizedString} +<color=orange>{Mathf.Round((m_newValue - 1) * 100)}</color>%";
                    appendedTooltip += defaultString;
                }
                if (m_newValue < 1)
                {
                    string defaultString = $"\n{normalizedString} -<color=orange>{Mathf.Round((1 - m_newValue) * 100)}</color>%";
                    appendedTooltip += defaultString;
                }
                break;
        }

        if (!string.IsNullOrWhiteSpace(damageMod))
        {
            string normalizedDamageMod = damageMod!.Replace(" ", "");
            string[] resistanceMods = normalizedDamageMod.Split(',');
            foreach (string resistance in resistanceMods)
            {
                string[] split = resistance.Split('=');
                if (split.Length != 2) continue;
                string damageType = split[0];
                string damageValue = split[1];

                HitData.DamageModPair pair = new()
                {
                    m_type = HitData.DamageType.Physical,
                    m_modifier = HitData.DamageModifier.Normal
                };

                if (damageType == "" || damageType == "None") continue;
                if (damageValue == "" || damageValue == "None") continue;

                switch (damageValue)
                {
                    case "Normal": break;
                    case "Resistant": pair.m_modifier = HitData.DamageModifier.Resistant; break;
                    case "Weak": pair.m_modifier = HitData.DamageModifier.Weak; break;
                    case "Immune": pair.m_modifier = HitData.DamageModifier.Immune; break;
                    case "Ignore": pair.m_modifier = HitData.DamageModifier.Ignore; break;
                    case "VeryResistant": pair.m_modifier = HitData.DamageModifier.VeryResistant; break;
                    case "VeryWeak": pair.m_modifier = HitData.DamageModifier.VeryWeak; break;
                }

                switch (damageType)
                {
                    case "Blunt": pair.m_type = HitData.DamageType.Blunt; break;
                    case "Slash": pair.m_type = HitData.DamageType.Slash; break;
                    case "Pierce": pair.m_type = HitData.DamageType.Pierce; break;
                    case "Chop": pair.m_type = HitData.DamageType.Chop; break;
                    case "Pickaxe": pair.m_type = HitData.DamageType.Pickaxe; break;
                    case "Fire": pair.m_type = HitData.DamageType.Fire; break;
                    case "Frost": pair.m_type = HitData.DamageType.Frost; break;
                    case "Lightning": pair.m_type = HitData.DamageType.Lightning; break;
                    case "Poison": pair.m_type = HitData.DamageType.Poison; break;
                    case "Spirit": pair.m_type = HitData.DamageType.Spirit; break;
                    case "Physical": pair.m_type = HitData.DamageType.Physical; break;
                    case "Elemental": pair.m_type = HitData.DamageType.Elemental; break;
                }

                this.damageMods.Add(pair);
                string modString = $"\n<color=orange>{pair.m_modifier}</color> VS <color=orange>{pair.m_type}</color>";
                appendedTooltip += modString;
            }
        }

        Sprite? icon = sprite;
        if (!icon)
        {
            if (!string.IsNullOrWhiteSpace(spriteName))
            {
                GameObject item = ZNetScene.instance.GetPrefab(spriteName);
                if (!item)
                {
                    SeasonalityLogger.LogWarning($"[{effectName}] : Failed to get prefab: {spriteName}");
                    return null;
                }

                if (item.TryGetComponent(out ItemDrop itemDrop)) icon = itemDrop.m_itemData.GetIcon();
                if (!icon) return null;
            }
        }
        SeasonEffect seasonEffect = ScriptableObject.CreateInstance<SeasonEffect>();
        seasonEffect.name = effectName;
        seasonEffect.data = this;
        seasonEffect.m_icon = _StatusEffectVisible.Value is Toggle.On ? icon : null;
        seasonEffect.m_name = displayName;
        seasonEffect.m_cooldown = duration; // guardian power cool down
        seasonEffect.m_ttl = duration; // status effect cool down
        seasonEffect.m_tooltip = appendedTooltip;
        seasonEffect.m_startMessageType = MessageHud.MessageType.TopLeft;
        seasonEffect.m_stopMessageType = MessageHud.MessageType.TopLeft;
        seasonEffect.m_startMessage = startMsg;
        seasonEffect.m_stopMessage = stopMsg;
        seasonEffect.m_activationAnimation = activationAnimation;
        if (startEffectNames is not null)
        {
            seasonEffect.m_startEffects = CreateEffectList(ZNetScene.instance, startEffectNames.ToList(), effectName);
        }

        if (stopEffectNames is not null)
        {
            seasonEffect.m_stopEffects = CreateEffectList(ZNetScene.instance, stopEffectNames.ToList(), effectName);
        }

        // Add base effect to ObjectDB
        obd.m_StatusEffects.Add(seasonEffect);
        return seasonEffect;
    }
    private static EffectList CreateEffectList(ZNetScene scene, List<string> effects, string seasonalEffectName)
    {
        EffectList list = new();
        List<GameObject> validatedPrefabs = new();

        foreach (string effect in effects)
        {
            if (effect.IsNullOrWhiteSpace()) continue;
            GameObject prefab = scene.GetPrefab(effect);
            if (!prefab)
            {
                SeasonalityLogger.LogDebug($"[{seasonalEffectName}]" + " : " + "Failed to find prefab: " + effect);
                continue;
            }
            // 0 = Default
            // 9 = Character
            // 12 = Item
            // 15 = Static Solid
            // 22 = Weapon
            switch (prefab.layer)
            {
                case 0:
                    prefab.TryGetComponent(out TimedDestruction timedDestruction);
                    prefab.TryGetComponent(out ParticleSystem particleSystem);
                    if (timedDestruction || particleSystem) validatedPrefabs.Add(prefab);
                    break;
                case 8 or 22: validatedPrefabs.Add(prefab); break;
            }
        }

        EffectList.EffectData[] allEffects = new EffectList.EffectData[validatedPrefabs.Count];

        for (int i = 0; i < validatedPrefabs.Count; ++i)
        {
            GameObject fx = validatedPrefabs[i];

            EffectList.EffectData effectData = new EffectList.EffectData()
            {
                m_prefab = fx,
                m_enabled = true,
                m_variant = -1,
                m_attach = true,
                m_inheritParentRotation = true,
                m_inheritParentScale = true,
                m_scale = true,
                m_childTransform = ""
            };
            allEffects[i] = effectData;
        }

        list.m_effectPrefabs = allEffects;

        return list;
    }


}
public class SeasonEffect : StatusEffect
{
    public SeasonalEffect data = null!;
    public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData) => hitData.ApplyModifier(data.Modifiers[Modifier.Attack]);
    public override void ModifyHealthRegen(ref float regenMultiplier) => regenMultiplier *= data.Modifiers[Modifier.HealthRegen];
    public override void ModifyStaminaRegen(ref float staminaRegen) => staminaRegen *= data.Modifiers[Modifier.StaminaRegen];
    public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value) => value *= data.Modifiers[Modifier.RaiseSkills];
    public override void ModifySpeed(float baseSpeed, ref float speed) => speed *= data.Modifiers[Modifier.Speed];
    public override void ModifyNoise(float baseNoise, ref float noise) => noise *= data.Modifiers[Modifier.Noise];
    public override void ModifyStealth(float baseStealth, ref float stealth) => stealth *= data.Modifiers[Modifier.Stealth];
    public override void ModifyMaxCarryWeight(float baseLimit, ref float limit) => limit += data.Modifiers[Modifier.MaxCarryWeight];
    public override void ModifyRunStaminaDrain(float baseDrain, ref float drain) => drain *= data.Modifiers[Modifier.RunStaminaDrain];
    public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse) => staminaUse *= data.Modifiers[Modifier.RunStaminaDrain];
    public override void OnDamaged(HitData hit, Character attacker)
    {
        float mod = Mathf.Clamp01(1f - data.Modifiers[Modifier.DamageReduction]);
        hit.ApplyModifier(mod);
    }
    public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers) => modifiers.Apply(data.damageMods);
    public override void ModifyFallDamage(float baseDamage, ref float damage)
    {
        if (m_character.m_seman.m_statusEffects.Exists(x => x.m_name == "$se_slowfall_name")) return;
        damage = baseDamage * data.Modifiers[Modifier.FallDamage];
        if (damage >= 0.0) return;
        damage = 0.0f;
    }
    public override void ModifyEitrRegen(ref float eitrRegen)
    {
        eitrRegen *= data.Modifiers[Modifier.EitrRegen];
    }
    public override string GetIconText()
    {
        TimeSpan span = SeasonalEffects.GetTimeDifference();
        return span.ToString(span.Days > 0 ? @"d\d hh\:mm\:ss" : span.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");
    }

}