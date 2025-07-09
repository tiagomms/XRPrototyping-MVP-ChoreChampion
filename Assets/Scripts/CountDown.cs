using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountDown : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float countdownTime = 5f;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float spawningDistance = 1.5f;
    [SerializeField] private AudioSource initialSound;
    [SerializeField] private AudioSource countdownSound;
    [SerializeField] private AudioSource finalSound;


    private float _currentTime;
    private bool _isCountingDown;

    public UnityEvent onCountDownFinished;

    private void Start()
    {
        _currentTime = countdownTime;
        canvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isCountingDown) return;
        _currentTime -= Time.deltaTime;
        if (countdownSound && _currentTime <= countdownTime - Mathf.Floor(countdownTime - _currentTime))
        {
            countdownSound.Play();
        }

        countdownText.text = Mathf.CeilToInt(_currentTime).ToString();
        if (_currentTime <= 0f)
        {
            OnCountdownComplete();
        }
    }

    public void StartCountDown()
    {
        _isCountingDown = true;
        _currentTime = countdownTime;
        canvas.gameObject.SetActive(true);
        canvas.transform.position = spawnPoint.transform.position + new Vector3(0, 0, spawningDistance);

        if (initialSound)
        {
            initialSound.Play();
        }
    }

    private void OnCountdownComplete()
    {
        if (finalSound)
        {
            finalSound.Play();
        }

        _isCountingDown = false;
        _currentTime = 0f;
        onCountDownFinished.Invoke();
        canvas.gameObject.SetActive(false);
    }
}