using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // ⭐ 필수 추가

public class DoorTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nameInputPanel;
    public GameObject interactionMessageObject;

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
        // ▼▼▼ [수정] New Input System ▼▼▼
        if (playerIsInsideTrigger && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ActivateDoor();
        }
        // ▲▲▲
    }

    private void ActivateDoor()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
        }

        if (interactionMessageText != null)
        {
            interactionMessageText.text = "";
        }

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.SetUIMode(true);
        }
        else
        {
            Debug.LogError("TransitionManager.Instance가 없습니다!");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInsideTrigger = true;
            if (interactionMessageText != null && nameInputPanel != null && nameInputPanel.activeSelf == false)
            {
                interactionMessageText.text = "Press F to Enter"; // 텍스트 약간 수정
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
                interactionMessageText.text = "";
            }
        }
    }
}