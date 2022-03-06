using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Environment
{
    [DisallowMultipleComponent]
    public class SurfaceIdentifier : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("List of surfaces of the GameObject.")]
        private SurfaceData[] surfaceList = new SurfaceData[1];
        
        private Terrain ActiveTerrain => GetComponent<Terrain>();

        public bool IsTerrain => GetComponent<Terrain>() != null;

        public Material[] Materials
        {
            get
            {
                Renderer renderere = gameObject.GetComponent<Renderer>();
                return renderere != null ? renderere.sharedMaterials : null;
            }
        }

        public Texture2D[] Textures
        {
            get
            {
                if (!IsTerrain || ActiveTerrain == null)
                    return null;

                TerrainLayer[] terrainLayers = ActiveTerrain.terrainData.terrainLayers;
                Texture2D[] tex = new Texture2D[terrainLayers.Length];

                for (int i = 0; i < terrainLayers.Length; i++)
                    tex[i] = terrainLayers[i].diffuseTexture;

                return tex;
            }
        }

        public int SurfacesCount => surfaceList.Length;

        public bool AllowDecals(int triangleIndex = -1)
        {
            if (surfaceList == null)
                return false;

            return !IsTerrain && surfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].AllowDecals;
        }

        public bool CanPenetrate(int triangleIndex = -1)
        {
            if (surfaceList == null)
                return false;

            return !IsTerrain && surfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].Penetration;
        }

        public float Density(int triangleIndex = -1)
        {
            if (surfaceList == null)
                return 1;

            if (!IsTerrain)
            {
                return surfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].Density;
            }
            return 1;
        }

        public SurfaceType GetSurfaceType(Vector3 position, int triangleIndex = -1)
        {
            if (surfaceList == null || surfaceList.Length <= 0)
                return null;

            if (IsTerrain)
            {
                int index = SurfaceUtility.GetMainTexture(position, ActiveTerrain.transform.position, ActiveTerrain.terrainData);
                return index < surfaceList.Length ? surfaceList[index].SurfaceType : null;

            }
            return surfaceList[SurfaceUtility.GetMaterialIndex(triangleIndex, gameObject)].SurfaceType;
        }

        public void Reset()
        {
            surfaceList = GetSurfaceList();
        }

        private SurfaceData[] GetSurfaceList()
        {
            SurfaceData[] surfaces;

            // Is this component attached to a terrain?
            if (IsTerrain)
            {
                TerrainLayer[] terrainLayers = ActiveTerrain.terrainData.terrainLayers;
                surfaces = new SurfaceData[terrainLayers.Length];

                for (int i = 0; i < terrainLayers.Length; i++)
                    surfaces[i] = new SurfaceData();
            }
            else
            {
                Renderer r = gameObject.GetComponent<Renderer>();

                if (r && r.sharedMaterials.Length > 0)
                {
                    surfaces = new SurfaceData[r.sharedMaterials.Length];

                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                        surfaces[i] = new SurfaceData();
                }
                else
                {
                    surfaces = new[] { new SurfaceData() };
                }
            }
            return surfaces;
        }
    }
}
