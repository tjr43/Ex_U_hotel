using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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

    // ▼▼▼ [새로 추가된 부분: 키보드 입력 감지] ▼▼▼
    private void Update()
    {
        // 엘리베이터 패널이 켜져 있을 때만 작동
        if (elevatorPanel != null && elevatorPanel.activeSelf)
        {
            // 숫자 키 (0~9) 입력 감지 (알파벳 위 숫자키 & 오른쪽 넘패드 모두 지원)
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    OnElevatorNumpadPress(i.ToString());
                }
            }

            // 엔터 키 (이동)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnElevatorGo();
            }

            // 백스페이스 (지우기)
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                OnElevatorClear();
            }

            // ESC (닫기)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAllPanels();
            }
        }
    }
    // ▲▲▲ [추가 끝] ▲▲▲

    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        // GameScene 퀴즈 로직
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

        // [중요] 여기에 elevatorPanel.SetActive 코드가 없어야 합니다!
    }

    // --- 패널 열기 (PlayerInteraction에서 호출) ---
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

    public void OnElevatorGo()
    {
        if (GameManager.Instance == null) return;
        if (int.TryParse(currentElevatorInput, out int newFloor))
        {
            GameManager.Instance.ChangeFloor(newFloor);
            CloseAllPanels();
        }
        else
        {
            ShowInteractionMessage("올바른 층을 입력하세요."); // 임시로 메시지 표시
            OnElevatorClear();
        }
    }

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
                // 텍스트가 하나뿐인 경우 (단순 텍스트 프리팹일 때)
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