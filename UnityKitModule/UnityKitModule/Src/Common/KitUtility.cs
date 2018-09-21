//=======================================================
// 作者：liangddyy
// 描述：
//=======================================================

using System;
using UnityEngine;

namespace Babybus.Project
{
    public class KitUtility
    {
        /// <summary>
        /// 遍历一个物体
        /// </summary>
        public static void ForEachChilds(Transform parent, Action<Transform> callback, bool isContainsParent = false)
        {
            if (isContainsParent)
                callback(parent);
            if (parent.childCount <= 0) return;
            for (int i = 0; i < parent.childCount; i++)
            {
                var item = parent.GetChild(i);
                ForEachChilds(item, callback, true);
            }
        }
    }
}