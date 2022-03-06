using System;
using UnityEngine;

namespace UnnaturalSelection.Audio
{
    public enum AudioCategory
    {
        SFx,
        Voice,
        Music
    }

    public class AudioEmitter
    {
        private readonly AudioCategory category;
        private readonly AudioSource source;

        public bool IsPlaying => source.isPlaying;

        public AudioEmitter(Transform parent, string name, AudioCategory category, float minDistance, float maxDistance, float spatialBlend)
        {
            this.category = category;

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            source = go.AddComponent<AudioSource>();

            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.spatialBlend = spatialBlend;

            source.outputAudioMixerGroup = (category == AudioCategory.Music) ? AudioManager.Instance.MusicMixer : AudioManager.Instance.SFxMixer;
        }

        public void Play(AudioClip clip, float volume)
        {
            if (!clip)
            {
                throw new ArgumentException("AudioManager: AudioClip '" + clip + "' was not found.");
            }

            if (source.isPlaying && source.clip == clip)
                return;

            source.clip = clip;
            source.volume = GetVolume(volume);
            source.Play();
        }

        /// <summary>
        /// Forces the AudioEmitter to stop playing the current AudioClip and play immediately the requested sound.
        /// </summary>
        /// <param name="clip">The AudioClip to be played.</param>
        /// <param name="volume">The AudioEmitter volume.</param>
        public void ForcePlay(AudioClip clip, float volume)
        {
            if (!clip)
            {
                throw new ArgumentException("AudioManager: AudioClip '" + clip + "' was not found.");
            }

            source.clip = clip;
            source.volume = GetVolume(volume);
            source.Play();
        }

        /// <summary>
        /// Immediately stop playing the current AudioClip.
        /// </summary>
        public void Stop()
        {
            source.Stop();
        }

        /// <summary>
        /// Calculate the AudioEmitter volume by using the formula: (volume = 1 - value / startValue).
        /// Useful when you need to start playing a sound at certain point. e.g when the character 
        /// vitality is below certain point, it will increase the volume as the vitality gets lower.
        /// </summary>
        /// <param name="startValue">The volume will be calculated from this value.</param>
        /// <param name="value">The current value to be calculated the volume.</param>
        /// <param name="maxVolume">The AudioEmitter maximum volume.</param>
        public float CalculateVolumeByPercent(float startValue, float value, float maxVolume)
        {
            float vol = 1 - value / startValue;
            source.volume = GetVolume(Mathf.Clamp(vol, 0, maxVolume));
            return source.volume;
        }

        /// <summary>
        /// Returns the volume value adjusted by its category.
        /// </summary>
        /// <param name="volume">Reference volume.</param>
        /// <returns></returns>
        private float GetVolume(float volume)
        {
            switch (category)
            {
                case AudioCategory.SFx:
                    return volume * AudioManager.Instance.SFxVolume;
                case AudioCategory.Voice:
                    return volume * AudioManager.Instance.VoiceVolume;
                case AudioCategory.Music:
                    return volume * AudioManager.Instance.MusicVolume;
                default:
                    return 0;
            }
        }
    }
}
