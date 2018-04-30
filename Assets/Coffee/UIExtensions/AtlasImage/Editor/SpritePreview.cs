using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace Mobcast.CoffeeEditor.UIExtensions
{
	/// <summary>
	/// スプライトプレビュー.
	/// </summary>
	internal class SpritePreview
	{
		/// <summary>
		/// スプライトカラー.
		/// プレビュー時のカラーを設定します.
		/// </summary>
		public Color color = Color.white;

		/// <summary>
		/// スプライトボーダー.
		/// </summary>
		Vector4 m_Border;

		/// <summary>
		/// ボーダーエディットモード.
		/// </summary>
		bool m_EnableBorderEdit = false;

		/// <summary>
		/// 対象のスプライト.
		/// nullの場合、何も表示されません.
		/// </summary>
		/// <value>The sprite.</value>
		public Sprite sprite
		{
			get{ return m_Sprite; }
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

		/// <summary>
		/// DrawSpriteメソッド.
		/// UnityEditor.UI.SpriteDrawUtility.DrawSpriteは公開されていないメソッドのため、リフレクションから実行します.
		/// </summary>
		MethodInfo miDrawSprite = System.Type.GetType("UnityEditor.UI.SpriteDrawUtility, UnityEditor.UI")
			.GetMethod("DrawSprite",
			                          BindingFlags.NonPublic | BindingFlags.Static,
			                          null,
			                          new System.Type[]{ typeof(Texture), typeof(Rect), typeof(Vector4), typeof(Rect), typeof(Rect), typeof(Rect), typeof(Color), typeof(Material) },
			                          null
		                          );


		/// <summary>
		/// プレビューのタイトルを返します.
		/// </summary>
		public GUIContent GetPreviewTitle()
		{
			return new GUIContent(sprite ? sprite.name : "-");
		}

		/// <summary>
		/// スプライトをボーダー付きで表示します.
		/// このメソッドは、UnityEditor.UI.SpriteDrawUtility.DrawSpriteとほぼ同等です.
		/// 詳しくは、UnityEditor.UI.SpriteDrawUtilityを逆コンパイルしてください.
		/// </summary>
		/// <param name="drawArea">描画領域.</param>
		/// <param name="border">ボーダー(LBRT).</param>
		public void DrawSprite(Rect drawArea, Vector4 border)
		{
			if (sprite == null)
				return;

			Texture2D tex = sprite.texture;
			if (tex == null)
				return;

			Rect outer = sprite.rect;
			Rect inner = outer;
			inner.xMin += border.x;
			inner.yMin += border.y;
			inner.xMax -= border.z;
			inner.yMax -= border.w;

			Vector4 uv4 = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
			Rect uv = new Rect(uv4.x, uv4.y, uv4.z - uv4.x, uv4.w - uv4.y);
			Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(sprite);
			padding.x /= outer.width;
			padding.y /= outer.height;
			padding.z /= outer.width;
			padding.w /= outer.height;

			miDrawSprite.Invoke(null, new object[]{ tex, drawArea, padding, outer, inner, uv, color, null });
		}

		/// <summary>
		/// インタラクティブなスプライトプレビューを表示します.
		/// </summary>
		public void OnPreviewGUI(Rect rect)
		{
			//スプライトを描画.
			DrawSprite(rect, m_Border);

			//ボーダー編集ウィンドウを描画します.
			DrawBorderEditWindow(rect);
		}

		/// <summary>
		/// ボーダー編集ウィンドウを描画します.
		/// </summary>
		/// <param name="rect">描画領域.</param>
		void DrawBorderEditWindow(Rect rect)
		{
			if (!m_EnableBorderEdit)
				return;

			//背景ウィンドウの描画.
			Rect boxRect = new Rect(rect.x + rect.width - 70, rect.y - 3, 70, 80);
			GUI.Box(boxRect, "", "helpbox");

			//ボーダー要素の描画.
			float labelWidth = EditorGUIUtility.labelWidth;
			int fontSize = EditorStyles.label.fontSize;
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

		/// <summary>
		/// MiniIntフィールド.
		/// </summary>
		/// <param name="rect">表示位置.</param>
		/// <param name="label">ラベル.</param>
		/// <param name="value">編集するボーダー(Vector4).</param>
		Rect MiniIntField(Rect rect, string label, ref float value)
		{
			value = Mathf.Max(0, EditorGUI.IntField(rect, label, (int)value, EditorStyles.miniTextField));
			rect.y += rect.height + 1;
			return rect;
		}

		/// <summary>
		/// ボーダーをスプライトに適用します.
		/// </summary>
		void ApplyBorder()
		{
			bool isDirty = false;
			TextureImporter t = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_Sprite)) as TextureImporter;

			switch (t.spriteImportMode) {
			case SpriteImportMode.Single:
				t.spriteBorder = m_Border;
				isDirty = true;
				break;
			case SpriteImportMode.Multiple:
				SpriteMetaData[] spritesheet = t.spritesheet;
				for (int i = 0; i < spritesheet.Length; i++)
				{
					if (spritesheet[i].name == m_Sprite.name)
					{
						spritesheet[i].border = m_Border;
						isDirty = true;
					}
				}
				t.spritesheet = spritesheet;
				break;
			}
//			if (t.spriteImportMode == SpriteImportMode.Single && 0 < (t.spriteBorder - m_Border).sqrMagnitude)
//			{
//				t.spriteBorder = m_Border;
//				isDirty = true;
//			}
//
//			SpriteMetaData[] spritesheet = t.spritesheet;
//			for (int i = 0; i < spritesheet.Length; i++)
//			{
//				if (spritesheet[i].name == m_Sprite.name && 0 < (spritesheet[i].border - m_Border).sqrMagnitude)
//				{
//					spritesheet[i].border = m_Border;
//					t.spritesheet = spritesheet;
//					isDirty = true;
//					break;
//				}
//			}


			if (isDirty) {
				EditorUtility.SetDirty(t);
				t.SaveAndReimport();

				if (onApplyBorder != null) {
					onApplyBorder();
				}
			}
		}

		/// <summary>
		/// プレビュー上部の情報テキストを返します。
		/// </summary>
		public string GetInfoString()
		{
			return m_Sprite ? string.Format("{0} : {1}x{2}", m_Sprite.name, Mathf.RoundToInt(m_Sprite.rect.width), Mathf.RoundToInt(m_Sprite.rect.height)) : "";
		}

		/// <summary>
		/// プレビュー用のヘッダーテキストを返します.
		/// </summary>
		public void OnPreviewSettings()
		{
			m_EnableBorderEdit = GUILayout.Toggle(m_EnableBorderEdit, "Border", "PreButton");
		}
	}
}