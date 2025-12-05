// LoadingDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingDisplay : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime0_5 = new(0.5f);
    public Slider progressBar;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI tipText;
    public TextMeshProUGUI loadingText;
    
    private readonly string[] tips = new string[]
    {
        "Tip: Explore every room for hidden treasures!",
        "Tip: Consider saving your gold for future powerful upgrades.",
        "Tip: The larger the enemy, the slower they move.",
        "Tip: Powers stack; collect multiple for devastating combos!",
        "Tip: Boss rooms contain guaranteed power rewards.",
    };
    
    void OnEnable()
    {
        if (tipText != null) tipText.text = tips[Random.Range(0, tips.Length)];
        
        if (loadingText != null) StartCoroutine(AnimateLoadingText());
    }
    
    public void SetProgress(float progress)
    {
        if (progressBar != null) progressBar.value = progress * 100f;
        
        if (percentageText != null) percentageText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
    }
    
    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dots = 0;
        
        while (gameObject.activeSelf)
        {
            loadingText.text = baseText + new string('.', dots);
            dots = (dots + 1) % 4;
            yield return _waitForSecondsRealtime0_5;
        }
    }
}