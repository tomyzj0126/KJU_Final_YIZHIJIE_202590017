using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Options")]
    public float maxHealth = 100f;
    private float currentHealth;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        // ScoreManager.Instance.currentHealth = maxHealth;
        // ScoreManager.Instance.AddPlayerHealth(0);
    }

    void Start()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.currentHealth = maxHealth;
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPlayerHealth(0);
    }

    void OnEnable()
    {
        Time.timeScale = 1f;
    }

    // 데미지를 받는 함수
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"[Player] 피격! 남은 체력: {currentHealth}/{maxHealth}");
        //ScoreManager.Instance.AddScore(-10);
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPlayerHealth(-damageAmount);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        float actualHeal = currentHealth - oldHealth;

        Debug.Log($"[Player] 회복! 현재 체력: {currentHealth}/{maxHealth}");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPlayerHealth(actualHeal);
        }
    }

    // 체력이 0 이하가 되었을 때 실행되는 함수
    private void Die()
    {
        isDead = true;
        Debug.Log("[GAME OVER] 플레이어가 사망했습니다.");
        
        // 시간 정지 (게임 오버 연출)
        Time.timeScale = 0f; 

        // TODO: 여기에 게임 오버 UI 띄우기 등의 로직을 추가하세요.

        if (ScoreManager.Instance != null) ScoreManager.Instance.ShowGameOverMenu(true);
    }

    public void Reset()
    {
        currentHealth = 100;
        isDead = false;
        Time.timeScale = 1f;
        //ScoreManager.Instance.currentHealth = maxHealth;
        //ScoreManager.Instance.AddPlayerHealth(0);
    }
}