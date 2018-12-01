using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class TypeDependantDrawer
    {
        private static float parameterBuffer = 10f;

        public static void DrawAllFields(dynamic element, Rect rect)
        {
            DrawFieldsFlowLayout(new GUIContent(""), element, rect);
        }

        private static void DrawFields(GUIContent label, dynamic target, Rect rect)
        {
            Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(target.ElementType);
            FieldInfo[] fields = type.GetFields();

            GUILayout.BeginArea(rect);
            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic)
                {
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(target, EditorGUILayout.IntField(
                            MakeLabel(field), (int)field.GetValue(target), GUILayout.ExpandWidth(false)));
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValue(target, EditorGUILayout.FloatField(
                            MakeLabel(field), (float)field.GetValue(target), GUILayout.ExpandWidth(false)));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(target, EditorGUILayout.TextField(
                            MakeLabel(field), (string)field.GetValue(target), GUILayout.ExpandWidth(false)));
                    }
                    else
                    {
                        Debug.LogError(
                            "DrawFields does not support fields of type " +
                            field.FieldType);
                    }
                }
                
            }
            GUILayout.EndArea();
        }

        private static GUIContent MakeLabel(FieldInfo field)
        {
            GUIContent guiContent = new GUIContent();
            guiContent.text = field.Name;
            object[] descriptions = field.GetCustomAttributes(typeof(DescriptionAttribute), true);

            if (descriptions.Length > 0)
            {
                //just use the first one.
                guiContent.tooltip = (descriptions[0] as DescriptionAttribute).Description;
            }
            
            return guiContent;
        }


        private static void DrawFieldsFlowLayout(GUIContent label, dynamic target, Rect rect)
        {
            Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(target.ElementType);
            FieldInfo[] fields = type.GetFields();

            var items = new List<string>();
            foreach (FieldInfo field in fields)
            {
                items.Add(field.Name);
            }

            var style = EditorStyles.objectField;
            style.stretchWidth = false;
            style.fixedWidth = EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth + 4;

            var boxes = EditorGUIUtility.GetFlowLayoutedRects(rect, style, 4, 4, items);
            for(int i = 0; i < items.Count; ++i)
            {
                using (new GUILayout.AreaScope(boxes[i],"", EditorStyles.boldLabel))
                {
                    //EditorGUILayout.LabelField(items[i]);
                    if (fields[i].FieldType == typeof(int))
                    {
                        fields[i].SetValue(target, EditorGUILayout.IntField(
                            MakeLabel(fields[i]), (int)fields[i].GetValue(target)));
                    }
                    else if (fields[i].FieldType == typeof(float))
                    {
                        fields[i].SetValue(target, EditorGUILayout.FloatField(
                            MakeLabel(fields[i]), (float)fields[i].GetValue(target)));
                    }
                    else if (fields[i].FieldType == typeof(string))
                    {
                        fields[i].SetValue(target, EditorGUILayout.TextField(
                            MakeLabel(fields[i]), (string)fields[i].GetValue(target)));
                    }
                    else
                    {
                        //Debug.LogError(
                        //    "DrawFields does not support fields of type " +
                        //    fields[i].FieldType);
                    }
                }
            }
        }

        public static float GetTotalHeightOfProperties(dynamic element)
        {
            Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(element.ElementType);
            var newHeight = 4 + EditorGUIUtility.singleLineHeight * type.GetFields().Length / 2;
            return newHeight + parameterBuffer;
        }
    }
}