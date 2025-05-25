using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ModernButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;
    public float animationSpeed = 0.15f;
    
    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressColor = Color.white;
    
    private Button button;
    private Image targetImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isPressed = false;
    private bool isHovered = false;
    
    void Start()
    {
        button = GetComponent<Button>();
        targetImage = GetComponent<Image>();
        originalScale = transform.localScale;
        originalColor = targetImage.color;
        normalColor = originalColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovered = true;
        if (!isPressed)
        {
            StartCoroutine(AnimateScale(originalScale * hoverScale));
            StartCoroutine(AnimateColor(hoverColor));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isPressed)
        {
            StartCoroutine(AnimateScale(originalScale));
            StartCoroutine(AnimateColor(normalColor));
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        StartCoroutine(AnimateScale(originalScale * pressScale));
        StartCoroutine(AnimateColor(pressColor));
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        
        if (isHovered)
        {
            StartCoroutine(AnimateScale(originalScale * hoverScale));
            StartCoroutine(AnimateColor(hoverColor));
        }
        else
        {
            StartCoroutine(AnimateScale(originalScale));
            StartCoroutine(AnimateColor(normalColor));
        }
    }
    
    IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    IEnumerator AnimateColor(Color targetColor)
    {
        Color startColor = targetImage.color;
        float elapsed = 0f;
        
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            t = Mathf.SmoothStep(0f, 1f, t);
            
            targetImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        targetImage.color = targetColor;
    }
    
    public void SetColors(Color normal, Color hover, Color press)
    {
        normalColor = normal;
        hoverColor = hover;
        pressColor = press;
    }
    
    public void UpdateNormalColor(Color newNormalColor)
    {
        normalColor = newNormalColor;
        if (!isHovered && !isPressed)
        {
            targetImage.color = normalColor;
        }
    }
} 