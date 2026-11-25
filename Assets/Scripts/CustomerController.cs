using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class CustomerController : MonoBehaviour
{
    public enum GuestState { Idle, Walk, Sit }

    [SerializeField] private Animator animator;
    [SerializeField, Min(0.1f)] private float walkSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float idleChance = 0.35f;
    [SerializeField] private Vector2 idleSeconds = new Vector2(1f, 3f);
    [SerializeField] private Vector2 seatedSeconds = new Vector2(8f, 18f);
    [SerializeField, Range(0f, 1f)] private float stayUntilEndChance = 0.25f;

    private static readonly WaitForFixedUpdate PhysicsStep = new WaitForFixedUpdate();
    private Rigidbody2D body;
    private CircleCollider2D feet;
    private Vector3 standingSpriteOffset;
    private CustomerSeat reservedSeat;
    private CustomerPathfinder navigation;
    private SimplePool2.Pool visitPool;
    private bool blocked;
    public GuestState State { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        feet = GetComponent<CircleCollider2D>();
        navigation = new CustomerPathfinder(feet);
        standingSpriteOffset = animator.transform.localPosition;
    }

    public bool BeginVisit(Vector2 spawn, CustomerSeat seat, SimplePool2.Pool pool = null)
    {
        if (reservedSeat != null || seat == null || !seat.IsAvailable) return false;

        var route = new List<Vector2> { spawn };
        foreach (Transform point in seat.ApproachPoints)
        {
            if (point != null && !navigation.AppendPath(route[route.Count - 1], point.position, route))
                return false;
        }

        Vector2 sitPosition = seat.transform.position;
        Vector2 besideChair = route[route.Count - 1];
        if (navigation.CanStand(sitPosition))
        {
            if (!navigation.AppendPath(besideChair, sitPosition, route)) return false;
        }
        else if (route.Count == 1 || Vector2.Distance(besideChair, sitPosition) > seat.BoardingDistance)
        {
            return false;
        }

        if (!seat.TryReserve(this)) return false;
        reservedSeat = seat;
        visitPool = pool;
        blocked = false;
        body.position = spawn;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        animator.SetFloat("LastInputX", 0f);
        animator.SetFloat("LastInputY", -1f);
        SetState(GuestState.Idle);
        StartCoroutine(Visit(route, sitPosition));
        return true;
    }

    private IEnumerator Visit(List<Vector2> route, Vector2 sitPosition)
    {
        if (Roll(idleChance))
        {
            yield return WalkTo(Vector2.Lerp(route[0], route[1], 0.5f));
            if (!blocked)
            {
                SetState(GuestState.Idle);
                yield return new WaitForSeconds(Duration(idleSeconds));
            }
        }

        for (int i = 1; i < route.Count && !blocked; i++)
            yield return WalkTo(route[i]);

        if (!blocked)
        {
            // The chair itself is solid. Leave physics at the boarding point and
            // align only the seated sprite to the chair, then restore it to stand up.
            Transform sprite = animator.transform;
            feet.enabled = false;
            sprite.position += (Vector3)(sitPosition - body.position);
            SetState(GuestState.Sit);
            if (Roll(stayUntilEndChance)) yield break;
            yield return new WaitForSeconds(Duration(seatedSeconds));
            while (!navigation.CanStand(body.position)) yield return new WaitForSeconds(1f);
            sprite.localPosition = standingSpriteOffset;
            feet.enabled = true;
        }

        reservedSeat.Release(this);
        reservedSeat = null;
        for (int i = route.Count - 2; i >= 0 && !blocked; i--)
            yield return WalkTo(route[i]);

        // If the layout changed after reserving the seat, get home from the
        // current position. Keep waiting if a newly closed aisle blocks the exit.
        while (blocked)
        {
            SetState(GuestState.Idle);
            var detour = new List<Vector2>();
            if (navigation.AppendPath(body.position, route[0], detour))
            {
                blocked = false;
                foreach (var point in detour)
                {
                    yield return WalkTo(point);
                    if (blocked) break;
                }
            }
            if (blocked) yield return new WaitForSeconds(1f);
        }
        if (!TryReturnToPool()) Destroy(gameObject); // Standalone visitor.
    }

    internal bool TryReturnToPool()
    {
        if (visitPool == null) return false;
        visitPool.Despawn(gameObject);
        return true;
    }

    private IEnumerator WalkTo(Vector2 destination)
    {
        yield return WalkSegment(destination);
        if (!blocked) yield break;

        var detour = new List<Vector2>();
        if (!navigation.AppendPath(body.position, destination, detour)) yield break;
        blocked = false;
        foreach (var point in detour)
        {
            yield return WalkSegment(point);
            if (blocked) yield break;
        }
    }

    private IEnumerator WalkSegment(Vector2 destination)
    {
        Vector2 delta = destination - body.position;
        if (delta.sqrMagnitude < 0.000001f) yield break;

        Vector2 facing = delta.normalized;
        animator.SetFloat("InputX", facing.x);
        animator.SetFloat("InputY", facing.y);
        animator.SetFloat("LastInputX", facing.x);
        animator.SetFloat("LastInputY", facing.y);
        if (State != GuestState.Walk) SetState(GuestState.Walk);
        while ((destination - body.position).sqrMagnitude >= 0.000001f)
        {
            Vector2 next = Vector2.MoveTowards(body.position, destination, walkSpeed * Time.fixedDeltaTime);
            if (!navigation.CanWalk(body.position, next))
            {
                blocked = true;
                yield break;
            }
            body.MovePosition(next);
            yield return PhysicsStep;
        }
    }

    private void SetState(GuestState state)
    {
        State = state;
        animator.SetBool("isWalking", state == GuestState.Walk);
        string animationState = state == GuestState.Sit && reservedSeat.Facing == CustomerSeat.SeatFacing.Left
            ? "Sit_Left"
            : state.ToString();
        animator.Play("Base Layer." + animationState, 0, 0f);
    }

    private static bool Roll(float chance) =>
        chance >= 1f || (chance > 0f && Random.value < chance);

    private static float Duration(Vector2 range) =>
        Random.Range(Mathf.Max(0f, range.x), Mathf.Max(0f, range.x, range.y));

    private void OnDisable()
    {
        StopAllCoroutines();
        if (reservedSeat != null) reservedSeat.Release(this);
        reservedSeat = null;
        if (State == GuestState.Sit)
        {
            animator.transform.localPosition = standingSpriteOffset;
            feet.enabled = true;
        }
        visitPool = null;
        State = GuestState.Idle;
    }
}
