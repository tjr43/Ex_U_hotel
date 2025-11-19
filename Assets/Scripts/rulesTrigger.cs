using UnityEngine;

public class rulesTrigger : MonoBehaviour
{
    [Header("설정")]
    public float detectionRadius = 5.0f;

    [Header("3D 텍스트 연결 (필수)")]
    public GameObject interactionText3D; // 여기에 3D Text 오브젝트를 넣으세요

    private Transform playerTransform;
    private bool isPlayerNear = false;

    private void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (interactionText3D != null) {
            interactionText3D.SetActive(false);
        }
    }

    private void Update() {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRadius) {
            if (!isPlayerNear) {
                isPlayerNear = true;
                if (interactionText3D != null) interactionText3D.SetActive(true);
                if (UIManager.Instance != null) UIManager.Instance.HideInteractionMessage();
            }

            if (Input.GetKeyDown(KeyCode.F)) {
                if (UIManager.Instance != null) {
                    if (UIManager.Instance.rulesPanel.activeSelf) {
                        UIManager.Instance.CloseAllPanels();
                        if (interactionText3D != null) interactionText3D.SetActive(true);
                    } else {
                        UIManager.Instance.ShowRulePanel();
                        if (interactionText3D != null) interactionText3D.SetActive(false);
                    }
                }
            }
        } else {
            if (isPlayerNear) {
                isPlayerNear = false;
                if (interactionText3D != null) interactionText3D.SetActive(false);

                if (UIManager.Instance != null && UIManager.Instance.rulesPanel.activeSelf) {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}