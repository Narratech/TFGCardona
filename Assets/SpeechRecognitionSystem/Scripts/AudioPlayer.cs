using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SpeechRecognitionSystem;
using System;
using System.Reflection;

public class AudioPlayer : MonoBehaviour, IAudioProvider {

    public float[ ] GetData( ) {
        var diff = audioSource.timeSamples - _currentPosition;
        if ( ( _currentPosition > 0 ) && ( audioSource.timeSamples == 0 ) ) {
            diff = _audioClip.samples - _currentPosition;
        }
        if ( diff > 0 ) {
            var res = new float[ diff ];

            _audioClip.GetData( res, _currentPosition );

            _currentPosition = audioSource.timeSamples;

            return res;
        }
        _currentPosition = audioSource.timeSamples;
        return ( _currentPosition == 0 ) ? new float[ 256 ] : null;
    }

    public AudioSource audioSource;

    public AudioReadyEvent AudioReady = new AudioReadyEvent( );

    public void Play( ) {
        if ( _onPause ) {
            audioSource.UnPause( );
            _onPause = false;
        }
        else {
            audioSource.Play( );
        }
    }

    public void Pause( ) {
        if ( audioSource.isPlaying ) {
            if ( !_onPause ) {
                audioSource.Pause( );
                _onPause = true;
            }
        }
    }

    public void Stop( ) {
        audioSource.Stop( );
        _onPause = false;
    }

    private void Awake( ) {
        _audioClip = audioSource.clip;
    }

    private void Update( ) {
        if ( audioSource.isPlaying ) {
            if ( !_init ) {
                this.AudioReady?.Invoke( this );
                _init = true;
            }
        }
    }

    private AudioClip _audioClip = null;

    private int _currentPosition = 0;

    private bool _init = false;

    private bool _onPause = false;
}
