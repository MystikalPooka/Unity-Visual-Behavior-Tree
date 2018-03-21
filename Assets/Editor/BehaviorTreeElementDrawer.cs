using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    [CustomPropertyDrawer(typeof(BehaviorTreeElement), false)]
    public class BehaviorTreeElementDrawer : PropertyDrawer
    {
        GUIStyle style = new GUIStyle();

        private Color GetBehaviorStateColor(int state)
        {
            switch (state)
            {
                case (int)BehaviorState.Fail:
                    return Color.red;
                case (int)BehaviorState.Running:
                    return Color.blue;
                case (int)BehaviorState.Success:
                    return new Color(0.1f, 0.9f, 0.2f);
                case (int)BehaviorState.Null:
                    return Color.grey;
                default:
                    return Color.black;
            }
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);
            Debug.Log("BTE gui called");
            var nameRect = new Rect(pos.x - 5, pos.y, 2 * pos.width / 3, pos.height);

            var behaviorState = prop.FindPropertyRelative("_CurrentState");

            if (behaviorState != null)
            {
                style.onNormal.textColor = GetBehaviorStateColor(behaviorState.intValue);
                style.normal.textColor = GetBehaviorStateColor(behaviorState.intValue);
            }
            else
            {
                style.onNormal.textColor = Color.black;
                style.normal.textColor = Color.black;
            }

            EditorGUI.LabelField(nameRect, "Name: " + prop.FindPropertyRelative("_Name").stringValue, style);

            EditorGUI.EndProperty();
        }
    }
}