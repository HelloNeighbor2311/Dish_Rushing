using UnityEngine;

public sealed class CustomerSeat : MonoBehaviour
{
    public enum SeatFacing { Right, Left }

    [Tooltip("Direction the seated guest looks toward the table.")]
    [SerializeField] private SeatFacing facing = SeatFacing.Right;
    [Tooltip("Walkable points in order. Place the last point on clear floor next to the chair.")]
    [SerializeField] private Transform[] approachPoints = new Transform[0];
    [Tooltip("Maximum distance from the final standing point to the seated sprite.")]
    [SerializeField, Min(0f)] private float boardingDistance = 1.3f;

    private CustomerController guest;
    public bool IsAvailable => isActiveAndEnabled && guest == null;
    public Transform[] ApproachPoints => approachPoints;
    public SeatFacing Facing => facing;
    public float BoardingDistance => boardingDistance;

    public bool TryReserve(CustomerController customer)
    {
        if (!IsAvailable || customer == null) return false;
        guest = customer;
        return true;
    }

    public void Release(CustomerController customer)
    {
        if (guest == customer) guest = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 next = transform.position;
        Gizmos.DrawWireSphere(next, 0.18f);
        Gizmos.DrawRay(next, (facing == SeatFacing.Left ? Vector3.left : Vector3.right) * 0.5f);
        for (int i = approachPoints.Length - 1; i >= 0; i--)
        {
            if (approachPoints[i] == null) continue;
            Vector3 point = approachPoints[i].position;
            Gizmos.DrawLine(point, next);
            Gizmos.DrawWireSphere(point, 0.1f);
            next = point;
        }
    }
}
