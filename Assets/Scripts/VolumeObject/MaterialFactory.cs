﻿using UnityEngine;

namespace UnityVolumeRendering
{
    public class MaterialFactory
    {
        public static Material CreateMaterialDVR(VolumeDataset dataset)
        {
            Shader shader = Shader.Find("VolumeRendering/DirectVolumeRenderingShader");
            Material material = new Material(shader);

            const int noiseDimX = 512;
            const int noiseDimY = 512;
            material.SetTexture("_NoiseTex", NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY));
            material.SetTexture("_DataTex", dataset.GetDataTexture());

            return material;
        }
    }
}
