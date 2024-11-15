# TEN AI Agent on Apple Vision Pro
This is the finished project that forked from the [HelloAgoraVision](https://github.com/AgoraIO-Community/HelloAgoraVision) and added support for Voice AI. It uses the TEN AI framework to utilize LLM service from OpenAI and Speech service from Azure. With the [TEN client plugin](https://github.com/AgoraIO-Community/TEN_AI_UnityPlugin), a developer can build from the original project to this version in as little as 15 minutes.

## Preparations
1. Set up    [TEN Frameworks Agent](https://github.com/TEN-framework/TEN-Agent)
2. Obtain credentials:
-   Text to Speech Support  API Key from  [Azure Speech Service](https://portal.azure.com/#view/Microsoft_Azure_ProjectOxford/CognitiveServicesHub/~/SpeechServices)
    
-   [API key from OpenAI](https://platform.openai.com/api-keys)
- Agora project [App ID](https://console.agora.io/projects) 
3. [Download](https://github.com/AgoraIO-Community/HelloAgoraVision/releases) the Unity SDK with visionPro support ([AgoraRTC-Plugin-VisionOS.unitypackage](https://github.com/AgoraIO-Community/HelloAgoraVision/releases/download/v4.2.6.6-preview/AgoraRTC-Plugin-VisionOS.unitypackage))


## Quick Start

1. Clone this repo and open the project from this folder

2. Set up Unity environment for the VisionOS

3. Import AgoraRTC-Plugin-VisionOS.unitypackage

4. Open TestAgoraVP scene

5. Fill in App ID and Channel Name.

![input](https://github.com/user-attachments/assets/974967e2-094a-4fdf-b6a3-635225e42161)

10. Build and Run the project on device


## License

  

The MIT License (MIT).