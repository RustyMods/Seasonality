# Changelog

## 3.7.2
- overhauled timer to be scheduled instead of checking if it is time to change season
- added command: `seasonality timer_reset` to reset schedule in order to fix if something interrupts scheduled invocation
- changing timer configs, sleep override, or using dev command `skiptime` will automatically reset schedule
- added debug logs to show when schedule is set
- changed how timer is displayed, if over a day, will show as: `1 day` or `2 days`

## 3.7.1
- minor change to season timer to try to make sure it does not skip seasons

## 3.7.0
- Rebuilt shaders with macOS & Linux settings, maybe works for mac now ?? I don't have mac, so cannot test
- Fixed ice shelves not spawning if config not enabled when game loads
- Tweaked timer to try to make sure it does not skip seasons
- Added winter bjorn texture

## 3.6.23
- Fixed mountain cave dungeon having moss be replaced, should stay snowy

## 3.6.22
- Changed order of operations for gameplay modifiers to make sure configs are adjusted correctly

## 3.6.21
- Small tweak to make sure ice shelves do not spawn if config is off

## 3.6.2
- Fixed issue reading yml for tweaks

## 3.6.1
- Added null check during ZNetScene awake

## 3.6.0
- New custom ice shader
- Removed dark wood winter version from default pack

## 3.5.9
- Minor fix, made sure to read Plants.yml config earlier to make sure it is shared with clients

## 3.5.8
- Fixed textures not reverting to originals if game isn't started as summer

## 3.5.7
- Added new command: `seasonality save [prefabName or empty]`
- `seasonality save` will save to folder all textures as PNG for easier customization
- PNGs will be named `[materialName]#[propertyName]`
- Added beehive interactable tweak configs
- Added Ice Shelves for winter configs
- Fixed HildirLox not being manipulated

## 3.5.6
- Updated Server Sync v1.19
- Materials with same asset name also registered (mods adding materials with the same name as existing materials were previously ignored)
- Added Hildir’s Lox fur winter variant

## 3.5.5
- Added config to fix shaders  

## 3.5.4
- Fix reverted: Added seasonal effects when player wakes up

## 3.5.3
- Null check for body model added

## 3.5.2
- Config to disable Oak Tree random color change
- Added asset loader to improve load times (HD)
- Added seasonal build pieces config (e.g. Xmas tree, Jackoturnip)
- Added seasonal trader items config

## 3.5.1
- Fixed weather configs to show biome options

## 3.5.0
- Removed leftover testing textures

## 3.4.9
- Code readability overhaul
- Fixed Mistlands grass not being affected  


## 3.4.8
- Fixed bugs with configs and season timer

## 3.4.7
- Screen fade check added
- Improved configs for weather
- Added color config for fall colors

## 3.4.6
- Timer now uses config file to save last season change
- Improved terrain color change

## 3.4.5
- Fixed wrong boolean check on season timer

## 3.4.4
- Added missing localized weather keys

## 3.4.3
- "Always cold" no longer applies in Ashlands

## 3.4.2
- Small patch to ensure screen fade readiness

## 3.4.1
- Config to control immunity while fading to black
- Hotfix to prevent fading repeatedly

## 3.4.0
- Changed timer to save last season change to disk

## 3.3.11
- My bad, fixed now

## 3.3.10
- Adjusted threshold to fix seasons switching constantly (?)

## 3.3.9
- Minor adjustment to timer threshold to fix desync issues

## 3.3.8
- Reverted timer to old version
- Added support for custom prefabs with seasonal textures
- Improved water freezing

## 3.3.7
- Season should change even if sleeping past timer

## 3.3.6
- Tweaked season change timer

## 3.3.5
- Added tooltip
- Added warm snow, warm snow storm, night frost environments
- Removed toggle to protect from freezing (use new environments)

## 3.3.4
- Modifiers are back
- More freezing protection

## 3.3.3
- Sleep override is back

## 3.3.2
- Fixed season not changing sometimes

## 3.3.1
- Added new season icons and some tweaks

