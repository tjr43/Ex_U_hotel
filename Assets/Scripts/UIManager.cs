using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // ⭐ KeyControl 사용을 위한 필수 네임스페이스

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
        }
        CloseAllPanels(); // 시작할 때 모든 패널 숨김

        if (elevatorDisplay != null) elevatorDisplay.text = "";
        currentElevatorInput = "";
    }

    private void Update()
    {
        // 엘리베이터 패널이 켜져 있을 때만 작동
        if (elevatorPanel != null && elevatorPanel.activeSelf)
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // 숫자 키 (0~9) 입력 감지 (상단 숫자키 & 키패드 모두 지원)
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

            // 엔터 키 (이동)
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                OnElevatorGo();
            }

            // 백스페이스 (지우기)
            if (kb.backspaceKey.wasPressedThisFrame)
            {
                OnElevatorClear();
            }

            // ESC (닫기)
            if (kb.escapeKey.wasPressedThisFrame)
            {
                CloseAllPanels();
            }
        }
    }

    // 입력 헬퍼 함수
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

        // 퀴즈 텍스트 업데이트
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

    // --- 패널 열기 ---
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
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
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

    // --- 패널 닫기 ---
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

    // --- 엘리베이터 기능 ---
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

    // ▼▼▼ [수정됨] 엘리베이터 이동 및 퀴즈 패널 자동 열기 로직 ▼▼▼
    public void OnElevatorGo()
    {
        if (GameManager.Instance == null) return;

        if (int.TryParse(currentElevatorInput, out int newFloor))
        {
            // 1. 유효한 층인지 확인
            int totalFloors = GameManager.Instance.gameState.gameFloors.Count;
            if (newFloor < 1 || newFloor > totalFloors)
            {
                ShowInteractionMessage("존재하지 않는 층입니다.");
                OnElevatorClear();
                return;
            }

            // 2. 층 이동 실행
            GameManager.Instance.ChangeFloor(newFloor);

            // 3. 엘리베이터 패널 닫기
            if (elevatorPanel != null) elevatorPanel.SetActive(false);

            // 4. 도착한 층에 따라 퀴즈 패널 열기 결정
            bool isLobbyOrRest = newFloor == 1 || newFloor == 7;
            bool isCleared = GameManager.Instance.gameState.IsFloorCleared(newFloor);

            // 퀴즈를 풀어야 하는 층이라면 (로비X, 휴식X, 클리어X)
            if (!isLobbyOrRest && !isCleared)
            {
                if (quizPanel != null)
                {
                    quizPanel.SetActive(true); // 퀴즈 창 열기!

                    // 다른 패널들은 확실히 닫기
                    if (memoPanel != null) memoPanel.SetActive(false);
                    if (rulesPanel != null) rulesPanel.SetActive(false);

                    UpdateUI(); // 텍스트(문제) 갱신
                    SetGameMode(true); // 마우스 커서 사용 모드 유지
                }
            }
            else
            {
                // 안전한 층이면 그냥 창 닫고 게임 진행
                CloseAllPanels();
            }
        }
        else
        {
            ShowInteractionMessage("올바른 층을 입력하세요.");
            OnElevatorClear();
        }
    }
    // ▲▲▲ [수정 완료] ▲▲▲

    // --- 기타 기능 ---
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