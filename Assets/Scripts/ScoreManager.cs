using UnityEngine;
// UI 점수 표시를 위해 텍스트메시프로(TMP)를 사용한다면 주석 해제
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    // 어디서나 접근 가능한 싱글톤 인스턴스
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;
    public float currentHealth = 100f;
    public int currentDestroy = 0;

    public TMP_Text ScoreText; // UI 텍스트 연결용 (선택)
    public TMP_Text PlayerHealthText; // UI 텍스트 연결용 (선택)
    public TMP_Text EnemyDestroyedText; // UI 텍스트 연결용 (선택)

    public GameObject GameOverUI;

    public GameObject GameClearUI;

    public GameObject PlayerHealthBar;

    int NumOfEnemies;

    public bool isShowGameOverUI = false;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 점수 유지 (필요 시)
        }
        else
        {
            Destroy(gameObject);
        }

        NumOfEnemies = Object.FindObjectsByType<Actor_Enemy>(FindObjectsSortMode.None).Length;
    }

    void Start()
    {
        ScoreText.text = "SCORE: " + currentScore.ToString("D4"); // 0000 형태로 표시
        PlayerHealthText.text = "Player Health: " + currentHealth.ToString("F0"); // 00 형태로 표시
        EnemyDestroyedText.text = "Enemy Destroyed: " + currentDestroy.ToString("D2") + "/" + NumOfEnemies; // 00 형태로 표시

        ShowCursor(isShowGameOverUI );
    }

    void OnEnable()
    {
        ShowGameOverMenu(isShowGameOverUI );   
    }

    // 점수를 추가하는 전역 함수
    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"[ScoreManager] 점수 획득! 현재 점수: {currentScore}");
        ScoreText.text = "SCORE: " + currentScore.ToString("D4"); // 0000 형태로 표시
    }

    // Player Health
    public void AddPlayerHealth(float amount)
    {
        currentHealth += amount;
        Debug.Log($"[ScoreManager] Player Health: {currentHealth}");
        PlayerHealthText.text = "Player Health: " + currentHealth.ToString("F0"); // 00 형태로 표시
        UpdatePlayerHealthBar();
    }

    public void AddDestroyCount()
    {
        currentDestroy ++;
        Debug.Log($"[ScoreManager] Enemy Destroyed: {currentDestroy}");
        //EnemyDestroyedText.text = "Enemy Destroyed: " + currentDestroy.ToString("D2"); // 00 형태로 표시
        EnemyDestroyedText.text = "Enemy Destroyed: " + currentDestroy.ToString("D2") + "/" + NumOfEnemies; // 00 형태로 표시

        if (currentDestroy >= NumOfEnemies) ShowGameClearMenu(true); 
    }

    public void ShowGameOverMenu(bool isShow)
    {
        ShowCursor(isShow);
        GameOverUI.SetActive(isShow);
    }

    public void ShowGameClearMenu(bool isShow)
    {
        ShowCursor(isShow);
        GameClearUI.SetActive(isShow);
        PlayerManager.Instance.CurrentInteractionState = PlayerInteractionState.Idle;
    }

    public void QuitGame()
    {
        // 1. 게임 종료 처리
        #if UNITY_EDITOR
                // 유니티 에디터 환경일 때: Play 모드를 끕니다.
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // 실제 빌드된 플랫폼 환경일 때: 애플리케이션을 종료
                Application.Quit();
        #endif
        PlayerHealthBar.GetComponent<Slider>().value = 0f;
        // 2. 메뉴 닫기 (필요 시 유지)
        ShowGameOverMenu(false);
    }

    public void Continue()
    {
        currentHealth = 100;
        UpdatePlayerHealthBar();
        //PlayerHealthBar.GetComponent<Slider>().value = 1f;
        PlayerManager.Instance.GetComponent<PlayerHealth>().Reset();
        AddPlayerHealth(0);
        ShowGameOverMenu(false);
    }

    public void PlayAgain(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        ShowGameOverMenu(false);
    }

    void ShowCursor(bool isShow)
    {
        if(isShow){
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void UpdatePlayerHealthBar()
    {
        Debug.Log($"UpdatePlayerHealthBar: {currentHealth}");
        if(currentHealth > 0){
            PlayerHealthBar.transform.Find("Fill Area").gameObject.SetActive(true);
            PlayerHealthBar.GetComponent<Slider>().value = currentHealth/100f;
        }
        else
        {
            PlayerHealthBar.transform.Find("Fill Area").gameObject.SetActive(false);  
            // PlayerHealthBar.transform.GetChild(1).gameObject.SetActive(false);  
        }
        
    }
}