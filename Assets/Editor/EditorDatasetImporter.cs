using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class EditorDatasetImporter
    {
        public static void ImportDataset(string filePath)
        {
            DatasetType datasetType = DatasetImporterUtility.GetDatasetType(filePath);

            switch (datasetType)
            {
                case DatasetType.Raw:
                    {
                        RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                        if (wnd != null)
                            wnd.Close();

                        wnd = new RAWDatasetImporterEditorWindow(filePath);
                        wnd.Show();
                        break;
                    }
            }
        }
    }
}
