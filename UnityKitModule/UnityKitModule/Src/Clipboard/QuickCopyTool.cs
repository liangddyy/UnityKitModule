using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Liangddyy.UnityKitModule
{
    public class QuickCopyTool
    {
        #region 菜单

        [MenuItem("Assets/粘贴", false, 20)]
        private static void Paste()
        {
            ClipItem item;
            try
            {
                byte[] bytes = Convert.FromBase64String(GUIUtility.systemCopyBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    item = formatter.Deserialize(stream) as ClipItem;
                }
            }
            catch (FormatException e)
            {
                throw new FormatException("没有从剪切板解析到有效的数据!" + e.Message);
            }

            if (item == null) throw new ArgumentException("没有从剪切板解析到有效的数据!");
            string assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            switch (item.Type)
            {
                case ContentType.File:
                    CopyListFileInEditor(item.Values, PathUtil.AssetPath2FullPath(assetPath));
                    break;
                case ContentType.Package:
                    if (item.Values.Count > 0 && Path.GetExtension(item.Values[0]).Equals(".unitypackage"))
                        Package2Folder.ImportPackageToFolder(item.Values[0], assetPath, true);
                    break;
                default:
                    break;
            }
        }

        [MenuItem("Assets/粘贴", true, 20)]
        private static bool PasteValidate()
        {
            if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
                return false;
            return CommonUtil.IsSelectSingleFloder();
        }

        [MenuItem("Assets/复制 - 到编辑器", false, 21)]
        private static void CopyToEditor()
        {
            ClipItem item = new ClipItem(ContentType.File);
            foreach (var guiD in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guiD);
                item.Values.Add(PathUtil.AssetPath2FullPath(path));
            }

            CopyClipboardItem(item);
            Debug.Log("已复制" + Selection.assetGUIDs.Length + "条数据，可在其他 Unity 编辑器里粘贴！");
        }

        [MenuItem("Assets/复制 - 到编辑器", true, 21)]
        private static bool CopyToEditorValidate()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        /// <summary>
        /// 仅限Windows平台下使用
        /// </summary>
        [MenuItem("Assets/工具箱/复制 - 到剪贴板", false, 22)]
        private static void CopyToClipboard()
        {
            List<string> paths = new List<string>();
            for (int i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
                path = PathUtil.AssetPath2FullPath(path);
                paths.Add(path);
            }

            CopyToClipboard(paths);
        }

        [MenuItem("Assets/工具箱/复制 - 到剪贴板", true)]
        private static bool CanCopyToClipboard()
        {
            return !CommonUtil.IsOSXEditor;
        }

        public static void CopyToClipboard(List<string> fullPaths)
        {
            StringBuilder stringBuilder = new StringBuilder("Set-Clipboard -Path ");
            for (int i = 0; i < fullPaths.Count; i++)
            {
                if (i != 0) stringBuilder.Append(",");
                stringBuilder.Append("\"");
                stringBuilder.Append(fullPaths[i]);
                stringBuilder.Append("\"");
            }

            if (ShellUtil.RunPowershellCommand(stringBuilder.Replace("/", "\\").ToString()))
                Debug.Log("已复制文件列表到 剪切板！！！");
            else
                Debug.LogError("复制出错!");
        }

        [MenuItem("Assets/工具箱/复制 - 到剪贴板", true, 22)]
        private static bool CopyToClipboardValidate()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        [MenuItem("Assets/工具箱/复制 - 到编辑器(导出包)", false, 22)]
        private static void CopyAsPackage()
        {
            string[] assetPaths = new string[Selection.assetGUIDs.Length];
            for (int i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                assetPaths[i] = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
            }

            string outPath = Path.Combine(Application.temporaryCachePath,
                Random.Range(0, 1024) + ".unitypackage");
            AssetDatabase.ExportPackage(assetPaths, outPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            ClipItem item = new ClipItem(ContentType.Package);
            item.Values.Add(outPath);
            CopyClipboardItem(item);
            Debug.Log("已导出选中资源!可直接粘贴至任意Assets目录!");
        }

        [MenuItem("Assets/工具箱/复制 - 到编辑器(导出包)", true, 22)]
        private static bool CopyAsPackageValidate()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        #endregion

        private static readonly string KeyAutoRefresh = "kAutoRefresh"; // 请勿修改


        public static void CopyClipboardItem(ClipItem item)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                TextEditor te = new TextEditor {text = Convert.ToBase64String(stream.ToArray())};
                te.OnFocus();
                te.Copy();
            }
        }

        /// <summary>
        ///     编辑器里复制文件列表
        /// </summary>
        /// <param name="sourcePaths"></param>
        /// <param name="targetPath"></param>
        public static void CopyListFileInEditor(List<string> sourcePaths, string targetPath)
        {
            bool isAuto = EditorPrefs.GetBool(KeyAutoRefresh, true);
            if (isAuto) EditorPrefs.SetBool(KeyAutoRefresh, false);
            bool isOverrite = false;
            bool isAsked = false;
            try
            {
                foreach (var source in sourcePaths)
                {
                    string destName = Path.Combine(targetPath, Path.GetFileName(source));
                    if (File.Exists(source))
                    {
                        if (File.Exists(destName))
                        {
                            if (!isAsked)
                            {
                                isOverrite = EditorUtility.DisplayDialog("提示", strOverriteAsk, "覆盖", "跳过");
                                isAsked = true;
                            }

                            if (isOverrite)
                                File.Delete(destName);
                            else
                                continue;
                        }

                        File.Copy(source, destName);
                    }
                    else
                    {
                        CopyDir(source, destName, ref isOverrite, ref isAsked);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                if (isAuto) EditorPrefs.SetBool(KeyAutoRefresh, true);
            }

            AssetDatabase.Refresh();
        }

        private static readonly string strOverriteAsk = "此次粘贴资源中有重名资源。如何操作？";

        public static void CopyDir(string sourcePath, string destinationPath, ref bool isOverrite, ref bool isAsked)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);

                if (fsi is FileInfo)
                {
                    if (File.Exists(destName))
                    {
                        if (!isAsked)
                        {
                            isOverrite = EditorUtility.DisplayDialog("提示", strOverriteAsk, "覆盖", "跳过");
                            isAsked = true;
                        }

                        if (isOverrite)
                            File.Delete(destName);
                        else
                            continue;
                    }

                    File.Copy(fsi.FullName, destName);
                }
                else
                {
                    Directory.CreateDirectory(destName);
                    CopyDir(fsi.FullName, destName, ref isOverrite, ref isAsked);
                }
            }
        }


        [Serializable]
        public class ClipItem
        {
            private ContentType type;
            private List<String> values;

            public ContentType Type
            {
                get { return type; }

                set { type = value; }
            }

            public List<string> Values
            {
                get { return values; }

                set { values = value; }
            }

            public ClipItem()
            {
                values = new List<string>();
            }

            public ClipItem(ContentType type)
            {
                this.type = type;
                values = new List<string>();
            }
        }

        [Serializable]
        public enum ContentType
        {
            File,
            Package
        }

        public class Package2Folder
        {
            #region reflection stuff

            private delegate AssetsItem[] ImportPackageStep1Delegate(string packagePath, out string packageIconPath);

            private static Type assetServerType;

            private static Type AssetServerType
            {
                get
                {
                    if (assetServerType == null)
                    {
                        assetServerType = typeof(MenuItem).Assembly.GetType("UnityEditor.AssetServer");
                    }

                    return assetServerType;
                }
            }

            private static ImportPackageStep1Delegate importPackageStep1;

            private static ImportPackageStep1Delegate ImportPackageStep1
            {
                get
                {
                    if (importPackageStep1 == null)
                    {
                        importPackageStep1 = (ImportPackageStep1Delegate) Delegate.CreateDelegate(
                            typeof(ImportPackageStep1Delegate),
                            null,
                            AssetServerType.GetMethod("ImportPackageStep1"));
                    }

                    return importPackageStep1;
                }
            }

            private static MethodInfo importPackageStep2MethodInfo;

            private static MethodInfo ImportPackageStep2MethodInfo
            {
                get
                {
                    if (importPackageStep2MethodInfo == null)
                    {
                        importPackageStep2MethodInfo = AssetServerType.GetMethod("ImportPackageStep2");
                    }

                    return importPackageStep2MethodInfo;
                }
            }

            private delegate object[] ExtractAndPrepareAssetListDelegate(string packagePath, out string packageIconPath,
                out bool allowReInstall);

            private static Type packageUtilityType;

            private static Type PackageUtilityType
            {
                get
                {
                    if (packageUtilityType == null)
                    {
                        packageUtilityType
                            = typeof(MenuItem).Assembly.GetType("UnityEditor.PackageUtility");
                    }

                    return packageUtilityType;
                }
            }

            private static ExtractAndPrepareAssetListDelegate extractAndPrepareAssetList;

            private static ExtractAndPrepareAssetListDelegate ExtractAndPrepareAssetList
            {
                get
                {
                    if (extractAndPrepareAssetList == null)
                    {
                        extractAndPrepareAssetList
                            = (ExtractAndPrepareAssetListDelegate) Delegate.CreateDelegate(
                                typeof(ExtractAndPrepareAssetListDelegate),
                                null,
                                PackageUtilityType.GetMethod("ExtractAndPrepareAssetList"));
                    }

                    return extractAndPrepareAssetList;
                }
            }

            private static FieldInfo destinationAssetPathFieldInfo;

            private static FieldInfo DestinationAssetPathFieldInfo
            {
                get
                {
                    if (destinationAssetPathFieldInfo == null)
                    {
                        Type importPackageItem
                            = typeof(MenuItem).Assembly.GetType("UnityEditor.ImportPackageItem");
                        destinationAssetPathFieldInfo
                            = importPackageItem.GetField("destinationAssetPath");
                    }

                    return destinationAssetPathFieldInfo;
                }
            }

            private static MethodInfo importPackageAssetsMethodInfo;

            private static MethodInfo ImportPackageAssetsMethodInfo
            {
                get
                {
                    if (importPackageAssetsMethodInfo == null)
                    {
                        // ImportPackageAssetsImmediately 是同步的导入5.4以上版本可用
                        importPackageAssetsMethodInfo
                            = PackageUtilityType.GetMethod("ImportPackageAssetsImmediately") ??
                              PackageUtilityType.GetMethod("ImportPackageAssets");
                    }

                    return importPackageAssetsMethodInfo;
                }
            }

            private static MethodInfo showImportPackageMethodInfo;

            private static MethodInfo ShowImportPackageMethodInfo
            {
                get
                {
                    if (showImportPackageMethodInfo == null)
                    {
                        Type packageImport = typeof(MenuItem).Assembly.GetType("UnityEditor.PackageImport");
                        showImportPackageMethodInfo = packageImport.GetMethod("ShowImportPackage");
                    }

                    return showImportPackageMethodInfo;
                }
            }

            #endregion reflection stuff

            public static void ImportPackageToFolder(string packagePath, string selectedFolderPath, bool interactive)
            {
                string packageIconPath;
                bool allowReInstall;
                // 临时的解决办法. 有时间再改。
                if (AssetServerType.GetMethod("ImportPackageStep1") != null)
                    IsOlder53VersionAPI = true;
                else
                    IsOlder53VersionAPI = false;

                object[] assetsItems = ExtractAssetsFromPackage(packagePath, out packageIconPath, out allowReInstall);
                if (assetsItems == null) return;
                foreach (object item in assetsItems)
                {
                    ChangeAssetItemPath(item, selectedFolderPath);
                }

                if (interactive)
                {
                    ShowImportPackageWindow(packagePath, assetsItems, packageIconPath, allowReInstall);
                }
                else
                {
                    ImportPackageSilently(assetsItems);
                }
            }

            private static bool IsOlder53VersionAPI = false;

            public static object[] ExtractAssetsFromPackage(string path, out string packageIconPath,
                out bool allowReInstall)
            {
                if (IsOlder53VersionAPI)
                {
                    AssetsItem[] array = ImportPackageStep1(path, out packageIconPath);
                    allowReInstall = false;
                    return array;
                }
                else
                {
                    object[] array = ExtractAndPrepareAssetList(path, out packageIconPath, out allowReInstall);
                    return array;
                }
            }

            private static void ChangeAssetItemPath(object assetItem, string selectedFolderPath)
            {
                if (IsOlder53VersionAPI)
                {
                    AssetsItem item = (AssetsItem) assetItem;
                    item.exportedAssetPath = selectedFolderPath + item.exportedAssetPath.Remove(0, 6);
                    item.pathName = selectedFolderPath + item.pathName.Remove(0, 6);
                }
                else
                {
                    string destinationPath
                        = (string) DestinationAssetPathFieldInfo.GetValue(assetItem);
                    destinationPath
                        = selectedFolderPath + destinationPath.Remove(0, 6);
                    DestinationAssetPathFieldInfo.SetValue(assetItem, destinationPath);
                }
            }

            public static void ShowImportPackageWindow(string path, object[] array, string packageIconPath,
                bool allowReInstall)
            {
                if (IsOlder53VersionAPI)
                {
                    ShowImportPackageMethodInfo.Invoke(null, new object[] {path, array, packageIconPath});
                }
                else
                {
                    ShowImportPackageMethodInfo.Invoke(null,
                        new object[] {path, array, packageIconPath, allowReInstall});
                }
            }

            public static void ImportPackageSilently(object[] assetsItems)
            {
                if (IsOlder53VersionAPI)
                {
                    ImportPackageStep2MethodInfo.Invoke(null, new object[] {assetsItems, false});
                }
                else
                {
                    ImportPackageAssetsMethodInfo.Invoke(null, new object[] {assetsItems, false});
                }
            }

            private static string GetSelectedFolderPath()
            {
                UnityEngine.Object obj = Selection.activeObject;
                if (obj == null) return null;
                string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                return !Directory.Exists(path) ? null : path;
            }
        }
    }
}