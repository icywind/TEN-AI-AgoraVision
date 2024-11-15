using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Agora.Rtc;

using Agora_RTC_Plugin.API_Example;
using Logger = Agora_RTC_Plugin.API_Example.Logger;

using Agora.TEN.Client;

namespace Agora.TEN.Demo
{
    public class TENDemoChat : MonoBehaviour
    {
        [SerializeField]
        Text TitleText;

        [SerializeField]
        RawImage VideoView;

        [SerializeField]
        Button BackButton;

        [SerializeField]
        internal Text LogText;

        [SerializeField]
        internal IChatTextDisplay TextDisplay;

        [SerializeField]
        internal TENSessionManager TENSession;

        [SerializeField]
        internal SphereVisualizer Visualizer;


        internal Logger Log;
        internal IRtcEngine RtcEngine;

        internal uint LocalUID { get; set; }


        [SerializeField]
        Button CamButton;


        void Start()
        {
            if (CheckAppId())
            {
                SetUpUI();
                InitEngine();
                GetTokenAndJoin();
            }
        }

        // Update is called once per frame
        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(AppConfig.Shared.AppId.Length > 10, "Please fill in your appId properly!");
        }


        void SetUpUI()
        {
            TitleText.text = AppConfig.Shared.Channel;

            var videoSurface = VideoView.gameObject.AddComponent<VideoSurface>();
            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                var transform = videoSurface.GetComponent<RectTransform>();
                if (transform)
                {
                    //If render in RawImage. just set rawImage size.
                    transform.sizeDelta = new Vector2(width / 2, height / 2);
                    transform.localScale = Vector3.one;
                }
                else
                {
                    //If render in MeshRenderer, just set localSize with MeshRenderer
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                }
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };

            BackButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TENEntryScreen");
            });

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
            CamButton.onClick.AddListener(() => {
                RtcEngine.SwitchCamera();
	        });
#else 
            CamButton.gameObject.SetActive(false);
#endif

            // invisible until Agent joins
            Visualizer?.gameObject.SetActive(false);
        }


        #region --- RTC Functions ---
        internal void ShowVideo()
        {
            var videoSurface = VideoView.gameObject.GetComponent<VideoSurface>();
            videoSurface.SetForUser(0);
            videoSurface.SetEnable(true);
        }

        void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = AppConfig.Shared.AppId;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_MEETING;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);

            Visualizer?.Init(RtcEngine);
        }

        async void GetTokenAndJoin()
        {
            AppConfig.Shared.RtcToken = await TENSession.GetToken();
            JoinChannel();
        }

        void JoinChannel()
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.JoinChannel(AppConfig.Shared.RtcToken, AppConfig.Shared.Channel, "", 0);
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}.{this.GetType()} OnDestroy");

            TENSession.StopSession();
            if (RtcEngine != null)
            {
                RtcEngine.LeaveChannel();
                RtcEngine.InitEventHandler(null);
                RtcEngine.Dispose();
                RtcEngine = null;
            }
        }
        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly TENDemoChat _app;

        internal UserEventHandler(TENDemoChat mgr)
        {
            _app = mgr;
        }

        public override void OnError(int err, string msg)
        {
            _app.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log(string.Format("sdk version: ${0}",
                _app.RtcEngine.GetVersion(ref build)));
            Debug.Log(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));

            _app.LocalUID = connection.localUid;

            _app.ShowVideo();
            _app.TENSession.StartSession(_app.LocalUID);
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("OnLeaveChannel");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                elapsed));
            if (uid == AppConfig.Shared.AgentUid) {
                _app.Visualizer?.gameObject.SetActive(true);
			}
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        /* SDK v4.2.6 */
         public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, uint length, System.UInt64 sentTs)
        /* SDK v4.4.0 or later */
        // public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, ulong length, ulong sentTs)
        {
            string str = System.Text.Encoding.UTF8.GetString(data, 0, (int)length);
            _app.TextDisplay.ProcessTextData(remoteUid, str);
            _app.TextDisplay.DisplayChatMessages(_app.LogText.gameObject);
        }

        public override void OnRemoteAudioStateChanged(RtcConnection connection, uint remoteUid, REMOTE_AUDIO_STATE state, REMOTE_AUDIO_STATE_REASON reason, int elapsed)
        {
            Debug.Log($"RemoteAudio state:{state} reason:{reason}");
        }
    }

    #endregion



}