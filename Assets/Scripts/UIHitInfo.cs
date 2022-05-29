using System.Collections;
using TMPro;
using UnityEngine;

public class UIHitInfo : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    private bool start;
    private bool change;
    private float startTime;
    private float speed = 0.5f;
    private Color passiveColor = new Color(1, 1, 1, 0);
    private Color activeColor = new Color(1, 1, 1, 1);

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        start = true;
    }

    private void Disappear()
    {
        if (change == true)
        {
            float distCovered = (Time.time - startTime) * speed;
            textMeshPro.color = Color.Lerp(textMeshPro.color, passiveColor, distCovered);

            if (textMeshPro.color == passiveColor)
            {
                change = false;
                textMeshPro.text = "";
                textMeshPro.color = activeColor;
                start = true;
            }
        }
    }

    private IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.1f);

        if (change == false)
        {
            startTime = Time.time;
            change = true;
        }
    }

    private void FixedUpdate()
    {
        if (textMeshPro.text != "")
        {
            if (start == true)
            {
                StartCoroutine(WaitTime());
                start = false;
            }
            Disappear();
        }
    }
}
