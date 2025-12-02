using UnityEngine;
using StarterAssets;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerReferences();

        string currentSceneName = scene.name;

        // ▼▼▼ [수정됨] WinScene도 움직일 수 있는 씬 목록에 추가 ▼▼▼
        if (currentSceneName == "GameScene" ||
            currentSceneName == "StartScene" ||
            currentSceneName == "winscene")  // <-- 여기 추가!
        {
            SetUIMode(false); // false = 마우스 숨김, 움직임 허용
        }
        else
        {
            SetUIMode(true);  // true = 마우스 보임, 움직임 차단
        }
    }

    // 플레이어 찾는 함수 분리 (못 찾으면 다시 찾기 위함)
    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            movementScript = player.GetComponent<FirstPersonController>();
            inputScript = player.GetComponent<StarterAssetsInputs>();
            playerInput = player.GetComponent<PlayerInput>();
            Debug.Log("✅ [TransitionManager] 플레이어 제어 스크립트를 연결했습니다.");
        }
        else
        {
            Debug.LogWarning("⚠️ [TransitionManager] 'Player' 태그를 가진 오브젝트를 찾지 못했습니다.");
        }
    }

    public void SetUIMode(bool showUI)
    {
        // 안전장치: 만약 플레이어 연결이 끊겨있다면 다시 찾는다.
        if (playerInput == null || inputScript == null)
        {
            FindPlayerReferences();
        }

        // 1. 입력 시스템 차단
        if (playerInput != null)
        {
            // UI가 켜지면 아예 입력을 꺼버림 (화면 회전 방지 핵심)
            playerInput.enabled = !showUI;
        }

        // 2. 움직임 스크립트 제어
        if (movementScript != null)
        {
            movementScript.enabled = !showUI;
        }

        // 3. 입력 값 초기화
        if (inputScript != null)
        {
            if (showUI)
            {
                inputScript.move = Vector2.zero;
                inputScript.look = Vector2.zero;
                inputScript.jump = false;
                inputScript.sprint = false;
                inputScript.cursorInputForLook = false; // 시선 처리 끄기
                inputScript.cursorLocked = false;
            }
            else
            {
                inputScript.cursorInputForLook = true;
                inputScript.cursorLocked = true;
            }
        }

        // 4. 마우스 커서 설정
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