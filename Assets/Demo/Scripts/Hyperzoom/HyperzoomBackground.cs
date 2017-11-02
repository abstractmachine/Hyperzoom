public class HyperzoomBackground : HyperzoomPointer
{
    #region Click

    /// <summary>
    /// Whenever a pointer (finger/mouse) selects the background
    /// </summary>

    public override void PointerClicked()
    {
        // if we didn't drag and we're not zooming
        if (!didDrag && !didZoom && !didHold)
        {
            // send null as the new target object
            ChangedTarget(null);
        }
    }

    #endregion
}
