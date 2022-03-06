using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Weapons
{
    public abstract class Equipment : MonoBehaviour
    {
        [SerializeField]
        protected EquipmentData equipmentData;

        public abstract void Init();

        public abstract void Use();

        #region PROPERTIES

        public virtual int Identifier => equipmentData.GetInstanceID();

        public abstract float UsageDuration
        {
            get;
        }

        #endregion

        /// <summary>
        /// Deactivates the shadows created by the equipment.
        /// </summary>
        public virtual void DisableShadowCasting()
        {
            // For each object that has a renderer inside the equipment gameObject.
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0, l = renderers.Length; i < l; i++)
            {
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].sharedMaterial.EnableKeyword("_VIEWMODEL");
            }
        }

        public abstract void Refill();
    }
}
