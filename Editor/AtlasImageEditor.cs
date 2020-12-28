using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using System.IO;
using System.Linq;
using Coffee.UIExtensions;
using UnityEngine.U2D;
using System.Reflection;

namespace CoffeeEditor.UIExtensions
{
    /// <summary>
    /// AtlasImage Editor.
    /// </summary>
    [CustomEditor(typeof(AtlasImage), true)]
    [CanEditMultipleObjects]
    public class AtlasImageEditor : ImageEditor
    {
        private static bool _openSelectorWindow = false;
        private static SpriteAtlas _lastSpriteAtlas;
        private readonly SpritePreview _preview = new SpritePreview();
        private SerializedProperty _spAtlas;
        private SerializedProperty _spSpriteName;
        private SerializedProperty _spType;
        private SerializedProperty _spPreserveAspect;
        private AnimBool _animShowType;

        protected override void OnEnable()
        {
            if (!target) return;

            base.OnEnable();
            _spAtlas = serializedObject.FindProperty("m_SpriteAtlas");
            _spSpriteName = serializedObject.FindProperty("m_SpriteName");
            _spType = serializedObject.FindProperty("m_Type");
            _spPreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            _animShowType = new AnimBool(_spAtlas.objectReferenceValue && !string.IsNullOrEmpty(_spSpriteName.stringValue));
            _animShowType.valueChanged.AddListener(new UnityAction(base.Repaint));

            _preview.onApplyBorder = () =>
            {
                PackAtlas(_spAtlas.objectReferenceValue as SpriteAtlas);
                (target as AtlasImage).sprite = (_spAtlas.objectReferenceValue as SpriteAtlas).GetSprite(_spSpriteName.stringValue);
            };

            _lastSpriteAtlas = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _preview.onApplyBorder = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAtlasPopupLayout(new GUIContent("Sprite Atlas"), new GUIContent("-"), _spAtlas);
            EditorGUI.indentLevel++;
            DrawSpritePopup(_spAtlas.objectReferenceValue as SpriteAtlas, _spSpriteName);
            EditorGUI.indentLevel--;

            AppearanceControlsGUI();
            RaycastControlsGUI();

            _animShowType.target = _spAtlas.objectReferenceValue && !string.IsNullOrEmpty(_spSpriteName.stringValue);
            if (EditorGUILayout.BeginFadeGroup(_animShowType.faded))
                this.TypeGUI();
            EditorGUILayout.EndFadeGroup();

            var imageType = (Image.Type) _spType.intValue;
            base.SetShowNativeSize(imageType == Image.Type.Simple || imageType == Image.Type.Filled, false);

            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spPreserveAspect);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            base.NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();

            // Draw preview
            var image = target as AtlasImage;
            _preview.sprite = GetOriginalSprite(image.spriteAtlas, image.spriteName);
            _preview.color = image ? image.canvasRenderer.GetColor() : Color.white;
        }

        public override GUIContent GetPreviewTitle()
        {
            return _preview.GetPreviewTitle();
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            _preview.OnPreviewGUI(rect);
        }

        public override string GetInfoString()
        {
            return _preview.GetInfoString();
        }

        public override void OnPreviewSettings()
        {
            _preview.OnPreviewSettings();
        }

        public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
        {
            DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
        }

        public static void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
        {
            DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
        }

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

        public static void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onSelect = null)
        {
            rect = EditorGUI.PrefixLabel(rect, label);
            var gm = new GenericMenu();
            if (GUI.Button(rect, atlas ? new GUIContent(atlas.name) : nullLabel, EditorStyles.popup))
            {
                gm.AddItem(nullLabel, !atlas, () => onSelect(null));

                foreach (string path in AssetDatabase.FindAssets("t:" + typeof(SpriteAtlas).Name).Select(x => AssetDatabase.GUIDToAssetPath(x)))
                {
                    string displayName = Path.GetFileNameWithoutExtension(path);
                    gm.AddItem(
                        new GUIContent(displayName),
                        atlas && (atlas.name == displayName),
                        x => onSelect(x == null ? null : AssetDatabase.LoadAssetAtPath((string) x, typeof(SpriteAtlas)) as SpriteAtlas),
                        path
                    );
                }

                gm.DropDown(rect);
            }
        }

        public static void DrawSpritePopup(SpriteAtlas atlas, SerializedProperty spriteName)
        {
            DrawSpritePopup(new GUIContent(spriteName.displayName, spriteName.tooltip), atlas, spriteName);
        }

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

        public static void DrawSpritePopup(GUIContent label, SpriteAtlas atlas, string spriteName, UnityAction<string> onChange)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            if (_openSelectorWindow)
            {
                var atlasLabel = SetAtlasLabelToSprites(atlas, true);
                EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + atlasLabel, controlID);
                _openSelectorWindow = false;
            }

            // Popup-styled button to select sprite in atlas.
            using (new EditorGUI.DisabledGroupScope(!atlas))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                if (GUILayout.Button(string.IsNullOrEmpty(spriteName) ? "-" : spriteName, "minipopup") && atlas)
                {
                    if (_lastSpriteAtlas != atlas)
                    {
                        _lastSpriteAtlas = atlas;
                        PackAtlas(atlas);
                    }

                    _openSelectorWindow = true;
                }
            }

            if (controlID != EditorGUIUtility.GetObjectPickerControlID()) return;
            var commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorUpdated")
            {
                Object picked = EditorGUIUtility.GetObjectPickerObject();
                onChange(picked ? picked.name.Replace("(Clone)", "") : "");
            }
            else if (commandName == "ObjectSelectorClosed")
            {
                // On close selector window, reomove the atlas label from sprites.
                SetAtlasLabelToSprites(atlas, false);
            }
        }

        private static string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
        {
            string[] assetLabels = {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas))};
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

        private static void PackAtlas(SpriteAtlas atlas)
        {
            System.Type
                .GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor")
                .GetMethod("PackAtlases", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] {new[] {atlas}, EditorUserBuildSettings.activeBuildTarget});
        }

        private static Sprite GetOriginalSprite(SpriteAtlas atlas, string name)
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
