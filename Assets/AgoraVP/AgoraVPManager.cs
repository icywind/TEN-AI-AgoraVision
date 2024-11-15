using UnityEngine;
using System.Collections.Generic;
using Agora.Rtc;
using Agora.Rtc.Utils;
using Agora.TEN.Client;

namespace Agora_RTC_Plugin.API_Example
{
    public class AgoraVPManager : MonoBehaviour
    {
        #region EDITOR INPUTS
        //[Header("_____________Basic Configuration_____________")]
        protected string _appID = "";

        protected string _token = "";

        [SerializeField]
        protected string _channelName = "";

        [SerializeField]
        internal TENConfigInput TENConfig;
        [SerializeField]
        internal IChatTextDisplay TextDisplay;
        [SerializeField]
        internal TENSessionManager TENSession;
        [SerializeField]
        internal SphereVisualizer Visualizer;

        [SerializeField]
        internal GameObject ViewContainerPrefab;

        [SerializeField]
        GameObject TargetObject; // to be looked at
        #endregion

        internal IRtcEngine RtcEngine = null;

        internal Dictionary<uint, GameObject> ViewObjects = new Dictionary<uint, GameObject>();

        private void Start()
        {
            if (CheckAppId())
            {
                InitEngine();
                SetBasicConfiguration();
                JoinChannel();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            foreach (var ob in ViewObjects.Values)
            {
                ob.transform.LookAt(TargetObject.transform);
            }
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            TENSession.StopSession();

            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        private void LoadAssetData()
        {
            AppConfig.Shared.SetValue(TENConfig);
            //if (_appIdInput == null) return;
            _appID = AppConfig.Shared.AppId;
            _token = AppConfig.Shared.RtcToken;
            _channelName = AppConfig.Shared.Channel = UtilFunctions.GenRandomString("agora_", 5);
        }

        private bool CheckAppId()
        {
            LoadAssetData();
            Debug.Assert(_appID.Length > 10, $"Please fill in your appId in {gameObject.name}");
            Debug.Log("Running platform is " + Application.platform);
            return _appID.Length > 10;
        }

        protected virtual void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING, AREA_CODE.AREA_CODE_GLOB);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        protected virtual void SetBasicConfiguration()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(640, 360);
            config.frameRate = 15;
            config.bitrate = 0;
            RtcEngine.SetVideoEncoderConfiguration(config);
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

            // For now this private API is needed to make voice chat working
            if (Application.platform == RuntimePlatform.VisionOS)
            {
                RtcEngine.SetParameters("che.audio.restartWhenInterrupted", true);
            }

            Visualizer?.Init(RtcEngine);
        }

        #region -- Button Events ---

        public virtual async void JoinChannel()
        {
            _token = await TENSession.GetToken();
            RtcEngine.JoinChannel(_token, _channelName);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }

        #endregion

        internal class UserEventHandler : IRtcEngineEventHandler
        {
            protected readonly AgoraVPManager _app;

            internal UserEventHandler(AgoraVPManager videoSample)
            {
                _app = videoSample;
            }

            public override void OnError(int err, string msg)
            {
                Debug.Log(string.Format("OnError err: {0}, msg: {1}", err, msg));
            }

            public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
            {
                int build = 0;
                Debug.Log("Agora: OnJoinChannelSuccess ");
                Debug.Log(string.Format("sdk version: ${0}",
                    _app.RtcEngine.GetVersion(ref build)));
                Debug.Log(string.Format("sdk build: ${0}",
                  build));
                Debug.Log(
                    string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                    connection.channelId, connection.localUid, elapsed));
                Vector3 pos = new Vector3(-2.5f, 0, 3.28f);
                CreateUserView(0, connection.channelId, pos);
                _app.TENSession.StartSession(connection.localUid);
            }

            public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
            {
                Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
                if (uid == _app.TENConfig.AgentUid) return;
                var count = _app.transform.childCount;
                Vector3 pos = new Vector3(count * 1.5f, 0, 3.28f);
                CreateUserView(uid, connection.channelId, pos);
            }

            protected virtual void CreateUserView(uint uid, string channelId, Vector3 parentLocation)
            {
                var view = AgoraViewUtils.MakeVideoView(uid, channelId, AgoraViewUtils.DisplayContainerType.CustomMesh, _app.ViewContainerPrefab);
                GameObject nobj = new GameObject();
                nobj.name = view.name + "_parent";
                nobj.transform.SetParent(_app.transform);
                nobj.transform.localScale = 0.3f * Vector3.one;
                nobj.transform.localPosition = parentLocation;
                nobj.transform.rotation = Quaternion.identity;

                view.transform.SetParent(nobj.transform);
                view.transform.localPosition = Vector3.zero;
                _app.ViewObjects[uid] = nobj;
            }

            public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
            {
                Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                    (int)reason));
                if (_app.ViewObjects.ContainsKey(uid)) _app.ViewObjects.Remove(uid);
                AgoraViewUtils.DestroyVideoView(uid);
            }

        }
    }
}
