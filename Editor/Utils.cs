using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace litefeel.AlignTools
{
    internal static class Utils
    {
        internal static string editorPath = "Assets/Unity-AlignTools/Editor";

        internal static Texture LoadTexture(string textureName)
        {
            var skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
            string path = string.Format("{0}/Icons/{1}/{2}.png", editorPath, skinName, textureName);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        internal static Transform[] GetTransforms()
        {
            return Selection.gameObjects.Select(x=>x.transform).ToArray();
        }

        internal static List<RectTransform> GetRectTransforms()
        {
            var arr = GetTransforms();
            var list = new List<RectTransform>();
            foreach (var trans in arr)
            {
                var rt = trans as RectTransform;
                if (!rt) continue;
                list.Add(rt);
            }
            return list;
        }

        internal static List<Transform> GetWorldTransforms()
        {
            var arr = GetTransforms();
            var list = new List<Transform>();
            foreach (var trans in arr)
            {
                var rt = trans as RectTransform;
                if (rt) continue;
                list.Add(trans);
            }
            return list;
        }

        public static List<UIWidget> GetNGUIWidgets()
        {
            var arr = Selection.gameObjects;
            var list = new List<UIWidget>();
            foreach (var trans in arr)
            {
                var widget = trans.GetComponent<UIWidget>();
                if (widget == null)
                    continue;
                list.Add(widget);
            }
            return list;
        }

        // 获取最后所选的Widget
        public static UIWidget GetLastSelectedTrans(IEnumerable<UIWidget> list)
        {
            var lastSelected = AlignToolsWindow.lastSelectedTrans;
            if (lastSelected == null)
                return list.First();
            foreach (var widget in list)
            {
                if (widget.transform == lastSelected)
                {
                    return widget;
                }
            }
            return list.First();
        }

        internal static void WorldCorners(this RectTransform rt, Vector3[] corners)
        {
            rt.GetWorldCorners(corners);
            //var p1 = corners[0];
            if (corners[0].x > corners[3].x)
                Swap(ref corners[0], ref corners[3]);
            if (corners[1].x > corners[2].x)
                Swap(ref corners[1], ref corners[2]);
            if (corners[0].y > corners[1].y)
                Swap(ref corners[0], ref corners[1]);
            if (corners[3].y > corners[2].y)
                Swap(ref corners[3], ref corners[2]);
        }

        private static void Swap(ref Vector3 v1, ref Vector3 v2)
        {
            var tx = v1;
            v1 = v2;
            v2 = tx;
        }
    }
}


