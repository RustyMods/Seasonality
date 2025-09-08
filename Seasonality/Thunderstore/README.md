# Seasonality
Dynamic seasons by altering textures and colors, providing an immersive environmental experience that changes with in-game time.

## 3.4.9 Update Notes
Delete old files

Config Folder: BepinEx/config/Seasonality

You can now specify which root folder the plugin reads, meaning you can have profiles.

This way, I do not need to have a seperate texture pack mod, you can install another texture pack and simply add it to the Config Folder, give its unique folder name, and change the configurations to target that specific folder.

Built-in SeasonalTweaks, toggleable configs, YML files found in Seasonality folder.

## Features
- Four seasons: Winter, Spring, Summer, Fall
- Counter determined by age of world
- Full configurability
- Weather control
- Use of custom textures
- Seasonal modifiers
- Save Textures
## Screenshots
![Imgur](https://i.imgur.com/p57oGRl.png)
![Imgur](https://i.imgur.com/Ba9WmIS.png)
![Imgur](https://i.imgur.com/BQIoncx.png)
## Global Keys
- `season_winter`
- `season_summer`
- `season_spring`
- `season_fall`
## Custom Textures
You can register your own seasonal textures. Simply follow the naming syntax:
```
[material name] [texture property(optional)] [season].png

Examples:
ArmorSilver_Mat#ChestTex@fall.png
ArmorIron_mat@spring.png
```
## Saving Textures
- Using the command: Seasonality save; you can save most in-game textures as PNG to disk
- Using the command: Seasonality save [prefabName<string>]; you can save most material textures associated with prefab

example: Seasonality save Blob

Notes: Best to set season to Summer, to avoid saving materials with modified textures. That is, before loading game, make sure the configs are set to Summer, load into the game, then run the command.

#### Notes
Come find me on OdinPlus Discord in order to share texture files and learn about what the texture needs to look like.

## Contact information
For Questions or Comments, find <span style="color:orange">Rusty</span> in the Odin Plus Team Discord

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/v89DHnpvwS)

Or come find me at the [Modding Corner](https://discord.gg/fB8aHSfA8B)

##
If you enjoy this mod and want to support me:
[PayPal](https://paypal.me/mpei)

<span>
<img src="https://i.imgur.com/rbNygUc.png" alt="" width="150">
<img src="https://i.imgur.com/VZfZR0k.png" alt="https://www.buymeacoffee.com/peimalcolm2" width="150">
</span>

## Environments
```
Clear
Twilight_Clear
Misty
Darklands_dark
Heath clear
DeepForest Mist
GDKing
Rain
LightRain
ThunderStorm
Eikthyr
GoblinKing
nofogts
SwampRain
Bonemass
Snow
Twilight_Snow
Twilight_SnowStorm
SnowStorm
Moder
Crypt
SunkenCrypt
Caves
CavesHildir
Mistlands_rain
Mistlands_thunder
InfectedMine
Queen
CryptHildir
Ashlands_ashrain
Ashlands_ashrain_clear
Ashlands_storm
Ashlands_meteorshower
Ashlands_misty
Ashlands_CinderRain
Ashlands_SeaStorm
Fader
WarmSnow
WarmSnowStorm
NightFrost
```
