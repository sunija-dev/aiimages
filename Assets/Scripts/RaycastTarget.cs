using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// from: https://answers.unity.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html
public class RaycastTarget : Graphic
{
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }

    /// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        return;
    }
}
