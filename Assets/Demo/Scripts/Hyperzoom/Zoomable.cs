using UnityEngine;

public class Zoomable : HyperzoomPointer
{
    #region Click

    /// <summary>
    /// Whenever a pointer (finger/mouse) selects this GameObject
    /// </summary>

    public override void PointerClicked()
    {
        // if we didn't drag and we're not zooming
        if (!didDrag && !didZoom /* && !didHold */)
        {
            // send the new target object
            ChangedTarget(this.gameObject);
        }
    }

    #endregion
}
