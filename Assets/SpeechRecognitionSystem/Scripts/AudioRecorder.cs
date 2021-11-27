using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using SpeechRecognitionSystem;

public class AudioRecorder : MonoBehaviour, IMicrophone {
    public int MicrophoneIndex = 0;
    public int GetRecordPosition( ) {
        return Microphone.GetPosition( _deviceName );
    }
    public AudioClip GetAudioClip( ) {
        return _audioClip;
    }
    public bool IsRecording( ) {
        return Microphone.IsRecording( _deviceName );
    }

    [System.Serializable]
    public class MicReadyEvent : UnityEvent<IMicrophone> { }

    public MicReadyEvent MicReady = new MicReadyEvent( );

    private void Awake( ) {
        if ( Application.platform == RuntimePlatform.Android ) {
            if ( !Permission.HasUserAuthorizedPermission( Permission.Microphone ) ) {
                Permission.RequestUserPermission( Permission.Microphone );
            }
        }
    }

    private void FixedUpdate( ) {
        bool micAutorized = true;
        if ( Application.platform == RuntimePlatform.Android ) {
            micAutorized = Permission.HasUserAuthorizedPermission( Permission.Microphone );
        }
        if ( micAutorized ) {
            if ( _firstLoad ) {
                _deviceName = Microphone.devices [ MicrophoneIndex ];
                _audioClip = Microphone.Start( _deviceName, true, LENGTH_SEC, FREQ );
                this.MicReady?.Invoke( this );
                _firstLoad = false;
            }
        }
    }
    private void OnDestroy( ) {
        Microphone.End( _deviceName );
        _firstLoad = true;
    }

    private bool _firstLoad = true;
    private AudioClip _audioClip = null;
    private const int LENGTH_SEC = 2;
    private const int FREQ = 16000;
    private string _deviceName;
}
