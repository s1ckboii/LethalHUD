using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD.Refs;

public class LHSlotRefs : MonoBehaviour
{
    [Header("Images")]
    [Tooltip("Main frame image for this custom slot prefab. LethalHUD assigns this to HUDManager.itemSlotIconFrames for the matching inventory slot.")]
    public Image frame;

    [Tooltip("Optional secondary frame layer. Its RGB color follows the main Frame while keeping its own alpha.")]
    public Image frameB;

    [Tooltip("Optional third frame layer. Its RGB color follows the main Frame while keeping its own alpha.")]
    public Image frameC;

    [Tooltip("Main item icon image for this custom slot prefab. LethalHUD assigns this to HUDManager.itemSlotIcons for the matching inventory slot.")]
    public Image icon;

    [Tooltip("Optional duplicate icon layer. Copies the main Icon sprite, enabled state, color, and preserveAspect setting.")]
    public Image iconB;

    [Header("Layout")]
    [Tooltip("If enabled, each instantiated slot prefab randomly rotates to 0, 90, 180, or 270 degrees. If disabled, it uses the default -90 degree rotation.")]
    public bool rotateRandomly = false;

    private Animator _animator;
    private bool _lastHasItem;

    private static readonly int hasItemHash = Animator.StringToHash("hasItem");
    private static readonly float[] quarterRotations = [0f, 90f, 180f, 270f];

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (rotateRandomly)
        {
            float z = quarterRotations[Random.Range(0, quarterRotations.Length)];
            transform.localRotation = Quaternion.Euler(0f, 0f, z);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }
    }

    private void LateUpdate()
    {
        if (icon != null)
        {
            icon.transform.rotation = Quaternion.identity;

            if (iconB != null)
            {
                iconB.transform.rotation = Quaternion.identity;
                iconB.enabled = icon.enabled;
                iconB.sprite = icon.sprite;
                iconB.color = icon.color;
                iconB.preserveAspect = true;
            }

            if (_animator != null)
            {
                bool hasItem = icon.enabled;

                if (hasItem != _lastHasItem)
                {
                    _lastHasItem = hasItem;
                    _animator.SetBool(hasItemHash, hasItem);
                }
            }
        }

        if (frame != null)
        {
            Color masterRGB = frame.color;

            if (frameB != null)
                frameB.color = new Color(masterRGB.r, masterRGB.g, masterRGB.b, frameB.color.a);

            if (frameC != null)
                frameC.color = new Color(masterRGB.r, masterRGB.g, masterRGB.b, frameC.color.a);
        }
    }
}