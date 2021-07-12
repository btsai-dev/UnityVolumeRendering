using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class GameMaster : MonoBehaviour
    {
        
        [SerializeField]
        public Slider vol_sur_slider;

        [SerializeField]
        public Slider vis_slider;

        [HideInInspector]
        public string DefaultFolder;

        [HideInInspector]
        public string TargetFolder;
        
        [HideInInspector]
        public ImageSequenceImporter importer;

        [HideInInspector]
        public VolumeDataset dataset;

        [HideInInspector]
        public VolumeRenderedObject renderedObj;

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
        }

        private void update_object()
        {
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
        }

        private void update_visibility()
        {
            float val = vis_slider.value;
            Vector2 visibilityWindow = renderedObj.GetVisibilityWindow();
        }

        private void loadNew(string folder)
        {
            // Destroy existing versions
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            CrossSectionPlane[] planes = GameObject.FindObjectsOfType<CrossSectionPlane>();

            foreach(var obj in objects)
            {
                Debug.Log("Destroying object!");
                UnityEngine.Object.Destroy(obj.transform.gameObject);
            }           

            foreach(var plane in planes)
            {
                Debug.Log("Destroying plane!");
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

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}