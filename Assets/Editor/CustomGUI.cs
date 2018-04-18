using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class CustomGUI
    {
        //Taken from https://answers.unity.com/questions/37752/how-to-render-a-colored-2d-rectangle.html
        private static List<KeyValuePair<Color, Texture2D>> quadTextures = new List<KeyValuePair<Color, Texture2D>>();
        public static void DrawQuad(Rect position, Color color)
        {
            var tmp = quadTextures.Find(t => t.Key == color);
            Texture2D texture;
            if (tmp.Equals(default(KeyValuePair<Color, Texture2D>)))
            {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, color);
                texture.Apply();
                quadTextures.Add(new KeyValuePair<Color, Texture2D>(color, texture));
            }
            else
            {
                texture = tmp.Value;
            }

            GUI.skin.box.normal.background = texture;
            GUI.Box(position, GUIContent.none);
        }
    }
}