## 3.3.0
- Removed version check
- Allows server-only sync of seasons

## 3.2.9
- Fixed possible memory leak

## 3.2.8
- Fixed update bugs (hopefully)

## 3.2.7
- Fixed freezing protection

## 3.2.6
- Fixed water collider being on even when not winter

## 3.2.5
- Ashland release
- Cleaned up project, removed fluff

## 3.2.4
- Fixed possible frozen water feature error

## 3.2.3
- Tweaked to prevent seasons being applied twice in multiplayer

## 3.2.2
- Added snow and rain to weather status effect localization

## 3.2.1
- Separated main plugin from textures
- Added feature to disable season by biome (beta)

## 3.2.0
- Overhauled weather system
- Tweaked fall damage modifier (recommended by bid-soft)

## 3.1.9
- Re-apply season status effect when user logs out and back in

## 3.1.8
- Re-apply season status effects after death

## 3.1.7
- Optimizations to reduce stuttering

## 3.1.6
- Fixed ship placement
- Fixed localization of weather tooltip

## 3.1.5
- Fixed weather not changing after boss event
- Improved frozen water

## 3.1.4
- Toggle to replace creature textures
- Mistland rocks moss changes to white during winter

## 3.1.3
- Timer is based off in-game time
- Optimized weather system
- Added "only change season when sleeping" feature

## 3.1.2
- Tweaked frozen water material
- Optimized to reduce possible stutters

## 3.1.1
- Cough cough

## 3.1.0
- Sorry

## 3.0.9
- Beta release of frozen water feature
- Syncing tweaks

## 3.0.8
- Fixed swamp tree branches using swamp grass textures

## 3.0.7
- Fixed season desyncing
- Fixed status effect timer visibility toggles

## 3.0.6
- Client-only server connected timer works properly
- Added seasonal armors

## 3.0.5
- Fixed weather SE disappearing

## 3.0.4
- Added deeper configuration options using YML

## 3.0.3
- Fixed custom barks not being applied
- Added new category: Pickables
- Color blind support for mushrooms and raspberries

## 3.0.2
- Minor fix to Clear Warm Snow environment

## 3.0.1
- WeatherMan redundancies added
- Gull turns into crow for fall

## 3.0.0
- WeatherMan properly changes when you transition biome
- Performance increase (should be near zero FPS impact now)
- More visual tweaks
- Timer visual tweaks

## 2.0.7
- Server synced weather system
- Tweaked textures

## 2.0.6
- Timespan hotfix

## 2.0.5
- Shrudnal compatibility with Auga and Minimal Status Effects by Randyknapp
- Expanded mosses to swamp and plains differentiation
- Plains grass and beyond now compressed using DTX1 and filter point

## 2.0.4
- Fixed timer UI timezone issue

## 2.0.3
- Mistland trees fix
- Timer timezone config added to fix timezone differences

## 2.0.2
- Texture hotfix and timer temp fix

## 2.0.1
- Invariant culture Tmp_Tex hotfix

## 2.0.0
- Texture replacement overhaul
- Commands added
- Warm Snow Weather
- Summer never cold
- Winter always cold
- Might need to delete old configs

## 1.1.2
- Fixed weather config for summer being in wrong spot
- Moved textures out of plugin into config folder
- Lox fur coat turns white for winter

## 1.1.1
- Weather by season by biome configs added — delete old cfgs might be necessary

## 1.1.0
- Visual tweaks and added command to force reload terrain

## 1.0.9
- Custom texture fix

## 1.0.8
- Fixed grass textures, added variety to water lilies and vass, and fixed admin death resetting counter

## 1.0.7
- Weather EnvMan bug fix

## 1.0.6
- Birch tree custom texture fix

## 1.0.5
- Minor improvements and Project Auga compatibility

## 1.0.4
- Fixed server side timer

## 1.0.3
- Tweaked configs

## 1.0.2
- Clutter visual improvements

## 1.0.1
- Minor improvements (Counter does not work well on servers)

## 1.0.0
- Initial release  
