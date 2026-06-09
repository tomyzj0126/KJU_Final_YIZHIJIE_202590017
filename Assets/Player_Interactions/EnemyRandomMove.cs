using UnityEngine;

public class EnemyRandomMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float MoveSpeed = 2f;
    public float TurnSpeed = 5f;
    public float MoveRadius = 5f;
    public float ArriveDistance = 0.3f;
    public float GroundHeightOffset = 0f;

    [Header("Wait Settings")]
    public float MinWaitTime = 1f;
    public float MaxWaitTime = 3f;

    [Header("Obstacle Check")]
    public LayerMask ObstacleLayer;
    public float WallCheckDistance = 0.8f;
    public float AvoidanceRadius = 0.35f;
    public float SideCheckAngle = 35f;
    public int TargetPickAttempts = 12;

    [Header("Player Awareness")]
    public float ChaseRange = 12f;
    public float StopChaseDistance = 6f;
    public bool RequireLineOfSight = true;

    [Header("Stuck Check")]
    public float StuckCheckTime = 1f;
    public float StuckDistance = 0.08f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer;
    private bool isWaiting = false;
    private float stuckTimer;
    private Vector3 lastPosition;
    private Transform playerTarget;
    private Actor_Enemy enemy;
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;
        enemy = GetComponent<Actor_Enemy>();
        rb = GetComponent<Rigidbody>();
        CachePlayer();
        ChooseNewTarget();
    }

    void Update()
    {
        if (IsPlayerInAttackRange())
        {
            return;
        }

        if (TryChasePlayer())
        {
            return;
        }

        RandomMove();
    }

    private void RandomMove()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ChooseNewTarget();
            }

            return;
        }

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= ArriveDistance)
        {
            StartWait();
            return;
        }

        if (IsBlocked(direction.normalized))
        {
            ChooseNewTarget();
            return;
        }

        MoveTowards(direction.normalized);
        CheckStuck();
    }

    private bool TryChasePlayer()
    {
        if (playerTarget == null)
        {
            CachePlayer();
        }

        if (playerTarget == null)
        {
            return false;
        }

        Vector3 toPlayer = playerTarget.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (distance > ChaseRange || distance <= StopChaseDistance)
        {
            return false;
        }

        Vector3 direction = toPlayer.normalized;
        if (RequireLineOfSight && !HasLineOfSight(direction, distance))
        {
            return false;
        }

        if (IsBlocked(direction))
        {
            Vector3 avoidedDirection = FindAvoidanceDirection(direction);
            if (avoidedDirection == Vector3.zero)
            {
                ChooseNewTarget();
                return false;
            }

            direction = avoidedDirection;
        }

        isWaiting = false;
        MoveTowards(direction);
        CheckStuck();
        return true;
    }

    private void ChooseNewTarget()
    {
        for (int i = 0; i < TargetPickAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * MoveRadius;
            Vector3 candidate = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            candidate.y = startPosition.y;

            Vector3 direction = candidate - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= ArriveDistance * ArriveDistance)
            {
                continue;
            }

            if (!IsBlocked(direction.normalized))
            {
                targetPosition = candidate;
                return;
            }
        }

        targetPosition = startPosition;
    }

    private void StartWait()
    {
        isWaiting = true;
        waitTimer = Random.Range(MinWaitTime, MaxWaitTime);
    }

    private bool IsPlayerInAttackRange()
    {
        if (enemy == null) return false;

        if (playerTarget == null)
        {
            CachePlayer();
        }

        if (playerTarget == null) return false;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        return distance <= enemy.AttackRange;
    }

    private void MoveTowards(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);

        Vector3 nextPosition = transform.position + transform.forward * MoveSpeed * Time.deltaTime;
        nextPosition.y = startPosition.y + GroundHeightOffset;

        if (rb != null && !rb.isKinematic)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position = nextPosition;
        }
    }

    private bool IsBlocked(Vector3 direction)
    {
        return Physics.SphereCast(
            transform.position + Vector3.up * 0.6f,
            AvoidanceRadius,
            direction,
            out _,
            WallCheckDistance,
            GetObstacleMask(),
            QueryTriggerInteraction.Ignore);
    }

    private bool HasLineOfSight(Vector3 direction, float distance)
    {
        if (!Physics.Raycast(
            transform.position + Vector3.up * 0.8f,
            direction,
            out RaycastHit hit,
            distance,
            GetObstacleMask(),
            QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.transform == playerTarget || hit.transform.IsChildOf(playerTarget);
    }

    private Vector3 FindAvoidanceDirection(Vector3 forwardDirection)
    {
        Vector3 rightDirection = Quaternion.Euler(0f, SideCheckAngle, 0f) * forwardDirection;
        if (!IsBlocked(rightDirection))
        {
            return rightDirection.normalized;
        }

        Vector3 leftDirection = Quaternion.Euler(0f, -SideCheckAngle, 0f) * forwardDirection;
        if (!IsBlocked(leftDirection))
        {
            return leftDirection.normalized;
        }

        return Vector3.zero;
    }

    private void CheckStuck()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer < StuckCheckTime)
        {
            return;
        }

        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        if (movedDistance < StuckDistance)
        {
            ChooseNewTarget();
        }

        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    private int GetObstacleMask()
    {
        return ObstacleLayer.value == 0 ? Physics.DefaultRaycastLayers : ObstacleLayer.value;
    }

    private void CachePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTarget = player != null ? player.transform : null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, MoveRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ChaseRange);
    }
}
