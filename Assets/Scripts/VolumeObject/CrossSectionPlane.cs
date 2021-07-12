using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cross section plane.
    /// Used for cutting a model (cross section view).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionPlane : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        public VolumeRenderedObject targetObject;

        public float x_dist = float.MaxValue;
        public float y_dist = float.MaxValue;
        public float z_dist = float.MaxValue;


        public void Start()
        {
            var meshExtents = targetObject.GetComponentInChildren<MeshRenderer>().bounds.extents;
            x_dist = meshExtents.x;
            y_dist = meshExtents.y;
            z_dist = meshExtents.z;
        }

        private void OnDisable()
        {
            if (targetObject != null)
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_PLANE");
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            Vector3 clampedPosition = transform.position;
            Vector3 targetPos = targetObject.transform.position;
            
            clampedPosition.x =  Mathf.Clamp(transform.position.x, targetPos.x - x_dist*2, targetPos.x + x_dist*2);
            clampedPosition.y =  Mathf.Clamp(transform.position.y, targetPos.y - y_dist*2, targetPos.y + y_dist*2);
            clampedPosition.z =  Mathf.Clamp(transform.position.z, targetPos.z - z_dist*2, targetPos.z + z_dist*2);

            transform.position = clampedPosition;

            mat.EnableKeyword("CUTOUT_PLANE");
            mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
        }
    }
}
