using UnityEngine;

public class TimedActivateDestroy : MonoBehaviour
{
    void Start()
    {
        // start disabled
        gameObject.SetActive(false);
        // schedule activation in 10s
        Invoke(nameof(ActivateAndScheduleDestroy), 10f);
    }

    void ActivateAndScheduleDestroy()
    {
        gameObject.SetActive(true);
        // destroy 5s after activation
        Destroy(gameObject, 5f);
    }
}