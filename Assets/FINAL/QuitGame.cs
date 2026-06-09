using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        // 在Unity编辑器中停止播放
        EditorApplication.isPlaying = false;
#else
        // 打包后真正退出游戏
        Application.Quit();
#endif
    }
}