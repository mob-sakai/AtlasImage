using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using System.IO;
using System.Linq;
using Mobcast.Coffee.UIExtensions;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Collections.Generic;
using System.Reflection;

namespace Mobcast.CoffeeEditor.UIExtensions
{
	/// <summary>
	/// AtlasImage用 インスペクタEditor.
	/// Imageのインスペクタを継承しています.
	/// Atlasの選択にはポップアップを、スプライトの選択には専用のセレクタを実装しています.
	/// スプライトプレビュー機能により、スプライトのボーダーをインスペクタから変更できます.
	/// </summary>
	[CustomEditor(typeof(AtlasImage), true)]
	[CanEditMultipleObjects]
	public class AtlasImageEditor : ImageEditor
	{
		/// <summary>スプライトプレビュー.</summary>
		SpritePreview preview = new SpritePreview();

		/// <summary>[SerializedProperty]アトラス(m_Atlas).</summary>
		protected SerializedProperty spAtlas;

		/// <summary>[SerializedProperty]スプライト名(m_SpriteName).</summary>
		SerializedProperty spSpriteName;

		/// <summary>[SerializedProperty]スプライトタイプ(m_Type).</summary>
		SerializedProperty spType;

		/// <summary>[SerializedProperty]アスペクト比を保持するか(m_PreserveAspect).</summary>
		SerializedProperty spPreserveAspect;

		/// <summary>スプライトタイプによるアニメーションブール.</summary>
		AnimBool animShowType;

		static SpriteAtlas lastSpriteAtlas;

		/// <summary>
		/// インスペクタ有効コールバック.
		/// </summary>
		protected override void OnEnable()
		{
			if (!target)
				return;
			
			base.OnEnable();
			spAtlas = serializedObject.FindProperty("m_SpriteAtlas");
			spSpriteName = serializedObject.FindProperty("m_SpriteName");
			spType = serializedObject.FindProperty("m_Type");
			spPreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

			animShowType = new AnimBool(spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue));
			animShowType.valueChanged.AddListener(new UnityAction(base.Repaint));

			preview.onApplyBorder = () =>
			{
				PackAtlas(spAtlas.objectReferenceValue as SpriteAtlas);
				(target as AtlasImage).sprite = (spAtlas.objectReferenceValue as SpriteAtlas).GetSprite(spSpriteName.stringValue);
			};

			lastSpriteAtlas = null;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			preview.onApplyBorder = null;
		}

		/// <summary>
		/// インスペクタGUIコールバック.
		/// Inspectorウィンドウを表示するときにコールされます.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
//			using (new EditorGUI.DisabledGroupScope(true))
//			{
//				EditorGUILayout.PropertyField(m_Script);
//			}

			//アトラスとスプライトを表示.
//			EditorGUILayout.PropertyField(spAtlas);


			DrawAtlasPopupLayout(new GUIContent("Sprite Atlas"), new GUIContent("-"), spAtlas);
			EditorGUI.indentLevel++;
			DrawSpritePopup(spAtlas.objectReferenceValue as SpriteAtlas, spSpriteName);
			EditorGUI.indentLevel--;
//			serializedObject.ApplyModifiedProperties();

//			base.OnInspectorGUI ();

			//Imageインスペクタの再現. ▼ ここから ▼.
			AppearanceControlsGUI();
			RaycastControlsGUI();

