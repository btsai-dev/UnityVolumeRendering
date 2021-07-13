using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UnityVolumeRendering
{
    public class GameMaster : MonoBehaviour
    {
        
        [SerializeField]
        public Slider vol_sur_slider;

        [SerializeField]
        public RangeSlider vis_slider;

        [SerializeField]
        public Text vis_slider_low_val;

        
        [SerializeField]
        public Canvas transferCanvas;

        [SerializeField]
        public Text vis_slider_hi_val;

        [HideInInspector]
        public string DefaultFolder;

        [HideInInspector]
        public string TargetFolder;
        
        [HideInInspector]
        public ImageSequenceImporter importer;

        [HideInInspector]
        public VolumeDataset dataset;

        [HideInInspector]
        public VolumeRenderedObject renderedObj = null;

        [HideInInspector]
        public CrossSectionPlane cross_section;

        [HideInInspector]
        public Vector3 disp_top_pos;

        // Start is called before the first frame update
        void Start()
        {
            disp_top_pos = new Vector3()
            {
                x = 0f,
                y = 1f,
                z = 2f
            };
            Debug.Log("Gamemaster Starting.");
            if (Application.platform == RuntimePlatform.Android)
                print ("Android Detected");
            else
            {
                DefaultFolder = Path.Combine(Application.streamingAssetsPath, "SkullPng");
                Debug.Log("Attempting to load at Default Folder + " + DefaultFolder);
            }

            if (Directory.Exists(DefaultFolder))
                loadNew(DefaultFolder);
            else
                Debug.Log("Directory not found, not loading anything!");


            vol_sur_slider.onValueChanged.AddListener(delegate {update_object();});
            vis_slider.OnValueChanged.AddListener(delegate {update_visibility();});
            

            // Load transfer function GUI
            
            if (renderedObj != null)
            {
                renderedObj.SetTransferFunctionMode(TFRenderMode.TF1D);
                transferCanvas.GetComponent<TFGui>().SetupTransferUi();
            }
            
        }
        

        private void update_object()
        {
            Debug.Log("Updating object!");
            int val = Mathf.RoundToInt(vol_sur_slider.value);
            if (val == 0)
            {
                RenderMode mode = RenderMode.DirectVolumeRendering;
                renderedObj.SetRenderMode(mode);
            } else if (val == 1)
            {
                RenderMode mode = RenderMode.IsosurfaceRendering;
                renderedObj.SetRenderMode(mode);
            }
            vis_slider.LowValue = vis_slider.MinValue;
            vis_slider.HighValue = vis_slider.MaxValue;
            
            vis_slider_low_val.text = vis_slider.LowValue.ToString("0.00");
            vis_slider_hi_val.text = vis_slider.HighValue.ToString("0.00");
        }

        private void update_visibility()
        {
            Debug.Log("Updating visibility!");
            float lowVal = vis_slider.LowValue;
            float hiVal = vis_slider.HighValue;
            vis_slider_low_val.text = vis_slider.LowValue.ToString("0.00");
            vis_slider_hi_val.text = vis_slider.HighValue.ToString("0.00");
            Vector2 visibilityWindow = renderedObj.GetVisibilityWindow();
            Vector2 values = new Vector2()
            {
                x = lowVal,
                y = hiVal
            };
            renderedObj.SetVisibilityWindow(values);
        }

        private void loadNew(string folder)
        {
            // Destroy existing versions
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            CrossSectionPlane[] planes = GameObject.FindObjectsOfType<CrossSectionPlane>();

            foreach(var obj in objects)
            {
                UnityEngine.Object.Destroy(obj.transform.gameObject);
            }           

            foreach(var plane in planes)
            {
                UnityEngine.Object.Destroy(plane.transform.gameObject);
            }      
        
            // Display volumetric object
            importer = new ImageSequenceImporter(DefaultFolder);
            dataset = importer.Import();


            renderedObj = VolumeObjectFactory.CreateObject(dataset);

            GameObject g_renderedObj = renderedObj.transform.gameObject;
            Vector3 targetPos = new Vector3()
            {
                x = disp_top_pos.x,
                y = disp_top_pos.y + renderedObj.GetComponentInChildren<MeshRenderer>().bounds.extents.y * 2,
                z = disp_top_pos.z
            };

            g_renderedObj.transform.position = targetPos;
            g_renderedObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
            
            VolumeObjectFactory.SpawnCrossSectionPlane(renderedObj);
        }

        
    }
}