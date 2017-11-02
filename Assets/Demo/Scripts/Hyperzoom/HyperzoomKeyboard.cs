using UnityEngine;

public class HyperzoomKeyboard : HyperzoomInteraction
{
    #region Controller Polling

    void Update()
    {
        // check keyboard inpts
        UpdateKeyboard();

    } // Update()


    void UpdateKeyboard()
    {
        // keyboard left arrow
        if (Input.GetKeyDown(KeyCode.LeftArrow)) SelectedPreviousTarget();
        // keyboard right arrow
        if (Input.GetKeyDown(KeyCode.RightArrow)) SelectedNextTarget();
        // keyboard up arrow
        if (Input.GetKeyDown(KeyCode.UpArrow)) ZoomedIn();
        // keyboard down arrow
        if (Input.GetKeyDown(KeyCode.DownArrow)) ZoomedOut();
    }

    #endregion


}
