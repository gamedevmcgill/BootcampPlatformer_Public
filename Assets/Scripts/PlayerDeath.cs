using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] float killZoneHeight = -10f;
    [SerializeField, Min(0)] int restartDelay = 3;
    [SerializeField] TextMeshProUGUI displayText;

    bool _isDead = false;

    void Update()
    {
        if (transform.position.y < killZoneHeight && !_isDead)
        {
            StartCoroutine(startDeathTimer());
            _isDead = true;
        }
    }

    IEnumerator startDeathTimer()
    {
        displayText.gameObject.SetActive(true);
        for (int i = restartDelay; i > 0; i--)
        {
            displayText.text = $"You Died\nRestart in {i}";
            yield return new WaitForSeconds(1f);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
