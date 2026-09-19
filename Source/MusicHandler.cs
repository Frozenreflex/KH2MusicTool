using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using FileAccess = Godot.FileAccess;

namespace KH2MusicTool.Source;

//all code here is based on the decompiled code of xad's music tool

public partial class MusicHandler : Node
{
    [Export] public AudioStreamPlayer AudioPlayer;
    [Export] public TrackUIHandler TrackUiHandler;
    [Export] public Button SetMusicLocationButton;
    [Export] public Label DebugLabel;

    private static readonly Dictionary<uint, string> StandardBattleMusic = new()
    {
        { 0x00, "OST/nothing.ogg" },
        { 0x01, "OST/nothing.ogg" },
        { 0x33, "OST/fieldb_twilight_dive.ogg" },
        { 0x39, "OST/fieldb_port.ogg" },
        { 0x41, "OST/fieldb_christmas.ogg" },
        { 0x68, "OST/fieldb_beast.ogg" },
        { 0x6f, "OST/boss_shadows.ogg" },
        { 0x70, "OST/fieldb_dragon.ogg" },
        { 0x8f, "OST/ex_fieldb_dizneecastle.ogg" },
        { 0x95, "OST/fieldb_halloween.ogg" },
        { 0x99, "OST/fieldb_hollow.ogg" },
        { 0x9b, "OST/fieldb_port.ogg" },
        { 0xbb, "OST/fieldb_pride.ogg" },
        { 0xbe, "OST/fieldb_time.ogg" },
        { 0xc1, "OST/ex_fieldb_secret.ogg" },
        { 0xc4, "OST/ex_boss_masked.ogg" },
        { 0xd0, "OST/ex_boss_blind.ogg" },
        { 0xd1, "OST/ex_field_preparation.ogg" },
        { 0xd7, "OST/ex_fieldb_wonders.ogg" },
        { 0xd9, "OST/ex_boss_unused2.ogg" },
        { 0xdd, "OST/ex_fieldb_space2.ogg" },
        { 0xde, "OST/ex_boss_longbattle2.ogg" },
        { 0xe4, "OST/ex_boss_space.ogg" },
        { 0xe6, "OST/ex_fieldb_dragon2.ogg" },
        { 0xe8, "OST/ex_boss_longbattle3.ogg" },
    };

