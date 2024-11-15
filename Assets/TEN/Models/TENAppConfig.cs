using Newtonsoft.Json;
using Agora.TEN.Server.Models;
using UnityEngine;

namespace Agora.TEN.Client
{
    // Azure voice samples: https://techcommunity.microsoft.com/t5/ai-azure-ai-services-blog/introducing-super-realistic-ai-voices-optimized-for/ba-p/3933744
    public enum AzureVoiceType
    {
        [JsonProperty("brian")]
        Brian,
        [JsonProperty("jane")]
        Jane,
        [JsonProperty("andrew")]
        Andrew,
        [JsonProperty("emma")]
        Emma,
        [JsonProperty("guy")]
        Guy,
        [JsonProperty("jenny")]
        Jenny,
        // Set these voice up with language zh-CN handled
        //[JsonProperty("yunjie")]
        //Yunjie,
        //[JsonProperty("yunxi")]
        //Yunxi
    }

    public class AppConfig
    {
        /// Instance access
        private static AppConfig _shared;
        public static AppConfig Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new AppConfig();
                }
                return _shared;
            }
        }

        /// Expected Agent UID
        public uint AgentUid { get; set; }

        /// Automatic Speech Recognition language
        public string AgoraAsrLanguage { get; set; }

        /// APP ID from https://console.agora.io
        public string AppId { get; set; }

        /// Channel prefill text to join
        public string Channel { get; set; }

        /// RTC token, provided by server
        public string RtcToken { get; set; }

        /// <summary>
        /// TEN Graph name
        /// </summary>
        public string GraphName { get; set; }

        /// The base URL of the server
        public string ServerBaseURL { get; set; }

        public AgentProperties AgentProperties { get; set; }

        public void SetValue(TENConfigInput input)
        {
            this.AgentUid = input.AgentUid;
            this.AgoraAsrLanguage = input.AgoraAsrLanguage;
            this.AppId = input.AppID;
            this.ServerBaseURL = input.ServerBaseURL;
            this.GraphName = input.Graph;
            this.AgentProperties = MakeAgentProperties(input);
        }

        /// <summary>
        ///    Create the Properties
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        virtual protected AgentProperties MakeAgentProperties(TENConfigInput input)
        {
            AgoraRtcExtConfig agoraConfig = new AgoraRtcExtConfig
            {
                AgoraAsrLanguage = input.AgoraAsrLanguage
            };

            AzureTtsExtConfig ttsConfig = new AzureTtsExtConfig
            {
                AzureSynthesisVoiceName = MakeVoiceName(input.AzureVoice)
            };

            // If the alt name is specified, overide the name.
            if (!string.IsNullOrWhiteSpace(input.AltVoiceName))
            {
                ttsConfig.AzureSynthesisVoiceName = input.AltVoiceName;
            }

            OpenaiChatgptExtConfig openaiChatgpt = new OpenaiChatgptExtConfig
            {
                Greeting = input.AgentOpeningGreeting,
                CheckingVisionTextItems = input.AgentVisionCheckingWords,
                Model = GetLLM(input.Graph)
            };

            return new AgentProperties
            {
                AgoraRtc = agoraConfig,
                AzureTts = ttsConfig,
                OpenaiChatgpt = openaiChatgpt
            };
        }

        virtual protected string MakeVoiceName(AzureVoiceType voiceType, string langauge = "en-US")
        {
            return $"{langauge}-{voiceType}Neural";
        }

        virtual public void SetAgentVoice(AzureVoiceType voiceType)
        {
            AgentProperties.AzureTts.AzureSynthesisVoiceName = MakeVoiceName(voiceType);
        }

        virtual protected string GetLLM(string graphName)
        {
            if (graphName == "camera.va.openai.azure")
            {
                return "gpt-4o";
            }
            else if (graphName == "va.openai.azure")
            {
                return "gpt-4o-mini";
            }

            return "gpt-4o";

        }
    }

}
