using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Agora.TEN.Server.Models;

namespace Agora.TEN.Client
{
    public class Simple2PeopleChatDisplay : IChatTextDisplay
    {
        Text DisplayText { get; set; }
        protected StreamTextProcessor _textProcessor = new StreamTextProcessor();
        protected STTStreamDecoder _streamDecoder = new STTStreamDecoder();

        public class ChatMessage
        {
            public string Speaker { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        ///   Process the incoming data of JSON format.
        /// </summary>
        /// <param name="uid">owner's uid</param>
        /// <param name="text">content</param>
        override internal void ProcessTextData(uint uid, string text)
        {
            //var stt = JsonConvert.DeserializeObject<STTStreamText>(text);
            try
            {
                var stt = _streamDecoder.ParseStream(text);
                // it could be a partial text that need to wait
                if (stt == null) {
                    return;
                }

                var msg = new IChatItem
                {
                    UserId = uid,
                    IsFinal = stt.IsFinal,
                    Time = stt.TextTS,
                    Text = stt.Text,
                    IsAgent = (stt.StreamID == 0)
                };
                _textProcessor.AddChatItem(msg);

            }
            catch (SttError e)
            {
                Debug.LogError(e.ToString());
                return;
            }

        }

        public override void DisplayChatMessages(GameObject displayObject)
        {
            var items = _textProcessor.GetConversation();
            var msgs = items.Select(x => new ChatMessage
            {
                Speaker = x.IsAgent ? "Agent" : "You",
                Message = x.Text
            }).ToList();

            DisplayText = displayObject.GetComponent<Text>();
            DisplayText.text = "";
            foreach (var msg in msgs)
            {
                string color = msg.Speaker == "Agent" ? "blue" : "black";
                string speaker = $"<color='{color}'><b>{msg.Speaker}</b></color>";
                DisplayText.text += $"{speaker}: {msg.Message}\n";
            }
        }
    }
}
