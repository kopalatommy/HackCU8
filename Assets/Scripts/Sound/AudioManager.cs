using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UnnaturalSelection.Audio
{
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = GameObject.Find("Managers");
                    if (!holder.TryGetComponent<AudioManager>(out instance))
                        holder.AddComponent<AudioManager>();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        private static AudioManager instance = null;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the Sound Effects volume, e.g explosions, environment and weapons.")]
        private float sfxVolume = 1.0f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the Voice volume, e.g character responses and CutScenes.")]
        private float voiceVolume = 1.0f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the Music volume.")]
        private float musicVolume = 1.0f;

        [SerializeField]
        [Tooltip("The SFx Mixer is used to apply effects such echoing and deafness to SFx sources.")]
        private AudioMixerGroup sfxMixer;

        [SerializeField]
        [Tooltip("The Music Mixer is not affect by any king of effect, for this reason, it is perfect for all Music Sources in game.")]
        private AudioMixerGroup musicMixer;

        private readonly Dictionary<string, AudioEmitter> audioSources = new Dictionary<string, AudioEmitter>();

        private readonly List<AudioSource> instancedSources = new List<AudioSource>();

        private const int maxInstancedSources = 32;

        private int lastPlayedIndex;

        public float SFxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        public float VoiceVolume
        {
            get => voiceVolume;
            set => voiceVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => musicVolume;
            set => musicVolume = Mathf.Clamp01(value);
        }

        public AudioMixerGroup SFxMixer => sfxMixer;


        public AudioMixerGroup MusicMixer => musicMixer;

        private void Awake()
        {
            if(Instance != this)
                Instance = this;
        }

        public AudioEmitter RegisterSource(string sourceName = "Generic Source", Transform parent = null, AudioCategory category = AudioCategory.SFx, float minDistance = 1, float maxDistance = 3, float spatialBlend = 0.3f)
        {
            if (ContainsSource(sourceName))
                return audioSources[sourceName];

            AudioEmitter audioSource = new AudioEmitter(parent, sourceName, category, minDistance, maxDistance, spatialBlend);
            audioSources.Add(sourceName, audioSource);

            return audioSources[sourceName];
        }

        private bool ContainsSource(string source)
        {
            return audioSources.ContainsKey(source);
        }

        public AudioEmitter GetSource(string sourceName)
        {
            return audioSources.ContainsKey(sourceName) ? audioSources[sourceName] : null;
        }

        public void Play(string sourceName, AudioClip clip, float volume)
        {
            if (audioSources.ContainsKey(sourceName))
            {
                AudioEmitter audioSource = audioSources[sourceName];
                audioSource.Play(clip, volume);
            }
            else
            {
                throw new ArgumentException("AudioManager: AudioSource '" + sourceName + "' was not found.");
            }
        }

        public void ForcePlay(string sourceName, AudioClip clip, float volume)
        {
            if (audioSources.ContainsKey(sourceName))
            {
                AudioEmitter audioSource = audioSources[sourceName];
                audioSource.ForcePlay(clip, volume);
            }
            else
            {
                throw new ArgumentException("AudioManager: AudioSource '" + sourceName + "' was not found.");
            }
        }

        public void CalculateVolumeByPercent(string sourceName, float startValue, float value, float maxVolume)
        {
            if (audioSources.ContainsKey(sourceName))
            {
                AudioEmitter audioSource = audioSources[sourceName];
                audioSource.CalculateVolumeByPercent(startValue, value, maxVolume);
            }
            else
            {
                throw new ArgumentException("AudioManager: AudioSource '" + sourceName + "' was not found.");
            }
        }

        public void Stop(string sourceName)
        {
            if (audioSources.ContainsKey(sourceName))
            {
                AudioEmitter audioSource = audioSources[sourceName];
                audioSource.Stop();
            }
            else
            {
                throw new ArgumentException("AudioManager: AudioSource '" + sourceName + "' was not found.");
            }
        }

        private AudioSource GetAvailableSource()
        {
            for (int i = 0, c = instancedSources.Count; i < c; i++)
            {
                if (!instancedSources[i].isPlaying)
                {
                    return instancedSources[i];
                }
            }

            if (instancedSources.Count < maxInstancedSources)
            {
                // If can't find any audio source available, create a new one and return it.
                GameObject go = new GameObject("Generic Source");
                AudioSource source = go.AddComponent<AudioSource>();

                instancedSources.Add(source);
                return source;
            }

            int index = lastPlayedIndex++ % instancedSources.Count;
            instancedSources[index].Stop();
            return instancedSources[index];
        }

        public void PlayClipAtPoint(AudioClip clip, Vector3 position, float minDistance, float maxDistance, float volume, float spatialBlend = 1)
        {
            if (!clip)
                return;

            AudioSource source = GetAvailableSource();
            source.gameObject.name = "Generic Source [Position " + position + "]";
            source.transform.position = position;
            source.playOnAwake = false;

            source.clip = clip;
            source.volume = volume * sfxVolume;

            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;

            source.spatialBlend = spatialBlend;
            source.outputAudioMixerGroup = sfxMixer;

            source.Play();
        }

        public virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnApplicationQuit()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}