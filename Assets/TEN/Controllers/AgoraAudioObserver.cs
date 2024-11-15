using Agora.Rtc;

public class AgoraAudioObserver :  IAudioFrameObserver
{
    ISoundVisualizer _visual;
    public AgoraAudioObserver(ISoundVisualizer visualizer)
    {
        _visual = visualizer;
    }

    public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
                                                    uint uid,
                                                    AudioFrame audio_frame)
    {
        var floatArray = UtilFunctions.ConvertByteToFloat16(audio_frame.RawBuffer);
        _visual?.UpdateVisualizer(floatArray);
        return false;
    }
}
