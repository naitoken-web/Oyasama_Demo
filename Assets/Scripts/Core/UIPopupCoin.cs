// UIPopupCoin.cs
using UnityEngine;
using TMPro;

public class UIPopupCoin : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField, Min(0f)] float duration = 0.75f;
    [SerializeField] Vector2 move = new Vector2(0, 40); // è„ï˚å¸Ç÷
    [SerializeField, Range(0f, 1f)] float startAlpha = 0f;
    [SerializeField, Range(0f, 1f)] float peakAlpha = 1f;

    RectTransform rt; CanvasGroup cg;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = gameObject.GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void Play(int amount)
    {
        label.text = amount >= 0 ? $"+{amount}" : amount.ToString();
        StopAllCoroutines();
        StartCoroutine(Anim());
    }

    System.Collections.IEnumerator Anim()
    {
        cg.alpha = startAlpha;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + move;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float a = (t < 0.3f) ? Mathf.Lerp(startAlpha, peakAlpha, t / 0.3f)
                                 : Mathf.Lerp(peakAlpha, 0f, (t - 0.3f) / 0.7f);
            cg.alpha = a;
            rt.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
