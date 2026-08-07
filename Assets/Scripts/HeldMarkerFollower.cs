using UnityEngine;

namespace PokerDice
{
    public class HeldMarkerFollower : MonoBehaviour
    {
        [SerializeField] private Transform dieTransform;

        private void OnValidate()
        {
            if (dieTransform == null)
            {
#if UNITY_EDITOR
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
                {
                    return;
                }
#endif
                Debug.LogWarning($"HeldMarkerFollower on {name}: dieTransform is not assigned.");
            }
        }

        private void LateUpdate()
        {
            if (dieTransform == null)
            {
                return;
            }

            Vector3 targetPosition = dieTransform.position;
            Vector3 currentPosition = transform.position;
            transform.position = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
        }
    }
}
