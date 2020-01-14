using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Liangddyy.UnityKitModule
{
    /// <summary>
    /// 自动安装成Unity的模块
    /// </summary>
    public class ModuleInstall
    {
        private static readonly string moduleOrDllName = "UnityKitModule";
        private static readonly string dllFileName = moduleOrDllName + ".dll";
        private static readonly string ivyFileName = "ivy.xml";

        private static readonly string ivyString =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ivy-module version = \"2.0\" ><info version=\"{UnityVersion}\" organisation=\"Unity\" module=\"{ModuleName}\" e:packageType=\"UnityExtension\" e:unityVersion=\"{UnityVersion}\" xmlns:e=\"http://ant.apache.org/ivy/extra\" /><publications xmlns:e=\"http://ant.apache.org/ivy/extra\"><artifact name = \"Editor/{ModuleName}\" type=\"dll\" ext=\"dll\" e:guid=\"80a3616ca19596e4da0f10f14d241e9f\" /></publications></ivy-module>";

        private static bool WriteModule2File(string dllFileFullPath, string modulePath, string moduleName)
        {
            bool isSuccess = true;
            try
            {
                if (!Directory.Exists(modulePath))
                    Directory.CreateDirectory(modulePath);
                string ivyFile = Path.Combine(modulePath, ivyFileName);
                string content = Regex.Replace(ivyString, "{UnityVersion}", Application.unityVersion);
                content = Regex.Replace(content, "{ModuleName}", moduleName);
                File.WriteAllText(ivyFile, content);
                string moduleEditorPath = Path.Combine(modulePath, "Editor");
                if (!Directory.Exists(moduleEditorPath))
                    Directory.CreateDirectory(moduleEditorPath);
                File.Copy(dllFileFullPath, Path.Combine(moduleEditorPath, dllFileName));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                isSuccess = false;
            }

            return isSuccess;
        }

        [MenuItem("Tools/安装UnityKit模块", false, 111)]
        private static void InstallModule()
        {
            string moduleDir = Path.Combine(PathUtil.UnityExtensionDir, moduleOrDllName);
            if (Directory.Exists(moduleDir) && File.Exists(Path.Combine(moduleDir, ivyFileName)))
            {
                EditorUtility.OpenWithDefaultApp(moduleDir);
                throw new ArgumentException("模块已安装，不必重复操作！若需要删除，请直接删除文件夹：" + moduleDir);
            }

            var files = AssetDatabase.FindAssets(moduleOrDllName);
            if (!files.Any())
                throw new ArgumentException("操作中断，在当前项目中没找到需要的 Dll文件");

            string filePath = ""; // = AssetDatabase.GUIDToAssetPath(files[0]);
            foreach (var file in files)
            {
                filePath = AssetDatabase.GUIDToAssetPath(file);
                if (filePath.EndsWith(".dll"))
                    break;
            }

            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("操作中断，在当前项目中没找到需要的 Dll文件");
            string dllFilePath = PathUtil.AssetPath2FullPath(filePath);

            bool isSuccess = WriteModule2File(dllFilePath, moduleDir, moduleOrDllName);
            if (isSuccess)
            {
                Debug.Log("安装成功,下次启动Unity时生效！");
                EditorUtility.OpenWithDefaultApp(moduleDir);
                return;
            }

            // 失败.
            if (CommonUtil.IsOSXEditor)
            {
                string modulePath =
                    Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp/" + moduleOrDllName);
                if (Directory.Exists(modulePath))
                    Directory.Delete(modulePath, true);
                isSuccess = WriteModule2File(dllFilePath, modulePath, moduleOrDllName);
                Debug.LogError("似乎没有权限写入路径:" + moduleDir);
                if (isSuccess)
                {
                    EditorUtility.OpenWithDefaultApp(modulePath);
                    EditorUtility.OpenWithDefaultApp(PathUtil.UnityExtensionDir);
                    Debug.Log("请手动拷贝文件夹:" + modulePath + " 到路径:" + PathUtil.UnityExtensionDir);
                    Debug.Log("以上两个路径已打开,拷贝到正确目录后,重启Unity生效.");
                }
            }
            else
            {
                isSuccess = EditorUtility.DisplayDialog("安装失败",
                    HowFixWindow, "手动操作", "cancel");
                if (isSuccess)
                {
                    string modulePath = Path.Combine(Application.temporaryCachePath, moduleOrDllName);
                    if (Directory.Exists(modulePath))
                        Directory.Delete(modulePath, true);
                    isSuccess = WriteModule2File(dllFilePath, modulePath, moduleOrDllName);
                    if (isSuccess)
                    {
                        QuickCopyTool.CopyToClipboard(new List<string>() {modulePath});
                        EditorUtility.OpenWithDefaultApp(PathUtil.UnityExtensionDir);
                        Debug.Log("已打开拓展文件夹" + PathUtil.UnityExtensionDir + " 直接右键粘贴即可安装模块。请注意，粘贴模块文件后，下次启动 Unity 生效。");
                    }
                    else
                    {
                        Debug.Log("error");
                    }
                }
            }
        }

        private static readonly string HowFixWindow =
            "任选如下其一方案解决 ：\n1. 使用管理员权限启动Unity后重新安装。\n2. 手动安装，点击手动操作后，在打开的文件夹中直接右键粘贴即可。";
    }
}