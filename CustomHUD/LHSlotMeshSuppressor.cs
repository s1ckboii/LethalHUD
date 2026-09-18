using System;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD;

[DisallowMultipleComponent]
internal sealed class LHSlotMeshSuppressor : BaseMeshEffect
{
    [NonSerialized] private bool _suppressed;

    internal void SetSuppressed(bool value)
    {
        if (_suppressed == value) return;
        _suppressed = value;
        if (graphic != null) graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vertices)
    {
        if (IsActive() && _suppressed) vertices.Clear();
    }
}
