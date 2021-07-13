using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UnityVolumeRendering
{
    public class TFGui : MonoBehaviour
    {
        [SerializeField]
        public GameObject gameMasterObj;

        [SerializeField]
        public Canvas tfCanvas;

        [SerializeField]
        public Canvas histCanvas;
        private GameMaster gameMaster;
        private VolumeRenderedObject renderedObj;
        private Material tfGUIMat = null;
        private Material tfPaletteGUIMat = null;
        private Texture2D histTex = null;

        private Image backgroundImg;
        public void SetupTransferUi()
        {
            
            tfGUIMat = Resources.Load<Material>("TransferFunctionGUIMat");
            tfPaletteGUIMat = Resources.Load<Material>("TransferFunctionPaletteGUIMat");
            backgroundImg = histCanvas.GetComponent<Image>();

            GameMaster gameMaster = gameMasterObj.GetComponent<GameMaster>();
            renderedObj = gameMaster.renderedObj;
            
            if (renderedObj == null)
                return;
            TransferFunction tf = renderedObj.transferFunction;
            tf.GenerateTexture();

            if(histTex == null)
            {
                if(SystemInfo.supportsComputeShaders)
                    histTex = HistogramTextureGenerator.GenerateHistogramTextureOnGPU(renderedObj.dataset);
                else
                    histTex = HistogramTextureGenerator.GenerateHistogramTexture(renderedObj.dataset);
            }

            tfGUIMat.SetTexture("_TFTex", tf.GetTexture());
            tfGUIMat.SetTexture("_HistTex", histTex);
            backgroundImg.material = tfGUIMat;

            // Alpha control points
            float canvasW = tfCanvas.GetComponent<RectTransform>().rect.width;
            float canvasH = tfCanvas.GetComponent<RectTransform>().rect.height;

            // Delete all current alpha control points, if any
            foreach (var obj in FindObjectsOfType(typeof(Draggable)) as Draggable[])
            {
                Debug.Log("Purging!");
                Destroy(obj.gameObject);
            }
            
            for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++)
            {
                TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[iAlpha];
                GameObject tfControlUI = new GameObject();
                tfControlUI.transform.parent = tfCanvas.transform;
                DiamondGraph ctrl = tfControlUI.AddComponent<DiamondGraph>();
                Draggable script = tfControlUI.AddComponent<Draggable>();
                RectTransform rect = ctrl.GetComponent<RectTransform>();

                script.alphaControlIndex = iAlpha;
                script.renderedObj = renderedObj;

                rect.localRotation = Quaternion.identity;
                rect.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                
                rect.anchoredPosition = new Vector2(
                    canvasW * alphaPoint.dataValue,
                    canvasH * alphaPoint.alphaValue
                );

                rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0);
            }
        }

    public void UpdateTransfer()
        {
            TransferFunction tf = renderedObj.transferFunction;
            tf.GenerateTexture();

            if(histTex == null)
            {
                if(SystemInfo.supportsComputeShaders)
                    histTex = HistogramTextureGenerator.GenerateHistogramTextureOnGPU(renderedObj.dataset);
                else
                    histTex = HistogramTextureGenerator.GenerateHistogramTexture(renderedObj.dataset);
            }

            tfGUIMat.SetTexture("_TFTex", tf.GetTexture());
            tfGUIMat.SetTexture("_HistTex", histTex);
            backgroundImg.material = tfGUIMat;
        }
    }
}
