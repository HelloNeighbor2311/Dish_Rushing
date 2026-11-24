using System.Collections.Generic;
using UnityEngine;

// Routes a circular footprint around the same static 2D colliders used by the player.
internal sealed class CustomerPathfinder
{
    private const float GridStep = 0.4f;
    private const int SearchLimit = 4096;
    private static readonly Vector2Int[] Neighbours =
        { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

    private readonly CircleCollider2D feet;
    private readonly ContactFilter2D solids = new ContactFilter2D { useTriggers = false };
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();

    public CustomerPathfinder(CircleCollider2D feet) => this.feet = feet;

    private Vector2 Offset => feet.transform.TransformVector(feet.offset);
    private float Radius => feet.radius * Mathf.Max(Mathf.Abs(feet.transform.lossyScale.x),
        Mathf.Abs(feet.transform.lossyScale.y)) + 0.02f;

    private static bool IsObstacle(Collider2D collider) =>
        collider.attachedRigidbody == null || collider.attachedRigidbody.bodyType == RigidbodyType2D.Static;

    public bool CanStand(Vector2 point)
    {
        Physics2D.OverlapCircle(point + Offset, Radius, solids, overlaps);
        foreach (var collider in overlaps)
            if (IsObstacle(collider)) return false;
        return true;
    }

    public bool CanWalk(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        if (delta.sqrMagnitude < 0.000001f) return CanStand(to);
        Physics2D.CircleCast(from + Offset, Radius, delta.normalized, solids, hits, delta.magnitude);
        foreach (var hit in hits)
            if (IsObstacle(hit.collider)) return false;
        return true;
    }

    // Append only after finding a complete route, so an unreachable seat can be skipped.
    public bool AppendPath(Vector2 from, Vector2 to, List<Vector2> path)
    {
        if (!CanStand(from) || !CanStand(to)) return false;
        if (CanWalk(from, to))
        {
            path.Add(to);
            return true;
        }

        var frontier = new SortedSet<(float priority, int x, int y)>();
        var cost = new Dictionary<Vector2Int, int> { [Vector2Int.zero] = 0 };
        var previous = new Dictionary<Vector2Int, Vector2Int>();
        frontier.Add((0f, 0, 0));
        for (int visited = 0; frontier.Count > 0 && visited < SearchLimit; visited++)
        {
            var best = frontier.Min;
            frontier.Remove(best);
            var cell = new Vector2Int(best.x, best.y);
            Vector2 position = from + (Vector2)cell * GridStep;
            if (CanWalk(position, to))
            {
                var turns = new List<Vector2> { to };
                while (cell != Vector2Int.zero)
                {
                    turns.Add(from + (Vector2)cell * GridStep);
                    cell = previous[cell];
                }
                // Omit grid corners when the full footprint fits along the shortcut.
                Vector2 anchor = from;
                for (int i = turns.Count - 1; i >= 0; i--)
                {
                    if (i > 0 && CanWalk(anchor, turns[i - 1])) continue;
                    path.Add(turns[i]);
                    anchor = turns[i];
                }
                return true;
            }

            int nextCost = cost[cell] + 1;
            foreach (var direction in Neighbours)
            {
                Vector2Int next = cell + direction;
                bool known = cost.TryGetValue(next, out int oldCost);
                if (known && oldCost <= nextCost) continue;
                Vector2 nextPosition = from + (Vector2)next * GridStep;
                if (!CanWalk(position, nextPosition)) continue;
                float estimate = Vector2.Distance(nextPosition, to) / GridStep;
                if (known) frontier.Remove((oldCost + estimate, next.x, next.y));
                previous[next] = cell;
                cost[next] = nextCost;
                frontier.Add((nextCost + estimate, next.x, next.y));
            }
        }
        return false;
    }
}