			animShowType.target = spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue);
			if (EditorGUILayout.BeginFadeGroup(animShowType.faded))
				this.TypeGUI();
			EditorGUILayout.EndFadeGroup();

			Image.Type imageType = (Image.Type)spType.intValue;
			base.SetShowNativeSize(imageType == Image.Type.Simple || imageType == Image.Type.Filled, false);

			if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(spPreserveAspect);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFadeGroup();
			base.NativeSizeButtonGUI();
			//Imageインスペクタの再現. ▲ ここまで ▲.


			serializedObject.ApplyModifiedProperties();

			//プレビューを更新.
			AtlasImage image = target as AtlasImage;
			preview.sprite = GetOriginalSprite(image.spriteAtlas, image.spriteName);
			preview.color = image ? image.canvasRenderer.GetColor() : Color.white;
		}

		/// <summary>
		/// オブジェクトプレビューのタイトルを返します.
		/// </summary>
		public override GUIContent GetPreviewTitle()
		{
			return preview.GetPreviewTitle();
		}

		/// <summary>
		/// インタラクティブなカスタムプレビューを表示します.
		/// </summary>
		public override void OnPreviewGUI(Rect rect, GUIStyle background)
		{
			preview.OnPreviewGUI(rect);
		}

		/// <summary>
		/// オブジェクトプレビューの上部にオブジェクト情報を示します。
		/// </summary>
		public override string GetInfoString()
		{
			return preview.GetInfoString();
		}

		/// <summary>
		/// プレビューのヘッダーを表示します.
		/// </summary>
		public override void OnPreviewSettings()
		{
			preview.OnPreviewSettings();
		}


		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
		{
			DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
		}

		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
		{
			DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
		}


		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="rect">描画範囲の矩形.</param>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<SpriteAtlas> onSelect = null)
		{
			DrawAtlasPopup(rect, label, nullLabel, atlas.objectReferenceValue as SpriteAtlas, obj =>
				{
					atlas.objectReferenceValue = obj;
					if (onSelect != null)
						onSelect(obj as SpriteAtlas);
					atlas.serializedObject.ApplyModifiedProperties();
				});
		}

		/// <summary>
		/// アトラスポップアップを描画します.
		/// </summary>
		/// <param name="rect">描画範囲の矩形.</param>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="onSelect">変更された時のコールバック.</param>
		public static void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onSelect = null)
		{
			rect = EditorGUI.PrefixLabel(rect, label);
			if (GUI.Button(rect, atlas ? new GUIContent(atlas.name) : nullLabel, EditorStyles.popup))
			{
				GenericMenu gm = new GenericMenu();
				//nullボタン.
				gm.AddItem(nullLabel, !atlas, () => onSelect(null));

				//プロジェクト内のアセットを、フィルタを使って全検索.
				foreach (string path in AssetDatabase.FindAssets ("t:" + typeof(SpriteAtlas).Name).Select (x => AssetDatabase.GUIDToAssetPath (x)))
				{
					string displayName = Path.GetFileNameWithoutExtension(path);
					gm.AddItem(
						new GUIContent(displayName),
						atlas && (atlas.name == displayName),
						x => onSelect(x == null ? null : AssetDatabase.LoadAssetAtPath((string)x, typeof(SpriteAtlas)) as SpriteAtlas),
						path
					);
				}

				gm.DropDown(rect);
			}
		}

		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		public static void DrawSpritePopup(SpriteAtlas atlas, SerializedProperty spriteName)
		{
			DrawSpritePopup(new GUIContent(spriteName.displayName, spriteName.tooltip), atlas, spriteName);
		}

		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="label">ラベル.</param>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		public static void DrawSpritePopup(GUIContent label, SpriteAtlas atlas, SerializedProperty spriteName)
		{
			DrawSpritePopup(
				label,
				atlas,
				string.IsNullOrEmpty(spriteName.stringValue) ? "-" : spriteName.stringValue,
				name =>
				{
					if (spriteName == null)
						return;

					spriteName.stringValue = name;
					spriteName.serializedObject.ApplyModifiedProperties();
				}
			);
		}

		static bool openSelectorWindow = false;

		/// <summary>
		/// スプライトポップアップを描画します.
		/// </summary>
		/// <param name="atlas">アトラス.</param>
		/// <param name="spriteName">スプライト名.</param>
		/// <param name="onChange">変更された時のコールバック.</param>
		public static void DrawSpritePopup(GUIContent label, SpriteAtlas atlas, string spriteName, UnityAction<string> onChange)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			if (openSelectorWindow)
			{
				var atlasLabel = SetAtlasLabelToSprites(atlas, true);
				EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + atlasLabel, controlID);
				openSelectorWindow = false;
			}

			// Popup-styled button to select sprite in atlas.
			using (new EditorGUI.DisabledGroupScope(!atlas))
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(label);
				if (GUILayout.Button(string.IsNullOrEmpty(spriteName) ? "-" : spriteName, "minipopup") && atlas)
				{
					if (lastSpriteAtlas != atlas)
					{
						lastSpriteAtlas = atlas;
						PackAtlas(atlas);
					}
					openSelectorWindow = true;
				}
			}

			//現在のオブジェクトピッカーであれば、イベント処理.
			if (controlID == EditorGUIUtility.GetObjectPickerControlID())
			{
				string commandName = Event.current.commandName;
				//選択オブジェクト更新イベント
				if (commandName == "ObjectSelectorUpdated")
				{
					Object picked = EditorGUIUtility.GetObjectPickerObject();
					onChange(picked ? picked.name.Replace("(Clone)", "") : "");
				}
				//クローズイベント
				else if (commandName == "ObjectSelectorClosed")
				{
					// On close selector window, reomove the atlas label from sprites.
					SetAtlasLabelToSprites(atlas, false);
				}
			}
		}

		/// <summary>
		/// Sets the atlas label to sprites.
		/// </summary>
		/// <returns>The atlas label to sprites.</returns>
		/// <param name="atlas">Atlas.</param>
		/// <param name="add">If set to <c>true</c> add.</param>
		static string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
		{
			string[] assetLabels = { AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas)) };
			SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
			Sprite[] sprites = Enumerable.Range(0, spPackedSprites.arraySize)
				.Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
				.OfType<Sprite>()
				.ToArray();

			foreach (var s in sprites)
			{
				string[] newLabels = add
					? AssetDatabase.GetLabels(s).Union(assetLabels).ToArray()
					: AssetDatabase.GetLabels(s).Except(assetLabels).ToArray();
				AssetDatabase.SetLabels(s, newLabels);
			}
			return assetLabels[0];
		}

		/// <summary>
		/// Packs the atlas.
		/// </summary>
		/// <param name="atlas">Atlas.</param>
		static void PackAtlas(SpriteAtlas atlas)
		{
			System.Type
				.GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor")
				.GetMethod("PackAtlases", BindingFlags.NonPublic | BindingFlags.Static)
				.Invoke(null, new object[]{ new []{ atlas }, EditorUserBuildSettings.activeBuildTarget });
		}

		static Sprite GetOriginalSprite(SpriteAtlas atlas, string name)
		{
			if (!atlas || string.IsNullOrEmpty(name))
			{
				return null;
			}

			SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
			return Enumerable.Range(0, spPackedSprites.arraySize)
				.Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
				.OfType<Sprite>()
				.FirstOrDefault(s => s.name == name);
		}


		//%%%% v Context menu for editor v %%%%
		[MenuItem("CONTEXT/Image/Convert To AtlasImage", true)]
		static bool _ConvertToAtlasImage(MenuCommand command)
		{
			return CanConvertTo<AtlasImage>(command.context);
		}

		[MenuItem("CONTEXT/Image/Convert To AtlasImage", false)]
		static void ConvertToAtlasImage(MenuCommand command)
		{
			ConvertTo<AtlasImage>(command.context);
		}

		[MenuItem("CONTEXT/Image/Convert To Image", true)]
		static bool _ConvertToImage(MenuCommand command)
		{
			return CanConvertTo<Image>(command.context);
		}

		[MenuItem("CONTEXT/Image/Convert To Image", false)]
		static void ConvertToImage(MenuCommand command)
		{
			ConvertTo<Image>(command.context);
		}

		/// <summary>
		/// Verify whether it can be converted to the specified component.
		/// </summary>
		protected static bool CanConvertTo<T>(Object context)
			where T : MonoBehaviour
		{
			return context && context.GetType() != typeof(T);
		}

		/// <summary>
		/// Convert to the specified component.
		/// </summary>
		protected static void ConvertTo<T>(Object context) where T : MonoBehaviour
		{
			var target = context as MonoBehaviour;
			var so = new SerializedObject(target);
			so.Update();

			bool oldEnable = target.enabled;
			target.enabled = false;

			// Find MonoScript of the specified component.
			foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
			{
				if (script.GetClass() != typeof(T))
					continue;

				// Set 'm_Script' to convert.
				so.FindProperty("m_Script").objectReferenceValue = script;
				so.ApplyModifiedProperties();
				break;
			}

			(so.targetObject as MonoBehaviour).enabled = oldEnable;
		}
	}
}