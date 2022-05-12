using System.Linq;
using UnityEditor;
using UnityEngine;

namespace litefeel.AlignTools
{
    public class AlignToolsWindow : EditorWindow
    {
        const int AXIS_X = 0;
        const int AXIS_Y = 1;
        const int AXIS_Z = 2;

        private Ruler _ruler;
        private string[] _modesStr = new string[] { "NGUI", "UGUI", "World" };

        private bool needPepaintScene = false;

        Transform[] lastSelectedArr;
        public static Transform lastSelectedTrans;

        private void OnSelectionChange()
        {
            if(lastSelectedArr == null)
                lastSelectedArr = new Transform[] { };
            var currSelectedTrans = Utils.GetTransforms();
            var newSelected = currSelectedTrans.Except(lastSelectedArr).ToList();
            lastSelectedArr = currSelectedTrans;
            if (newSelected == null || newSelected.Count() == 0)
                return;
            lastSelectedTrans = newSelected.First();
        }

        // Update the editor window when user changes something (mainly useful when selecting objects)
        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            // head
            EditorGUI.BeginChangeCheck();
            Settings.OperatorModeInt = GUILayout.Toolbar(Settings.OperatorModeInt, _modesStr);
            needPepaintScene = EditorGUI.EndChangeCheck();

            switch (Settings.OperatorMode)
            {
                case OperatorMode.NGUI:
                    ShowNGUIMode();
                    break;
                case OperatorMode.UGUI:
                    ShowUGUIMode();
                    break;
                case OperatorMode.World:
                    ShowWorldMode();
                    break;
            }
            AdjustPosition.Execute();
            if (needPepaintScene)
                SceneView.RepaintAll();
        }

        private void ShowUGUIMode()
        {
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_left", UGUIAlignTools.AlignToMin, AXIS_X, "Align Left");
            DrawButton("align_center_h", UGUIAlignTools.AlignToCenter, AXIS_X, "Align Center by Horizontal");
            DrawButton("align_right", UGUIAlignTools.AlignToMax, AXIS_X, "Align Right");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_top", UGUIAlignTools.AlignToMax, AXIS_Y, "Align Top");
            DrawButton("align_center_v", UGUIAlignTools.AlignToCenter, AXIS_Y, "Align Center by Vertical");
            DrawButton("align_bottom", UGUIAlignTools.AlignToMin, AXIS_Y, "Align Bottom");
            EditorGUILayout.EndHorizontal();

            DrawLine();
            EditorGUILayout.BeginHorizontal();
            DrawButton("distribution_h", UGUIAlignTools.DistributionGap, AXIS_X, "Distribute by Horizontal");
            DrawButton("distribution_v", UGUIAlignTools.DistributionGap, AXIS_Y, "Distribute by Vertical");
            EditorGUILayout.LabelField("Order By", GUILayout.Width(60), GUILayout.ExpandWidth(false));
            Settings.DistributionOrder = (DistributionOrder)EditorGUILayout.EnumPopup(Settings.DistributionOrder);
            EditorGUILayout.EndHorizontal();

            DrawLine();
            EditorGUILayout.BeginHorizontal();
            DrawButton("expand_h", UGUIAlignTools.Expand, AXIS_X, "Expand Size by Horizontal");
            DrawButton("expand_v", UGUIAlignTools.Expand, AXIS_Y, "Expand Size by Vertical");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("shrink_h", UGUIAlignTools.Shrink, AXIS_X, "Shrink Size by Horizontal");
            DrawButton("shrink_v", UGUIAlignTools.Expand, AXIS_Y, "Shrink Size by Vertical");
            EditorGUILayout.EndHorizontal();


            DrawLine();
            Settings.AdjustPositionByKeyboard = EditorGUILayout.ToggleLeft("Adjust Position By Keyboard", Settings.AdjustPositionByKeyboard);
            DrawLine();
            if (null == _ruler) _ruler = new Ruler();
            EditorGUI.BeginChangeCheck();
            Settings.ShowRuler = EditorGUILayout.ToggleLeft("Show Ruler", Settings.ShowRuler);
            needPepaintScene |= EditorGUI.EndChangeCheck();

            if (Settings.ShowRuler)
            {
                EditorGUI.BeginChangeCheck();
                Settings.RulerLineColor = EditorGUILayout.ColorField("Ruler Line Color", Settings.RulerLineColor);
                Settings.RulerLineWidth = EditorGUILayout.IntField("Ruler Line Width", Settings.RulerLineWidth);
                if (GUILayout.Button("Clear All Rulers"))
                {
                    _ruler.ClearAllRulers();
                }
                needPepaintScene |= EditorGUI.EndChangeCheck();
            }
        }
        private void ShowNGUIMode()
        {
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_left", NGUIAlignTools.AlignToMin, AXIS_X, "Align Min by Axis X");
            DrawButton("align_center_h", NGUIAlignTools.AlignToCenter, AXIS_X, "Align Center by Axis X");
            DrawButton("align_right", NGUIAlignTools.AlignToMax, AXIS_X, "Align Max by Axis X");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_top", NGUIAlignTools.AlignToMax, AXIS_Y, "Align Max by Axis Y");
            DrawButton("align_center_v", NGUIAlignTools.AlignToCenter, AXIS_Y, "Align Center by Axis Y");
            DrawButton("align_bottom", NGUIAlignTools.AlignToMin, AXIS_Y, "Align Min by Axis Y");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            Settings.AlignToLastSelectction = EditorGUILayout.ToggleLeft("Align To Last Selectction", Settings.AlignToLastSelectction);
            EditorGUILayout.EndHorizontal();

            DrawLine();
            EditorGUILayout.BeginHorizontal();
            DrawButton("distribution_h", NGUIAlignTools.Distribution, AXIS_X, "Distribute by Axis X");
            DrawButton("distribution_v", NGUIAlignTools.Distribution, AXIS_Y, "Distribute by Axis Y");
            EditorGUILayout.EndHorizontal();

            DrawLine();
            EditorGUILayout.BeginHorizontal();
            DrawButton("expand_h", NGUIAlignTools.Expand, AXIS_X, "Expand Size by Horizontal");
            DrawButton("expand_v", NGUIAlignTools.Expand, AXIS_Y, "Expand Size by Vertical");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("shrink_h", NGUIAlignTools.Shrink, AXIS_X, "Shrink Size by Horizontal");
            DrawButton("shrink_v", NGUIAlignTools.Shrink, AXIS_Y, "Shrink Size by Vertical");
            EditorGUILayout.EndHorizontal();

            DrawLine();

            if (null == _ruler) _ruler = new Ruler();
            EditorGUI.BeginChangeCheck();
            Settings.ShowRuler = EditorGUILayout.ToggleLeft("Show Ruler", Settings.ShowRuler);
            needPepaintScene |= EditorGUI.EndChangeCheck();

            if (Settings.ShowRuler)
            {
                EditorGUI.BeginChangeCheck();
                Settings.RulerLineColor = EditorGUILayout.ColorField("Ruler Line Color", Settings.RulerLineColor);
                Settings.RulerLineWidth = EditorGUILayout.IntField("Ruler Line Width", Settings.RulerLineWidth);
                if(GUILayout.Button("Clear All Rulers"))
                {
                    _ruler.ClearAllRulers();
                }
                needPepaintScene |= EditorGUI.EndChangeCheck();
            }
        }

