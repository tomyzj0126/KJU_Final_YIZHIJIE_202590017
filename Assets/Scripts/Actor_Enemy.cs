using UnityEngine;

public class Actor_Enemy : MonoBehaviour
{
    [Header("Detection Settings")]
    public float AttackRange = 15f;       // 플레이어 감지 및 공격 사거리
    public float RotationSpeed = 5f;      // 플레이어를 바라보는 회전 속도
    public LayerMask PlayerLayer;         // 플레이어 식별용 레이어Mask

    [Header("References")]
    private Transform playerTarget;
    private Actor_EnemyShooter enemyShooter;
    private EnemyState currentState = EnemyState.Idle;
    bool isNear = false;
    bool wasNear = false;

    void Awake()
    {
        enemyShooter = GetComponent<Actor_EnemyShooter>();
    }

    void Start()
    {
        // 씬 내의 플레이어를 미리 태그나 레이어로 찾아서 캐싱해둡니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            Debug.Log($"Found Player: {playerTarget.name}");
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 사거리 내에 있는지 판단하여 상태 변경 (트리거 역할)
        if (distanceToPlayer <= AttackRange)
        {
            isNear = true;
            currentState = EnemyState.Attack;
        }
        else
        {
            isNear = false;
        }

        if (!isNear && wasNear)
        {
            currentState = EnemyState.Idle;
        }
        wasNear = isNear;

        HandleStateBehavior();
    }

    public void Act_SetStateToAttact()
    {
        currentState = EnemyState.Attack;
    }

    public void Act_SetStateToIdel()
    {
        currentState = EnemyState.Idle;
    }

    private void HandleStateBehavior()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // 공격 대기 시 정지 혹은 순찰 로직 배치 가능
                enemyShooter.SetFire(false);
                break;

            case EnemyState.Patrol:
                // 공격 대기 시 정지 혹은 순찰 로직 배치 가능
                enemyShooter.SetFire(false);
                break;
            

            case EnemyState.Attack:
                // Debug.Log($"[Actor_Enemy] LookAtTarget");
                // 1. 플레이어 바라보기 (Y축 기준 부드러운 회전)
                LookAtTarget(playerTarget.position);

                // 2. 총 발사 시도
                if (enemyShooter != null)
                {
                    enemyShooter.SetFire(true);
                }
                break;
        }
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        // Debug.Log($"[Actor_Enemy] LookAtTarget");
        // 대상 방향 벡터 구하기 (Y축 높이는 같게 맞추어 넘어짐 방지)
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; 

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Slerp를 이용하여 부드럽게 회전 처리
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        }
    }

    // 에디터 뷰에서 사거리를 시각적으로 확인하기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}