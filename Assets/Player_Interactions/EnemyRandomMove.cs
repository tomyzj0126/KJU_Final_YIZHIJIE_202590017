using UnityEngine;

public class EnemyRandomMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float MoveSpeed = 2f;
    public float TurnSpeed = 5f;
    public float MoveRadius = 5f;
    public float ArriveDistance = 0.3f;

    [Header("Wait Settings")]
    public float MinWaitTime = 1f;
    public float MaxWaitTime = 3f;

    [Header("Obstacle Check")]
    public LayerMask ObstacleLayer;
    public float WallCheckDistance = 0.8f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer;
    private bool isWaiting = false;

    void Start()
    {
        startPosition = transform.position;
        ChooseNewTarget();
    }

    void Update()
    {
        if (IsPlayerInAttackRange())
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

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction.normalized, WallCheckDistance, ObstacleLayer))
        {
            ChooseNewTarget();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);

        transform.position += transform.forward * MoveSpeed * Time.deltaTime;
    }

    private void ChooseNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * MoveRadius;
        targetPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    private void StartWait()
    {
        isWaiting = true;
        waitTimer = Random.Range(MinWaitTime, MaxWaitTime);
    }

    private bool IsPlayerInAttackRange()
    {
        Actor_Enemy enemy = GetComponent<Actor_Enemy>();
        if (enemy == null) return false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= enemy.AttackRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, MoveRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }
}