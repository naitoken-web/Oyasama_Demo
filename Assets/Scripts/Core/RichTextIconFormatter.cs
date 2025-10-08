using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class RichTextIconFormatter
{
    // {coin} / :coin:
    static readonly Regex Token = new(@"\{([a-zA-Z0-9_]+)\}|:([a-zA-Z0-9_]+):",
                                      RegexOptions.Compiled);

    /// <summary>
    /// 連続アイコンは { ... } にまとめ、グループ先頭のみ { の内側に半角スペース×3 を入れる
    /// （ただし直前が空白や開き記号ならスペース無し）
    /// </summary>
    public static string Format(
        string src,
        IDictionary<string, string> alias = null,
        string size = "200%",
        int? voffset = 10,
        string hexColor = null
    )
    {
        if (string.IsNullOrEmpty(src)) return src;

        var sb = new StringBuilder();
        int lastIndex = 0;

        bool inGroup = false;      // いま { … } の中か
        bool lastWasIcon = false;  // 直前にアイコンを出したか

        foreach (Match m in Token.Matches(src))
        {
            // トークン手前の通常文字
            if (m.Index > lastIndex)
            {
                // 非アイコンに触れるので、開いていたら閉じる
                if (inGroup) { sb.Append('}'); inGroup = false; }
                sb.Append(src, lastIndex, m.Index - lastIndex);
                lastWasIcon = false;
            }

            // トークン名
            var key = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            if (alias != null && alias.TryGetValue(key, out var mapped)) key = mapped;

            // ★ グループをまだ開いていなければ開く（スペースは { の“内側”）
            if (!inGroup)
            {
                bool needInnerGap = !lastWasIcon && NeedsGapBefore(sb);
                sb.Append('{');
                if (needInnerGap) sb.Append("    ");
                inGroup = true;
            }
            else
            {
                sb.Append(' ');
            }

                // スプライト本体（name だけ）
                string sprite = $"<sprite name={key}>";
            // 外側タグで装飾
            if (!string.IsNullOrEmpty(hexColor)) sprite = $"<color={hexColor}>{sprite}</color>";
            if (voffset.HasValue) sprite = $"<voffset={voffset.Value}>{sprite}</voffset>";
            if (!string.IsNullOrEmpty(size)) sprite = $"<size={size}>{sprite}</size>";

            sb.Append(sprite);

            lastIndex = m.Index + m.Length;
            lastWasIcon = true; // 直前はアイコン
        }

        // 残りの通常文字
        if (lastIndex < src.Length)
        {
            if (inGroup) { sb.Append('}'); inGroup = false; }
            sb.Append(src, lastIndex, src.Length - lastIndex);
        }
        else
        {
            if (inGroup) { sb.Append('}'); inGroup = false; }
        }

        return sb.ToString();
    }

    // 直前が“可視の文字”なら true（空白系・開き記号の直後は false）
    static bool NeedsGapBefore(StringBuilder sb)
    {
        if (sb.Length == 0) return false; // 行頭は不要
        char c = sb[sb.Length - 1];
        if (char.IsWhiteSpace(c)) return false;
        switch (c)
        {
            case '{':
            case '｛':
                return false;
        }
        return true;
    }
}
