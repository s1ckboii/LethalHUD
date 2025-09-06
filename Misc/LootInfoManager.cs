/*
using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LethalHUD.Misc;

public static class LootInfoManager
{
    private static GameObject _ship;
    private static GameObject _totalCounter;
    private static GameObject _vanillaTotalLoot;
    private static TextMeshProUGUI _textMesh;
    private static TextMeshProUGUI _textMeshVanillaTotalNum;

    private static float _displayTimeLeft;
    private static bool _isCoroutineRunning = false;

    private static readonly AnimationCurve FadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private const float FadeDuration = 0.5f;

    private static GameObject Ship
    {
        get
        {
            if (_ship == null)
                _ship = GameObject.Find("/Environment/HangarShip");
            return _ship;
        }
    }

    public static void LootScan()
    {
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null) return;

        if (localPlayer.isInHangarShipRoom)
            ShowShipLoot();
        else if (!localPlayer.isInHangarShipRoom)
            ShowOutsideLoot();

            _displayTimeLeft = Plugins.ConfigEntries.DisplayTime.Value;
    }

    public static void ShowShipLoot()
    {
        if (!Plugins.ConfigEntries.ShowShipLoot.Value) return;

        if (!_totalCounter)
            CopyValueCounter();
        
        int value = CalculateLootValue();
        UpdateText($"Ship: ${value}");

        if (!_isCoroutineRunning)
            GameNetworkManager.Instance.StartCoroutine(ShipLootCoroutine());
    }

    public static void ShowOutsideLoot()
    {
        //if (Plugins.ConfigEntries.ReplaceScrapCounterVisual.Value)
        //    HideVanillaTotalInfo();

        if (!_totalCounter)
            CopyValueCounter();

        EnsureVanillaTotalLoot();

        if (_textMeshVanillaTotalNum != null)
            UpdateText($"Total: {_textMeshVanillaTotalNum.text}");

        _displayTimeLeft = Plugins.ConfigEntries.DisplayTime.Value;

        if (!_isCoroutineRunning)
            GameNetworkManager.Instance.StartCoroutine(ShipLootCoroutine());
    }

    internal static void UpdateText(string text) => _textMesh?.SetText(text);

    public static void HideVanillaTotalInfo()
    {
        if (_vanillaTotalLoot == null)
            _vanillaTotalLoot = GameObject.Find("UI/Canvas/ObjectScanner/GlobalScanInfo/AnimContainer/Image");

        if (_vanillaTotalLoot != null)
        {
            var cg = _vanillaTotalLoot.GetComponent<CanvasGroup>() ?? _vanillaTotalLoot.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            Loggers.Debug("Vanilla total loot counter hidden by setting alpha to 0.");
        }
        else
        {
            Loggers.Warning("Image GameObject not found!");
        }
    }

    private static IEnumerator ShipLootCoroutine()
    {
        if (_totalCounter == null) yield break;

        _isCoroutineRunning = true;
        _totalCounter.SetActive(true);

        var cg = _totalCounter.GetComponent<CanvasGroup>() ?? _totalCounter.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Fade in
        for (float t = 0f; t < FadeDuration; t += Time.deltaTime)
        {
            cg.alpha = FadeCurve.Evaluate(t / FadeDuration);
            yield return null;
        }
        cg.alpha = 1f;

        // Display
        yield return new WaitForSeconds(_displayTimeLeft);

        // Fade out
        for (float t = 0f; t < FadeDuration; t += Time.deltaTime)
        {
            cg.alpha = FadeCurve.Evaluate(1f - t / FadeDuration);
            yield return null;
        }
        cg.alpha = 0f;

        _totalCounter.SetActive(false);
        _isCoroutineRunning = false;
    }

    private static int CalculateLootValue()
    {
        if (Ship == null) return 0;

        var loot = Ship.GetComponentsInChildren<GrabbableObject>()
            .Where(obj => obj.itemProperties.isScrap && obj is not RagdollGrabbableObject)
            .ToList();

        Loggers.Debug("Calculating total ship scrap value.");
        loot.ForEach(scrap => Loggers.Debug($"{scrap.name} - ${scrap.scrapValue}"));

        return loot.Sum(scrap => scrap.scrapValue);
    }
    private static void EnsureVanillaTotalLoot()
    {
        if (_vanillaTotalLoot == null)
            _vanillaTotalLoot = GameObject.Find("UI/Canvas/ObjectScanner/GlobalScanInfo/AnimContainer/Image");

        if (_vanillaTotalLoot != null)
        {
            _textMeshVanillaTotalNum = _vanillaTotalLoot
                .GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(t => t.gameObject.name == "TotalNum");

            if (_textMeshVanillaTotalNum == null)
                Loggers.Warning("TotalNum TMP_Text not found!");
        }
        else
        {
            Loggers.Warning("Vanilla total loot container not found!");
        }
    }

    private static void CopyValueCounter()
    {
        GameObject valueCounter = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
        if (!valueCounter)
        {
            Loggers.Error("Failed to find ValueCounter object to copy!");
            return;
        }

        _totalCounter = Object.Instantiate(valueCounter, valueCounter.transform.parent, false);

        var cg = _totalCounter.GetComponent<CanvasGroup>() ?? _totalCounter.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        Vector3 pos = _totalCounter.transform.localPosition;
        _totalCounter.transform.localPosition = new Vector3(pos.x + 50f, pos.y - 50f, pos.z);

        _textMesh = _totalCounter.GetComponentInChildren<TextMeshProUGUI>();
        if (_textMesh == null)
            Loggers.Warning("TMP_Text not found on copied value counter!");
    }
}*/