using UnityEngine;
using TMPro;

public static class BibNumberGenerator
{
    public static Texture2D Generate(Texture2D baseTexture, int number, TMP_FontAsset font, Color textColor, int fontSize, Vector2 position)
    {
        // 1. Clone base bib
        Texture2D tex = Object.Instantiate(baseTexture);

        // 2. Create RenderTexture for drawing
        RenderTexture rt = new RenderTexture(baseTexture.width, baseTexture.height, 0);
        RenderTexture.active = rt;

        // Draw base bib first
        Graphics.Blit(baseTexture, rt);

        // 3. Create TMP object to render the number
        var tempGO = new GameObject("BibTMP");
        var tmp = tempGO.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.text = number.ToString();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(fontSize * 2, fontSize * 2);
        tmp.rectTransform.anchoredPosition = position; // position in pixels

        // Force render TMP
        tmp.ForceMeshUpdate();

        // Draw TMP into RT
        CanvasRenderer cr = tempGO.AddComponent<CanvasRenderer>();
        cr.SetMaterial(tmp.fontMaterial, null);
        cr.SetMesh(tmp.mesh);

        // 4. Copy RT back to Texture2D
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        rt.Release();
        Object.Destroy(tempGO);

        return tex;
    }
}
