using GameNetcodeStuff;
using LethalHUD.HUD;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.Misc;

public static class LootInfoManager
{
    private static GameObject _ship;
    private static GameObject _totalCounter;
    private static GameObject _shipCounter;
    private static GameObject _vanillaTotalLoot;

    private static TextMeshProUGUI _totalText;
    private static TextMeshProUGUI _shipText;
    private static TextMeshProUGUI _vanillaTotalNum;

    private static Image _totalBg;
    private static Image _shipBg;

    private static RectTransform _totalDiagonalRT;
    private static RectTransform _shipDiagonalRT;

    private static float _displayTimeLeft;
    private static bool _isCoroutineRunning;

    private static int _shipLootValue;
    private static int _scanRealTotal;
    private static int _scanDisplayTotal;

    private static Color _lootInfoColor = Color.white;

    private const float CountSpeed = 1500f;

    public static void ApplyLootInfoColor()
    {
        _lootInfoColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.LootInfoColor.Value);
        ApplyColor(_totalText, _totalBg);
        ApplyColor(_shipText, _shipBg);
    }

    private static void ApplyColor(TextMeshProUGUI text, Image bg)
    {
        if (text != null)
            text.color = _lootInfoColor;

        if (bg != null)
        {
            Color c = _lootInfoColor;
            c.a *= 0.25f;
            bg.color = c;
        }
    }

    public static void OnDisplayTimeChanged()
    {
        _displayTimeLeft = Plugins.ConfigEntries.DisplayTime.Value;
    }

    public static void OnShowShipLootChanged()
    {
        if (!Plugins.ConfigEntries.ShowShipLoot.Value && _shipCounter != null)
            _shipCounter.SetActive(false);
    }

    public static void OnReplaceScrapCounterVisualChanged()
    {
        if (!Plugins.ConfigEntries.ReplaceScrapCounterVisual.Value)
        {
            if (_totalCounter != null)
                _totalCounter.SetActive(false);

            RestoreVanillaScanUI();
        }
    }

    public static void LootScan()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        _displayTimeLeft = Plugins.ConfigEntries.DisplayTime.Value;

        if (!_isCoroutineRunning)
            GameNetworkManager.Instance.StartCoroutine(DisplayCoroutine());
    }

    private static IEnumerator DisplayCoroutine()
    {
        _isCoroutineRunning = true;

        EnsureTotalCounter();
        EnsureShipCounter();
        EnsureVanillaTotalLoot();

        AlignShipToTotalDiagonal();

        float timer = 0f;

        while (timer < _displayTimeLeft)
        {
            timer += Time.deltaTime;

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            bool inShip = player != null && player.isInHangarShipRoom;

            if (Plugins.ConfigEntries.ReplaceScrapCounterVisual.Value)
            {
                UpdateVanillaScanTotal();

                _scanDisplayTotal = (int)Mathf.MoveTowards(
                    _scanDisplayTotal,
                    _scanRealTotal,
                    CountSpeed * Time.deltaTime
                );

                if (_scanDisplayTotal > 0)
                {
                    _totalCounter.SetActive(true);
                    _totalText.SetText($"Total: ${_scanDisplayTotal}");
                    HideVanillaScanUI();
                }
                else
                {
                    _totalCounter.SetActive(false);
                }

            }
            else
            {
                _totalCounter.SetActive(false);
            }

            if (inShip && Plugins.ConfigEntries.ShowShipLoot.Value)
            {
                _shipLootValue = CalculateShipLoot();
                _shipCounter.SetActive(true);
                _shipText.SetText($"Ship: ${_shipLootValue}");
            }
            else
            {
                _shipCounter.SetActive(false);
            }

            yield return null;
        }

        _totalCounter.SetActive(false);
        _shipCounter.SetActive(false);

        _scanDisplayTotal = 0;
        _scanRealTotal = 0;
        _isCoroutineRunning = false;
    }

    private static void EnsureTotalCounter()
    {
        if (_totalCounter != null)
            return;

        GameObject prefab = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
        if (!prefab)
            return;

        _totalCounter = Object.Instantiate(prefab, prefab.transform.parent, false);
        _totalText = _totalCounter.GetComponentInChildren<TextMeshProUGUI>(true);
        _totalBg = _totalCounter.GetComponentInChildren<Image>(true);

        _totalDiagonalRT = _totalCounter
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.name.ToLower().Contains("line"))
            ?.GetComponent<RectTransform>();

        ApplyLootInfoColor();
    }

    private static void EnsureShipCounter()
    {
        if (_shipCounter != null)
            return;

        EnsureTotalCounter();

        _shipCounter = Object.Instantiate(
            _totalCounter,
            _totalCounter.transform.parent,
            false
        );

        RectTransform totalRT = _totalCounter.GetComponent<RectTransform>();
        RectTransform shipRT = _shipCounter.GetComponent<RectTransform>();
        
        shipRT.localScale = totalRT.localScale * 0.95f;
        shipRT.anchoredPosition = totalRT.anchoredPosition + new Vector2(18f, -17f);


        _shipText = _shipCounter.GetComponentInChildren<TextMeshProUGUI>(true);
        _shipBg = _shipCounter.GetComponentInChildren<Image>(true);

        _shipDiagonalRT = _shipCounter
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.name.ToLower().Contains("line"))
            ?.GetComponent<RectTransform>();

        ApplyLootInfoColor();
    }


    private static void AlignShipToTotalDiagonal()
    {
        if (_totalDiagonalRT == null || _shipDiagonalRT == null)
            return;

        RectTransform shipRT = _shipCounter.GetComponent<RectTransform>();

        float totalEndY = _totalDiagonalRT.TransformPoint(_totalDiagonalRT.rect.min).y;
        float shipStartY = _shipDiagonalRT.TransformPoint(_shipDiagonalRT.rect.max).y;

        float deltaY = totalEndY - shipStartY;
        shipRT.position += new Vector3(0f, deltaY, 0f);
    }

    private static int CalculateShipLoot()
    {
        if (_ship == null)
            _ship = GameObject.Find("/Environment/HangarShip");

        if (_ship == null)
            return 0;

        return _ship.GetComponentsInChildren<GrabbableObject>()
            .Where(o => o.itemProperties.isScrap && o is not RagdollGrabbableObject)
            .Sum(o => o.scrapValue);
    }

    private static void EnsureVanillaTotalLoot()
    {
        if (_vanillaTotalLoot == null)
            _vanillaTotalLoot = GameObject.Find("UI/Canvas/ObjectScanner/GlobalScanInfo/AnimContainer/Image");

        if (_vanillaTotalLoot != null && _vanillaTotalNum == null)
        {
            _vanillaTotalNum = _vanillaTotalLoot
                .GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(t => t.gameObject.name == "TotalNum");
        }
    }

    private static void UpdateVanillaScanTotal()
    {
        if (_vanillaTotalNum == null)
            return;

        string raw = _vanillaTotalNum.text.Replace("$", "");
        int.TryParse(raw, out _scanRealTotal);
    }

    private static void HideVanillaScanUI()
    {
        if (_vanillaTotalLoot == null)
            return;

        CanvasGroup cg = _vanillaTotalLoot.GetComponent<CanvasGroup>() ?? _vanillaTotalLoot.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private static void RestoreVanillaScanUI()
    {
        if (_vanillaTotalLoot == null)
            return;

        CanvasGroup cg = _vanillaTotalLoot.GetComponent<CanvasGroup>();
        if (cg == null)
            return;

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
}
