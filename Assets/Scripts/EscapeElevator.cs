using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class EscapeElevator : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject memoPanel;
    public TMP_InputField memoInput;
    public GameObject interactionText;

    private bool isPlayerNear = false;

    private void Start()
    {
        if (memoPanel != null) memoPanel.SetActive(false);
        if (interactionText != null) interactionText.SetActive(false);
    }

    private void Update()
    {
        // ▼▼▼ [추가] 타자 치는 중이면 F키 감지 안 함 (오작동 방지) ▼▼▼
        if (UIManager.Instance != null && UIManager.Instance.IsTyping())
        {
            return;
        }

        // 플레이어가 근처에 있고 + F 키를 눌렀을 때
        if (isPlayerNear && Keyboard.current.fKey.wasPressedThisFrame)
        {
            OpenMemoPanel();
        }
    }

    private void OpenMemoPanel()
    {
        if (memoPanel != null)
        {
            memoPanel.SetActive(true);

            // ▼▼▼ [핵심 수정] 마우스 커서 강제로 보이기 & 풀기 ▼▼▼
            Cursor.lockState = CursorLockMode.None; // 마우스 가두기 해제
            Cursor.visible = true;                  // 마우스 보이기

            // TransitionManager에게도 알림 (움직임 멈춤)
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(true);

            if (interactionText != null) interactionText.SetActive(false);

            // 입력창에 바로 포커스 주기 (편의성)
            if (memoInput != null)
            {
                memoInput.ActivateInputField();
            }
        }
    }

    public void OnSubmitMemo()
    {
        string content = "";
        if (memoInput != null) content = memoInput.text;

        if (GameManager.Instance != null)
        {
            string playerName = GameManager.Instance.gameState.currentPlayerId;
            string finalMemo = $"{playerName}: {content}";
            GameManager.Instance.SaveMemoAndExit(finalMemo, "Success");
        }
        else
        {
            SceneManager.LoadScene("GoodbyeScene");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactionText != null) interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactionText != null) interactionText.SetActive(false);
            if (memoPanel != null) memoPanel.SetActive(false);
        }
    }
}