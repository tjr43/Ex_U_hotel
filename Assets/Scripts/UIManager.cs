using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections;

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

    // ▼▼▼ [추가] 타이머 표시용 텍스트 ▼▼▼
    public TMP_Text timerText;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

    [Header("Memo Display")]
    public TMP_Text memoHistoryText;
    public RectTransform memoListContent;
    public GameObject memoItemPrefab;

    [Header("References")]
    public GameObject eventSystem;

    private Coroutine messageCoroutine;
    private bool isMoving = false;

    // ▼▼▼ [추가] 타이머 관련 변수 ▼▼▼
    private float currentQuizTime = 0f;
    private bool isTimerRunning = false;
    private const float QUIZ_TIME_LIMIT = 30.0f; // 30초 제한
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        if (GameManager.Instance != null && GameManager.Instance.gameState != null) {
            UpdateUI();
        }
        CloseAllPanels();

        if (elevatorDisplay != null) elevatorDisplay.text = "";
    }

    private void Update() {
        // 1. 엘리베이터 입력 로직
        if (elevatorPanel != null && elevatorPanel.activeSelf) {
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

            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) OnElevatorGo();
            if (kb.backspaceKey.wasPressedThisFrame) OnElevatorClear();
            if (kb.escapeKey.wasPressedThisFrame) CloseAllPanels();
        }

        // ▼▼▼ [추가] 2. 퀴즈 타이머 로직 ▼▼▼
        if (quizPanel != null && quizPanel.activeSelf && isTimerRunning) {
            currentQuizTime -= Time.deltaTime;

            // UI 업데이트 (소수점 없이 정수만 표시)
            if (timerText != null) {
                timerText.text = Mathf.CeilToInt(currentQuizTime).ToString();

                // 5초 이하일 때 빨간색으로 경고 (선택사항)
                if (currentQuizTime <= 5f) timerText.color = Color.red;
                else timerText.color = Color.white;
            }

            // 시간 초과 체크
            if (currentQuizTime <= 0) {
                currentQuizTime = 0;
                isTimerRunning = false;

                // GameManager에게 시간 초과 알림
                if (GameManager.Instance != null) {
                    GameManager.Instance.OnQuizTimeout();
                }
            }
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    private void CheckNumberInput(KeyControl mainKey, KeyControl numpadKey, string value) {
        if (mainKey.wasPressedThisFrame || numpadKey.wasPressedThisFrame) {
            OnElevatorNumpadPress(value);
        }
    }

    private void UpdateMemoList() {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        var history = GameManager.Instance.gameState.playerHistory;

        if (memoHistoryText != null) {
            string finalString = "";
            if (history != null && history.Count > 0) {
                foreach (var record in history) {
                    finalString += $"[{record.playerId}] {record.memo}\n\n";
                }
            } else {
                finalString = "아직 작성된 메모가 없습니다.";
            }
            memoHistoryText.text = finalString;
        }
    }

    public void ShowMemoPanel() {
        if (memoPanel != null) {
            memoPanel.SetActive(true);
            UpdateMemoList();
            SetGameMode(true);
        }
    }

    public void UpdateUI() {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        GameState state = GameManager.Instance.gameState;

        if (playerNameText != null) playerNameText.text = $"이름: {state.currentPlayerId} | 목숨: {state.attemptsLeft}";
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        if (quizRiddleText != null) {
            int currentFloor = state.currentFloor;
            bool isCleared = state.IsFloorCleared(currentFloor);

            if (currentFloor == 1 || currentFloor == 7) {
                if (quizDescriptionText != null) quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
                quizRiddleText.text = "안전한 구역입니다.";
            } else if (isCleared) {
                if (quizDescriptionText != null) quizDescriptionText.text = $"{currentFloor}층 (클리어)";
                quizRiddleText.text = "이미 클리어했습니다.";
            } else {
                if (state.gameFloors != null && currentFloor - 1 < state.gameFloors.Count) {
                    var floor = state.gameFloors[currentFloor - 1];
                    if (floor.traps.Count > 0) {
                        if (quizDescriptionText != null) quizDescriptionText.text = $"{currentFloor}층 문제";
                        quizRiddleText.text = floor.traps[0].riddle;
                    }
                }
            }
        }
    }

    public void ShowQuizPanel() {
        if (quizPanel != null) {
            quizPanel.SetActive(true);
            UpdateUI();
            SetGameMode(true);

            // ▼▼▼ [추가] 퀴즈 패널이 열릴 때 타이머 시작 ▼▼▼
            currentQuizTime = QUIZ_TIME_LIMIT;
            isTimerRunning = true;
            if (timerText != null) {
                timerText.gameObject.SetActive(true);
                timerText.text = "30";
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
    }

    public void ShowRulePanel() { if (rulesPanel != null) { rulesPanel.SetActive(true); SetGameMode(true); } }

    public void ShowElevatorPanel() {
        if (elevatorPanel != null) {
            elevatorPanel.SetActive(true);
            SetGameMode(true);
            currentElevatorInput = "";
            if (elevatorDisplay != null) elevatorDisplay.text = "";
        }
    }

    public void CloseAllPanels() {
        if (memoPanel != null) memoPanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(false);

        // ▼▼▼ [추가] 패널 닫을 때 타이머도 끄기 ▼▼▼
        isTimerRunning = false;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        SetGameMode(false);
    }

    private void SetGameMode(bool isUI) {
        if (TransitionManager.Instance != null) TransitionManager.Instance.SetUIMode(isUI);
        if (eventSystem != null) eventSystem.SetActive(isUI);
    }

    public void OnSubmitAnswerButton() { if (answerInput != null) GameManager.Instance.SubmitAnswer(answerInput.text); }

    public void OnElevatorNumpadPress(string digit) {
        if (currentElevatorInput.Length < 2) {
            currentElevatorInput += digit;
            if (elevatorDisplay != null) elevatorDisplay.text = currentElevatorInput;
        }
    }

    public void OnElevatorClear() {
        currentElevatorInput = "";
        if (elevatorDisplay != null) elevatorDisplay.text = "";
    }

    public void OnElevatorGo() {
        if (GameManager.Instance == null || isMoving) return;
        if (int.TryParse(currentElevatorInput, out int newFloor)) {
            StartCoroutine(ProcessFloorMoveRoutine(newFloor));
        } else {
            ShowInteractionMessage("층을 입력하세요.");
        }
    }

    private IEnumerator ProcessFloorMoveRoutine(int newFloor) {
        isMoving = true;
        int totalFloors = GameManager.Instance.gameState.gameFloors.Count;

        if (newFloor < 1 || newFloor > totalFloors) {
            ShowInteractionMessage("존재하지 않는 층입니다.");
            OnElevatorClear();
            isMoving = false;
            yield break;
        }

        if (newFloor == 22) {
            GameManager.Instance.ChangeFloor(newFloor);
            CloseAllPanels();
            ShowInteractionMessage("22층은 함정입니다! 탈락!");
            yield return new WaitForSeconds(3.0f);
            SceneManager.LoadScene("WinScene"); // 여기도 WinScene으로 변경됨
            yield break;
        }

        bool isAlreadyCleared = GameManager.Instance.gameState.IsFloorCleared(newFloor);
        bool isLobbyOrRest = (newFloor == 1 || newFloor == 7);

        if (!isLobbyOrRest && isAlreadyCleared) {
            ShowInteractionMessage($"{newFloor}층은 이미 클리어했습니다!");
            OnElevatorClear();
            isMoving = false;
            yield break;
        }

        GameManager.Instance.ChangeFloor(newFloor);
        CloseAllPanels();
        ShowInteractionMessage($"{newFloor}층으로 이동합니다.");
        yield return new WaitForSeconds(3.0f);

        if (newFloor == 1) SceneManager.LoadScene("GameScene");
        else SceneManager.LoadScene("FloorScene");

        isMoving = false;
    }

    public void ShowInteractionMessage(string msg) {
        if (messageText != null) {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageCoroutine = StartCoroutine(HideMessageRoutine(3.0f));
        }
    }

    private IEnumerator HideMessageRoutine(float delay) { yield return new WaitForSeconds(delay); HideInteractionMessage(); }
    public void HideInteractionMessage() { if (messageText != null) messageText.gameObject.SetActive(false); }
    public bool IsTyping() { return (answerInput != null && answerInput.isFocused); }
}