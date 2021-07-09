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

        float x_min = float.MinValue;
        float x_max = float.MaxValue;
        float y_min = float.MinValue;
        float y_max = float.MaxValue;
        float z_min = float.MinValue;
        float z_max = float.MaxValue;

        public void Start()
        {
            var meshBounds = targetObject.GetComponentInChildren<MeshRenderer>().bounds;
            x_min = meshBounds.min.x;
            x_max = meshBounds.max.x;
            y_min = meshBounds.min.y;
            y_max = meshBounds.max.y;
            z_min = meshBounds.min.z;
            z_max = meshBounds.max.z;
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
            clampedPosition.x =  Mathf.Clamp(transform.position.x, x_min*2, x_max*2 );
            clampedPosition.y =  Mathf.Clamp(transform.position.y, y_min*2, y_max*2);
            clampedPosition.z =  Mathf.Clamp(transform.position.z, z_min*2, z_max*2);

            transform.position = clampedPosition;

            mat.EnableKeyword("CUTOUT_PLANE");
            mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
        }
    }
}
