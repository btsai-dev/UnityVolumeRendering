using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Profiling;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Converts a directory of image slices into a VolumeDataset for volumetric rendering.
    /// </summary>
    public class ImageSequenceImporter : DatasetImporterBase
    {
        ProfilerRecorder totalReservedMemoryRecorder;
        ProfilerRecorder gcReservedMemoryRecorder;
        ProfilerRecorder systemUsedMemoryRecorder;
        private string directoryPath;
        private string[] supportedImageTypes = new string[] 
        {
            "*.png",
            "*.jpg"
        };

        public ImageSequenceImporter(string directoryPath)
        {
            this.directoryPath = directoryPath;
        }

        public override VolumeDataset Import()
        {
            if (!Directory.Exists(directoryPath))
                throw new NullReferenceException("No directory found: " + directoryPath);

            List<string> imagePaths = GetSortedImagePaths();

            Vector3Int volume_dimensions = GetVolumeDimensions(imagePaths);
            
            Debug.Log("True volume dimensions: " + volume_dimensions);
            Vector2Int flat_dimensions = new Vector2Int()
            {
                x = volume_dimensions.x,
                y = volume_dimensions.y
            };

            int face_dim = volume_dimensions.x * volume_dimensions.y;
            if ((Int32.MaxValue / face_dim) + 1 < volume_dimensions.z)
            {
                throw new IndexOutOfRangeException("Beyond reasonable size, " + face_dim + " * " + volume_dimensions.z + " greater than Int32's max val");
            }

            if (!AssertIdenticalDimension(imagePaths, flat_dimensions))
                throw new IndexOutOfRangeException("Image sequence has non-uniform dimensions");

            int[] data = FillSequentialData(volume_dimensions, imagePaths);
            
            VolumeDataset dataset = FillVolumeDataset(data, volume_dimensions, volume_dimensions);

            Debug.Log("Done importing.");
            return dataset;
        }

        public VolumeDataset Import(int resizeX, int resizeY, int slices)
        {

            if (!Directory.Exists(directoryPath))
                throw new NullReferenceException("No directory found: " + directoryPath);

            List<string> imagePaths = GetSortedImagePaths();

            Vector3Int volume_dimensions = GetVolumeDimensions(imagePaths);
            Vector2Int flat_dimensions = new Vector2Int()
            {
                x = volume_dimensions.x,
                y = volume_dimensions.y
            };

            Vector3Int resize_dimensions = new Vector3Int()
            {
                x = resizeX,
                y = resizeY,
                z = slices
            };

            
            Debug.Log("True volume dimensions: " + volume_dimensions);
            Debug.Log("New resized volume dimensions: " + resize_dimensions);

            int face_dim = resize_dimensions.x * resize_dimensions.y;
            if ((Int32.MaxValue / face_dim) + 1 < resize_dimensions.z)
            {
                throw new IndexOutOfRangeException("Beyond reasonable size, " + face_dim + " * " + resize_dimensions.z + " greater than Int32's max val");
            }

            if (!AssertIdenticalDimension(imagePaths, flat_dimensions))
                throw new IndexOutOfRangeException("Image sequence has non-uniform dimensions");

            int[] data = FillSequentialData(resize_dimensions, imagePaths);
            
            VolumeDataset dataset = FillVolumeDataset(data, volume_dimensions, resize_dimensions);

            Debug.Log("Done importing.");
            return dataset;
        }


        /// <summary>
        /// Gets every file path in the directory with a supported suffix.
        /// </summary>
        /// /// <returns>A sorted list of image file paths.</returns>
        private List<string> GetSortedImagePaths()
        {
            var imagePaths = new List<string>();

            foreach (var type in supportedImageTypes)
            {
                imagePaths.AddRange(Directory.GetFiles(directoryPath, type));
            }

            imagePaths.Sort();

            return imagePaths;
        }

        /// <summary>
        /// Checks if every image in the set has the same XY dimensions.
        /// </summary>
        /// <param name="imagePaths">The list of image paths to check.</param>
        /// <returns>True if at least one image differs from another.</returns>
        private bool AssertIdenticalDimension(List<string> imagePaths, Vector2Int dimensions)
        {
            
            foreach (string path in imagePaths)
            {
                Vector2Int current = GetImageDimensions(path);
                if (current.x != dimensions.x || current.y != dimensions.y)
                {
                    Debug.Log("Error: Invalid dimension @" + path);
                    Debug.Log("Dimension: " + current);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the XY dimensions of an image at the path.
        /// </summary>
        /// <param name="path">The image path to check.</param>
        /// <returns>The XY dimensions of the image.</returns>
        private Vector2Int GetImageDimensions(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            Texture2D texture = new Texture2D(1, 1);
            //texture.hideFlags = HideFlags.HideAndDontSave;  // Fix memory leak
            texture.LoadImage(bytes);

            Vector2Int dimensions = new Vector2Int()
            {
                x = texture.width,
                y = texture.height
            };
            
            UnityEngine.Object.DestroyImmediate(texture);            // Fix memory leak
            return dimensions;
        }

        /// <summary>
        /// Adds a depth value Z to the XY dimensions of the first image.
        /// </summary>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The dimensions of the volume.</returns>
        private Vector3Int GetVolumeDimensions(List<string> paths)
        {
            Vector2Int twoDimensional = GetImageDimensions(paths[0]);
            Vector3Int threeDimensional = new Vector3Int()
            {
                x = twoDimensional.x,
                y = twoDimensional.y,
                z = paths.Count
            };
            return threeDimensional;
        }

        /// <summary>
        /// Converts a volume set of images into a sequential series of values.
        /// </summary>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The set of sequential values for the volume.</returns>
        private int[] FillSequentialData(Vector3Int resize_dimensions, List<string> paths)
        {            
            var data = new List<int>(resize_dimensions.x * resize_dimensions.y * resize_dimensions.z);
            Debug.Log("Size of data list (START)" + data.Capacity);

            int index = 0;
        
            foreach (var path in paths)
            {
                if (index >= resize_dimensions.z)
                    break;
                var texture = new Texture2D(1, 1);
                //texture.hideFlags = HideFlags.HideAndDontSave;  // Fix memory leak
                byte[] bytes = File.ReadAllBytes(path);
                texture.LoadImage(bytes);
                TextureScale.Bilinear(texture, resize_dimensions.x, resize_dimensions.y);

                Color[] pixels = texture.GetPixels(); // Order priority: X -> Y -> Z
                int[] imageData = DensityHelper.ConvertColorsToDensities(pixels);
                data.AddRange(imageData);
                UnityEngine.Object.DestroyImmediate(texture);            // Fix memory leak
                index ++;
            }

            return data.ToArray();
        }

        /// <summary>
        /// Wraps volume data into a VolumeDataset.
        /// </summary>
        /// <param name="data">Sequential value data for a volume.</param>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <returns>The wrapped volume data.</returns>
        private VolumeDataset FillVolumeDataset(int[] data, Vector3Int true_dimensions, Vector3Int resize_dimensions)
        {
            string name = Path.GetFileName(directoryPath);

            VolumeDataset dataset = new VolumeDataset()
            {
                name = name,
                datasetName = name,
                data = data,
                dimX = resize_dimensions.x,
                dimY = resize_dimensions.y,
                dimZ = resize_dimensions.z,
                scaleX = 1f, // Scale arbitrarily normalised around the x-axis 
                scaleY = (float)true_dimensions.y / (float)true_dimensions.x,
                scaleZ = (float)true_dimensions.z / (float)true_dimensions.x
            };

            return dataset;
        }
    }
}