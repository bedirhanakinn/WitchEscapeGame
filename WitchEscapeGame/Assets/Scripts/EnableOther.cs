using UnityEngine;

public class EnableOther : MonoBehaviour
{
    [Header("Objects to Toggle")]
    public GameObject[] objectsToToggle;

    private void OnEnable()
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void OnDisable()
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}