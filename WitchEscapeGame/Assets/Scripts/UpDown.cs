using UnityEngine;

public class UpDown : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpHeight = 2f;     // How high it moves upward
    public float upSpeed = 4f;        // Speed going up
    public float downSpeed = 7f;      // Speed coming down

    [Header("Options")]
    public bool useLocalPosition = true;

    private Vector3 startPos;
    private bool goingUp = true;

    void Start()
    {
        startPos = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;

        float targetY;

        if (goingUp)
        {
            targetY = startPos.y + jumpHeight;

            currentPos.y = Mathf.MoveTowards(
                currentPos.y,
                targetY,
                upSpeed * Time.deltaTime
            );

            if (Mathf.Abs(currentPos.y - targetY) < 0.01f)
            {
                goingUp = false;
            }
        }
        else
        {
            targetY = startPos.y;

            currentPos.y = Mathf.MoveTowards(
                currentPos.y,
                targetY,
                downSpeed * Time.deltaTime
            );

            if (Mathf.Abs(currentPos.y - targetY) < 0.01f)
            {
                goingUp = true;
            }
        }

        if (useLocalPosition)
            transform.localPosition = currentPos;
        else
            transform.position = currentPos;
    }
}