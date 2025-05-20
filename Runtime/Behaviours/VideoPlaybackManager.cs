using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

namespace Pretia
{
    /// <summary>
    /// Manages video playback lifecycle with events for key moments in playback.
    /// Handles video preparation, playback state, and responds to application focus changes.
    /// </summary>
    [AddComponentMenu("Pretia/VideoPlaybackManager")]
    [HelpURL(PretiaDocs.COMPONENTS_ROOT + "/video-playback-manager")]
    public class VideoPlaybackManager : MonoBehaviour
    {
        [Tooltip("Event triggered when the video starts playing after preparation")]
        [SerializeField] private UnityEvent OnVideoStarted;
        [Tooltip("Event triggered when the video reaches its end")]
        [SerializeField] private UnityEvent OnVideoFinished;
        [Tooltip("Event triggered when this component becomes disabled")]
        [SerializeField] private UnityEvent OnDisabled;
        [Tooltip("Reference to the VideoPlayer component that will be managed")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [Tooltip("Minimum time in seconds to wait after preparation before starting the video")]
        [SerializeField] private float _minimumCallbackTime = 0.0f;
        [Tooltip("When enabled, this component will be disabled automatically after video playback completes")]
        [SerializeField] private bool _disableOnComplete = true;

        private float _startTime = 0.0f;
        private bool _playing = false;

        private void OnEnable()
        {
            _playing = false;
            _startTime = Time.time;
            _videoPlayer.Prepare();
        }

        private void OnDisable()
        {
            OnDisabled?.Invoke();
        }

        private IEnumerator OnApplicationFocus(bool focus)
        {
            if (focus && _playing)
            {
                yield return null;
                _videoPlayer.Play();
            }
            #if !UNITY_EDITOR
            else if (_playing)
            {
                _videoPlayer.Pause();
            }
            #endif
        }

        void LateUpdate()
        {
            if (_playing)
            {
                // Check if video is near its end
                if (_videoPlayer.time > 0 && (_videoPlayer.time > _videoPlayer.length - 0.18f))
                {
                    OnVideoFinished?.Invoke();
                    if (_disableOnComplete)
                        this.enabled = false;
                }
            }
            else
            {
                // Once video is prepared and minimum wait time has passed, start playback
                if (_videoPlayer.isPrepared)
                {
                    if (Time.time - _startTime >= _minimumCallbackTime)
                    {
                        _playing = true;
                        OnVideoStarted?.Invoke();
                    }
                }
            }
        }
    }
}
