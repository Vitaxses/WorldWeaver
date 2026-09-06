using UnityEditor;

namespace WorldWeaver.Editor
{
    [CustomPropertyDrawer(typeof(AtmosCue.AtmosChannelInfo))]
    public class AtmosChannelInfoEditor : ChannelInfoEditor<AtmosChannels> { }

    [CustomPropertyDrawer(typeof(MusicCue.MusicChannelInfo))]
    public class MusicChannelInfoEditor : ChannelInfoEditor<MusicChannels> { }
   
}