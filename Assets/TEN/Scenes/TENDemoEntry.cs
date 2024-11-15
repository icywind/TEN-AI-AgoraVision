using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

namespace Agora.TEN.Client
{
    public class TENDemoEntry : MonoBehaviour
    {
        [SerializeField]
        Text footnote;

        [SerializeField]
        InputField ChannelInput;

        [SerializeField]
        Button JoinButton;

        [SerializeField]
        ToggleGroup ToggleGroup;
        [SerializeField]
        GameObject TogglePrefab;

        [SerializeField]
        TENConfigInput TENConfig;


        // Start is called before the first frame update
        void Start()
        {
            SetupUI();
        }

        void SetupUI()
        {
            ChannelInput.text = UtilFunctions.GenRandomString(prefix: "agora_", length: 5);
            footnote.text = "Agora IO 2024.  ver." + Application.version;
            JoinButton.onClick.AddListener(JoinChannel);

            int enumCount = Enum.GetValues(typeof(AzureVoiceType)).Length;
            string configVoiceType = TENConfig.AzureVoice.ToString();
            foreach (string voiceName in Enum.GetNames(typeof(AzureVoiceType)))
            {
                GameObject go = Instantiate(TogglePrefab);
                Toggle toggle = go.GetComponent<Toggle>();
                toggle.group = ToggleGroup;
                Text label = toggle.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = voiceName;
                }
                go.transform.SetParent(ToggleGroup.transform);
                go.name = voiceName;
                var tran = go.GetComponent<RectTransform>();
                if (tran)
                {
                    tran.localScale = Vector3.one;
                }
                if (voiceName == configVoiceType) {
                    toggle.SetIsOnWithoutNotify(true);
				}
            }
        }

        void JoinChannel()
        {
            if (string.IsNullOrWhiteSpace(ChannelInput.text))
            {
                Debug.LogError("Channel name can't be empty!");
                return;
            }
            UpdateConfig();
            SceneManager.LoadScene("TENDemoScene");
        }

        void UpdateConfig()
        {
            AppConfig.Shared.SetValue(TENConfig);
            AppConfig.Shared.Channel = ChannelInput.text;
            Toggle activeToggle = ToggleGroup.ActiveToggles().FirstOrDefault();
            if (activeToggle != null)
            {
                Debug.Log("Active Toggle: " + activeToggle.name);
                AzureVoiceType vtype;
                if (Enum.TryParse(activeToggle.name, true, out vtype))
                {
                    AppConfig.Shared.SetAgentVoice(voiceType: vtype);
                }
                else
                {
                    Debug.LogWarning("Unable to get the voice type input from toggles.");
                }
            }
            else
            {
                Debug.Log("No toggle is active.");
            }
        }
    }
}
