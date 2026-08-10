using UnityEngine;

namespace PokerDice
{
    public class DiceBoxReskin : MonoBehaviour
    {
        [SerializeField] private Renderer[] boxRenderers;
        [SerializeField] private Color playerColor = new Color(0.2f, 0.5f, 0.9f);
        [SerializeField] private Color botColor = new Color(0.85f, 0.25f, 0.2f);

        private MaterialPropertyBlock _propertyBlock;
        private bool _subscribed;
        private bool _hasStarted;

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

            ApplyColor(TurnAuthority.Instance.CurrentOwner == TurnOwner.Player ? playerColor : botColor);
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
            ApplyColor(newOwner == TurnOwner.Player ? playerColor : botColor);
        }

        private void ApplyColor(Color color)
        {
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
                _propertyBlock.SetColor("_Color", color);
                _propertyBlock.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
