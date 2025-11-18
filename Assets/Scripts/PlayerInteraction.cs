using UnityEngine;
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
                // 함수 이름이 ShowMemoPanel로 통일되었습니다.
                if (Input.GetKeyDown(KeyCode.F) && UIManager.Instance != null)
                    UIManager.Instance.ShowMemoPanel();
            }
            else if (tag == "Rule")
            {
                ShowMessage("F를 눌러 [규칙] 확인");
                if (Input.GetKeyDown(KeyCode.F) && UIManager.Instance != null)
                    UIManager.Instance.ShowRulePanel();
            }
            else if (tag == "Elevator")
            {
                ShowMessage("F를 눌러 [탈출] 하기");
                if (Input.GetKeyDown(KeyCode.F) && UIManager.Instance != null)
                    UIManager.Instance.ShowElevatorPanel();
            }
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