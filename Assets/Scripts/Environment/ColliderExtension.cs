using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Environment
{
    public static class ColliderExtension
    {
        private static readonly Dictionary<int, SurfaceIdentifier> surfaceIdentifiers = new Dictionary<int, SurfaceIdentifier>();

        public static SurfaceIdentifier GetSurface(this Collider col)
        {
            if (surfaceIdentifiers.ContainsKey(col.GetInstanceID()))
            {
                return surfaceIdentifiers[col.GetInstanceID()];
            }

            surfaceIdentifiers.Add(col.GetInstanceID(), col.GetComponent<SurfaceIdentifier>());
            return surfaceIdentifiers[col.GetInstanceID()];
        }
    }
}
