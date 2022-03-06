using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection.Environment
{
    public class SurfaceUtility
    {
        public static int GetMainTexture(Vector3 worldPos, Vector3 terrainPos, TerrainData terrainData)
        {
            float[] mix = GetTextureMix(worldPos, terrainPos, terrainData);

            float maxMix = 0;
            int maxIndex = 0;

            // Loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; n++)
            {
                if (!(mix[n] > maxMix))
                    continue;

                maxIndex = n;
                maxMix = mix[n];
            }
            return maxIndex;
        }

        private static float[] GetTextureMix(Vector3 worldPos, Vector3 terrainPos, TerrainData terrainData)
        {
            // Calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            // Get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatMapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // Extract the 3D array data to a 1D array
            float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatMapData[0, 0, n];
            }
            return cellMix;
        }

        public static int GetMaterialIndex(int triangleIndex, GameObject gameObject)
        {
            // Return 0 if the mesh hasn't any sub meshes
            if (triangleIndex == -1)
                return 0;

            Mesh mesh = GetSharedMesh(gameObject);

            if (!mesh || !mesh.isReadable)
                return 0;

            int[] hitTriangle =
            {
                mesh.triangles[triangleIndex * 3],
                mesh.triangles[triangleIndex * 3 + 1],
                mesh.triangles[triangleIndex * 3 + 2]
            };

            // Loop through each sub mesh
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subMeshTris = mesh.GetTriangles(i);

                // Loop through each triangle in the sub mesh
                for (int t = 0; t < subMeshTris.Length; t += 3)
                {
                    // Return the index of which sub mesh the triangle is inscribed in.
                    if (subMeshTris[t] == hitTriangle[0] && subMeshTris[t + 1] == hitTriangle[1] && subMeshTris[t + 2] == hitTriangle[2])
                    {
                        return i;
                    }
                }
            }

            return 0;
        }

        private static Mesh GetSharedMesh(GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter)
            {
                return meshFilter.sharedMesh;
            }

            SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            return !skinnedMeshRenderer ? null : skinnedMeshRenderer.sharedMesh;
        }
    }
}
