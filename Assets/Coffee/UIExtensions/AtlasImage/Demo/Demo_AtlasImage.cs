using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mobcast.Coffee.UIExtensions;

namespace Mobcast.Coffee.UIExtensions.Demo
{
	public class Demo_AtlasImage : MonoBehaviour
	{
		[SerializeField]
		private AtlasImage atlasImage;


		public void ChangeSpriteName(string spriteName)
		{
			if (!atlasImage)
			{
				return;
			}

			atlasImage.spriteName = spriteName;
		}
	}
}
