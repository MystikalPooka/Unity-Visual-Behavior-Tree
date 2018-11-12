using Assets.Editor.BehaviorTreeViewEditor;
using Assets.Scripts.AI;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    [CustomEditor(typeof(BehaviorManager))]
    public class BehaviorManagerEditor : BaseEditor<BehaviorManager>
    {
        BehaviorManager BTreeManager;
        BehaviorTreeManagerAsset _BTreeAsset;

        protected override void OnEnable()
        {
            base.OnEnable();
            BTreeManager = (BehaviorManager)serializedObject.targetObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reload"))
            {
                BTreeManager.Reinitialize();
            }

            if(GUILayout.Button("Debug"))
            {
                TreeDebuggerWindow.ShowWindow();
            }
        }
    }
}