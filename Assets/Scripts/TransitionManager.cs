using UnityEngine;
using StarterAssets; // StarterAssets의 클래스를 사용하기 위해 필요합니다.

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("1인칭 컨트롤러 컴포넌트 연결")]

    [Tooltip("PlayerCapsule 오브젝트에 있는 'FirstPersonController' 스크립트")]
    public FirstPersonController movementScript;

    [Tooltip("PlayerCapsule 오브젝트에 있는 'StarterAssetsInputs' 스크립트")]
    public StarterAssetsInputs inputScript;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 1인칭 모드로 게임 시작
        SetUIMode(false);
    }

    // UI 모드 설정: true = UI 조작 모드, false = 1인칭 탐험 모드
    public void SetUIMode(bool showUI)
    {
        if (movementScript != null)
        {
            movementScript.enabled = !showUI; // 이동 비활성화
        }

        if (inputScript != null)
        {
            inputScript.enabled = !showUI; // 입력 비활성화
        }

        if (showUI)
        {
            // UI 모드: 마우스 커서 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 입력 스크립트의 내부 값도 초기화 (카메라가 갑자기 튀는 것 방지)
            inputScript.look = Vector2.zero;
            inputScript.move = Vector2.zero;
        }
        else
        {
            // 1인칭 모드: 마우스 커서 잠그기
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