    private static readonly Dictionary<uint, string> StandardFieldMusic = new()
    {
        { 0x32, "OST/field_twilight_dive.ogg" },
        { 0x33, "OST/fieldb_twilight_dive.ogg" },
        { 0x36, "OST/field_never.ogg" },
        { 0x38, "OST/field_port.ogg" },
        { 0x39, "OST/fieldb_port.ogg" },
        { 0x3c, "OST/boss_final2.ogg" },
        { 0x3d, "OST/boss_final3.ogg" },
        { 0x41, "OST/fieldb_christmas.ogg" },
        { 0x42, "OST/boss_roxas.ogg" },
        { 0x43, "OST/boss_terra.ogg" },
        { 0x44, "OST/field_cavern.ogg" },
        { 0x52, "OST/boss_minigame.ogg" },
        { 0x54, "OST/fieldb_agrabah_escape.ogg" },
        { 0x55, "OST/fieldb_space_cycle.ogg" },
        { 0x58, "OST/scene_sora.ogg" },
        { 0x59, "OST/scene_friends.ogg" },
        { 0x5a, "OST/scene_riku.ogg" },
        { 0x5b, "OST/scene_kairi.ogg" },
        { 0x5c, "OST/scene_andante.ogg" },
        { 0x5d, "OST/scene_villains.ogg" },
        { 0xa2, "OST/scene_villains.ogg" },
        { 0xa3, "OST/scene_villains.ogg" },
        { 0x5e, "OST/scene_xiii.ogg" },
        { 0x5f, "OST/scene_apprehension.ogg" },
        { 0x60, "OST/scene_courage.ogg" },
        { 0x61, "OST/scene_laughter.ogg" },
        { 0x62, "OST/scene_hesitation.ogg" },
        { 0x63, "OST/scene_missing.ogg" },
        { 0x67, "OST/field_olympus.ogg" },
        { 0x68, "OST/fieldb_beast.ogg" },
        { 0x6e, "OST/boss_diznee.ogg" },
        { 0x6f, "OST/boss_shadows.ogg" },
        { 0x70, "OST/fieldb_dragon.ogg" },
        { 0x72, "OST/boss_tension.ogg" },
        { 0x74, "OST/field_dragon.ogg" },
        { 0x78, "OST/boss_tournament.ogg" },
        { 0x81, "OST/field_atlantica.ogg" },
        { 0x84, "OST/scene_beast.ogg" },
        { 0x85, "OST/field_tower.ogg" },
        { 0x8d, "OST/field_acrewood.ogg" },
        { 0x8f, "OST/field_dizneecastle.ogg" },
        { 0x90, "OST/field_halloween.ogg" },
        { 0x92, "OST/scene_roxas.ogg" },
        { 0x95, "OST/fieldb_halloween.ogg" },
        { 0x98, "OST/field_hollow.ogg" },
        { 0x99, "OST/fieldb_hollow.ogg" },
        { 0x9a, "OST/field_port.ogg" },
        { 0x9b, "OST/fieldb_port.ogg" },
        { 0x9e, "OST/boss_minigame_pooh.ogg" },
        { 0x9f, "OST/boss_minigame_pooh.ogg" },
        { 0xa9, "OST/placeholder.ogg" },
        { 0xaf, "OST/placeholder2.ogg" },
        { 0xbb, "OST/fieldb_pride.ogg" },
        { 0xbc, "OST/boss_sephiroth_main.ogg" },
        { 0xbd, "OST/field_time.ogg" },
        { 0xbe, "OST/fieldb_time.ogg" },
        { 0xbf, "OST/ex_boss_marluxia.ogg" },
        { 0xc0, "OST/ex_field_secret.ogg" },
        { 0xc1, "OST/ex_fieldb_secret.ogg" },
        { 0xc3, "OST/ex_boss_diznee_alt.ogg" },
        { 0xc4, "OST/ex_boss_masked.ogg" },
        { 0xc5, "OST/ex_field_static.ogg" },
        { 0xc6, "OST/ex_boss_lost.ogg" },
        { 0xc7, "OST/ex_boss_city.ogg" },
        { 0xc8, "OST/ex_boss_xigbar.ogg" },
        { 0xc9, "OST/ex_boss_256.ogg" },
        { 0xca, "OST/ex_field_wonders.ogg" },
        { 0xcb, "OST/ex_boss_ravine.ogg" },
        { 0xcc, "OST/ex_field_sadness.ogg" },
        { 0xcd, "OST/ex_boss_longbattle.ogg" },
        { 0xce, "OST/ex_boss_despair.ogg" },
        { 0xd0, "OST/ex_boss_blind.ogg" },
        { 0xd1, "OST/ex_field_preparation.ogg" },
        { 0xd2, "OST/ex_field_space2.ogg" },
        { 0xd3, "OST/boss_finalall.ogg" },
        { 0xd4, "OST/ex_boss_origin1.ogg" },
        { 0xd5, "OST/ex_boss_origin2.ogg" },
        { 0xd6, "OST/ex_boss_originall.ogg" },
        { 0xd7, "OST/ex_fieldb_wonders.ogg" },
        { 0xd8, "OST/streamed_titlescreen.ogg" },
        { 0xd9, "OST/ex_boss_unused2.ogg" },
        { 0xda, "OST/streamed_endscreen.ogg" },
        { 0xdb, "OST/boss_sephiroth_main.ogg" },
        { 0xdc, "OST/ex_boss_unused3.ogg" },
        { 0xdd, "OST/ex_fieldb_space2.ogg" },
        { 0xde, "OST/ex_boss_longbattle2.ogg" },
        { 0xdf, "OST/ex_boss_memories.ogg" },
        { 0xe0, "OST/ex_field_pride2.ogg" },
        { 0xe1, "OST/ex_ending_purpose.ogg" },
        { 0xe2, "OST/ex_field_text.ogg" },
        { 0xe3, "OST/ex_field_shop.ogg" },
        { 0xe4, "OST/ex_boss_space.ogg" },
        { 0xe5, "OST/ex_field_death.ogg" },
        { 0xe6, "OST/ex_fieldb_dragon2.ogg" },
        { 0xe7, "OST/ex_field_death2.ogg" },
        { 0xe8, "OST/ex_boss_longbattle3.ogg" },
        { 0xe9, "OST/ex_field_tutorial.ogg" },
        { 0xea, "OST/ex_field_beast2.ogg" },
        { 0xeb, "OST/ex_boss_challenge.ogg" },
        { 0xec, "OST/ex_boss_red.ogg" },
        { 0xed, "OST/ex_field_christmas2.ogg" },
        { 0xee, "OST/ex_field_rod_despair.ogg" },
        { 0xef, "OST/ex_field_under2.ogg" },
        { 0xf0, "OST/ex_field_rod_horror.ogg" },
        { 0xf1, "OST/ex_field_rod_forward.ogg" },
        { 0xf2, "OST/ex_field_prelude.ogg" },
        { 0xf3, "OST/ex_field_book.ogg" },
        { 0xf4, "OST/ex_boss_destined.ogg" },
        { 0xf5, "OST/ex_boss_barbossa.ogg" },
        { 0xf6, "OST/ex_boss_datafinalxemnas.ogg" },
    };
    
