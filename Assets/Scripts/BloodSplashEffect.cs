using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnnaturalSelection
{
    public class BloodSplashEffect : MonoBehaviour
    {
        [SerializeField]
        private Texture2D bloodTexture;

        [SerializeField]
        private Texture2D bloodNormalMap;

        [SerializeField]
        [Range(0, 1)]
        private float bloodAmount;

        [SerializeField]
        private float distortion = 1.0f;

        private Material material;
        private static readonly int BloodTex = Shader.PropertyToID("_BloodTex");
        private static readonly int BloodBump = Shader.PropertyToID("_BloodBump");
        private static readonly int Distortion = Shader.PropertyToID("_Distortion");
        private static readonly int Amount = Shader.PropertyToID("_BloodAmount");

        [SerializeField] private Material bloodSplatterMaterial = null;

        //Properties
        public float BloodAmount
        {
            get => bloodAmount;
            set => bloodAmount = value;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (material == null)
                material = new Material(Shader.Find("Hidden/BloodSplashEffect"));

            if (material == null)
                return;

            //Send data into Shader
            if (bloodTexture != null)
                material.SetTexture(BloodTex, bloodTexture);

            if (bloodNormalMap != null)
                material.SetTexture(BloodBump, bloodNormalMap);

            material.SetFloat(Distortion, distortion);
            material.SetFloat(Amount, bloodAmount);

            Graphics.Blit(src, dest, material);
        }
    }
}
