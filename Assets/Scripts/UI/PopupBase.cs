using System.Collections;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, true, OnShown));
        }

        public virtual void Hide()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(1f, 0f, false, OnHidden));
        }

        protected virtual void OnShown() { }

        protected virtual void OnHidden() { }

        private IEnumerator FadeRoutine(float from, float to, bool interactable, System.Action onComplete)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = interactable;
            _canvasGroup.alpha = from;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = to;
            _fadeRoutine = null;
            onComplete?.Invoke();
        }
    }
}
