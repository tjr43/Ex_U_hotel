using UnityEngine;
using StarterAssets;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

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
        StartCoroutine(InitializeSceneRoutine(scene.name));
    }

    private IEnumerator InitializeSceneRoutine(string sceneName)
    {
        yield return null;

        FindPlayerReferences();

        string lowerName = sceneName.ToLower();

        // ▼▼▼ [수정] "floor"를 목록에 추가했습니다! ▼▼▼
        if (lowerName.Contains("game") ||
            lowerName.Contains("start") ||
            lowerName.Contains("win") ||
            lowerName.Contains("floor")) // <-- 이제 FloorScene도 움직일 수 있습니다.
        {
            Debug.Log($"[TransitionManager] {sceneName}: 이동 모드로 시작");
            SetUIMode(false); // 이동 허용 (마우스 숨김)
        }
        else
        {
            Debug.Log($"[TransitionManager] {sceneName}: UI 모드로 시작");
            SetUIMode(true);  // 이동 차단 (마우스 보임)
        }
    }

    public void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            movementScript = player.GetComponent<FirstPersonController>();
            inputScript = player.GetComponent<StarterAssetsInputs>();
            playerInput = player.GetComponent<PlayerInput>();
        }
    }

    public void SetUIMode(bool showUI)
    {
        if (playerInput == null) FindPlayerReferences();

        // 1. 입력 시스템
        if (playerInput != null)
        {
            playerInput.enabled = !showUI;
        }

        // 2. 이동 스크립트
        if (movementScript != null)
        {
            movementScript.enabled = !showUI;
        }

        // 3. 마우스 커서 및 시선 처리
        if (inputScript != null)
        {
            if (showUI) // UI 모드
            {
                inputScript.cursorLocked = false;
                inputScript.cursorInputForLook = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else // 게임 모드 (이동)
            {
                inputScript.cursorLocked = true;
                inputScript.cursorInputForLook = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }
    }
}