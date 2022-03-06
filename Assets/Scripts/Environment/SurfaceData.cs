using UnityEngine;

namespace UnnaturalSelection.Environment
{
    [System.Serializable]
    public class SurfaceData
    {
        [SerializeField]
        [Tooltip("The type of the surface.")]
        private SurfaceType surfaceType;

        [SerializeField]
        [Tooltip("Allows bullet marks to be rendered on this surface.")]
        private bool allowDecals = true;

        [SerializeField]
        [Tooltip("Allows projectiles to penetrate this surface.")]
        private bool penetration;

        [SerializeField]
        [Tooltip("Defines surface density. A higher value will make it harder for a projectile to get through the object." +
                 "(This value is directly related to the penetration power of each weapon)")]
        private float density = 1;

        public SurfaceType SurfaceType => surfaceType;

        public bool AllowDecals => allowDecals;

        public bool Penetration => penetration;

        public float Density => density;
    }
}
