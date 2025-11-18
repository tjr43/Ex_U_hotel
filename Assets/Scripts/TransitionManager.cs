using UnityEngine;
using StarterAssets;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    // [수정] 이 변수들은 이제 Inspector에 보이지 않습니다.
    // 스크립트가 씬을 로드할 때마다 자동으로 찾아줍니다.
    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // GameManager 오브젝트가 씬 전환 시 파괴되지 않도록 설정합니다.
        DontDestroyOnLoad(gameObject);

        // [추가] 씬이 로드될 때마다 OnSceneLoaded 함수를 실행하도록 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // [추가] 씬이 로드될 때마다 실행될 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 새로 로드된 씬에서 "Player" 태그를 가진 오브젝트를 찾습니다.
        // (중요: StartScene과 GameScene의 PlayerCapsule 모두 태그가 "Player"여야 합니다.)
        GameObject player = GameObject.FindWithTag("Player");

        // 2. 플레이어를 찾았으면, 스크립트 참조를 새로고침합니다.
        if (player != null)
        {
            movementScript = player.GetComponent<FirstPersonController>();
            inputScript = player.GetComponent<StarterAssetsInputs>();
            Debug.Log("Player found in scene: " + scene.name); // 확인용 로그
        }
        else
        {
            // WinScene, GameOverScene 등 플레이어가 없는 씬
            movementScript = null;
            inputScript = null;
            Debug.Log("No player found in scene: " + scene.name); // 확인용 로그
        }

        // 3. 씬 이름에 따라 UI 모드를 설정합니다.
        string currentSceneName = scene.name;
        if (currentSceneName == "GameScene" || currentSceneName == "StartScene")
        {
            SetUIMode(false); // 1인칭 탐험 모드
        }
        else
        {
            SetUIMode(true); // UI 조작 모드
        }
    }

    // [추가] 스크립트가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // UI 모드 설정 (이 함수는 거의 그대로입니다)
    public void SetUIMode(bool showUI)
    {
        // 스크립트 참조가 null일 때를 대비한 안전 확인
        if (movementScript != null)
        {
            movementScript.enabled = !showUI;
        }
        if (inputScript != null)
        {
            inputScript.enabled = !showUI;
        }

        if (showUI)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (inputScript != null)
            {
                inputScript.look = Vector2.zero;
                inputScript.move = Vector2.zero;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}