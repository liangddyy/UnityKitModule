using System.Linq;
using UnityEditor;

namespace Liangddyy.UnityKitModule.Common
{
    public class SelectionUtil
    {
        public static bool IsSingleFloder()
        {
            if (Selection.assetGUIDs.Length != 1)
                return false;
            if (AssetDatabase.IsValidFolder(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])))
                return true;
            return false;
        }
    }
}