    //yes, this is where i put the music tool on my computer
    private const string FallbackPath = "/mnt/LocalDisk2/ProgramFiles/Development/kh2fmffvii/Custom Music Tool";
    private string _automaticPath = OS.GetExecutablePath().GetBaseDir();
    private bool IsEditor = OS.HasFeature("editor");
    private bool IsAndroid = OS.GetName() == "Android";
    private string _androidMusicPath;

    private const string AndroidMusicRoot = "user://music/";

    public string SoundtrackPath
    {
        get
        {
            if (IsEditor) return FallbackPath;
            if (IsAndroid) return AndroidMusicRoot;
            return _automaticPath;
        }
    }

    private bool Dead;
    private bool AquaISO;
    private bool DeathCheck;
    private bool GlobalTrashTalk;
    private bool SongSwapAllowed = true;
    private bool MusicIsHalted;
    private bool StopOggDeath;
    private bool SlowSaveState;
    private uint SaveSongState;
    private int GlobalSongCheck;
    private bool WasSongStateSaved;
    private bool StopOgg;
    private bool StopOggInstant;
    private byte ManualSong;
    private byte ManualSongState;
    private byte ManualSongTracker;
    private byte ManualSongStateTracker;
    private bool Training;
    private byte PreviousAutoSong;
    private bool DeathThemeOn;
    private bool ModifySongSaveStateWithOgg;
    private bool UseCopyrightFree;
    private bool PhantomAquaTheme;

    private readonly ConcurrentQueue<string> _logQueue = new();

    private string _currentSong;
    private string _queuedSong;

    private void EnqueueSong(string path)
    {
        _logQueue.Enqueue($"Enqueuing {path}");
        _queuedSong = path;
    }

