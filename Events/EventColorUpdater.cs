using UnityEngine;

namespace LethalHUD.Events;

public class EventColorUpdater : MonoBehaviour
{
    private readonly float _interval = 5f;
    private float _timer = 0f;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        EventColorManager.Load();
        EventColorManager.Update();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            EventColorManager.Update();
        }
    }
}