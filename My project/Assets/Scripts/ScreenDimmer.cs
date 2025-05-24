using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ARPetGame
{
    public class ScreenDimmer : MonoBehaviour
    {
        [Header("Dimming Settings")]
        public Image dimOverlay;
        public Color dimColor = new Color(0, 0, 0, 0.7f);
        public float fadeDuration = 1f;
        
        [Header("UI Protection")]
        public Canvas uiCanvas;
        public int originalSortingOrder = 0;
        public int dimmedSortingOrder = 10;
        
        private bool isDimmed = false;
        private Coroutine currentFadeCoroutine;
        
        public bool IsDimmed => isDimmed;
        
        private void Start()
        {
            SetupDimOverlay();
            
            if (uiCanvas != null)
            {
                originalSortingOrder = uiCanvas.sortingOrder;
            }
        }
        
        private void SetupDimOverlay()
        {
            if (dimOverlay == null)
            {
                GameObject dimObject = new GameObject("ScreenDimOverlay");
                dimObject.transform.SetParent(transform);
                
                RectTransform rectTransform = dimObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                dimOverlay = dimObject.AddComponent<Image>();
                dimOverlay.color = new Color(dimColor.r, dimColor.g, dimColor.b, 0f);
                dimOverlay.raycastTarget = false;
                
                Canvas dimCanvas = dimObject.AddComponent<Canvas>();
                dimCanvas.overrideSorting = true;
                dimCanvas.sortingOrder = 1;
            }
            
            dimOverlay.gameObject.SetActive(false);
        }
        
        public void DimScreen()
        {
            if (isDimmed) return;
            
            isDimmed = true;
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeToColor(dimColor, fadeDuration));
            
            if (uiCanvas != null)
            {
                uiCanvas.sortingOrder = dimmedSortingOrder;
            }
            
            Debug.Log("Screen dimmed");
        }
        
        public void UndimScreen()
        {
            if (!isDimmed) return;
            
            isDimmed = false;
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            Color clearColor = new Color(dimColor.r, dimColor.g, dimColor.b, 0f);
            currentFadeCoroutine = StartCoroutine(FadeToColor(clearColor, fadeDuration));
            
            if (uiCanvas != null)
            {
                uiCanvas.sortingOrder = originalSortingOrder;
            }
            
            Debug.Log("Screen undimmed");
        }
        
        private IEnumerator FadeToColor(Color targetColor, float duration)
        {
            if (dimOverlay == null) yield break;
            
            dimOverlay.gameObject.SetActive(true);
            
            Color startColor = dimOverlay.color;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                dimOverlay.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            dimOverlay.color = targetColor;
            
            if (targetColor.a == 0f)
            {
                dimOverlay.gameObject.SetActive(false);
            }
            
            currentFadeCoroutine = null;
        }
        
        public void SetDimColor(Color color)
        {
            dimColor = color;
        }
        
        public void SetFadeDuration(float duration)
        {
            fadeDuration = Mathf.Max(0.1f, duration);
        }
    }
}