    private void EnqueueCopywrittenSong(string copyWrittenPath, string fallbackPath)
    {
        _queuedSong = UseCopyrightFree ? fallbackPath : copyWrittenPath;
    }
    private async Task Run()
    {
        _logQueue.Enqueue("Starting thread");

        var started = false;
        var OnTitle = true;
        var CycleSuccess = false;
        var WasOnTitle = false;
        var IsSongActive = false;
        var IsWaitingInCutscene = false;
        byte OldCombatCheck = 0;

        ushort IsSongActiveInGameCheck = 0;
        uint oldmusic2 = 0;
        uint oldmusic = 0;

        while (true)
        {
            await Task.Delay(100);

            var emulator = Main.Instance.Emulator;

            if (!emulator.Connected)
            {
                //_logQueue.Enqueue("Waiting for emulator...");
                continue;
            }

            var music = emulator.Get32(0x347d34);
            var music2 = emulator.Get32(0x347d44);
            var combatCheck = emulator.Get8(0x347d55);
            var healthCheck = emulator.Get32(0x1c6c750);
            var roomCheck = emulator.Get16(0x32bae0);
            var roomCheck2 = emulator.Get16(0x32bae4);
            var isRoxas6ThDay = emulator.Get16(0x347d38) != 0;

            if (roomCheck == 0x202 && roomCheck2 == 0x3f)
            {
                emulator.Write32(0x1c6c750, 1);
            }

            if (music == 1 && music2 == 0x77) started = true;
            else started = false;

            if (healthCheck == 0 && !Dead && !OnTitle)
            {
                _logQueue.Enqueue("I'm dead.");
                IsWaitingInCutscene = false;
                Dead = true;
                if (CycleSuccess)
                {
                    //TODO: there's a bunch of logic here for aqua mix global trash talk, it has a lot of gotos
                    //and i don't feel like fixing it here since it's really ugly and i dont have access to aqua

                    DeathCheck = true;
                    if (!SongSwapAllowed) MusicIsHalted = true;
                    StopOggDeath = true;
                }
            }

            if (healthCheck == 0 && !Dead && OnTitle && !WasOnTitle)
            {
                _logQueue.Enqueue("I am on the title screen. Waiting for the game to start.");
                EnqueueSong("OST/nothing.ogg");
                WasOnTitle = true;
                SlowSaveState = true;
            }

            if (healthCheck != 0 && !Dead && !OnTitle && !isRoxas6ThDay && !IsSongActive && SongSwapAllowed)
            {
                SaveSongState = 0;
                GlobalSongCheck = 0;
                WasSongStateSaved = false;
                IsSongActive = true;
                _logQueue.Enqueue("Now waiting in a cutscene for music to trigger...");
                if (IsWaitingInCutscene)
                {
                    StopOgg = true;
                    await Task.Delay(100);
                }

                EnqueueSong("OST/nothing.ogg");
            }

            if (healthCheck != 0 && !Dead && !OnTitle && IsSongActive && isRoxas6ThDay &&
                SongSwapAllowed)
            {
                IsSongActive = false;
                IsWaitingInCutscene = false;
                _logQueue.Enqueue("Cutscene has now started music, triggering it.");
                StopOggInstant = true;
            }

            if (ManualSong != 0)
            {
                if (Training)
                {
                    PreviousAutoSong = emulator.Get8(0x347d34);
                    await Task.Delay(300);
                    emulator.Write8(0x347d34, ManualSong);
                    emulator.Write8(0x347d44, ManualSongState);
                    ManualSongTracker = ManualSong;
                    ManualSongStateTracker = ManualSongState;
                    ManualSong = 0;
                    ManualSongState = 0;
                }
                else
                {
                    emulator.Write8(0x347d34, ManualSong);
                    emulator.Write8(0x347d44, ManualSongState);
                    ManualSongTracker = ManualSong;
                    ManualSongStateTracker = ManualSongState;
                    ManualSong = 0;
                    ManualSongState = 0;
                    _logQueue.Enqueue("Song manually selected.");
                }
            }

            if (MusicIsHalted && Dead && healthCheck != 0)
            {
                Dead = false;
                _logQueue.Enqueue("Resuming forced song after death.");
            }

            if ((oldmusic != music || OldCombatCheck != combatCheck ||
                 (oldmusic2 != music2 && music2 != 0) || !IsWaitingInCutscene || Dead || WasOnTitle) &&
                healthCheck != 0 && isRoxas6ThDay && !IsSongActive && SongSwapAllowed)
            {
                _logQueue.Enqueue("Must play new music.");
                if (DeathThemeOn)
                {
                    StopOgg = true;
                    DeathCheck = false;
                    DeathThemeOn = false;
                    await Task.Delay(50);
                }

                if (combatCheck == '\x01' && IsWaitingInCutscene && SongSwapAllowed)
                {
                    ModifySongSaveStateWithOgg = true;
                    GlobalSongCheck = 1;
                    WasSongStateSaved = false;
                }

                if (combatCheck == '\0' && IsWaitingInCutscene && SongSwapAllowed)
                {
                    GlobalSongCheck = 0;
                    WasSongStateSaved = true;
                    _logQueue.Enqueue("Loading saved song position.");
                }

                if (combatCheck == '\x02' && IsWaitingInCutscene && SongSwapAllowed)
                {
                    SaveSongState = 0;
                    GlobalSongCheck = 2;
                    WasSongStateSaved = false;
                    _logQueue.Enqueue("Scripted battle, resetting saved song position.");
                }

                if (IsWaitingInCutscene && !Dead)
                {
                    StopOgg = true;
                    _logQueue.Enqueue("I have stopped previous music.");
                }

                Dead = false;
                if (combatCheck == '\x01')
                {
                    OnTitle = false;
                    CycleSuccess = true;
                    WasOnTitle = false;

                    if (StandardBattleMusic.TryGetValue(music2, out var mus))
                    {
                        EnqueueSong(mus);
                    }
                    else
                    {
                        switch (music2)
                        {
                            case 0x35:
                                EnqueueSong(AquaISO
                                    ? "OST/fieldb_twilight.ogg"
                                    : "OST/KH2FM_Override/fieldb_twilight.ogg");
                                break;
                            case 0x37:
                                EnqueueCopywrittenSong("OST/fieldb_never.ogg", "OST/ex_fieldb_secret.ogg");
                                break;
                            case 0x45:
                                EnqueueSong(AquaISO ? "OST/fieldb_cavern.ogg" : "OST/KH2FM_Override/fieldb_cavern.ogg");
                                break;
                            case 0x66:
                                EnqueueSong(roomCheck is 0xb06 or 0x1006 or 0x1106 or 0xc06 ? "OST/ex_fieldb_under2.ogg" : "OST/fieldb_under.ogg");
                                break;
                            case 0x77:
                                EnqueueSong(AquaISO ? "OST/fieldb_twilight_r.ogg" : "OST/KH2FM_Override/fieldb_twilight_r.ogg");
                                break;
                            case 0x80:
                                EnqueueSong(roomCheck is 0x707 or 0x907 or 0xa07 or 0xb07 or 0xc07 or 0xd07 ? "OST/ex_fieldb_wonders.ogg" : "OST/fieldb_agrabah.ogg");
                                break;
                            case 0x86:
                            {
                                if (Training)
                                {
                                    ManualSong = ManualSongTracker;
                                    ManualSongState = ManualSongStateTracker;
                                    EnqueueSong("OST/nothing.ogg");
                                }
                                else
                                {
                                    EnqueueSong("OST/fieldb_tower.ogg");
                                }
                                break;
                            }
                            case 0x88:
                                EnqueueSong(emulator.Get16(0x32ed94) == 1
                                    ? "OST/ex_fieldb_space2.ogg"
                                    : "OST/fieldb_space.ogg");
                                break;
                        }
                    }
                }
                else
                {
                    OnTitle = false;
                    CycleSuccess = true;
                    WasOnTitle = false;

                    if (StandardFieldMusic.TryGetValue(music, out var mus))
                    {
                        EnqueueSong(mus);
                    }
                    else
                    {
                        switch (music)
                        {
                            case 0x34:
                                EnqueueSong(
                                    AquaISO ? "OST/field_twilight.ogg" : "OST/KH2FM_Override/field_twilight.ogg");
                                break;
                            case 0x35:
                                EnqueueSong(AquaISO
                                    ? "OST/fieldb_twilight.ogg"
                                    : "OST/KH2FM_Override/fieldb_twilight.ogg");
                                break;
                            case 0x37:
                                EnqueueCopywrittenSong("OST/fieldb_never.ogg", "OST/ex_fieldb_secret.ogg");
                                break;
                            case 0x3b:
                                EnqueueCopywrittenSong("OST/boss_final1.ogg", "OST/boss_final3.ogg");
                                break;
                            case 0x3e:
                                EnqueueSong((roomCheck == 0x1412 && roomCheck2 is 0x69 or 0x62)
                                    ? "OST/ex_boss_datafinalxemnas.ogg"
                                    : "OST/boss_final4.ogg");
                                break;
                            case 0x3f:
                                if (AquaISO)
                                    EnqueueSong(roomCheck == 0x2604
                                        ? "OST/ex_boss_marluxia.ogg"
                                        : "OST/boss_reflection.ogg");
                                else
                                    EnqueueSong(roomCheck is 0x2604 or 0x2904
                                        ? "OST/ex_boss_marluxia.ogg"
                                        : "OST/KH2FM_Override/boss_reflection.ogg");

                                break;
                            case 0x40:
                                EnqueueSong(emulator.Get16(0x32ed9c) == 1
                                    ? "OST/ex_field_christmas2.ogg"
                                    : "OST/field_christmas.ogg");
                                break;
                            case 0x45:
                                EnqueueSong(AquaISO
                                    ? "OST/fieldb_cavern.ogg"
                                    : "OST/KH2FM_Override/fieldb_cavern.ogg");
                                break;
                            case 0x57:
                                EnqueueSong(roomCheck == 0x1312 && roomCheck2 is 0x67 or 0x61
                                    ? "OST/ex_boss_dataxemnas1.ogg"
                                    : "OST/boss_xemnas1.ogg");
                                break;
                            case 0x64:
                                EnqueueSong(roomCheck is 0xb06 or 0x1006 or 0x1106 or 0xc06
                                    ? "OST/ex_field_under2.ogg"
                                    : "OST/field_under.ogg");
                                break;
                            case 0x65:
                                EnqueueSong(emulator.Get16(0x32ed9c) == 1
                                    ? "OST/ex_field_beast2.ogg"
                                    : "OST/field_beast.ogg");
                                break;
                            case 0x66:
                                switchD_0042bab5_caseD_66:
                                EnqueueSong(roomCheck is 0xb06 or 0x1006 or 0x1106 or 0xc06
                                    ? "OST/ex_fieldb_under2.ogg"
                                    : "OST/fieldb_under.ogg");
                                break;
                            case 0x73:
                            {
                                var check = emulator.Get8(0x32bae0);
                                if (check == '\x10')
                                {
                                    EnqueueSong("OST/ex_boss_barbossa.ogg");
                                }
                                else if (check == '\x11')
                                {
                                    EnqueueSong("OST/ex_boss_program.ogg");
                                }
                                else
                                {
                                    EnqueueSong("OST/boss_heartless.ogg");
                                }

                                break;
                            }
                            case 0x75:
                                EnqueueSong(emulator.Get8(0x32bae0) == '\x10'
                                    ? "OST/boss_dance.ogg"
                                    : "OST/boss_rowdy.ogg");
                                break;
                            case 0x76:
                                EnqueueSong(
                                    AquaISO
                                        ? "OST/field_twilight_r.ogg"
                                        : "OST/KH2FM_Override/field_twilight_r.ogg");
                                break;
                            case 0x77:
                                EnqueueSong(AquaISO
                                    ? "OST/fieldb_twilight_r.ogg"
                                    : "OST/KH2FM_Override/fieldb_twilight_r.ogg");
                                break;
                            case 0x79:
                            {
                                var check = emulator.Get8(0x32bae0);
                                if (check == '\x05')
                                {
                                    EnqueueSong("OST/ex_boss_256.ogg");
                                }
                                else if (check == '\a')
                                {
                                    EnqueueSong("OST/boss_heartless.ogg");
                                }
                                else
                                {
                                    EnqueueSong("OST/boss_desire.ogg");
                                }

                                break;
                            }
                            case 0x7f:
                                EnqueueSong(roomCheck is 0x707 or 0x907 or 0xa07 or 0xb07 or 0xc07 or 0xd07
                                    ? "OST/ex_field_wonders.ogg"
                                    : "OST/field_agrabah.ogg");
                                break;
                            case 0x80:
                                switch (roomCheck)
                                {
                                    case 0x907:
                                        EnqueueSong("OST/ex_field_wonders.ogg");
                                        break;
                                    case 0xd07:
                                        EnqueueSong("OST/ex_fieldb_wonders.ogg");
                                        break;
                                    default:
                                        EnqueueSong("OST/fieldb_agrabah.ogg");
                                        break;
                                }

                                break;
                            case 0x83:
                                EnqueueSong(roomCheck == 0xb05 ? "OST/boss_heartless.ogg" : "OST/boss_dance.ogg");
                                break;
                            case 0x86:
                                if (Training)
                                {
                                    ManualSong = ManualSongTracker;
                                    ManualSongState = ManualSongStateTracker;
                                    EnqueueSong("OST/nothing.ogg");
                                }
                                else
                                {
                                    EnqueueSong("OST/fieldb_tower.ogg");
                                }

                                break;
                            case 0x87:
                                EnqueueSong(emulator.Get16(0x32ed94) == 1
                                    ? "OST/ex_field_space2.ogg"
                                    : "OST/field_space.ogg");
                                break;
                            case 0x88:
                                if (roomCheck == 0x211 || (roomCheck == 0x411 && roomCheck2 == 0x38))
                                    EnqueueSong("OST/ex_fieldb_space2.ogg");
                                else
                                    EnqueueSong("OST/fieldb_space.ogg");
                                break;
                            case 0x89:
                                EnqueueCopywrittenSong("OST/field_worldmap.ogg", "OST/ex_field_text.ogg");
                                break;
                            case 0x91:
                                switch (roomCheck)
                                {
                                    case 0x1004:
                                        EnqueueSong("OST/ex_boss_ravine.ogg");
                                        break;
                                    case 0xa10:
                                        EnqueueSong("OST/ex_boss_barbossa.ogg");
                                        break;
                                    default:
                                        EnqueueSong("OST/boss_vigor.ogg");
                                        break;
                                }

                                break;
                            case 0x97:
                                EnqueueSong(roomCheck == 0x2802 ? "OST/ex_boss_destined.ogg" : "OST/boss_struggle.ogg");
                                break;
                            case 0xa4:
                                if (!AquaISO)
                                {
                                    EnqueueSong("OST/boss_minigame.ogg");
                                    goto switchD_0042bab5_caseD_66;
                                }

                                EnqueueSong("OST/fieldb_olympus.ogg");
                                break;
                            case 0xb9:
                                switch (roomCheck)
                                {
                                    case 0xa12:
                                        EnqueueSong("OST/ex_boss_xigbar.ogg");
                                        break;
                                    case 0xd05 or 0xf05:
                                        EnqueueSong("OST/ex_boss_challenge.ogg");
                                        break;
                                    default:
                                        EnqueueSong("OST/boss_dilemma.ogg");
                                        break;
                                }

                                break;
                            case 0xba:
                                EnqueueSong(emulator.Get16(0x32ed9c) == 1 ? "OST/ex_field_pride2.ogg" : "OST/field_pride.ogg");
                                break;
                            case 0xc2:
                                PhantomAquaTheme = true;
                                EnqueueSong("OST/ex_boss_phantom.ogg");
                                break;
                            case 0xcf:
                                EnqueueSong("OST/ex_field_creepy.ogg");
                                /*
                                _logQueue.Enqueue(
                                    ".larips drawnwod eht si evah ew lla tub ,derit nehw ot kcab og ot deb elbatrofmoc r ieht evah ylsuoicarg nopu dekool esohT .ti timda ot pu kcuts oot tsuj syawla si yrot s eht fo oreh eht eb ot sevlesmeht seveileb taht enoyreve tub ,noitisop ruo ni gniht  emas eht enod evah dluow uoY ?stsehc ruo hguorht niap gnibbats siht si ti htiw semo c taht lla nehw gnivil si doog tahW .thgis ni dne on htiw efil demood a dael eW .ere h su fo yna rof gnihton si erehT\n"
                                );
                                */
                                break;
                            case 0:
                            case 1:
                            case 0x6a:
                            case 0x6b:
                            case 0x6c:
                            case 0x6d:
                            case 0x71:
                            case 0x7a:
                            case 0x8a:
                            case 0x8b:
                            case 0x8e:
                            case 0x94:
                                EnqueueSong("OST/nothing.ogg");
                                break;
                        }
                    }
                }
            }

            IsWaitingInCutscene = true;
            OldCombatCheck = combatCheck;
            oldmusic2 = music2;
            oldmusic = music;
        }
    }

