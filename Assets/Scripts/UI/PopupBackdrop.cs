using System.Collections;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupBackdrop : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (GameplayVisibility.Instance != null)
            {
                GameplayVisibility.Instance.OnGameplayActiveChanged += HandleGameplayActiveChanged;
            }
            else
            {
                Debug.LogError("PopupBackdrop: GameplayVisibility.Instance is missing from the scene.");
            }
        }

        private void OnDestroy()
        {
            if (GameplayVisibility.Instance != null)
            {
                GameplayVisibility.Instance.OnGameplayActiveChanged -= HandleGameplayActiveChanged;
            }
        }

        private void HandleGameplayActiveChanged(bool isActive)
        {
            FadeTo(isActive ? 0f : 1f);
        }

        private void FadeTo(float target)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeRoutine(target));
        }

        private IEnumerator FadeRoutine(float target)
        {
            float from = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, target, elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = target;
            _fadeRoutine = null;
        }
    }
}
