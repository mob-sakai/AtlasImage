using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace CoffeeEditor.UIExtensions
{
    internal class SpritePreview
    {
        public Color color = Color.white;

        Vector4 m_Border;

        bool m_EnableBorderEdit = false;

        public Sprite sprite
        {
            get { return m_Sprite; }
            set
            {
                if (m_Sprite != value)
                {
                    m_Sprite = value;
                    m_Border = m_Sprite ? m_Sprite.border : Vector4.zero;
                }
            }
        }

        public System.Action onApplyBorder;

        Sprite m_Sprite;

        MethodInfo miDrawSprite = System.Type.GetType("UnityEditor.UI.SpriteDrawUtility, UnityEditor.UI")
            .GetMethod("DrawSprite",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new System.Type[] {typeof(Texture), typeof(Rect), typeof(Vector4), typeof(Rect), typeof(Rect), typeof(Rect), typeof(Color), typeof(Material)},
                null
            );

        public GUIContent GetPreviewTitle()
        {
            return new GUIContent(sprite ? sprite.name : "-");
        }

        /// <summary>
        /// Displays a sprite with a border.
        /// This method is almost equivalent to UnityEditor.UI.SpriteDrawUtility.
        /// For more information, decompile UnityEditor.UI.SpriteDrawUtility.
        /// </summary>
        private void DrawSprite(Rect drawArea, Vector4 border)
        {
            if (sprite == null)
                return;

            var tex = sprite.texture;
            if (tex == null)
                return;

            var outer = sprite.rect;
            var inner = outer;
            inner.xMin += border.x;
            inner.yMin += border.y;
            inner.xMax -= border.z;
            inner.yMax -= border.w;

            var uv4 = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            var uv = new Rect(uv4.x, uv4.y, uv4.z - uv4.x, uv4.w - uv4.y);
            var padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
            padding.x /= outer.width;
            padding.y /= outer.height;
            padding.z /= outer.width;
            padding.w /= outer.height;

            miDrawSprite.Invoke(null, new object[] {tex, drawArea, padding, outer, inner, uv, color, null});
        }

        /// <summary>
        /// Displays an interactive sprite preview.
        /// </summary>
        public void OnPreviewGUI(Rect rect)
        {
            DrawSprite(rect, m_Border);
            DrawBorderEditWindow(rect);
        }

        /// <summary>
        /// Draws a border editing window.
        /// </summary>
        private void DrawBorderEditWindow(Rect rect)
        {
            if (!m_EnableBorderEdit)
                return;

            // Draw background
            var boxRect = new Rect(rect.x + rect.width - 70, rect.y - 3, 70, 80);
            GUI.Box(boxRect, "", "helpbox");

            // Draw border
            var labelWidth = EditorGUIUtility.labelWidth;
            var fontSize = EditorStyles.label.fontSize;
            {
                EditorGUIUtility.labelWidth = 40;
                EditorStyles.label.fontSize = 9;
                Rect elementRect = new Rect(boxRect.x + 2, boxRect.y + 3, boxRect.width - 6, 14);

                //ボーダーを編集.
                elementRect = MiniIntField(elementRect, "Left", ref m_Border.x);
                elementRect = MiniIntField(elementRect, "Right", ref m_Border.z);
                elementRect = MiniIntField(elementRect, "Top", ref m_Border.w);
                elementRect = MiniIntField(elementRect, "Bottom", ref m_Border.y);

                //ボーダーを適用.
                if (GUI.Button(elementRect, "Apply", "minibutton"))
                {
                    m_EnableBorderEdit = false;
                    ApplyBorder();
                }
            }
            EditorStyles.label.fontSize = fontSize;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private static Rect MiniIntField(Rect rect, string label, ref float value)
        {
            value = Mathf.Max(0, EditorGUI.IntField(rect, label, (int) value, EditorStyles.miniTextField));
            rect.y += rect.height + 1;
            return rect;
        }

        /// <summary>
        /// Apply the border settings to sprite.
        /// </summary>
        private void ApplyBorder()
        {
            var isDirty = false;
            var t = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_Sprite)) as TextureImporter;

            switch (t.spriteImportMode)
            {
                case SpriteImportMode.Single:
                    t.spriteBorder = m_Border;
                    isDirty = true;
                    break;
                case SpriteImportMode.Multiple:
                    var spritesheet = t.spritesheet;
                    for (var i = 0; i < spritesheet.Length; i++)
                    {
                        if (spritesheet[i].name != m_Sprite.name) continue;

                        spritesheet[i].border = m_Border;
                        isDirty = true;
                    }

                    t.spritesheet = spritesheet;
                    break;
            }

            if (!isDirty) return;

            EditorUtility.SetDirty(t);
            t.SaveAndReimport();

            if (onApplyBorder != null)
            {
                onApplyBorder();
            }
        }

        public string GetInfoString()
        {
            return m_Sprite ? string.Format("{0} : {1}x{2}", m_Sprite.name, Mathf.RoundToInt(m_Sprite.rect.width), Mathf.RoundToInt(m_Sprite.rect.height)) : "";
        }

        public void OnPreviewSettings()
        {
            m_EnableBorderEdit = GUILayout.Toggle(m_EnableBorderEdit, "Border", "PreButton");
        }
    }
}
