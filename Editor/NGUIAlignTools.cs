using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace litefeel.AlignTools
{

    public class NGUIAlignTools
    {
        delegate float CalcValueOne(int axis, List<UIWidget> widgets);
        delegate float CalcValueTwo(int axis, Vector3[] corners, bool isFirst, ref float minV, ref float maxV);
        delegate float CalcSize(int axis, Vector3[] corners, out float minV, out float maxV);
        delegate Vector3 ApplyValue(int axis, UIWidget widget, float v);

        public static void AlignToMin(int axis)
        {
            AlignUI(axis, CalcValueMin, ApplyValueMin);
        }
        public static void AlignToMax(int axis)
        {
            AlignUI(axis, CalcValueMax, ApplyValueMax);
        }

        public static void AlignToCenter(int axis)
        {
            AlignCenterUI(axis, CalcValue, ApplyValueCenter);
        }

        public static void Distribution(int axis)
        {
            DistributionUI(axis, CalcValue, ApplyValueCenter);
        }

        public static void Expand(int axis)
        {
            ExpandUI(axis, CalcUISize, ApplyValueSize);
        }
        public static void Shrink(int axis)
        {
            ShrinkUI(axis, CalcUISize, ApplyValueSize);
        }

        #region logic
        private static void AlignUI(int axis, CalcValueOne calcValue, ApplyValue applyValue)
        {
            var list = Utils.GetNGUIWidgets();
            if (list.Count < 2) return;

            float v = calcValue(axis, list);

            foreach (var widget in list)
            {
                var pos = applyValue(axis, widget, v);
                Undo.RecordObject(widget.transform, "Align UI");
                widget.transform.position = pos;
            }
        }

        private static void AlignCenterUI(int axis, CalcValueTwo calcValue, ApplyValue applyValue)
        {
            var list = Utils.GetNGUIWidgets();
            if (list.Count < 2) return;

            float minV = 0f, maxV = 0f;

            if (Settings.AlignToLastSelectction)
            {
                var lastSelectedWidget = Utils.GetLastSelectedTrans(list);
                var pos = lastSelectedWidget.worldCenter;
                minV = pos[axis];
                maxV = minV;
            }
            else
            {
                for (var i = 0; i < list.Count; i++)
                {
                    calcValue(axis, list[i].worldCorners, 0 == i, ref minV, ref maxV);
                }
            }

            float v = (minV + maxV) * 0.5f;
            foreach (var widget in list)
            {
                var pos = applyValue(axis, widget, v);
                Undo.RecordObject(widget.transform, "Align Center UI");
                widget.transform.position = pos;
            }
        }

        struct Value
        {
            public UIWidget widget;
            public float v;
        }

        private static void DistributionUI(int axis, CalcValueTwo calcValue, ApplyValue applyValue)
        {
            var list = Utils.GetNGUIWidgets();
            if (list.Count < 3) return;

            var vlist = new List<Value>(list.Count);

            float minV = 0f, maxV = 0f;
            for (var i = 0; i < list.Count; i++)
            {
                vlist.Add(new Value
                {
                    widget = list[i],
                    v = calcValue(axis, list[i].worldCorners, 0 == i, ref minV, ref maxV)
                });
            };

            vlist.Sort(SortByPosition);


            float gap = (maxV - minV) / (list.Count - 1);
            for (var i = 1; i < vlist.Count - 1; i++)
            {
                var widget = vlist[i].widget;
                var pos = applyValue(axis, widget, minV + gap * i);
                Undo.RecordObject(widget.transform, "Distribution UI");
                widget.transform.position = pos;
            }
        }
        
        private static void ExpandUI(int axis, CalcSize calcSize, ApplyValue applyValue)
        {
            var list = Utils.GetNGUIWidgets();
            if (list.Count < 2) return;

            float size = 0f;
            for (var i = 0; i < list.Count; i++)
            {
                float _minV, _maxV;
                size = Mathf.Max(size, calcSize(axis, list[i].localCorners, out _minV, out _maxV));
            }
            foreach (var widget in list)
            {
                Undo.RecordObject(widget, "Expand or Shark UI");
                applyValue(axis, widget, size);
            }
        }
        private static void ShrinkUI(int axis, CalcSize calcSize, ApplyValue applyValue)
        {
            var list = Utils.GetNGUIWidgets();
            if (list.Count < 2) return;

            float size = float.MaxValue;
            for (var i = 0; i < list.Count; i++)
            {
                float _minV, _maxV;
                size = Mathf.Min(size, calcSize(axis, list[i].localCorners, out _minV, out _maxV));
            }
            foreach (var widget in list)
            {
                Undo.RecordObject(widget, "Expand or Shark UI");
                applyValue(axis, widget, size);
            }
        }
        #endregion


        #region calc value left right top bottom
        private static float CalcValueMin(int axis, List<UIWidget> widgets)
        {
            if (Settings.AlignToLastSelectction)
            {
                var lastSelectedWidget = Utils.GetLastSelectedTrans(widgets);
                return lastSelectedWidget.worldCorners[0][axis];
            }

            float v = 0f;

            for (int i = 0; i < widgets.Count; i++)
            {
                var corner = widgets[i].worldCorners;
                if (i == 0)
                    v = corner[0][axis];
                else
                    v = Mathf.Min(v, corner[0][axis]);
            }
            return v;
        }

        private static float CalcValueMax(int axis, List<UIWidget> widgets)
        {
            if (Settings.AlignToLastSelectction)
            {
                var lastSelectedWidget = Utils.GetLastSelectedTrans(widgets);
                return lastSelectedWidget.worldCorners[2][axis];
            }

            float v = 0f;

            for (int i = 0; i < widgets.Count; i++)
            {
                var corner = widgets[i].worldCorners;
                if (i == 0)
                    v = corner[2][axis];
                else
                    v = Mathf.Max(v, corner[2][axis]);
            }
            return v;
        }

        #endregion



        #region calc value min and max
        // calc min and max via left
        private static float CalcValue(int axis, Vector3[] corners, bool isFirst, ref float minV, ref float maxV)
        {
            var v = (corners[0][axis] + corners[2][axis]) * 0.5f;
            if (isFirst)
            {
                minV = v;
                maxV = v;
            }
            else
            {
                minV = Mathf.Min(minV, v);
                maxV = Mathf.Max(maxV, v);
            }
            return v;
        }
        #endregion

        #region calc size min and max
        private static float CalcUISize(int axis, Vector3[] corners, out float minV, out float maxV)
        {
            minV = Mathf.Min(corners[0][axis], corners[2][axis]);
            maxV = Mathf.Max(corners[0][axis], corners[2][axis]);
            return maxV - minV;
        }
        #endregion

        #region applay value

        private static Vector3 ApplyValueCenter(int axis, UIWidget widget, float v)
        {
            var pos = widget.transform.position;
            var offset = widget.worldCenter[axis] - pos[axis];
            pos[axis] = v - offset;
            return pos;
        }

        private static Vector3 ApplyValueMin(int axis, UIWidget widget, float v)
        {
            var pos = widget.transform.position;
            var offset = pos[axis] - widget.worldCorners[0][axis];
            pos[axis] = v + offset;
            return pos;
        }

        private static Vector3 ApplyValueMax(int axis, UIWidget widget, float v)
        {
            var pos = widget.transform.position;
            var offset = pos[axis] - widget.worldCorners[2][axis];
            pos[axis] = v + offset;
            return pos;
        }

        private static Vector3 ApplyValueSize(int axis, UIWidget widget, float v)
        {
            if (axis ==0)
            {
                widget.width = (int)v;
            }
            else
            {
                widget.height = (int)v;
            }
            return Vector3.zero;
        }
        #endregion


        private static int SortByPosition(Value a, Value b)
        {
            if (Mathf.Approximately(a.v, b.v)) return 0;
            if (a.v < b.v) return -1;
            else if (a.v > b.v) return 1;
            return 0;
        }
    }
}


