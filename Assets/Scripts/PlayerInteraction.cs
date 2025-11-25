using UnityEngine;
using UnityEngine.InputSystem; // 필수
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3.0f;

    [Header("UI References")]
    public TMP_Text interactionText;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null) mainCam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            string tag = hit.collider.tag;

            if (tag == "Memo")
            {
                ShowMessage("F를 눌러 [메모] 읽기");
                if (Keyboard.current.fKey.wasPressedThisFrame && UIManager.Instance != null)
                    UIManager.Instance.ShowMemoPanel();
            }
            else if (tag == "Rule")
            {
                ShowMessage("F를 눌러 [규칙] 확인");
                if (Keyboard.current.fKey.wasPressedThisFrame && UIManager.Instance != null)
                    UIManager.Instance.ShowRulePanel();
            }
            else if (tag == "Elevator")
            {
                ShowMessage("F를 눌러 [탈출] 하기");
                if (Keyboard.current.fKey.wasPressedThisFrame && UIManager.Instance != null)
                    UIManager.Instance.ShowElevatorPanel();
            }
            // ▼▼▼ [추가] 퀴즈 단말기 상호작용 ▼▼▼
            else if (tag == "Quiz")
            {
                ShowMessage("F를 눌러 [퀴즈] 풀기");
                if (Keyboard.current.fKey.wasPressedThisFrame && UIManager.Instance != null)
                    UIManager.Instance.ShowQuizPanel();
            }
            // ▲▲▲
            else
            {
                HideMessage();
            }
        }
        else
        {
            HideMessage();
        }
    }

    void ShowMessage(string msg)
    {
        if (interactionText != null)
        {
            interactionText.text = msg;
            interactionText.gameObject.SetActive(true);
        }
    }

    void HideMessage()
    {
        if (interactionText != null) interactionText.gameObject.SetActive(false);
    }
}