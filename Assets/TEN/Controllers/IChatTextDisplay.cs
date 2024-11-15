using UnityEngine;

namespace Agora.TEN.Client
{
    public abstract class IChatTextDisplay : MonoBehaviour
    {

        /// <summary>
        ///   Process the incoming data of JSON format.
        /// </summary>
        /// <param name="uid">owner's uid</param>
        /// <param name="text">content</param>
        abstract internal void ProcessTextData(uint uid, string text);
   
        /// <summary>
        ///   Display the Chat Message that contains in _textProcessor.  The 
        /// Implementation should leverage the display object for showing the 
        /// chat history.
        /// </summary>
        /// <param name="display">An object that contains script to display the data.</param>
        abstract public void DisplayChatMessages(GameObject display);
    }
}
