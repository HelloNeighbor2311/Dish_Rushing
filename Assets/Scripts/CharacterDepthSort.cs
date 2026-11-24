using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup), typeof(CircleCollider2D))]
public sealed class CharacterDepthSort : MonoBehaviour
{
    [SerializeField] private SpriteRenderer characterSprite;
    private SortingGroup group;
    private Vector3 localFeet;

    private void Awake()
    {
        group = GetComponent<SortingGroup>();
        group.sortingLayerID = characterSprite.sortingLayerID;
        var collider = GetComponent<CircleCollider2D>();
        // Keep the sort point with the seated sprite when it moves away from the body.
        localFeet = characterSprite.transform.InverseTransformPoint(transform.TransformPoint(collider.offset));
    }

    private void LateUpdate()
    {
        float y = characterSprite.transform.TransformPoint(localFeet).y;
        group.sortingOrder = Mathf.Clamp(Mathf.RoundToInt(-y * 100f), short.MinValue, short.MaxValue);
    }
}
