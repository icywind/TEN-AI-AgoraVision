using UnityEngine;
using Agora.Rtc;

public abstract class ISoundVisualizer : MonoBehaviour
{
    abstract public void UpdateVisualizer(float[] pcmData);
    abstract public void Init(IRtcEngine RtcEngine);
}
