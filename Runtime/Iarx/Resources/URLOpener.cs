using UnityEngine;
using UnityEngine.Events;

namespace Pretia
{
    /// <summary>
    /// This behaviour allows an external url link to be opened in the browser via UnityEvent event callbacks.
    /// </summary>
    [AddComponentMenu("Pretia/URLOpener")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/url-opener")]
    public class URLOpener : MonoBehaviour
    {
        [Tooltip("Default URL to open when OpenURL is called")]
        [SerializeField] private string _url;

        /// <summary>
        /// Open the default url in external browser
        /// </summary>
        public void OpenURL()
        {
            Application.OpenURL(_url);
        }

        /// <summary>
        /// Set the URL to be opened by default when OpenURL is called
        /// </summary>
        public void SetURL(string url)
        {
            _url = url;
        }
        
        /// <summary>
        /// Opens a given URL, rather than using the default url
        /// </summary>
        public void OpenCustomURL(string url)
        {
            Application.OpenURL(url);
        }

        public string GetURL()
        {
            return _url;
        }

        public void GetUrlViaCallback(UnityEvent<string> callback)
        {
            callback.Invoke(GetURL());
        }
    }
}
