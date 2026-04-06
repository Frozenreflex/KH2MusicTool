using Godot;

namespace KH2MusicTool.Source;


public partial class TrackUIHandler : Node
{
    public class TrackInfo
    {
        public string RawName;
        public string Title;
        public string Artist;
        public string Composer;
    }

    public TrackInfo QueuedTrack;
    
    [Export] public Label Label;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (QueuedTrack is not null)
        {
            var track = QueuedTrack;
            QueuedTrack = null;

            var text = "Now Playing:\n";

            switch (track.Title)
            {
                case "Nothing":
                {
                    text = "No song currently playing.";
                    break;
                }
                case null:
                {
                    text += track.RawName;
                    break;
                }
                default:
                {
                    text += $"{track.Title}";
                    if (track.Artist is not null)
                    {
                        text += $", made by {track.Artist}";
                    }
                    if (track.Composer is not null)
                    {
                        text += $", composed by {track.Composer}";
                    }

                    break;
                }
            }
            Label.Text = text;
        }
    }
}