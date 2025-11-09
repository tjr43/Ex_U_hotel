using UnityEngine;
using StarterAssets; // StarterAssets의 클래스를 사용하기 위해 필요합니다.
using UnityEngine.SceneManagement; // 씬 관리를 위해 이 줄을 추가합니다.

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("1인칭 컨트롤러 컴포넌트 연결")]

    [Tooltip("PlayerCapsule 오브젝트에 있는 'FirstPersonController' 스크립트")]
    public FirstPersonController movementScript;

    [Tooltip("PlayerCapsule 오브젝트에 있는 'StarterAssetsInputs' 스크립트")]
    public StarterAssetsInputs inputScript;

    // Awake()는 Instance 설정용으로만 사용합니다.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // DontDestroyOnLoad(gameObject); // GameManager가 이미 DontDestroyOnLoad이므로, 이 스크립트가 GameManager와 같은 오브젝트에 있다면 이 줄은 필요 없습니다.
    }

    // 씬이 완전히 로드된 후 Start()에서 모드를 설정합니다.
    private void Start()
    {
        // 현재 활성화된 씬의 이름을 가져옵니다.
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 씬 이름이 "GameScene"일 때만 1인칭 모드(마우스 잠금)로 시작합니다.
        if (currentSceneName == "GameScene")
        {
            SetUIMode(false); // 1인칭 탐험 모드
        }
        else
        {
            // "StartScene", "WinScene", "GameOverScene" 등 다른 모든 씬에서는 UI 모드로 시작합니다.
            SetUIMode(true); // UI 조작 모드 (마우스 보이기)
        }
    }

    // UI 모드 설정: true = UI 조작 모드, false = 1인칭 탐험 모드
    public void SetUIMode(bool showUI)
    {
        if (movementScript != null)
        {
            movementScript.enabled = !showUI; // 이동 비활성화 [cite: 230-231]
        }

        if (inputScript != null)
        {
            inputScript.enabled = !showUI; // 입력 비활성화 [cite: 233-234]
        }

        if (showUI)
        {
            // UI 모드: 마우스 커서 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // (수정된 부분) inputScript가 null이 아닐 때만 내부 값을 초기화합니다.
            // (StartScene 등에는 inputScript가 할당되어 있지 않으므로 오류 방지)
            if (inputScript != null)
            {
                inputScript.look = Vector2.zero;
                inputScript.move = Vector2.zero;
            }
        }
        else
        {
            // 1인칭 모드: 마우스 커서 잠그기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}