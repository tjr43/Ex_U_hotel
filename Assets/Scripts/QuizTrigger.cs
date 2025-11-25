using UnityEngine;
using UnityEngine.InputSystem; // New Input System

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (interactionText3D != null) interactionText3D.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRadius)
        {
            // 플레이어가 가까이 왔을 때
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (interactionText3D != null) interactionText3D.SetActive(true);
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            // F 키 입력 감지
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (UIManager.Instance != null)
                {
                    // 퀴즈 패널이 켜져 있으면 닫고, 꺼져 있으면 켭니다.
                    if (UIManager.Instance.quizPanel.activeSelf)
                    {
                        UIManager.Instance.CloseAllPanels();
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}