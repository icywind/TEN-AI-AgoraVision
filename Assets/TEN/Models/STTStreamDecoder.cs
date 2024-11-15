using System;
using System.Text;
using Newtonsoft.Json;


namespace Agora.TEN.Server.Models
{

    /// <summary>
    /// STTStreamDecoder is responsible for decoding incoming text streams that are split into multiple parts.
    /// It assembles these parts, decodes the base64 content, and parses the JSON data to return a complete STTStreamText object.
    /// </summary>
    public class STTStreamDecoder
    {
        private string contentBuffer = "";

        public STTStreamText ParseStream(string str)
        {
            var message = new STTStreamMessage(str);
            contentBuffer += message.Content;

            if (message.PartIndex == message.PartsTotal)
            {
                var jsonString = DecodeBase64(contentBuffer);
                contentBuffer = "";

                if (jsonString != null)
                {
                    try
                    {
                        var stt = JsonConvert.DeserializeObject<STTStreamText>(jsonString);
                        return stt;
                    }
                    catch (JsonException)
                    {
                        throw new SttError("Failed to decode JSON.");
                    }
                }
            }
            return null;
        }

        private string DecodeBase64(string base64String)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64String);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }

    ///  Incoming stream is in Text Format
    /// <id>|<index>|<total>|<content part>
    ///   sample: 0038dbd0|1|1|eyJpc19maW5hbCI6IHRydWUsICJzdHJlYW1faWQiOiAwLCAibWVzc2FnZV9pZCI6ICIwMDM4ZGJkMCIsICJkYXRhX3R5cGUiOiAidHJhbnNjcmliZSIsICJ0ZXh0X3RzIjogMTczMDMxOTcwNjIzMCwgInRleHQiOiAiVEVOIEFnZW50IGNvbm5lY3RlZC4gSG93IGNhbiBJIGhlbHAgeW91IHRvZGF5PyJ9
    ///
    public class STTStreamMessage
    {
        public string MessageId { get; set; }
        public int PartIndex { get; set; }
        public int PartsTotal { get; set; }
        public string Content { get; set; }

        public STTStreamMessage(string input)
        {
            var components = input.Split('|');
            MessageId = components[0];
            PartIndex = int.Parse(components[1]);
            PartsTotal = int.Parse(components[2]);
            Content = components[3];
        }
    }

    public class SttError : Exception
    {
        public SttError(string message) : base(message) { }
    }

}