    private CancellationTokenSource _threadCancel;
    private Task _thread;

    private float _volume;
    private float _volumeModifier = 0.125f;
    private float _volumeAdd = 0f;
    private float _loopStart;
    private float _loopEnd;
    private float _loopOffset;

    public static void RemoveRecursive(string directory)
    {
        foreach (var dirName in DirAccess.GetDirectoriesAt(directory)) RemoveRecursive(Path.Join(directory, dirName));
        foreach (var fileName in DirAccess.GetFilesAt(directory)) DirAccess.RemoveAbsolute(Path.Join(directory, fileName));
        DirAccess.RemoveAbsolute(directory);
    }

    public static void PrintDirectoryRecursively(string path)
    {
        GD.Print(path);
        
        var files = DirAccess.GetFilesAt(path);
        var dirs = DirAccess.GetDirectoriesAt(path);

        foreach (var f in files) GD.Print(f);

        foreach (var dir in dirs) PrintDirectoryRecursively(Path.Combine(path, dir));
    }
    
    public static void ExtractZip(string inputPath, string outputDirectory)
    {
        DirAccess.MakeDirAbsolute(outputDirectory);
        var zip = new ZipReader();
        var error = zip.Open(inputPath);
        if (error is not Error.Ok) throw new Exception();
        
        foreach (var entryPath in zip.GetFiles())
        {
            var entry = entryPath.Replace('\\', '/');
            if (entry.EndsWith('/')) continue;
            if (entry.StartsWith('/') || entry.Contains("../") || entry == "..") continue;

            var destinationPath = Path.Combine(outputDirectory, entry);
            var parentDirectory = Path.GetDirectoryName(destinationPath);
            
            if (!string.IsNullOrWhiteSpace(parentDirectory)) DirAccess.MakeDirRecursiveAbsolute(parentDirectory.Replace("user:/", "user://").Replace("user:///", "user://"));
            

            var data = zip.ReadFile(entryPath);
            var f = FileAccess.Open(destinationPath, FileAccess.ModeFlags.Write);
            f.StoreBuffer(data);
            f.Flush();
            f.Close();
        }
        zip.Close();
    }
    
