using UnityEngine;

namespace Pretia
{
    /// <summary>
    /// This Behaviour provides functions for closing the app
    /// or restarting the experience
    /// </summary>
    [AddComponentMenu("Pretia/ApplicationController")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/application-controller")]
    public class ApplicationController : MonoBehaviour
    {
        /// <summary>
        /// Close the app
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("In AR the application would be closed...");
            Application.Quit();
        }
        
        /// <summary>
        /// Reload the experience as if it were just launched
        /// </summary>
        public void RestartExperience()
        {
            Debug.Log("In AR the experience would be restarted...");
        }
    }
}
