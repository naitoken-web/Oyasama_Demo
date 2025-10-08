// CoinPopupManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPopupManager : MonoBehaviour
{
    [SerializeField] UIPopupCoin popupPrefab;
    [SerializeField] float groupDelay = 0.3f;

    // BoardView から各セルの RectTransform（＝SymbolView）をもらう
    public IEnumerator ShowAscending(BoardView board, List<CellGain> gains)
    {
        if (gains == null || gains.Count == 0) yield break;

        // 1) 金額で昇順ソート＆グループ化
        gains.Sort((a, b) => a.amount.CompareTo(b.amount));
        int i = 0;
        while (i < gains.Count)
        {
            int amt = gains[i].amount;
            // 同額をまとめて
            int j = i;
            while (j < gains.Count && gains[j].amount == amt) j++;

            // 2) 同額分を同時にスポーン
            for (int k = i; k < j; k++)
            {
                var g = gains[k];
                var view = board.GetCellView(g.x, g.y);   // SymbolView
                if (view == null) continue;

                // セルの子として生成
                var rtParent = view.GetComponent<RectTransform>();
                var popup = Instantiate(popupPrefab, rtParent);
                var prt = popup.GetComponent<RectTransform>();
                prt.anchoredPosition = Vector2.zero; // 中央から
                popup.Play(g.amount);
            }

            // 3) 次グループまで少し待つ
            yield return new WaitForSeconds(groupDelay);
            i = j;
        }

        yield return new WaitForSeconds(0.2f);
    }
}
