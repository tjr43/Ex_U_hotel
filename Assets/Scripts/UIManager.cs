using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Components (HUD)")]
    public TMP_Text playerNameText;
    public TMP_Text attemptsText;
    public TMP_Text messageText;

    [Header("UI Text Components (Quiz Panel)")]
    public TMP_Text quizRiddleText;
    public TMP_Text quizDescriptionText;

    [Header("Input Fields")]
    public TMP_InputField answerInput;
    public TMP_InputField memoInputField;

    [Header("Buttons")]
    public Button submitButton;

    [Header("Elevator UI")]
    public GameObject elevatorPanel;
    public TMP_Text elevatorDisplay;
    private string currentElevatorInput = "";

    [Header("Panel References")]
    public GameObject quizPanel;
    public GameObject memoPanel;
    public GameObject rulesPanel;

    [Header("Memo List Components")]
    public RectTransform memoListContent;
    public GameObject memoItemPrefab;

    [Header("References")]
    public GameObject eventSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameState != null)
        {
            UpdateUI();
            CheckAutoPopup();
        }

        CloseAllPanels();

        if (elevatorDisplay != null) elevatorDisplay.text = "";
        currentElevatorInput = "";
    }

    private void CheckAutoPopup()
    {
        int floor = GameManager.Instance.gameState.currentFloor;
        bool isCleared = GameManager.Instance.gameState.IsFloorCleared(floor);
        bool isLobbyOrRest = (floor == 1 || floor == 7);

        if (!isLobbyOrRest && !isCleared)
        {
            ShowQuizPanel();
        }
    }

    private void Update()
    {
        if (elevatorPanel != null && elevatorPanel.activeSelf)
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            CheckNumberInput(kb.digit0Key, kb.numpad0Key, "0");
            CheckNumberInput(kb.digit1Key, kb.numpad1Key, "1");
            CheckNumberInput(kb.digit2Key, kb.numpad2Key, "2");
            CheckNumberInput(kb.digit3Key, kb.numpad3Key, "3");
            CheckNumberInput(kb.digit4Key, kb.numpad4Key, "4");
            CheckNumberInput(kb.digit5Key, kb.numpad5Key, "5");
            CheckNumberInput(kb.digit6Key, kb.numpad6Key, "6");
            CheckNumberInput(kb.digit7Key, kb.numpad7Key, "7");
            CheckNumberInput(kb.digit8Key, kb.numpad8Key, "8");
            CheckNumberInput(kb.digit9Key, kb.numpad9Key, "9");

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                OnElevatorGo();
            }
            if (kb.backspaceKey.wasPressedThisFrame)
            {
                OnElevatorClear();
            }
            if (kb.escapeKey.wasPressedThisFrame)
            {
                CloseAllPanels();
            }
        }
    }

    private void CheckNumberInput(KeyControl mainKey, KeyControl numpadKey, string value)
    {
        if (mainKey.wasPressedThisFrame || numpadKey.wasPressedThisFrame)
        {
            OnElevatorNumpadPress(value);
        }
    }

    // ▼▼▼ [수정됨] 한글 "목숨" 적용 및 텍스트 포맷 변경 ▼▼▼
    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        // 1. HUD 업데이트 (이름: OOO    목숨: 2)
        if (playerNameText != null)
        {
            // \n을 없애고 한 줄로 표시 (Inspector에서 Width를 800 이상으로 늘려야 안 잘림)
            playerNameText.text = $"이름: <color=yellow>{state.currentPlayerId}</color>    목숨: <color=red>{state.attemptsLeft}</color>";
        }

        if (attemptsText != null)
        {
            attemptsText.text = state.attemptsLeft.ToString();
        }

        // 2. 퀴즈 패널 텍스트 업데이트
        if (quizRiddleText == null) return;

        int currentFloor = state.currentFloor;
        if (state.gameFloors == null || state.gameFloors.Count == 0) return;
        if (currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

        Floor currentFloorData = state.gameFloors[currentFloor - 1];
        bool isCleared = state.IsFloorCleared(currentFloor);
        bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

        if (isLobbyOrRest)
        {
            if (quizDescriptionText != null) quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
            if (quizRiddleText != null) quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요.";
        }
        else if (isCleared)
        {
            if (quizDescriptionText != null) quizDescriptionText.text = $"{currentFloor}층 (클리어)";
            if (quizRiddleText != null) quizRiddleText.text = "이미 클리어한 층입니다.";
        }
        else
        {
            if (currentFloorData.traps != null && currentFloorData.traps.Count > 0)
            {
                Trap trap = currentFloorData.traps[0];
                if (quizDescriptionText != null) quizDescriptionText.text = $"--- {currentFloor}층 --- [방송] {trap.description}";
                if (quizRiddleText != null) quizRiddleText.text = $"[문제] {trap.riddle}";
            }
        }

        bool canSubmit = !isCleared && !isLobbyOrRest;
        if (answerInput != null) answerInput.gameObject.SetActive(canSubmit);
        if (submitButton != null) submitButton.gameObject.SetActive(canSubmit);
    }
    // ▲▲▲

    public void ShowQuizPanel()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
            if (memoPanel != null) memoPanel.SetActive(false);
            if (rulesPanel != null) rulesPanel.SetActive(false);
            if (elevatorPanel != null) elevatorPanel.SetActive(false);

            UpdateUI();
            SetGameMode(true);
        }
    }

    public void ShowMemoPanel()
    {
        if (memoPanel != null)
        {
            memoPanel.SetActive(true);
            UpdateMemoList();
            SetGameMode(true);
        }
    }

    public void ShowRulePanel()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            SetGameMode(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // ▼▼▼ [핵심 수정] 스크롤 초기화 ▼▼▼
            ScrollRect scrollRect = rulesPanel.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                StartCoroutine(ForceScrollToTop(scrollRect));
            }
        }
    }

    // 스크롤 강제 초기화 코루틴
    private System.Collections.IEnumerator ForceScrollToTop(ScrollRect scrollRect)
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ShowElevatorPanel()
    {
        if (elevatorPanel != null)
        {
            elevatorPanel.SetActive(true);
            SetGameMode(true);
            currentElevatorInput = "";
            if (elevatorDisplay != null) elevatorDisplay.text = "";
        }
    }

    public void CloseAllPanels()
    {
        if (memoPanel != null) memoPanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(false);

        SetGameMode(false);
    }

    private void SetGameMode(bool isUI)
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.SetUIMode(isUI);

        if (eventSystem != null) eventSystem.SetActive(isUI);
    }

    public void OnElevatorNumpadPress(string digit)
    {
        if (currentElevatorInput.Length < 2)
        {
            currentElevatorInput += digit;
            if (elevatorDisplay != null) elevatorDisplay.text = currentElevatorInput;
        }
    }

    public void OnElevatorClear()
    {
        currentElevatorInput = "";
        if (elevatorDisplay != null) elevatorDisplay.text = "";
    }

    // ▼▼▼ [수정됨] 이동 제한 & 씬 이동 로직 ▼▼▼
    public void OnElevatorGo()
    {
        if (GameManager.Instance == null) return;

        if (int.TryParse(currentElevatorInput, out int newFloor))
        {
            int totalFloors = GameManager.Instance.gameState.gameFloors.Count;
            if (newFloor < 1 || newFloor > totalFloors)
            {
                ShowInteractionMessage("존재하지 않는 층입니다.");
                OnElevatorClear();
                return;
            }

            int currentFloor = GameManager.Instance.gameState.currentFloor;
            bool isLobbyOrRest = (currentFloor == 1 || currentFloor == 7);
            bool isCurrentCleared = GameManager.Instance.gameState.IsFloorCleared(currentFloor);

            // 현재 층을 못 깼으면 이동 불가 (로비 제외)
            if (!isLobbyOrRest && !isCurrentCleared)
            {
                ShowInteractionMessage($"현재 {currentFloor}층의 문제를 먼저 해결해야 합니다!");
                OnElevatorClear();
                return;
            }

            GameManager.Instance.ChangeFloor(newFloor);
            CloseAllPanels();

            // 씬 이동
            if (newFloor == 1)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                SceneManager.LoadScene("FloorScene");
            }
        }
        else
        {
            ShowInteractionMessage("올바른 층을 입력하세요.");
            OnElevatorClear();
        }
    }
    // ▲▲▲

    public void OnSubmitAnswerButton()
    {
        if (GameManager.Instance == null || answerInput == null) return;
        GameManager.Instance.SubmitAnswer(answerInput.text);
        answerInput.text = "";
    }

    public void OnExitGameButton()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    private void UpdateMemoList()
    {
        if (memoListContent == null || memoItemPrefab == null) return;
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        foreach (Transform child in memoListContent) Destroy(child.gameObject);

        var history = GameManager.Instance.gameState.playerHistory;
        if (history == null) return;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            PlayerRecord record = history[i];
            if (record == null || string.IsNullOrEmpty(record.memo)) continue;

            GameObject item = Instantiate(memoItemPrefab, memoListContent);
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = record.playerId;
                texts[1].text = record.memo;
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{record.playerId}: {record.memo}";
            }
        }
    }

    public void ShowInteractionMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }

    public void HideInteractionMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    // ▼▼▼ [추가됨] 입력 중인지 확인하는 함수 (창 닫힘 방지용) ▼▼▼
    public bool IsTyping()
    {
        // 1. 답 입력창이 켜져 있고 & 포커스 되어 있거나
        if (answerInput != null && answerInput.isFocused) return true;

        // 2. 메모 입력창이 켜져 있고 & 포커스 되어 있다면
        if (memoInputField != null && memoInputField.isFocused) return true;

        return false; // 둘 다 아니면 타자 치는 중 아님
    }

    // ▲▲▲
}