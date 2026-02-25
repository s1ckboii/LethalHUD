using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD.Refs
{
    public class LHSlotRefs : MonoBehaviour
    {
        public Image Frame;
        public Image FrameB;
        public Image FrameC;
        public Image Icon;
        public Image IconB;

        [Header("Layout")]
        public bool rotateRandomly = false;

        private Animator _animator;
        private bool _lastHasItem;

        private static readonly int HasItemHash = Animator.StringToHash("hasItem");

        private static readonly float[] QuarterRotations = [0f, 90f, 180f, 270f];


        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (rotateRandomly)
            {
                float z = QuarterRotations[Random.Range(0, QuarterRotations.Length)];
                transform.localRotation = Quaternion.Euler(0f, 0f, z);
            }
            else
                transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }

        private void LateUpdate()
        {
            if (Icon != null)
            {
                Icon.transform.rotation = Quaternion.identity;

                if (IconB != null)
                {
                    IconB.transform.rotation = Quaternion.identity;
                    IconB.enabled = Icon.enabled;
                    IconB.sprite = Icon.sprite;
                    IconB.color = Icon.color;
                    IconB.preserveAspect = true;
                }

                if (_animator != null)
                {
                    bool hasItem = Icon.enabled;
                    if (hasItem != _lastHasItem)
                    {
                        _lastHasItem = hasItem;
                        _animator.SetBool(HasItemHash, hasItem);
                    }
                }
            }

            if (Frame != null && FrameB != null && FrameC != null)
            {
                Color masterRGB = Frame.color;

                FrameB.color = new Color(masterRGB.r, masterRGB.g, masterRGB.b, FrameB.color.a);
                FrameC.color = new Color(masterRGB.r, masterRGB.g, masterRGB.b, FrameC.color.a);
            }
        }
    }
}