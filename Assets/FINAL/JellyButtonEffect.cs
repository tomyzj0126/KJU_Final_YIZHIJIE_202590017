using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class JellyButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("悬停放大")]
    public float hoverScale = 1.08f;

    [Header("按下果冻压缩")]
    public float pressScaleX = 1.12f;
    public float pressScaleY = 0.88f;

    [Header("松开回弹")]
    public float bounceScaleX = 0.95f;
    public float bounceScaleY = 1.08f;

    [Header("动画速度")]
    public float animationSpeed = 12f;

    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    private bool isHovering = false;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        Vector3 targetScale = originalScale * hoverScale;
        PlayScaleAnimation(targetScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        PlayScaleAnimation(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 targetScale = new Vector3(
            originalScale.x * pressScaleX,
            originalScale.y * pressScaleY,
            originalScale.z
        );

        PlayScaleAnimation(targetScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(JellyBounce());
    }

    IEnumerator JellyBounce()
    {
        Vector3 bounceScale = new Vector3(
            originalScale.x * bounceScaleX,
            originalScale.y * bounceScaleY,
            originalScale.z
        );

        Vector3 finalScale = isHovering ? originalScale * hoverScale : originalScale;

        yield return ScaleTo(bounceScale);
        yield return ScaleTo(finalScale);
    }

    void PlayScaleAnimation(Vector3 targetScale)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.unscaledDeltaTime * animationSpeed
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }
}