using UnityEngine;

public class MemoTrigger : MonoBehaviour
{
    [Header("설정")]
    public float detectionRadius = 5.0f; // 감지 거리 (이 안에 들어오면 텍스트 뜸)

    [Header("3D 텍스트 연결 (필수)")]
    public GameObject interactionText3D; // 여기에 3D Text 오브젝트를 넣으세요

    private Transform playerTransform;
    private bool isPlayerNear = false;

    private void Start() {
        // 1. 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerTransform = player.transform;
            Debug.Log("✅ [MemoTrigger] 플레이어를 찾았습니다!");
        } else {
            Debug.LogError("❌ [MemoTrigger] 'Player' 태그가 달린 오브젝트를 찾을 수 없습니다! PlayerCapsule의 Tag를 확인하세요.");
        }

        // 2. 시작할 때 3D 텍스트 숨기기
        if (interactionText3D != null) {
            interactionText3D.SetActive(false);
        } else {
            Debug.LogWarning("[MemoTrigger] 3D 텍스트가 연결되지 않았습니다!");
        }
    }

    private void Update() {
        if (playerTransform == null) return;

        // 1. 거리 계산 (물리 충돌체 설정 없이도 거리를 잴 수 있음)
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 2. 범위 안에 있는지 확인
        if (distance <= detectionRadius) {
            // --- 범위 진입 ---
            if (!isPlayerNear) {
                isPlayerNear = true;
                // 3D 텍스트 켜기
                if (interactionText3D != null) interactionText3D.SetActive(true);
                // UI 메시지 숨기기 (3D 텍스트 사용 시 중복 방지)
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            // F키 입력 감지
            if (Input.GetKeyDown(KeyCode.F)) {
                if (UIManager.Instance != null) {
                    // 메모 패널이 열려있으면 닫고, 닫혀있으면 엽니다
                    if (UIManager.Instance.memoPanel.activeSelf) {
                        UIManager.Instance.CloseAllPanels();
                        // 패널 닫을 때 3D 텍스트 다시 보이기
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    } else {
                        UIManager.Instance.ShowMemoPanel();
                        // 패널 열릴 때 3D 텍스트 숨기기 (선택사항, 가려지는 것 방지)
                        if (interactionText3D != null) interactionText3D.SetActive(false);
                    }
                }
            }
        } else {
            // --- 범위 이탈 ---
            if (isPlayerNear) {
                isPlayerNear = false;
                // 3D 텍스트 끄기
                if (interactionText3D != null) interactionText3D.SetActive(false);

                // 멀어지면 열려있던 패널도 닫기
                if (UIManager.Instance != null && UIManager.Instance.memoPanel.activeSelf) {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    // 에디터에서 감지 범위를 노란 원으로 보여줌
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}