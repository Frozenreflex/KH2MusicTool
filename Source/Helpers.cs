using Godot;

namespace KH2MusicTool.Source;

public static class Helpers
{
    public static float Damp(this float a, float b, float lambda, float dt) => Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
}