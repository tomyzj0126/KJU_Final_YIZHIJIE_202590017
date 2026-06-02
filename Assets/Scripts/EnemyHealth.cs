using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Options")]
    public float maxHealth = 20f; // 적 체력 (예: 총알 4방)
    private float currentHealth;

    [Header("Score Reward")]
    public int scoreReward = 100; // 처치 시 지급할 점수

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        // ScoreManager.Instance.currentDestroy = 0;
    }

    void Start()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.currentDestroy = 0;        
    }

    // 데미지를 받는 함수
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"[{gameObject.name}] 데미지 입음! 남은 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"🎯 [{gameObject.name}] 처치 완료! 점수 +{scoreReward}");

        // 싱글톤 ScoreManager를 호출하여 점수 반영
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreReward);
            ScoreManager.Instance.AddDestroyCount();
        }

        // 오브젝트 소멸
        Destroy(gameObject);
    }
}