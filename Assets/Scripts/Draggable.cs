using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityVolumeRendering
{
    public class Draggable : Selectable, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        private RectTransform dragRectTransform;
        private Canvas canvas;
        float canvasW;
        float canvasH;
        private float canvasScaleFactor;
        private float max_x;
        public bool dragOnSurfaces = true;
        private RectTransform draggingPlane;
        public TFAlphaControlPoint alphaPoint;
        public int alphaControlIndex;
        public VolumeRenderedObject renderedObj;
        public TFGui tFGui;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.canvas = transform.GetComponentInParent<Canvas>();
            this.dragRectTransform = transform.GetComponent<RectTransform>();
            this.canvasW = canvas.GetComponent<RectTransform>().rect.width;
            this.canvasH = canvas.GetComponent<RectTransform>().rect.height;
            this.tFGui = canvas.GetComponent<TFGui>();

            canvasScaleFactor = canvas.scaleFactor;
        }

        public void OnInitializePotentialDrag(PointerEventData ped)
        {
            ped.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("BEGIN DRAG!");
            if (dragOnSurfaces)
                draggingPlane = transform as RectTransform;
            else
                draggingPlane = canvas.transform as RectTransform;

            SetDraggedPosition(eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging!");
            if (dragRectTransform != null)
            {
                SetDraggedPosition(eventData);
                //var anchorPos = dragRectTransform.anchoredPosition;
                //alphaPoint.dataValue = Mathf.Clamp(anchorPos.x / canvasW, 0.0f, 1.0f);
                //alphaPoint.alphaValue = Mathf.Clamp(anchorPos.y / canvasH, 0.0f, 1.0f);
                //TransferFunction tf = renderedObj.transferFunction;
                //tf.alphaControlPoints[alphaControlIndex] = alphaPoint;
                // canvas.GetComponent<TFGui>().UpdateTransfer();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragRectTransform != null)
            {
                var anchorPos = dragRectTransform.anchoredPosition;
                alphaPoint.dataValue = Mathf.Clamp(anchorPos.x / canvasW, 0.0f, 1.0f);
                alphaPoint.alphaValue = Mathf.Clamp(anchorPos.y / canvasH, 0.0f, 1.0f);
                TransferFunction tf = renderedObj.transferFunction;
                tf.alphaControlPoints[alphaControlIndex] = alphaPoint;
                canvas.GetComponent<TFGui>().UpdateTransfer();
            }
        }

        private void SetDraggedPosition(PointerEventData data)
        {
            Debug.Log("Drag positioning!");
            if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
                draggingPlane = data.pointerEnter.transform as RectTransform;
    
            //var rt = m_DraggingIcon.GetComponent<RectTransform>();
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out globalMousePos))
            {
                Debug.Log("Changing positioning!");
                dragRectTransform.position = globalMousePos;
                dragRectTransform.rotation = draggingPlane.rotation;
            }
        }

        void Update()
        {
            var anchorPos = dragRectTransform.anchoredPosition;
            anchorPos.x = Mathf.Clamp(anchorPos.x, 0, canvasW);
            anchorPos.y = Mathf.Clamp(anchorPos.y, 0, canvasH);
            dragRectTransform.anchoredPosition = anchorPos;
            
            var localPos = dragRectTransform.localPosition;
            localPos.z = Mathf.Clamp(localPos.z, 0, 0);
            dragRectTransform.localPosition = localPos;
        }
    }
}