using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [Header("설정")]
    public float detectionRadius = 5.0f; // 감지 거리

    [Header("3D 텍스트 연결")]
    public GameObject interactionText3D; // "Press F" 3D 텍스트 오브젝트

    private Transform playerTransform;
    private bool isPlayerNear = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (interactionText3D != null)
        {
            interactionText3D.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRadius)
        {
            // --- 범위 안 ---
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                // 3D 텍스트 켜기
                if (interactionText3D != null) interactionText3D.SetActive(true);
                // 기존 UI 메시지 숨김
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            // F키 입력 시 엘리베이터 패널 열기
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (UIManager.Instance != null)
                {
                    // 이미 열려있으면 닫고, 닫혀있으면 엽니다
                    if (UIManager.Instance.elevatorPanel.activeSelf)
                    {
                        UIManager.Instance.CloseAllPanels();
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    }
                    else
                    {
                        UIManager.Instance.ShowElevatorPanel(); // 엘리베이터 패널 열기!
                        if (interactionText3D != null) interactionText3D.SetActive(false);
                    }
                }
            }
        }
        else
        {
            // --- 범위 밖 ---
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (interactionText3D != null) interactionText3D.SetActive(false);

                // 멀어지면 패널 닫기
                if (UIManager.Instance != null && UIManager.Instance.elevatorPanel.activeSelf)
                {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}