        private void ShowWorldMode()
        {
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_left", WorldAlignTools.AlignToMin, AXIS_X, "Align Min by Axis X");
            DrawButton("align_center_h", WorldAlignTools.AlignToCenter, AXIS_X, "Align Center by Axis X");
            DrawButton("align_right", WorldAlignTools.AlignToMax, AXIS_X, "Align Max by Axis X");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_top", WorldAlignTools.AlignToMax, AXIS_Y, "Align Min by Axis Y");
            DrawButton("align_center_v", WorldAlignTools.AlignToCenter, AXIS_Y, "Align Center by Axis Y");
            DrawButton("align_bottom", WorldAlignTools.AlignToMin, AXIS_Y, "Align Max by Axis Y");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawButton("align_max_z", WorldAlignTools.AlignToMax, AXIS_Z, "Align Min by Axis Z");
            DrawButton("align_center_z", WorldAlignTools.AlignToCenter, AXIS_Z, "Align Center by Axis Z");
            DrawButton("align_min_z", WorldAlignTools.AlignToMin, AXIS_Z, "Align Max by Axis Z");
            EditorGUILayout.EndHorizontal();

            DrawLine();
            EditorGUILayout.BeginHorizontal();
            DrawButton("distribution_h", WorldAlignTools.Distribution, AXIS_X, "Distribute by Axis X");
            DrawButton("distribution_v", WorldAlignTools.Distribution, AXIS_Y, "Distribute by Axis Y");
            DrawButton("distribution_z", WorldAlignTools.Distribution, AXIS_Z, "Distribute by Axis Z");
            EditorGUILayout.LabelField("Order By", GUILayout.Width(60), GUILayout.ExpandWidth(false));
            Settings.DistributionOrder = (DistributionOrder)EditorGUILayout.EnumPopup(Settings.DistributionOrder);
            EditorGUILayout.EndHorizontal();

            //DrawLine();
            //EditorGUILayout.BeginHorizontal();
            //DrawButton("expand_h", AlignTools.ExpandWidth, "Expand Size by Horizontal");
            //DrawButton("expand_v", AlignTools.ExpandHeight, "Expand Size by Vertical");
            //EditorGUILayout.EndHorizontal();
            //EditorGUILayout.BeginHorizontal();
            //DrawButton("shrink_h", AlignTools.ShrinkWidth, "Shrink Size by Horizontal");
            //DrawButton("shrink_v", AlignTools.ShrinkHeight, "Shrink Size by Vertical");
            //EditorGUILayout.EndHorizontal();

            DrawLine();
            Settings.AdjustPositionByKeyboard = EditorGUILayout.ToggleLeft("Adjust Position By Keyboard", Settings.AdjustPositionByKeyboard);
        }

        private void DrawLine()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        }

        private GUIContent btnContent;
        private void DrawButton(string iconName, System.Action action, string tooltip = null)
        {
            if (null == btnContent) btnContent = new GUIContent();
            btnContent.image = Utils.LoadTexture(iconName);
            btnContent.tooltip = tooltip;
            if (GUILayout.Button(btnContent, GUILayout.ExpandWidth(false)))
                action();
        }
        private void DrawButton(string iconName, System.Action<int> action, int axis, string tooltip = null)
        {
            if (null == btnContent) btnContent = new GUIContent();
            btnContent.image = Utils.LoadTexture(iconName);
            btnContent.tooltip = tooltip;
            if (GUILayout.Button(btnContent, GUILayout.ExpandWidth(false)))
                action(axis);
        }


        private void OnEnable()
        {
            Utils.editorPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
        }

        private void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            AdjustPosition.Execute();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            AdjustPosition.Execute();
            if (_ruler != null && (Settings.OperatorMode == OperatorMode.NGUI || Settings.OperatorMode == OperatorMode.UGUI))
                _ruler.OnSceneGUI(sceneView);
        }

    }
}


