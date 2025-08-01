# LethalHUD

Started with maintaining ScanRecolor that quickly got out of hand (oops), I will maintain that for people who want a simpler mod.

Currently this is a ScanRecolor rework with some fixes, custom textures, new configs, and also integrated Hold Scan Button into it.
This mod will be a bit larger as I mess with it in my freetime. Hope you guys will enjoy it!

Notible differences compared to ScanRecolor:
- HEX format for ScanColor which lets players use a colorwheel in lethalconfig and gale as well
- Fixes an unnoticed issue when you enabled RecolorScanLines - it reformatted them to Linear instead of sRGB causing the default scanline be less visible
- Hold Scan Button integration, which works well with RandomColor config
- RandomColor config option is slightly modified on what colorrange it can choose from
- new custom scanline textures which work with all other config options
- added dirtIntensity config option for the scanline texture visibility ranging from -500 to 500 (float), Default scanline uses 352.08 and so you can either increase it or decrease it (goes down to 0, cant go negative), while custom ones use 42 as default

Everything can be changed mid-game ^^

## Credits to

- Niro for creating ScanRecolor per my request and letting me continue it