using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Liangddyy.UnityKitModule.Folder
{
    public class MkLinkTool
    {
        [MenuItem("Assets/工具箱/硬链接", false, 22)]
        private static void LinkFloder()
        {
            string sourseFloder = EditorUtility.OpenFolderPanel("选择源文件夹", PathUtil.ApplicationBasePath, "");
            if (string.IsNullOrEmpty(sourseFloder))
            {
                Debug.Log("无效的路径");
                return;
            }

            string toPath = PathUtil.AssetPath2FullPath(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
            string toFolderPath = Path.Combine(toPath, Path.GetFileName(sourseFloder));
            if (Directory.Exists(toFolderPath))
                throw new ArgumentException("操作终止" + toPath + " 路径下已存在同名文件夹:" + Path.GetFileName(sourseFloder));

            // 判断是否Mac平台,在Module DLL 下不能用 PlatformEditor.IsOSXPlatform来判断.
            bool isOsx = sourseFloder.StartsWith("/");
//            Debug.Log(isOsx);
            if (!isOsx)
            {
                sourseFloder = sourseFloder.Replace("/", "\\");
                toFolderPath = toFolderPath.Replace("/", "\\");
                string cmd = string.Format("mklink /j {0} {1} & echo ok", toFolderPath, sourseFloder);

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
            else
            {
                sourseFloder = sourseFloder.Replace("\\", "/");
                toFolderPath = toFolderPath.Replace("\\", "/");

                string cmd = string.Format("ln -s {0} {1}", sourseFloder, toFolderPath);

                ShellHelper.ShellRequest req = ShellHelper.ProcessCommand(cmd, "");
                req.onLog += delegate(int arg1, string arg2) { Debug.Log(arg2); };
                req.onDone += delegate()
                {
                    Debug.Log("success");
                    AssetDatabase.Refresh();
                };
                req.onError += delegate { Debug.LogError("link fail."); };

//                var files = KitUtility.GetFiles(sourseFloder);
//                Debug.Log("sources:" + sourseFloder);
//                foreach (var file in files)
//                {
//                    Debug.Log(file);
//                    if (Path.GetFileName(file) == ".DS_Store")
//                    {
//                        continue;
//                    }
//
//                    var tmp = file.Replace("\\", "/");
//                    var target = tmp.Replace(sourseFloder, toFolderPath);
//                    var targetDir = Path.GetDirectoryName(target);
//
//                    Debug.Log("target:" + target);
//                    Debug.Log("dir:" + targetDir);
//                    if (!Directory.Exists(targetDir))
//                    {
//                        Directory.CreateDirectory(targetDir);
////                        AssetDatabase.Refresh();
//                    }
//
//                    ExecuteMacLink(file, target);
//                    AssetDatabase.Refresh();
//                }
            }
        }

//        private static void ExecuteMacLink(string sourceFile, string targetFile)
//        {
//            // 软链接 -s参数. 10.14 环境下 硬链接无法同步;而Unity里可以认软链接,故这里改为使用软链接.
//            string cmd = string.Format("ln -s {0} {1}", sourceFile, targetFile);
//
//            ShellHelper.ShellRequest req = ShellHelper.ProcessCommand(cmd, "");
//            req.onLog += delegate(int arg1, string arg2) { Debug.Log(arg2); };
////            req.onDone += delegate() { Debug.Log("end:"+ targetFile); };
//            req.onError += delegate { Debug.LogError("link error:" + sourceFile); };
//            Debug.Log("Mac OS Excute：" + cmd);
//        }

        [MenuItem("Assets/工具箱/硬链接", true, 22)]
        private static bool LinkFloderValidate()
        {
            return CommonUtil.IsSelectSingleFloder();
        }
    }
}