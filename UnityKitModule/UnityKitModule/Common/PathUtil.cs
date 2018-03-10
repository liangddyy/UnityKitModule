using System.IO;
using UnityEditor;
using UnityEngine;

namespace Liangddyy.UnityKitModule.Common
{
    public class PathUtil
    {
        public static string ApplicationBasePath
        {
            get { return new DirectoryInfo(Application.dataPath).Parent.FullName; }
        }

        public static string AssetPath2FullPath(string assetPath)
        {
            return Path.Combine(ApplicationBasePath, assetPath);
        }

        public static string UnityExtensionDir
        {
            get
            {
                return Path.Combine(EditorApplication.applicationContentsPath,
                    "UnityExtensions" + Path.DirectorySeparatorChar + "Unity" + Path.DirectorySeparatorChar);
            }
        }
    }
}