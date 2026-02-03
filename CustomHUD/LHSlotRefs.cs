using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD
{
    public class LHSlotRefs : MonoBehaviour
    {
        public Image Frame;
        public Image FrameB;
        public Image Icon;

        public void LateUpdate()
        {
            if (Frame == null || FrameB == null)
                return;

            FrameB.color = Frame.color;
        }
    }
}
