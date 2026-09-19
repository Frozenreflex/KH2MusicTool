# KH2MusicTool

A cross-platform version (music only, for now) of Xaddgx's KH2 Music Tool, made in Godot.

Currently works on Windows, Linux, and Android. Android only works on devices with multiple screens, such as the Ayn Thor, and the music tool should go on the second screen.

Tested with the [FFVII mod](https://youtu.be/26YUc2Rm7QY), but should work for vanilla. It might work for Aqua, but I don't have access to it.

On Linux, copy the ``OST`` folder from the original music tool, and put it next to this tool's executable (``KH2MusicTool.x86_64`` on Linux)

```
KH2MusicTool/
├─ data_KH2MusicTool_linuxbsd_x86_64/
├─ OST/
│  ├─ boss_dance.ogg
│  ├─ boss_desire.ogg
│  ├─ ...
├─ KH2MusicTool.pck
├─ KH2MusicTool.x86_64
```

On Android, the process is slightly more involved. You'll need to create a zip file containing the OST, that looks something like this

```
ZippedSoundtrack.zip (this is your zip file)/
├─ OST/
│  ├─ boss_dance.ogg
│  ├─ boss_desire.ogg
│  ├─ ...
```
Then, inside the tool will be a button to import this zip file. If you want to modify the music, you can re-import the zip file.

## FFVII Linux Setup Notes

1. Use the [latest](https://pcsx2.net/downloads/) Linux version of PCSX2, I use the AppImage, the Flatpak may work but you'll likely have to change permissions with Flatseal
2. Extract the mod archive with the NON-FREE version of unrar, unrar-free has problems extracting and may cause patching issues
3. Run the patcher with Wine ``wine KH2FM_Toolkit.exe "<name of the patch file>"``
   * If you are on Steam Deck, or if you encounter issues in-game, you may have to run it under Proton instead
4. Copy over the cheats folder from the mod to PCSX2's data folder, with the AppImage this is under ``.config/PCSX2`` in your home folder.
5. If you haven't used PCSX2 before, also copy the BIOS files over
6. Enable cheats ``Settings -> Emulation -> System Settings -> Enable Cheats``
7. Enable advanced settings ``Tools -> Show Advanced Settings``
8. Enable PINE ``Settings -> Advanced (not in dropdown) -> PINE Settings -> Enable`` and ensure Slot is set to ``28011``

## FFVII Android Setup Notes

You'll have to patch KH2 on a PC. This section assumes you've already got a patched copy of KH2, and have transfered it and any required mod files to your device (via something like Warpinator).

1. I used ARMSX2. Other emulators may work, but I've only tried ARMSX2, and these notes assume you're using ARMSX2.
2. Complete the ARMSX2 setup, make sure to set it so that the app data path is accessible (not internal)
3. Copy over the cheats folder from the mod 
4. Enable cheats and PINE, PINE will be under Advanced Settings
5. Set up the tool and ARMSX2 to launch on both screens in your app launcher. With Cocoon, this can be done by first setting up the PS2 integration (if you haven't already), scanning for games, then setting the companion app for KH2 to the music tool.