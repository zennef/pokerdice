using System.Collections;
using UnityEngine;

namespace PokerDice
{
    public class DiceBoxReskin : MonoBehaviour
    {
        [SerializeField] private Renderer[] boxRenderers;
        [SerializeField] private Color playerColor = new Color(0.2f, 0.5f, 0.9f);
        [SerializeField] private Color botColor = new Color(0.85f, 0.25f, 0.2f);
        [SerializeField] private string colorPropertyName = "_BaseColor";
        [SerializeField] private float transitionDuration = 0.3f;

        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _transitionRoutine;
        private Color _currentColor;
        private bool _subscribed;
        private bool _hasStarted;

        private void Awake()
        {
            if (boxRenderers == null || boxRenderers.Length == 0)
            {
                boxRenderers = GetComponentsInChildren<Renderer>();
            }
        }

        private void Start()
        {
            // All Awake() calls in the scene are guaranteed to have run by Start(), so
            // TurnAuthority.Instance is reliably set here even if OnEnable() raced ahead of it.
            // A null Instance at this point is a genuine problem, not just an ordering race.
            _hasStarted = true;
            TrySubscribe();

            if (TurnAuthority.Instance == null)
            {
                return;
            }

            SetOwnerColor(TurnAuthority.Instance.CurrentOwner, true);
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (!_subscribed || TurnAuthority.Instance == null)
            {
                _subscribed = false;
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
            _subscribed = false;
        }

        private void TrySubscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (TurnAuthority.Instance == null)
            {
                // Before Start(), a null Instance just means our OnEnable() ran ahead of
                // TurnAuthority's Awake() — Start() will retry once all Awakes have run.
                // Only warn if that retry itself finds Instance still missing.
                if (_hasStarted)
                {
                    Debug.LogWarning("DiceBoxReskin: TurnAuthority.Instance is null — skipping subscription.");
                }

                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
            _subscribed = true;
        }

        private void HandleTurnOwnerChanged(TurnOwner newOwner)
        {
            SetOwnerColor(newOwner, false);
        }

        private void SetOwnerColor(TurnOwner owner, bool instant)
        {
            Color targetColor = owner == TurnOwner.Player ? playerColor : botColor;

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            if (instant)
            {
                ApplyColor(targetColor);
            }
            else
            {
                _transitionRoutine = StartCoroutine(TransitionColor(targetColor));
            }
        }

        private IEnumerator TransitionColor(Color targetColor)
        {
            Color startColor = _currentColor;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                ApplyColor(Color.Lerp(startColor, targetColor, elapsed / transitionDuration));
                yield return null;
            }

            ApplyColor(targetColor);
            _transitionRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            _currentColor = color;

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            for (int i = 0; i < boxRenderers.Length; i++)
            {
                var renderer = boxRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(colorPropertyName, color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
