using System;
using System.IO;
using Liangddyy.UnityKitModule.Common;
using UnityEditor;
using UnityEngine;

namespace Liangddyy.UnityKitModule.Folder
{
    public class MkLink
    {
        [MenuItem("Assets/工具箱/硬链接", false, 22)]
        private static void LinkFloder()
        {
            string sourseFloder = EditorUtility.OpenFolderPanel("选择源文件夹", PathUtil.ApplicationBasePath, "");
            if (string.IsNullOrEmpty(sourseFloder))
                throw new ArgumentException("无效的路径");

            string toPath = PathUtil.AssetPath2FullPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
            string toFolderPath = Path.Combine(toPath, Path.GetFileName(sourseFloder));
            if (Directory.Exists(toFolderPath))
                throw new ArgumentException("操作终止" + toPath + " 路径下已存在同名文件夹");
            sourseFloder = sourseFloder.Replace("/", "\\");
            toFolderPath = toFolderPath.Replace("/", "\\");
            string cmd = string.Format("mklink /j {0} {1} & echo ok", toFolderPath, sourseFloder);
            Debug.Log(cmd);
            string result = ShellUtil.RunCmdCommand(cmd);
            if (result.Contains("<<===>>"))
            {
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("操作可能出错，请自行检查");
            }
        }

        [MenuItem("Assets/工具箱/硬链接", true, 22)]
        private static bool LinkFloderValidate()
        {
            return SelectionUtil.IsSingleFloder();
        }
    }
}