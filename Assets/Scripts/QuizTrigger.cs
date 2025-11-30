using UnityEngine;
using UnityEngine.InputSystem; // New Input System 필수

public class QuizTrigger : MonoBehaviour
{
    [Header("설정")]
    public float detectionRadius = 5.0f;

    [Header("3D 텍스트 연결 (선택)")]
    public GameObject interactionText3D; // "F 눌러 퀴즈 풀기" 같은 3D 텍스트

    private Transform playerTransform;
    private bool isPlayerNear = false;

    private void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 시작할 때 3D 텍스트 숨기기
        if (interactionText3D != null) interactionText3D.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // ▼▼▼ [핵심 수정] 플레이어가 타자 치는 중이면 F키 감지 안 함 (창 닫힘 방지) ▼▼▼
        if (UIManager.Instance != null && UIManager.Instance.IsTyping())
        {
            return; // 아무것도 안 하고 함수 종료!
        }
        // ▲▲▲

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRadius)
        {
            // 플레이어가 가까이 왔을 때
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (interactionText3D != null) interactionText3D.SetActive(true);

                // 화면 중앙 메시지는 숨김 (3D 텍스트가 있으니까)
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            // F 키 입력 감지
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (UIManager.Instance != null)
                {
                    // 퀴즈 패널이 이미 켜져 있으면 -> 닫기
                    if (UIManager.Instance.quizPanel.activeSelf)
                    {
                        UIManager.Instance.CloseAllPanels();
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    }
                    // 꺼져 있으면 -> 열기
                    else
                    {
                        UIManager.Instance.ShowQuizPanel(); // 퀴즈 패널 열기!
                        if (interactionText3D != null) interactionText3D.SetActive(false);
                    }
                }
            }
        }
        else
        {
            // 멀어졌을 때
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (interactionText3D != null) interactionText3D.SetActive(false);

                // 멀어지면 패널 자동 닫기
                if (UIManager.Instance != null && UIManager.Instance.quizPanel.activeSelf)
                {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    // 에디터에서 범위 확인용 원 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}