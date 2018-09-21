//=======================================================
// 作者：liangddyy
// 描述：
//=======================================================

using UFrame;
using UnityEngine;
using UnityEditor;

namespace Babybus.Project
{
    public class ContextUtility
    {
        /// <summary>
        /// 重置当前物体和所有子物体的Transform
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/Transform/Reset All Child", false, 111)]
        private static void ResetChildsTransform(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            var childs = trans.GetComponentsInChildren<Transform>();
            Undo.RecordObjects(childs, "Reset All Child");

            KitUtility.ForEachChilds(trans, (x) =>
            {
                x.localPosition = Vector3.zero;
                x.transform.localEulerAngles = Vector3.zero;
                x.transform.localScale = Vector3.one;
            });
        }

        [MenuItem("CONTEXT/Transform/Reset All Child Position", false, 112)]
        private static void ResetChildsPosition(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            var childs = trans.GetComponentsInChildren<Transform>();
            Undo.RecordObjects(childs, "Reset All Child Position");
            KitUtility.ForEachChilds(trans, (x) => { x.localPosition = Vector3.zero; });
        }

        [MenuItem("CONTEXT/Transform/Reset All Child Rotate", false, 113)]
        private static void ResetChildsRotate(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            var childs = trans.GetComponentsInChildren<Transform>();
            Undo.RecordObjects(childs, "Reset All Child Rotate");
            KitUtility.ForEachChilds(trans, (x) => { x.localEulerAngles = Vector3.zero; });
        }

        [MenuItem("CONTEXT/Transform/Reset All Child Scale", false, 114)]
        private static void ResetChildsScale(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            var childs = trans.GetComponentsInChildren<Transform>();
            Undo.RecordObjects(childs, "Reset All Child Scale");
            KitUtility.ForEachChilds(trans, (x) => { x.localScale = Vector3.one; });
        }

        /// <summary>
        /// 移除当前对象的所有Component
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/Transform/Remove All Components")]
        private static void RemoveAllComponents(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            Undo.RecordObject(trans, "Remove All Components");
            RemoveAllComponents(trans);
        }

        private static void RemoveAllComponents(Transform trans)
        {
            var coponents = trans.GetComponents<Component>();
            for (var i = 0; i < coponents.Length; i++)
            {
                if (coponents[i] != trans)
                    GameObject.DestroyImmediate(coponents[i]);
            }
        }

        [MenuItem("CONTEXT/Transform/Remove All Childs Components")]
        private static void RemoveAllChildsComponents(MenuCommand command)
        {
            Transform trans = (Transform) command.context;
            Undo.RecordObject(trans, "Remove All Components");
            KitUtility.ForEachChilds(trans, RemoveAllComponents);
        }
    }
}