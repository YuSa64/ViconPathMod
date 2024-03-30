using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;

public class AnimationPathModifier : EditorWindow
{
    private string name;
    private string rootbone = "Armature";
    private DefaultAsset folderPath;

    [UnityEditor.MenuItem("YuSa64/Vicon Path Modifier")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPathModifier>("Vicon Path Modifier");
    }

    private void OnGUI()
    {
        name = EditorGUILayout.TextField("이니셜", name);
        rootbone = EditorGUILayout.TextField("루트 본", rootbone);
        folderPath = (DefaultAsset)EditorGUILayout.ObjectField("폴더", folderPath, typeof(DefaultAsset), false);

        if (GUILayout.Button("패스 수정"))
        {
            ModifyPaths();
        }

        GUILayout.Label("패스 수정을 하기 전에 백업을 해주세요. \n By YuSa64", EditorStyles.helpBox);
    }

    private void ModifyPaths()
    {
        if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog("Error", "이니셜을 입력해주세요.", "OK");
            return;
        }
        if(string.IsNullOrEmpty(rootbone))
        {
            EditorUtility.DisplayDialog("Error", "루트 본을 입력해주세요.", "OK");
            return;
        }
    if (folderPath != null)
    {
        // Get the asset path of the folder
        string assetFolderPath = AssetDatabase.GetAssetPath(folderPath);

        // Get all .anim files in the folder
        string[] animFiles = Directory.GetFiles(assetFolderPath, "*.anim", SearchOption.AllDirectories);

        foreach (string filePath in animFiles)
        {
            // Convert the file path to an asset path
            string assetPath = filePath.Replace(UnityEngine.Application.dataPath, "Assets");

            // Read the file line by line
            string[] lines = File.ReadAllLines(assetPath);

            for (int i = 0; i < lines.Length; i++)
            {
                // Replace 'name+":"' in every path
                lines[i] = lines[i].Replace(name + ":", "");

                // Change every "Hips" in path to "rootbone/Hips"
                lines[i] = lines[i].Replace("Hips", rootbone + "/Hips");
            }

            // Write the modified content back to the file
            File.WriteAllLines(assetPath, lines);
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
}