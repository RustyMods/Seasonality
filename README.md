# Seasonality
Dynamic seasons to the Valheim game by altering textures and colors, providing an immersive environmental experience that changes with in-game time.
## Features
- Four seasons: Winter, Spring, Summer, Fall
- Counter determined by age of world
- Full configurability
- Status Effects depending on season
- Weather control
- Use of custom textures
- Season global keys
## Screenshots
![Imgur](https://i.imgur.com/p57oGRl.png)
![Imgur](https://i.imgur.com/Ba9WmIS.png)
![Imgur](https://i.imgur.com/BQIoncx.png)
## Configurations
- Control configuration enables user to decide season at will
- Current season displays season and is a drop down choice for user
- Player modifiers can be toggled on and off
- Seasonal weather can be toggled on and off
- Modifiers can be customized
## Status Effects
![Imgur](https://i.imgur.com/rgf8CDj.png)
## Color
This plugin color tints the materials in the scene, meaning any color choices you apply will almost always darken the object. In order to lighten an object, it is necessary to replace the texture. For example, most of the winter objects are texture replaced meaning any color choices you make won't be used.
## Custom Textures
The plugin allows to use of custom textures by adding png files into the designated directories within your configuration folder in bepinex.
## Global Keys
- `season_winter`
- `season_summer`
- `season_spring`
- `season_fall`
#### File structure
```
BepInEx
 - config
  - Seasonality
   - Textures
    - Beech
    - BeechSmall
        - spring.png
        - summer.png
        - winter.png
        - fall.png
    - Birch
    - ...
```
The names of the png files must follow the `summer.png` syntax in order for the plugin to recognize it.
#### Notes
Come find me on OdinPlus Discord in order to share texture files and learn about what the texture needs to look like.
### Texture Example
![Imgur](https://i.imgur.com/rsACu9K.png)
![Imgur](https://i.imgur.com/lagODPg.png)
![Imgur](https://i.imgur.com/dATEKVF.png)
These are the images used to modify `Beech` trees in the meadows, `spring.png` and `winter.png`, the last one is for `BeechSmall` tree `spring.png`
