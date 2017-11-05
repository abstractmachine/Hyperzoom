using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using System.IO;

[CustomEditor(typeof(Hyperzoom))]
public class HyperzoomEditor : Editor
{

    #region Properties

    private static string lastHyperzoomSaveFolder = "Assets/";

    #endregion


    #region Menu Item

    /// <summary>
    /// Create a Menu Bar item to create a new Hyperzoom material
    /// </summary>
    /// 

    [MenuItem("Tools/Hyperzoom/Create Fader Material")]
    static void CreateFaderMaterial()
    {
        // tell the user to select a path
        string path = EditorUtility.SaveFolderPanel("Select a folder for the 'Fader' material", lastHyperzoomSaveFolder, "");
        // remember this folder for the next time
        lastHyperzoomSaveFolder = CleanUpPath(path);

        // first copy the shader

        // shader path
        string finalShaderPath = lastHyperzoomSaveFolder + "/Basic-Fader.shader";
        // make sure the shader is not already at this path
        if (!File.Exists(finalShaderPath))
        {
            // find the original shader path
            string[] shaderResults = AssetDatabase.FindAssets("Hyperzoom-Basic-Fader t:Shader");
            // go through each result
            foreach (string guid in shaderResults)
            {
                // original path for shader
                string originalShaderPath = AssetDatabase.GUIDToAssetPath(guid);
                // add file to project
                if (!AssetDatabase.CopyAsset(originalShaderPath, finalShaderPath))
                {
                    Debug.LogWarning("Couldn't copy file '" + originalShaderPath + "' to '" + finalShaderPath + "'");
                }
            }
        }

        // now copy the material

        // material path
        string finalMaterialPath = lastHyperzoomSaveFolder + "/Basic-Fader.mat";
        // make sure the shader is not already at this path
        if (!File.Exists(finalMaterialPath))
        {
            // find the original shader path
            string[] materialResults = AssetDatabase.FindAssets("Hyperzoom-Basic-Fader t:Material");
            // go through each result
            foreach (string guid in materialResults)
            {
                // original path for shader
                string originalMaterialPath = AssetDatabase.GUIDToAssetPath(guid);
                // add file to project
                if (!AssetDatabase.CopyAsset(originalMaterialPath, finalMaterialPath))
                {
                    Debug.LogWarning("Couldn't copy file '" + originalMaterialPath + "' to '" + finalMaterialPath + "'");
                }
            }
        }
        else
        {
            Debug.LogWarning("File '" + finalMaterialPath + "' already exists");
        }

        //GameObject shaderResource = Resources.Load<GameObject>("Hyperzoom/Materials/Basic-Fader.shader");
    }


    protected static string CleanUpPath(string path)
    {
        // remove full data path
        if (path.StartsWith(Application.dataPath))
        {
            // remove start of full data path, just take the characters after the word "Assets"
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        // return cleaned up path
        return path;
    }

    #endregion

}
