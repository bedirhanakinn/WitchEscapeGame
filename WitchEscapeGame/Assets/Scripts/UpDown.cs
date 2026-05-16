using UnityEngine;

public class UpDown : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveRange = 1f;     // How far up/down it moves
    public float moveSpeed = 2f;     // How fast it moves

    [Header("Options")]
    public bool useLocalPosition = true;

    private Vector3 startPos;

    void Start()
    {
        startPos = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveRange;

        Vector3 newPos = startPos + Vector3.up * offset;

        if (useLocalPosition)
            transform.localPosition = newPos;
        else
            transform.position = newPos;
    }
}