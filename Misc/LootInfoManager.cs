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

    private static CanvasGroup _totalCG;
    private static CanvasGroup _shipCG;

    private static float _displayTimeLeft;
    private static bool _isCoroutineRunning;

    private static int _shipLootValue;
    private static int _scanRealTotal;
    private static int _scanDisplayTotal;

    private static Color _lootInfoColor = Color.white;

    private const float CountSpeed = 1500f;
    private const float FadeDuration = 0.25f;

    private static readonly AnimationCurve FadeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public static void ApplyLootInfoColor()
    {
        _lootInfoColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.LootInfoColor.Value);
        ApplyColor(_totalText, _totalBg);
        ApplyColor(_shipText, _shipBg);
    }

    public static void ApplyLootInfoPos()
    {
        RectTransform totalRT = _totalCounter.GetComponent<RectTransform>();
        totalRT.anchoredPosition = new(Plugins.ConfigEntries.LootPosX.Value, Plugins.ConfigEntries.LootPosY.Value);
        totalRT.localRotation = Quaternion.identity;

        RectTransform shipRT = _shipCounter.GetComponent<RectTransform>();
        shipRT.localRotation = Quaternion.identity;
        shipRT.localScale = totalRT.localScale * 0.95f;
        shipRT.anchoredPosition = totalRT.anchoredPosition + new Vector2(18f, -17f);
    }

    public static void OnDisplayTimeChanged()
    {
        _displayTimeLeft = Plugins.ConfigEntries.DisplayTime.Value;
    }

    public static void OnShowShipLootChanged()
    {
        if (!Plugins.ConfigEntries.ShowShipLoot.Value)
            FadeOutCounter(_shipCounter, _shipCG);
    }
    public static void OnReplaceScrapCounterVisualChanged()
    {
        if (!Plugins.ConfigEntries.ReplaceScrapCounterVisual.Value)
        {
            FadeOutCounter(_totalCounter, _totalCG);
            RestoreVanillaScanUI();
        }
    }
    public static void LootScan()
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
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
                    FadeInCounter(_totalCounter, _totalCG);
                    _totalText.SetText($"Total: ${_scanDisplayTotal}");
                    HideVanillaScanUI();
                }
                else
                {
                    FadeOutCounter(_totalCounter, _totalCG);
                }
            }
            else
            {
                FadeOutCounter(_totalCounter, _totalCG);
            }

            if (inShip && Plugins.ConfigEntries.ShowShipLoot.Value)
            {
                _shipLootValue = CalculateShipLoot();
                FadeInCounter(_shipCounter, _shipCG);
                _shipText.SetText($"Ship: ${_shipLootValue}");
            }
            else
            {
                FadeOutCounter(_shipCounter, _shipCG);
            }

            yield return null;
        }

        FadeOutCounter(_totalCounter, _totalCG);
        FadeOutCounter(_shipCounter, _shipCG);

        _scanDisplayTotal = 0;
        _scanRealTotal = 0;
        _isCoroutineRunning = false;
    }
    private static void EnsureTotalCounter()
    {
        if (_totalCounter != null)
            return;

        GameObject prefab = GameObject.Find(
            "/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter"
        );
        if (!prefab)
            return;

        _totalCounter = Object.Instantiate(prefab, prefab.transform.parent, false);
        _totalText = _totalCounter.GetComponentInChildren<TextMeshProUGUI>(true);
        _totalBg = _totalCounter.GetComponentInChildren<Image>(true);

        _totalDiagonalRT = _totalCounter
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.name.ToLower().Contains("line"))
            ?.GetComponent<RectTransform>();

        _totalCG = _totalCounter.GetComponent<CanvasGroup>()
                  ?? _totalCounter.AddComponent<CanvasGroup>();

        _totalCG.alpha = 0f;
        _totalCounter.SetActive(false);

        ApplyLootInfoColor();
    }
    private static void EnsureShipCounter()
    {
        if (_shipCounter != null)
            return;

        EnsureTotalCounter();

        _shipCounter = Object.Instantiate(_totalCounter, _totalCounter.transform.parent, false);

        _shipText = _shipCounter.GetComponentInChildren<TextMeshProUGUI>(true);
        _shipBg = _shipCounter.GetComponentInChildren<Image>(true);

        _shipDiagonalRT = _shipCounter
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(i => i.name.ToLower().Contains("line"))
            ?.GetComponent<RectTransform>();

        _shipCG = _shipCounter.GetComponent<CanvasGroup>()
                 ?? _shipCounter.AddComponent<CanvasGroup>();

        _shipCG.alpha = 0f;
        _shipCounter.SetActive(false);

        ApplyLootInfoPos();
        ApplyLootInfoColor();
    }
    private static IEnumerator Fade(CanvasGroup cg, float from, float to)
    {
        float t = 0f;

        while (t < FadeDuration)
        {
            t += Time.deltaTime;
            float eval = FadeCurve.Evaluate(t / FadeDuration);
            cg.alpha = Mathf.Lerp(from, to, eval);
            yield return null;
        }

        cg.alpha = to;
    }
    private static void FadeInCounter(GameObject go, CanvasGroup cg)
    {
        if (go == null || cg == null || go.activeSelf || cg.alpha > 0.99f)
            return;

        go.SetActive(true);
        GameNetworkManager.Instance.StartCoroutine(Fade(cg, 0f, 1f));
    }
    private static void FadeOutCounter(GameObject go, CanvasGroup cg)
    {
        if (go == null || cg == null || !go.activeSelf || cg.alpha < 0.01f)
            return;

        GameNetworkManager.Instance.StartCoroutine(FadeOutRoutine(go, cg));
    }
    private static IEnumerator FadeOutRoutine(GameObject go, CanvasGroup cg)
    {
        yield return Fade(cg, cg.alpha, 0f);
        go.SetActive(false);
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
    private static void AlignShipToTotalDiagonal()
    {
        if (_totalDiagonalRT == null || _shipDiagonalRT == null)
            return;

        RectTransform shipRT = _shipCounter.GetComponent<RectTransform>();

        float totalEndY = _totalDiagonalRT.TransformPoint(_totalDiagonalRT.rect.min).y;
        float shipStartY = _shipDiagonalRT.TransformPoint(_shipDiagonalRT.rect.max).y;

        shipRT.position += new Vector3(0f, totalEndY - shipStartY, 0f);
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
            _vanillaTotalLoot = GameObject.Find(
                "UI/Canvas/ObjectScanner/GlobalScanInfo/AnimContainer/Image"
            );

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

        CanvasGroup cg = _vanillaTotalLoot.GetComponent<CanvasGroup>()
                         ?? _vanillaTotalLoot.AddComponent<CanvasGroup>();

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
