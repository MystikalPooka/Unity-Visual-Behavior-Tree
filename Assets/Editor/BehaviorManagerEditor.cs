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
            if(name == "")
            {
                if(GUILayout.Button("Create New Tree"))
                {
                    CustomAssetUtility.CreateAsset<BehaviorTreeManagerAsset>();
                    _BTreeAsset = (BehaviorTreeManagerAsset)Selection.activeObject;
                }
            }

            if (GUILayout.Button("Save"))
            {
                string name = serializedObject.FindProperty("FileName").stringValue;
                if (name == "")
                {
                    Debug.LogError("Name Field is required to save a behavior tree manager asset");
                }
                else
                {
                    BTreeManager.SaveBehaviorAsset("Assets/Behaviors/" + name + ".asset", _BTreeAsset);
                }
                Debug.Log("Attempted save.");
            }


            if (GUILayout.Button("Reload"))
            {
                BTreeManager.Reinitialize();
            }
        }
    }
}