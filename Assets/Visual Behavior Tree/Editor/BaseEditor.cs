using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class BaseEditor<T> : UnityEditor.Editor where T : MonoBehaviour
    {
        T data;
        protected virtual void OnEnable()
        {
            data = (T)serializedObject.targetObject;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GUIContent label = new GUIContent();
            label.text = "Properties";

            DrawDefaultInspectors(label, data);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawDefaultInspectors(GUIContent label, T target)
        {
            EditorGUILayout.Separator();
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields();
            EditorGUI.indentLevel++;

            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic)
                {
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(target, EditorGUILayout.IntField(
                            MakeLabel(field), (int)field.GetValue(target)));
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValue(target, EditorGUILayout.FloatField(
                            MakeLabel(field), (float)field.GetValue(target)));
                    }
                    else if(field.FieldType == typeof(List<BehaviorTreeManagerAsset>))
                    {
                        var fieldProp = serializedObject.FindProperty(field.Name);
                        EditorList.Show(fieldProp, EditorListOption.Buttons | EditorListOption.ElementLabels);
                    }
                    else if(field.FieldType == typeof(GameObject[]))
                    {
                        var fieldProp = serializedObject.FindProperty(field.Name);
                        EditorList.Show(fieldProp, EditorListOption.Buttons | EditorListOption.ElementLabels);
                    }
                    else if(serializedObject != null)
                    {
                        //Debug.Log("Trying to draw: " + field.Name);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                    }
                    else
                    {
                        Debug.LogError(
                           "DrawDefaultInspectors does not support fields of type " +
                           field.FieldType);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        private static GUIContent MakeLabel(FieldInfo field)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = field.Name;
            object[] descriptions =
               field.GetCustomAttributes(typeof(DescriptionAttribute), true);

            if (descriptions.Length > 0)
            {
                //just use the first one.
                guiContent.tooltip =
                   (descriptions[0] as DescriptionAttribute).Description;
            }

            return guiContent;
        }
    }
}