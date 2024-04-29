using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;
using System;
using System.Linq;

public class AnimationPathModifier : EditorWindow
{
    private string name;
    private bool inputRootBone = true;
    private GameObject rootBone;
    private string rootBoneName = "Armature";
    private DefaultAsset folderPath;

    [UnityEditor.MenuItem("YuSa64/Vicon Path Modifier")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPathModifier>("Vicon Path Modifier");
    }

    private void OnGUI()
    {
        name = EditorGUILayout.TextField("액터 이니셜", name);
        inputRootBone = EditorGUILayout.Toggle("루트 본 직접 입력", inputRootBone);
        if (inputRootBone)
        {
            rootBoneName = EditorGUILayout.TextField("루트 본", rootBoneName);
        }
        else
        {
            rootBone = EditorGUILayout.ObjectField("루트 본", rootBone, typeof(GameObject), true) as GameObject;
            rootBoneName = rootBone != null ? rootBone.name : "Armature";
        }
        folderPath = (DefaultAsset)EditorGUILayout.ObjectField("폴더", folderPath, typeof(DefaultAsset), false);

        if (GUILayout.Button("패스 수정"))
        {
            ModifyPaths();
        }

        GUILayout.Label("패스 수정이 완료되고 나면,\n대상 폴더 내의 converted 폴더에 변환된 애니메이션들이 있습니다. \nBy YuSa64", EditorStyles.helpBox);
    }

    private void ModifyPaths()
    {
        if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog("Error", "이니셜을 입력해주세요.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(rootBoneName))
        {
            EditorUtility.DisplayDialog("Error", "루트 본을 입력해주세요.", "OK");
            return;
        }
        if (folderPath != null)
        {
            string assetFolderPath = AssetDatabase.GetAssetPath(folderPath);
            string convertedFolderPath = Path.Combine(assetFolderPath, "converted");

            // Create the 'converted' folder if it doesn't exist
            if (!Directory.Exists(convertedFolderPath))
            {
                Directory.CreateDirectory(convertedFolderPath);
            }

            string[] fbxFiles = Directory.GetFiles(assetFolderPath, "*.fbx", SearchOption.AllDirectories);

            foreach (string filePath in fbxFiles)
            {
                ExtractAnim(assetFolderPath, filePath);
            }

            // Get all .anim files in the folder
            string[] animFiles = Directory.GetFiles(assetFolderPath, "*.anim", SearchOption.AllDirectories);

            foreach (string filePath in animFiles)
            {
                // Ignore files already in the 'converted' folder
                if (filePath.StartsWith(convertedFolderPath))
                {
                    continue;
                }

                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(convertedFolderPath, fileName);

                // Duplicate the .anim file to the 'converted' folder using AssetDatabase.CopyAsset
                string sourcePath = filePath.Replace(Application.dataPath, "Assets");
                string destinationPath = destFilePath.Replace(Application.dataPath, "Assets");

                if (AssetDatabase.CopyAsset(sourcePath, destinationPath))
                {
                    // Modify the paths in the duplicated file
                    ModifyFile(destinationPath);
                }
                else
                {
                    Debug.LogError("Failed to copy asset: " + sourcePath);
                }
            }

            // Refresh the AssetDatabase after modifying the files
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "패스 수정이 완료되었습니다.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "애님 파일이 들어있는 폴더를 선택해주세요.", "OK");
        }
    }

    private void ModifyFile(string assetPath)
    {
        string[] lines = File.ReadAllLines(assetPath);

        for (int i = 0; i < lines.Length; i++)
        {
            // Replace 'name+":"' in every path
            lines[i] = lines[i].Replace(name + ":", "");

            // Change every "Hips" in path to "rootBoneName/Hips"
            lines[i] = lines[i].Replace("Hips", rootBoneName + "/Hips");
        }

        // Write the modified content back to the file
        File.WriteAllLines(assetPath, lines);
    }
    private void ExtractAnim(string assetFolderPath, string assetPath)
    {
        // Load all assets at the path
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        // Filter the assets by type
        AnimationClip[] clips = Array.FindAll(allAssets, asset => asset is AnimationClip).Cast<AnimationClip>().ToArray();
        
        foreach (AnimationClip clip in clips)
        {
            AnimationClip newClip = Object.Instantiate(clip);
            if(newClip == null || newClip.name == "" || newClip.name.Contains("__preview__")) continue;
            string fbxName = Path.GetFileNameWithoutExtension(assetPath);
            string clipPath = Path.Combine(assetFolderPath, fbxName + "_" + newClip.name + ".anim");
            AssetDatabase.CreateAsset(newClip, clipPath);
        }
    }
}
