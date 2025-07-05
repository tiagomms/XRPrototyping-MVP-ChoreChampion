using UnityEngine;
using UnityEngine.UI;

public class SwapFrames : MonoBehaviour
{
    public Sprite frameA;
    public Sprite frameB;
    public float swapSpeed = 0.2f;

    private Image image;
    private float timer;
    private bool toggle;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= swapSpeed)
        {
            toggle = !toggle;
            image.sprite = toggle ? frameA : frameB;
            timer = 0f;
        }
    }
}
