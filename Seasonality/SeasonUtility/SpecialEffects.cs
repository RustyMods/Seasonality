namespace Seasonality.SeasonUtility;

public static class SpecialEffects
{
    public enum SpecialEffect
    {
        None,
        DvergerPower,
        MistileExplosion,
        HealthUpgrade,
        PreSpawn,
        BlackCoreFireWork,
        RocketFireWork,
        BlueRocketFireWork,
        CyanRocketFireWork,
        GreenRocketFireWork,
        PurpleRocketFireWork,
        RedRocketFireWork,
        YellowRocketFireWork,
        SurtlingCoreFireWork,
        ThunderStoneFireWork,
        GodExplosion,
        Hearts,
        HealthPotion,
        EitrPotion,
        StaminaPotion,
        // UndeadBurn,
        PheromoneBomb,
        
    }

    public static string GetEffectPrefabName(SpecialEffect option)
    {
        return option switch
        {
            SpecialEffect.DvergerPower => "fx_DvergerMage_Support_start",
            SpecialEffect.MistileExplosion => "fx_DvergerMage_Mistile_die",
            SpecialEffect.HealthUpgrade => "vfx_HealthUpgrade",
            SpecialEffect.PreSpawn => "vfx_prespawn",
            SpecialEffect.BlackCoreFireWork => "vfx_FireWork_BlackCore",
            SpecialEffect.RocketFireWork => "vfx_Firework_Rocket",
            SpecialEffect.BlueRocketFireWork => "vfx_Firework_Rocket_Blue",
            SpecialEffect.CyanRocketFireWork => "vfx_Firework_Rocket_Cyan",
            SpecialEffect.GreenRocketFireWork => "vfx_Firework_Rocket_Green",
            SpecialEffect.PurpleRocketFireWork => "vfx_Firework_Rocket_Purple",
            SpecialEffect.RedRocketFireWork => "vfx_Firework_Rocket_Red",
            SpecialEffect.YellowRocketFireWork => "vfx_Firework_Rocket_Yellow",
            SpecialEffect.SurtlingCoreFireWork => "vfx_FireWork_SurtlingCore",
            SpecialEffect.ThunderStoneFireWork => "vfx_FireWork_ThunderStone",
            SpecialEffect.GodExplosion => "vfx_GodExplosion",
            SpecialEffect.Hearts => "vfx_lox_love",
            SpecialEffect.HealthPotion => "vfx_Potion_health_medium",
            SpecialEffect.EitrPotion => "vfx_Potion_eitr_minor",
            SpecialEffect.StaminaPotion => "vfx_Potion_stamina_medium",
            // SpecialEffect.UndeadBurn => "vfx_UndeadBurn",
            SpecialEffect.PheromoneBomb => "fx_pheromonebomb_explode",
            _ => ""
        };

    }
}