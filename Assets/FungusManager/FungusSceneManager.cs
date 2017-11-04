using UnityEngine;

namespace Fungus
{
    public class FungusSceneManager : MonoBehaviour
    {
        public void BackgroundColorChanged(Color color)
        {
            Debug.Log("BackgroundColorChanged(" + color + ");");
        }

        public void ZoomInStarted(string targetName)
        {
            Debug.Log("ZoomInStarted(" + targetName + ");");
        }

        public void ZoomInFinished(string targetName)
        {
            Debug.Log("ZoomInFinished(" + targetName + ");");
        }

        public void ZoomOutStarted(string targetName)
        {
            Debug.Log("ZoomOutStarted(" + targetName + ");");
        }

        public void ZoomOutFinished(string targetName)
        {
            Debug.Log("ZoomOutFinished(" + targetName + ");");
        }

    }
}