    public override void _Ready()
    {
        base._Ready();

        _threadCancel = new CancellationTokenSource();
        _thread = Task.Run(Run, _threadCancel.Token);
        AudioPlayer.VolumeLinear = 0.00001f;

        if (IsAndroid)
        {
            OS.RequestPermissions();
            
            SetMusicLocationButton.Pressed += () =>
            {
                DisplayServer.FileDialogShow("Input music zip", "", "", false, DisplayServer.FileDialogMode.OpenFile, ["*.zip"],
                    Callable.From((bool status, string[] paths, int filter) =>
                    {
                        if (!status || paths.Length <= 0) return;
                        
                        var path = paths[0];

                        if (DirAccess.DirExistsAbsolute(AndroidMusicRoot)) RemoveRecursive(AndroidMusicRoot);

                        GD.Print($"Extracting music from {path}");
                        
                        ExtractZip(path, AndroidMusicRoot);

                        PrintDirectoryRecursively("user://");
                    }));
            };
        }
        else
        {
            SetMusicLocationButton.Visible = false;
            SetMusicLocationButton.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private bool _debug;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_thread.IsCompleted)
        {
            GD.Print(_thread.Exception);
        }

        while (_logQueue.TryDequeue(out var log))
        {
            GD.Print(log);
        }

        var deltaf = (float)delta;
        const float fadeSpeed = 0.5f;

        if (_currentSong != _queuedSong)
        {
            var nextVolume = _volume - (deltaf * fadeSpeed);
            if (nextVolume <= 0)
            {
                nextVolume = 0.001f;
                AudioPlayer.Stop();
                _currentSong = _queuedSong;
                GD.Print("Starting song " + _currentSong);
                var path = Path.Combine(SoundtrackPath, _currentSong);
                if (FileAccess.FileExists(path))
                {
                    var stream = AudioStreamOggVorbis.LoadFromFile(path);

                    var startPoint = 0f;

                    _volumeAdd = 1;
                    _loopStart = -1;
                    _loopEnd = -1;
                    _loopOffset = -1;

                    var track = new TrackUIHandler.TrackInfo { RawName = _currentSong, };
                    var tags = stream.Tags;
                    
                    if (tags.TryGetValue("title", out var title)) track.Title = title.AsString();
                    if (tags.TryGetValue("artist", out var artist)) track.Artist = artist.AsString();
                    if (tags.TryGetValue("composer", out var composer)) track.Composer = composer.AsString();
                    if (tags.TryGetValue("genre", out var metadata))
                    {
                        var metadataString = metadata.AsString();
                        var metadataSplit = metadataString.Split(",");
                        if (metadataSplit.Length >= 3)
                        {
                            //we have loop information
                            startPoint = float.Parse(metadataSplit[0]);
                            _loopStart = float.Parse(metadataSplit[1]);
                            _loopEnd = float.Parse(metadataSplit[2]);
                            _loopOffset = _loopEnd - _loopStart;
                        }

                        if (metadataSplit.Length >= 4)
                        {
                            _volumeAdd = 1f + (float.Parse(metadataSplit[3]) * 0.01f);
                        }
                    }
                    else
                    {
                        GD.Print("No extra data defined");
                    }
                    
                    TrackUiHandler.QueuedTrack = track;
                    
                    AudioPlayer.Stream = stream;
                    AudioPlayer.Play(startPoint);
                }
            }

            _volume = nextVolume;
        }
        else
        {
            _volume = Mathf.Clamp(_volume + (deltaf * fadeSpeed), 0.00001f, 1);
        }

        var position = AudioPlayer.GetPlaybackPosition();
        var playing = AudioPlayer.Playing;
        if (playing && _loopStart >= 0 && position > _loopEnd) AudioPlayer.Seek(position - _loopOffset);
        if (!playing && AudioPlayer.Stream is not null) AudioPlayer.Play();
        
        AudioPlayer.VolumeLinear = Mathf.Remap(_volume, 0, 1, 0, _volumeAdd) * _volumeModifier;
        DebugLabel.Text = $"{AudioPlayer.VolumeLinear}";
    }
}