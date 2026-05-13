using UnityEngine;
using System.Collections.Generic;

public class EnemyStateController : MonoBehaviour
{
    [System.Serializable]
    public class StateEntry
    {
        public string triggerTag;
        public GameObject stateObject;
    }

    [Header("Default State")]
    public GameObject defaultState;

    [Header("Transformation States")]
    public List<StateEntry> states = new List<StateEntry>();

    [Header("Settings")]
    public bool transformOnlyOnce = true;

    private bool hasTransformed = false;

    void Start()
    {
        DisableAllStates();

        if (defaultState != null)
        {
            defaultState.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transformOnlyOnce && hasTransformed)
            return;

        foreach (StateEntry state in states)
        {
            if (other.CompareTag(state.triggerTag))
            {
                ActivateState(state.stateObject);

                hasTransformed = true;
                return;
            }
        }
    }

    void ActivateState(GameObject targetState)
    {
        DisableAllStates();

        if (targetState != null)
        {
            targetState.SetActive(true);
        }
    }

    void DisableAllStates()
    {
        if (defaultState != null)
        {
            defaultState.SetActive(false);
        }

        foreach (StateEntry state in states)
        {
            if (state.stateObject != null)
            {
                state.stateObject.SetActive(false);
            }
        }
    }
}