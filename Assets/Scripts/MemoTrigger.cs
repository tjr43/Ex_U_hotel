using UnityEngine;
using UnityEngine.InputSystem; // ⭐ 필수 추가

public class MemoTrigger : MonoBehaviour
{
    [Header("설정")]
    public float detectionRadius = 5.0f;

    [Header("3D 텍스트 연결")]
    public GameObject interactionText3D;

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
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (interactionText3D != null) interactionText3D.SetActive(true);
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            // ▼▼▼ 여기가 변경된 부분 (New Input System) ▼▼▼
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (UIManager.Instance != null)
                {
                    if (UIManager.Instance.memoPanel.activeSelf)
                    {
                        UIManager.Instance.CloseAllPanels();
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    }
                    else
                    {
                        UIManager.Instance.ShowMemoPanel();
                        if (interactionText3D != null) interactionText3D.SetActive(false);
                    }
                }
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
        else
        {
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (interactionText3D != null) interactionText3D.SetActive(false);

                if (UIManager.Instance != null && UIManager.Instance.memoPanel.activeSelf)
                {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}