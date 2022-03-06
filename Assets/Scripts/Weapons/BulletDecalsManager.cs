using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnnaturalSelection.Audio;
using UnnaturalSelection.Environment;

namespace UnnaturalSelection.Weapons
{
    [DisallowMultipleComponent]
    public class BulletDecalsManager : MonoBehaviour
    {
        public static BulletDecalsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject holder = GameObject.Find("Managers");
                    if (!holder.TryGetComponent<BulletDecalsManager>(out instance))
                        holder.AddComponent<BulletDecalsManager>();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        private static BulletDecalsManager instance = null;

        [SerializeField]
        [Tooltip("Defines the maximum number of decals that can be generated on the scene.")]
        protected int maxDecals = 250;

        /// <summary>
        /// Defines the minimum distance between two decals. (If the distance between two decals is less than the indicated value they will be merged into a single one)
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the minimum distance between two decals. (If the distance between two decals is less than the indicated value they will be merged into a single one)")]
        protected float decalSeparator = 0.166f;

        private readonly List<GameObject> decalList = new List<GameObject>();
        private readonly List<float> pushDistances = new List<float>();

        private void Awake()
        {
            if(instance == null)
                instance = this;
        }

        /// <summary>
        /// Generates a bullet hole decal.
        /// </summary>
        /// <param name="surface">The surface properties of the hit object.</param>
        /// <param name="hitInfo">The information about the projectile impact such as position and rotation.</param>
        public virtual void CreateBulletDecal(SurfaceIdentifier surface, RaycastHit hitInfo)
        {
            if (!surface)
                return;

            SurfaceType surfaceType = surface.GetSurfaceType(hitInfo.point, hitInfo.triangleIndex);

            if (!surfaceType)
                return;

            if (surface.AllowDecals(hitInfo.triangleIndex) && maxDecals > 0)
            {
                Material material = surfaceType.GetRandomDecalMaterial();
                if (material)
                {
                    GameObject decal = new GameObject("BulletMark_Decal");
                    decal.transform.position = hitInfo.point;
                    decal.transform.rotation = Quaternion.FromToRotation(Vector3.back, hitInfo.normal);

                    decal.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)));

                    float scale = surfaceType.GetRandomDecalSize() / 5;
                    decal.transform.localScale = new Vector3(scale, scale, scale);

                    decal.transform.parent = hitInfo.transform;

                    DecalPresets decalPresets = new DecalPresets()
                    {
                        maxAngle = 60,
                        pushDistance = 0.009f + RegisterDecal(decal, scale),
                        material = material
                    };

                    Decal d = decal.AddComponent<Decal>();
                    d.Calculate(decalPresets, hitInfo.collider.gameObject);
                }
            }

            GameObject particle = surfaceType.GetRandomImpactParticle();
            if (particle)
            {
                Instantiate(particle, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
            }

            AudioClip clip = surfaceType.GetRandomImpactSound();
            if (clip)
            {
                AudioManager.Instance.PlayClipAtPoint(clip, hitInfo.point, 2.5f, 25, surfaceType.BulletImpactVolume);
            }
        }

        /// <summary>
        /// Register a new decal and validate if it can be generated.
        /// </summary>
        /// <param name="decal">The decal object.</param>
        /// <param name="scale">The decal size.</param>
        protected virtual float RegisterDecal(GameObject decal, float scale)
        {
            GameObject auxGO;
            Transform currentT = decal.transform;
            Vector3 currentPos = currentT.position;

            float radius = Mathf.Sqrt((scale * scale * 0.25f) * 3);

            float realRadius = radius * 2;
            radius *= decalSeparator;

            if (decalList.Count == maxDecals)
            {
                auxGO = decalList[0];
                Destroy(auxGO);
                decalList.RemoveAt(0);
                pushDistances.RemoveAt(0);
            }

            float pushDistance = 0;

            for (int i = 0; i < decalList.Count; i++)
            {
                auxGO = decalList[i];

                if (auxGO)
                {
                    Transform auxT = auxGO.transform;
                    float distance = (auxT.position - currentPos).magnitude;

                    if (distance < radius)
                    {
                        Destroy(auxGO);
                        decalList.RemoveAt(i);
                        pushDistances.RemoveAt(i);
                        i--;
                    }
                    else if (distance < realRadius)
                    {
                        float cDist = pushDistances[i];
                        pushDistance = Mathf.Max(pushDistance, cDist);
                    }
                }
                else
                {
                    decalList.RemoveAt(i);
                    pushDistances.RemoveAt(i);
                    i--;
                }
            }

            pushDistance += 0.001f;

            decalList.Add(decal);
            pushDistances.Add(pushDistance);

            return pushDistance;
        }

        /// <summary>
        /// Clear all decals generated in the scene.
        /// </summary>
        public virtual void RemoveDecals()
        {
            if (decalList.Count <= 0)
                return;

            for (int i = 0, c = decalList.Count; i < c; i++)
            {
                GameObject go = decalList[i];
                Destroy(go);
            }
            decalList.Clear();
            pushDistances.Clear();
        }
    }
}
