using UnityEngine;
using TMPro; // TextMeshPro Text를 사용하기 위해 필수입니다.

// 1. 클래스 이름이 "DoorTrigger"로 파일 이름("DoorTrigger.cs")과 일치해야 합니다.
// 2. ": MonoBehaviour" 부분이 빠지면 안 됩니다.
public class DoorTrigger : MonoBehaviour
{
    // 1단계에서 만든 UI들을 유니티 에디터에서 연결합니다.
    [Header("UI References")]
    public GameObject nameInputPanel; // 켜고 끌 이름 입력창 패널

    // ▼▼▼ [오류 수정!] ▼▼▼
    // "TMP_Text" (부품) 대신 "GameObject" (통)를 받도록 변경합니다.
    // 이렇게 하면 Hierarchy에서 드래그가 100% 가능해집니다.
    [Tooltip("Hierarchy의 'InteractionMessageText' 오브젝트를 연결하세요.")]
    public GameObject interactionMessageObject;
    // ▲▲▲ [오류 수정 완료] ▲▲▲

    private TMP_Text interactionMessageText; // 실제 텍스트 부품은 여기서 저장
    private bool playerIsInsideTrigger = false;

    // Start()에서 텍스트 부품을 찾아옵니다.
    private void Start() {
        if (interactionMessageObject != null) {
            // 게임오브젝트 안에서 TMP_Text 부품을 찾아냅니다.
            interactionMessageText = interactionMessageObject.GetComponent<TMP_Text>();
        } else {
            Debug.LogError("DoorTrigger에 interactionMessageObject가 연결되지 않았습니다!");
        }
    }

    // 플레이어가 이 트리거 영역 안에 머무를 때 매 프레임 호출됨
    private void Update() {
        // 플레이어가 영역 안에 있고 "F"키를 눌렀을 때
        if (playerIsInsideTrigger && Input.GetKeyDown(KeyCode.F)) {
            ActivateDoor();
        }
    }

    // 문을 활성화 (이름 입력창 열기)
    private void ActivateDoor() {
        if (nameInputPanel != null) {
            nameInputPanel.SetActive(true); // 이름 입력창 켜기
        }

        if (interactionMessageText != null) {
            interactionMessageText.text = ""; // 상호작용 메시지 숨기기
        }

        // 마우스 커서 잠금 해제 및 보이기
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // --- 트리거 감지 ---

    // 플레이어가 이 콜라이더 영역에 들어왔을 때
    private void OnTriggerEnter(Collider other) {
        // 들어온 오브젝트가 "Player" 태그를 가졌는지 확인
        if (other.CompareTag("Player")) {
            playerIsInsideTrigger = true;
            // (수정) nameInputPanel이 null이 아닌지 함께 확인
            if (interactionMessageText != null && nameInputPanel != null && nameInputPanel.activeSelf == false) // 패널이 꺼져있을 때만 메시지 표시
            {
                interactionMessageText.text = "F 키를 눌러 입장하기";
            }
        }
    }

    // 플레이어가 이 콜라이더 영역에서 나갔을 때
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerIsInsideTrigger = false;
            if (interactionMessageText != null) {
                interactionMessageText.text = ""; // 메시지 지우기
            }
        }
    }
}