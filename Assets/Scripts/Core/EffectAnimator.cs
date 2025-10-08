// EffectAnimator.cs
using System.Collections;
using UnityEngine;

public class EffectAnimator : MonoBehaviour
{
    // EffectAnimator.cs（ポイントだけ）
    public IEnumerator Bounce(SymbolView view, float dur = 0.15f, float scale = 1.2f)
    {
        if (!view) yield break;
        var rt = view.VisualRoot;        // ★ ここを VisualRoot に
        if (!rt) yield break;
        Vector3 s0 = rt.localScale, s1 = Vector3.one * scale;
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / dur; rt.localScale = Vector3.Lerp(s0, s1, EaseOut(t)); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / dur; rt.localScale = Vector3.Lerp(s1, s0, EaseIn(t)); yield return null; }
    }

    public void Shatter(SymbolView view, float dur = 0.18f)
    {
        if (!view) return;
        var rt = view.VisualRoot;                 // ★ VisualRoot を揺らす
        if (!rt) return;
        var cg = rt.GetComponent<CanvasGroup>();  // ★ VisualRoot 側に付ける
        if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
        view.StartCoroutine(ShakeAndFade(rt, cg, dur));
    }

    IEnumerator ShakeAndFade(RectTransform rt, CanvasGroup cg, float dur)
    {
        Vector2 basePos = rt.anchoredPosition;  // VisualRoot の相対位置を基準に揺らす
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = (1f - t);
            rt.anchoredPosition = basePos + Random.insideUnitCircle * (6f * k);
            cg.alpha = k;
            yield return null;
        }
        rt.anchoredPosition = basePos;  // ★ 元に戻す
        cg.alpha = 1f;                  // Bind/Clearでも1に戻すが、ここでも戻しておくと安心
    }

    float EaseOut(float x) => 1f - (1f - x) * (1f - x);
    float EaseIn(float x) => x * x;
}
