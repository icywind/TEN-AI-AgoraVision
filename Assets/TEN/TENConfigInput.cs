using UnityEngine;
using System;

namespace Agora.TEN.Client
{
    [CreateAssetMenu(menuName = "Agora/TEN App Config", fileName = "TENConfigInput", order = 2)]
    [Serializable]
    public class TENConfigInput : ScriptableObject
    {
        [SerializeField]
        /// APP ID from https://console.agora.io
        public string AppID = "";

        [SerializeField]
        /// Expected Agent UID
        public uint AgentUid = 1234;

        [SerializeField]
        /// Automatic Speech Recognition language
        public string AgoraAsrLanguage = "en-US";

        [SerializeField]
        /// TEN Graph Name
        public string Graph = "";

        [SerializeField]
        /// The voice used by the Agent
        public AzureVoiceType AzureVoice;

        [SerializeField]
        [Tooltip("if the voice is not provided by Azure, enter the vendor's voice name here")]
        /// Alternative voice override
        public string AltVoiceName;

        [SerializeField]
        /// The base URL of the server
        public string ServerBaseURL;

        [SerializeField]
        [TextArea(3, 10)]
        public string AgentOpeningGreeting = "TEN agent connected.How can I help you today?";

        [SerializeField]
        [TextArea(3, 10)]
        public string AgentVisionCheckingWords = "[\"Let me take a look...\",\"Let me check your camera...\",\"Please wait for a second...\"]";
    }
}
