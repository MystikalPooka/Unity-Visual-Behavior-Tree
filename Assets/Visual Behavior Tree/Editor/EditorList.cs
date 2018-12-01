using UnityEditor;
using UnityEngine;
using System;

namespace Assets.Editor
{
    [Flags]
    public enum EditorListOption
    {
        None = 0,
        ListSize = 1,
        ListLabel = 2,
        ElementLabels = 4,
        Buttons = 8,
        Default = ListSize | ListLabel | ElementLabels,
        NoElementLabels = ListSize | ListLabel,
        All = Default | Buttons
    }

    public static class EditorList
    {

        private static GUIContent
            moveButtonContent = new GUIContent("\u21b4", "move down"),
            duplicateButtonContent = new GUIContent("+", "duplicate"),
            deleteButtonContent = new GUIContent("-", "delete"),
            addButtonContent = new GUIContent("+", "add element");

        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

        public static void Show(SerializedProperty list, EditorListOption options = EditorListOption.Default)
        {
            if (!list.isArray)
            {
                EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
                return;
            }

            bool
                showListLabel = (options & EditorListOption.ListLabel) != 0,
                showListSize = (options & EditorListOption.ListSize) != 0;

            if (showListLabel)
            {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }
            if (!showListLabel || list.isExpanded)
            {
                SerializedProperty size = list.FindPropertyRelative("Array.size");
                if (showListSize)
                {
                    EditorGUILayout.PropertyField(size);
                }
                if (size.hasMultipleDifferentValues)
                {
                    EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
                }
                else
                {
                    ShowElements(list, options);
                }
            }
            if (showListLabel)
            {
                EditorGUI.indentLevel -= 1;
            }
        }

        private static void ShowElements(SerializedProperty list, EditorListOption options)
        {
            bool
                showElementLabels = (options & EditorListOption.ElementLabels) != 0,
                showButtons = (options & EditorListOption.Buttons) != 0;

            for (int i = 0; i < list.arraySize; i++)
            {
                if (showButtons)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                if (showElementLabels)
                {
                    var element = list.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element);
                }
                else
                {
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);
                }
                if (showButtons)
                {
                    ShowButtons(list, i);
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (showButtons && list.arraySize == 0 && GUILayout.Button(addButtonContent, EditorStyles.miniButton))
            {
                list.arraySize += 1;
            }
        }

        private static void ShowButtons(SerializedProperty list, int index)
        {
            if (GUILayout.Button(moveButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth))
            {
                list.MoveArrayElement(index, index + 1);
            }
            if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth))
            {
                list.InsertArrayElementAtIndex(index);
            }
            if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
            {
                int oldSize = list.arraySize;
                list.DeleteArrayElementAtIndex(index);
                if (list.arraySize == oldSize)
                {
                    list.DeleteArrayElementAtIndex(index);
                }
            }
        }
    }
}