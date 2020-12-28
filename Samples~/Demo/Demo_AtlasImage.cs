using UnityEngine;

namespace Coffee.UIExtensions.Demo
{
    public class Demo_AtlasImage : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlasImage;

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
