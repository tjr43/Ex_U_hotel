using UnityEngine;
using TMPro; // TextMeshPro Text를 사용하기 위해 필수입니다.

public class DoorTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nameInputPanel; // 켜고 끌 이름 입력창 패널
    public GameObject interactionMessageObject; // "F 키" 텍스트 오브젝트

    private TMP_Text interactionMessageText;
    private bool playerIsInsideTrigger = false;

    private void Start()
    {
        if (interactionMessageObject != null)
        {
            interactionMessageText = interactionMessageObject.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("DoorTrigger에 interactionMessageObject가 연결되지 않았습니다!");
        }
    }

    private void Update()
    {
        if (playerIsInsideTrigger && Input.GetKeyDown(KeyCode.F))
        {
            ActivateDoor();
        }
    }

    // 문을 활성화 (이름 입력창 열기)
    private void ActivateDoor()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true); // 이름 입력창 켜기
        }

        if (interactionMessageText != null)
        {
            interactionMessageText.text = ""; // 상호작용 메시지 숨기기
        }

        // ▼▼▼ [수정된 부분!] ▼▼▼
        // 마우스 커서를 직접 제어하는 대신,
        // TransitionManager에게 "UI 모드"로 전환하라고 요청합니다.
        // (이 함수가 커서 잠금 해제 + 플레이어 움직임 정지를 모두 처리합니다.)
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.SetUIMode(true); // true = UI 모드 (움직임 정지)
        }
        else
        {
            // TransitionManager가 없는 비상시, 커서만 잠금 해제
            Debug.LogError("TransitionManager.Instance가 없습니다!");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // ▲▲▲ [수정 완료] ▲▲▲
    }


    // --- 트리거 감지 ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInsideTrigger = true;
            if (interactionMessageText != null && nameInputPanel != null && nameInputPanel.activeSelf == false)
            {
                interactionMessageText.text = "Session the F keys";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInsideTrigger = false;
            if (interactionMessageText != null)
            {
                interactionMessageText.text = ""; // 메시지 지우기
            }
        }
    }
}