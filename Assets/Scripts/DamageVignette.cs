using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [SerializeField] private Material vignetteMaterial;
    [SerializeField] private float peakAlpha = 0.85f;
    [SerializeField] private float fadeDuration = 0.6f;

    private Image vignette;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        var vignetteObject = new GameObject(
            "DamageVignette",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        var rectTransform = vignetteObject.GetComponent<RectTransform>();
        rectTransform.SetParent(transform, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.SetAsLastSibling();

        vignette = vignetteObject.GetComponent<Image>();
        vignette.material = vignetteMaterial;
        vignette.raycastTarget = false;
        vignette.color = Color.clear;
    }

    private void OnEnable()
    {
        Player.Damaged += Play;
    }

    private void OnDisable()
    {
        Player.Damaged -= Play;
    }

    private void Play()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        var color = Color.white;
        color.a = peakAlpha;
        vignette.color = color;

        var elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(peakAlpha, 0f, elapsed / fadeDuration);
            vignette.color = color;
            yield return null;
        }

        color.a = 0f;
        vignette.color = color;
        fadeCoroutine = null;
    }
}
