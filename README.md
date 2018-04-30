AtlasImage
===

AtlasImage is a component that use SpriteAtlas for UI.
In addition, add useful `sprite selector` and `border editor` to the inspector.

![image](https://user-images.githubusercontent.com/12690315/39434547-d5f34956-4cd3-11e8-82b1-f7f2f7be953a.png)

[![](https://img.shields.io/github/release/mob-sakai/AtlasImage.svg?label=latest%20version)](https://github.com/mob-sakai/AtlasImage/release)
[![](https://img.shields.io/github/release-date/mob-sakai/AtlasImage.svg)](https://github.com/mob-sakai/AtlasImage/releases)
![](https://img.shields.io/badge/requirement-Unity%202017.1%2B-green.svg)
[![](https://img.shields.io/github/license/mob-sakai/AtlasImage.svg)](https://github.com/mob-sakai/AtlasImage/blob/master/LICENSE.txt)
[![](https://img.shields.io/github/last-commit/mob-sakai/AtlasImage/develop.svg?label=last%20commit)](https://github.com/mob-sakai/AtlasImage/commits/develop)
[![](https://img.shields.io/github/issues/mob-sakai/AtlasImage.svg)](https://github.com/mob-sakai/AtlasImage/issues)
[![](https://img.shields.io/github/commits-since/mob-sakai/AtlasImage/latest.svg)](https://github.com/mob-sakai/AtlasImage/compare/master...develop)


<< [Description](#Description) | [Demo](#demo) | [Download](https://github.com/mob-sakai/AtlasImage/releases) | [Usage](#usage) | [Development Note](#development-note) | [Change log](https://github.com/mob-sakai/AtlasImage/blob/develop/CHANGELOG.md) >>



<br><br><br><br>
## Description

Are you still fatigued with SpriteAtlas and Image?  
AtlasImage provides useful feature to use SpriteAtlas for UI.

### Sprite for renderring can be changed using a SpriteAtlas and a sprite name.

```cs
atlasImage.spriteAtlas = Resources.Load("A SpriteAtlas name") as SpriteAtlas;
atlasImage.spriteName = "A sprite name in the SpriteAtlas";
```

### In the inspector, sprite selection window shows only sprites in SpriteAtlas.

![image](https://user-images.githubusercontent.com/12690315/39434547-d5f34956-4cd3-11e8-82b1-f7f2f7be953a.png)


### You can edit the border in the preview window.

![image](https://user-images.githubusercontent.com/12690315/39434440-869e54ea-4cd3-11e8-9506-cdf0b62207ac.png)



<br><br><br><br>
## Demo



<br><br><br><br>
## Usage

1. Download `AtlasImage.unitypackage` from [Releases](https://github.com/mob-sakai/AtlasImage/releases).
1. Import the package into your Unity project. Go to `Assets > Import Package > Custom Package` and select `AtlasImage.unitypackage`.
1. Enable SpriteAtlas. Go to `Edit > Project Settings > Editor`, and change the sprite packing mode from Disabled to either:
    * Enabled for Builds, when you want to use packing for builds only and not when in Play mode.
    * Always Enabled when you want the packed Sprite to resolve its texture from the Sprite Atlas during Play mode, but resolve its texture from the original Texture during Edit mode.
1. Add `AtlasImage` component instead of `Image` component from `Add Component` in inspector.
1. Select the SpriteAtlas by dropdown manu, and select the Sprite by ObjectSelectorWindow.
1. Enjoy!


##### Requirement

* Unity 2017.1+
* No other SDK are required



<br><br><br><br>
## Development Note



<br><br><br><br>
## License

* MIT
* [Superpowers Asset Packs](http://sparklinlabs.itch.io/superpowers)![License](https://img.shields.io/badge/license-CC0-blue.svg)



## Author

[mob-sakai](https://github.com/mob-sakai)



## See Also

* GitHub page : https://github.com/mob-sakai/AtlasImage
* Releases : https://github.com/mob-sakai/AtlasImage/releases
* Issue tracker : https://github.com/mob-sakai/AtlasImage/issues
* Current project : https://github.com/mob-sakai/AtlasImage/projects/1
* Change log : https://github.com/mob-sakai/AtlasImage/blob/master/CHANGELOG.md