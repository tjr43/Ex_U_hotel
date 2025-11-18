using UnityEngine;
using StarterAssets;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

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
        DontDestroyOnLoad(gameObject);

        // 씬 로드 이벤트 연결
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // [추가된 부분] 게임 시작 시 강제로 초기화 실행
    private void Start()
    {
        // 에디터에서 Play를 눌렀을 때 바로 현재 씬의 플레이어를 찾도록 합니다.
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            movementScript = player.GetComponent<FirstPersonController>();
            inputScript = player.GetComponent<StarterAssetsInputs>();
            // Debug.Log("Player found in scene: " + scene.name);
        }
        else
        {
            movementScript = null;
            inputScript = null;
        }

        // 씬 이름에 따라 초기 모드 설정
        string currentSceneName = scene.name;
        if (currentSceneName == "GameScene" || currentSceneName == "StartScene")
        {
            SetUIMode(false); // 이동 가능 모드로 시작
        }
        else
        {
            SetUIMode(true); // UI 모드로 시작
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SetUIMode(bool showUI)
    {
        if (movementScript != null)
        {
            movementScript.enabled = !showUI;
        }

        // [중요] 입력 스크립트 처리
        if (inputScript != null)
        {
            // 입력을 끄기 전에 기존 입력값을 0으로 초기화해야 계속 걷는 버그가 안 생김
            if (showUI)
            {
                inputScript.move = Vector2.zero;
                inputScript.look = Vector2.zero;
                inputScript.jump = false;
                inputScript.sprint = false;
            }

            // StarterAssetsInputs는 입력을 받아 변수에 저장하는 역할이므로 
            // cursorInputForLook 같은 변수 제어가 필요할 수 있지만,
            // 보통 enabled를 끄거나 cursorLocked를 푸는 것으로 처리합니다.
            inputScript.cursorInputForLook = !showUI;
            inputScript.cursorLocked = !showUI;
        }

        if (showUI)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}