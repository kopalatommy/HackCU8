using UnityEngine;
using UnityEngine.InputSystem;

namespace UnnaturalSelection
{
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance 
        {
            get 
            { 
                if(instance == null)
                {
                    GameObject holder = GameObject.Find("Managers");
                    if(!holder.TryGetComponent<GameplayManager>(out instance))
                        holder.AddComponent<GameplayManager>();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        private static GameplayManager instance = null;

        [SerializeField]
        [Tooltip("Provides the behaviour settings of several actions, such Running, Aiming and mouse axes.")]
        private GameplaySettings gameplaySettings;

        [SerializeField]
        [Tooltip("Provides all buttons and axes used by the character.")]
        private InputActionAsset inputBindings;


        /// <summary>
        /// Is the character dead?
        /// </summary>
        public bool IsDead
        {
            get;
            set;
        }

        public float OverallMouseSensitivity => gameplaySettings.OverallMouseSensitivity;

        public bool InvertHorizontalAxis => gameplaySettings.InvertHorizontalAxis;

        public bool InvertVerticalAxis => gameplaySettings.InvertVerticalAxis;

        public float FieldOfView => gameplaySettings.FieldOfView;

        public InputActionMap GetActionMap(string mapName)
        {
            return inputBindings.FindActionMap(mapName);
        }

        public void SetFOV(float fov)
        {
            gameplaySettings.FieldOfView = fov;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            gameplaySettings.OverallMouseSensitivity = sensitivity;
        }

        public void SetInvertHorizontalAxis(bool invert)
        {
            gameplaySettings.InvertHorizontalAxis = invert;
        }

        public void SetInvertVerticalAxis(bool invert)
        {
            gameplaySettings.InvertVerticalAxis = invert;
        }

        public virtual void Awake()
        {
            if (Instance != this)
                Instance = this;
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
