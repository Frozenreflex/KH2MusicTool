using Godot;
using KH2MusicTool.Source.IPC;

namespace KH2MusicTool.Source;

public partial class Main : Node
{
    //[Export] public Sprite2D Sprite;
    [Export] public AnimatedSprite2D Sprite;
    public static Main Instance { get; private set; }

    public PCSX2 Emulator { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        Instance = this;
        Emulator = new PCSX2();
        Sprite.Play();
    }
}