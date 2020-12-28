Atlas Image
===

A graphic component use `SpriteAtlas` for uGUI.  
In addition, add useful **sprite picker** and **border editor** to the inspector.

![image](https://user-images.githubusercontent.com/12690315/39434547-d5f34956-4cd3-11e8-82b1-f7f2f7be953a.png)

[![](https://img.shields.io/npm/v/com.coffee.atlas-image?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.atlas-image/)
[![](https://img.shields.io/github/v/release/mob-sakai/AtlasImage?include_prereleases)](https://github.com/mob-sakai/AtlasImage/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/AtlasImage.svg)](https://github.com/mob-sakai/AtlasImage/releases)  [![](https://img.shields.io/github/license/mob-sakai/AtlasImage.svg)](https://github.com/mob-sakai/AtlasImage/blob/main/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)  
![](https://img.shields.io/badge/Unity%202017.1+-supported-blue.svg)  

<< [Description](#Description) | [Demo](#demo) | [Installation](#installation) | [Usage](#usage) | [Development Note](#development-note) | [Change log](https://github.com/mob-sakai/AtlasImage/blob/main/CHANGELOG.md) >>



<br><br><br><br>

## Description

Are you still fatigued with `SpriteAtlas` and `Image`?  
* No interface for SpriteAtlas
    * Support only `Sprite`.
    * We pack sprites for drawing call optimization, but there is no interface.
* Confusing sprite picker
    * You can select sprites using object picker.  
    * Opject picker displays **all sprites in the project**...  
    * Do you know which sprite is included in atlas?
* Troublesome border setting
    * You can edit sprite border using sprite editor.
    * It is troublesome to select a sprite, open a sprite editor, and edit the border.


`AtlasImage` provides useful feature to use `SpriteAtlas` for UI!

### Sprite for renderring can be changed with a SpriteAtlas or a sprite name.

```cs
atlasImage.spriteAtlas = Resources.Load("A SpriteAtlas name") as SpriteAtlas;
atlasImage.spriteName = "A sprite name in the SpriteAtlas";
```

### In the inspector, sprite picker displays only sprites in the SpriteAtlas.

![image](https://user-images.githubusercontent.com/12690315/39434547-d5f34956-4cd3-11e8-82b1-f7f2f7be953a.png)


### You can edit the border in the preview window.

![image](https://user-images.githubusercontent.com/12690315/39434440-869e54ea-4cd3-11e8-9506-cdf0b62207ac.png)

### Convert `Image` to `AtlasImage` by context menu.



<br><br><br><br>

## Demo



<br><br><br><br>

## Installation

### Requirement

* Unity 2017.1 or later

### (For Unity 2018.3 or later) Using OpenUPM

This package is available on [OpenUPM](https://openupm.com).  
You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.coffee.atlas-image
```

### (For Unity 2018.3 or later) Using Git

Find the manifest.json file in the Packages folder of your project and add a line to `dependencies` field.

* Major version: ![](https://img.shields.io/github/v/release/mob-sakai/AtlasImage)  
`"com.coffee.atlas-image": "https://github.com/mob-sakai/AtlasImage.git"`

To update the package, change suffix `#{version}` to the target version.

* e.g. `"com.coffee.atlas-image": "https://github.com/mob-sakai/AtlasImage.git#1.0.0",`

Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) to install and update the package.

#### For Unity 2018.2 or earlier

1. Download a source code zip file from [Releases](https://github.com/mob-sakai/AtlasImage/releases) page
2. Extract it
3. Import it into the following directory in your Unity project
   - `Packages` (It works as an embedded package. For Unity 2018.1 or later)
   - `Assets` (Legacy way. For Unity 2017.1 or later)



<br><br><br><br>

## Usage

1. Download `AtlasImage.unitypackage` from [Releases](https://github.com/mob-sakai/AtlasImage/releases).
1. Import the package into your Unity project. Go to `Assets > Import Package > Custom Package` and select `AtlasImage.unitypackage`.
1. Enable SpriteAtlas. Go to `Edit > Project Settings > Editor`, and change the sprite packing mode from Disabled to either:
    * Enabled for Builds, when you want to use packing for builds only and not when in Play mode.
    * Always Enabled when you want the packed Sprite to resolve its texture from the Sprite Atlas during Play mode, but resolve its texture from the original Texture during Edit mode.
1. Add `AtlasImage` component instead of `Image` component from `Add Component` in inspector.
1. Select the `SpriteAtlas` by dropdown manu, and select the sprite with object piker.
1. Enjoy!



<br><br><br><br>

## Development Note

### How to work?

1. Pack atlas on open select sprite window.
```cs
static void PackAtlas(SpriteAtlas atlas)
{
    System.Type
        .GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor")
        .GetMethod("PackAtlases", BindingFlags.NonPublic | BindingFlags.Static)
        .Invoke(null, new object[]{ new []{ atlas }, EditorUserBuildSettings.activeBuildTarget });
}
```
1. Add label `<atlas-guid>` to sprites in atlas.
```cs
static string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
{
    // GUID for the atlas. 
    string[] atlasLabel = { AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas)) };

    // Packed sprites in atlas.
    SerializedProperty spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
    Sprite[] sprites = Enumerable.Range(0, spPackedSprites.arraySize)
        .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
        .OfType<Sprite>()
        .ToArray();

    // Add/remove label to sprites.
    foreach (var s in sprites)
    {
        string[] newLabels = add
            ? AssetDatabase.GetLabels(s).Union(atlasLabel).ToArray()
            : AssetDatabase.GetLabels(s).Except(atlasLabel).ToArray();
        AssetDatabase.SetLabels(s, newLabels);
    }
    
    return atlasLabel[0];
}
```
1. Open the object picker with label. It filter the sprites to display.
```cs
EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + atlasLabel, controlID);
```
1. On closed the object picker, remove label from sprites in atlas.



<br><br><br><br>

## Contributing

### Issues

Issues are very valuable to this project.

- Ideas are a valuable source of contributions others can make
- Problems show where this project is lacking
- With a question you show where contributors can improve the user experience

### Pull Requests

Pull requests are, a great way to get your ideas into this repository.  
See [sandbox/README.md](https://github.com/mob-sakai/AtlasImage/blob/sandbox/README.md).

### Support

This is an open source project that I am developing in my spare time.  
If you like it, please support me.  
With your support, I can spend more time on development. :)

[![](https://user-images.githubusercontent.com/12690315/50731629-3b18b480-11ad-11e9-8fad-4b13f27969c1.png)](https://www.patreon.com/join/mob_sakai?)  
[![](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)



<br><br><br><br>

## License

* MIT



## Author

* ![](https://user-images.githubusercontent.com/12690315/96986908-434a0b80-155d-11eb-8275-85138ab90afa.png) [mob-sakai](https://github.com/mob-sakai) [![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai) ![GitHub followers](https://img.shields.io/github/followers/mob-sakai?style=social)



## See Also

* GitHub page : https://github.com/mob-sakai/AtlasImage
* Releases : https://github.com/mob-sakai/AtlasImage/releases
* Issue tracker : https://github.com/mob-sakai/AtlasImage/issues
* Change log : https://github.com/mob-sakai/AtlasImage/blob/main/CHANGELOG.md
