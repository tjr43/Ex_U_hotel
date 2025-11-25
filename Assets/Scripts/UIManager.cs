using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수
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
            CheckAutoPopup(); // 씬 시작 시 퀴즈 팝업 체크
        }

        // 시작할 때 기본적으로 패널들은 닫아둠 (퀴즈 층이면 CheckAutoPopup이 다시 켬)
        CloseAllPanels();

        if (elevatorDisplay != null) elevatorDisplay.text = "";
        currentElevatorInput = "";
    }

    // ▼▼▼ [추가됨] 씬이 로드되자마자 "여기가 퀴즈 층인가?" 확인하는 함수 ▼▼▼
    private void CheckAutoPopup()
    {
        int floor = GameManager.Instance.gameState.currentFloor;
        bool isCleared = GameManager.Instance.gameState.IsFloorCleared(floor);
        bool isLobbyOrRest = (floor == 1 || floor == 7);

        // 퀴즈 층이고, 아직 안 깼다면 -> 바로 퀴즈 패널 열기!
        if (!isLobbyOrRest && !isCleared)
        {
            // 약간의 딜레이 후 띄우거나 바로 띄움
            ShowQuizPanel();
        }
    }
    // ▲▲▲

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

    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        if (quizRiddleText == null) return;

        int currentFloor = state.currentFloor;
        if (state.gameFloors == null || state.gameFloors.Count == 0) return;
        // 데이터 안전 검사
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
            ScrollRect scrollRect = rulesPanel.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        }
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

    // ▼▼▼ [수정됨] 씬 이동 로직 적용 ▼▼▼
    public void OnElevatorGo()
    {
        if (GameManager.Instance == null) return;

        if (int.TryParse(currentElevatorInput, out int newFloor))
        {
            // 1. 층 정보 업데이트
            int totalFloors = GameManager.Instance.gameState.gameFloors.Count;
            if (newFloor < 1 || newFloor > totalFloors)
            {
                ShowInteractionMessage("존재하지 않는 층입니다.");
                OnElevatorClear();
                return;
            }

            GameManager.Instance.ChangeFloor(newFloor);
            CloseAllPanels();

            // 2. 씬 이동 결정
            if (newFloor == 1)
            {
                // 1층으로 가면 로비(GameScene)로 이동
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                // 2층 이상이면 퀴즈 방(FloorScene)으로 이동
                // *주의* Build Settings에 "FloorScene"이 추가되어 있어야 함
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
}