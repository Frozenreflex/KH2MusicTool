# KH2MusicTool

A cross-platform version (music only, for now) of Xaddgx's KH2 Music Tool, made in Godot.

Tested with the [FFVII mod](https://youtu.be/26YUc2Rm7QY), but should work for vanilla. It might work for Aqua, but I don't have access to it.

Copy the ``OST`` folder from the original music tool, and put it next to this tool's executable (``KH2MusicTool.x86_64`` on Linux)

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