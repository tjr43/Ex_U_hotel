using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections; // 코루틴 필수

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

    // 타이머 코루틴 변수
    private Coroutine messageCoroutine;
    private bool isMoving = false; // 이동 중 중복 입력 방지

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

        CloseAllPanels();

        if (elevatorDisplay != null) elevatorDisplay.text = "";
        currentElevatorInput = "";
        isMoving = false;
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

    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        if (playerNameText != null)
        {
            playerNameText.text = $"이름: <color=yellow>{state.currentPlayerId}</color>    목숨: <color=red>{state.attemptsLeft}</color>";
        }

        if (attemptsText != null)
        {
            attemptsText.text = state.attemptsLeft.ToString();
        }

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
            if (scrollRect != null)
            {
                StartCoroutine(ForceScrollToTop(scrollRect));
            }
        }
    }

    private IEnumerator ForceScrollToTop(ScrollRect scrollRect)
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

    // ▼▼▼ 이동 버튼 (코루틴 호출) ▼▼▼
    public void OnElevatorGo()
    {
        if (GameManager.Instance == null || isMoving) return;

        if (int.TryParse(currentElevatorInput, out int newFloor))
        {
            StartCoroutine(ProcessFloorMoveRoutine(newFloor));
        }
        else
        {
            ShowInteractionMessage("올바른 층을 입력하세요.");
            OnElevatorClear();
        }
    }

    // ▼▼▼ [수정됨] 3초 대기 후 이동 + 문구 변경 ▼▼▼
    private IEnumerator ProcessFloorMoveRoutine(int newFloor)
    {
        isMoving = true;

        int totalFloors = GameManager.Instance.gameState.gameFloors.Count;

        // 1. 없는 층 체크
        if (newFloor < 1 || newFloor > totalFloors)
        {
            ShowInteractionMessage("존재하지 않는 층입니다.");
            OnElevatorClear();
            isMoving = false;
            yield break;
        }

        // 2. 함정 층 (22층)
        if (newFloor == 22)
        {
            GameManager.Instance.ChangeFloor(newFloor);
            CloseAllPanels();

            // 함정 메시지 출력
            ShowInteractionMessage("22층은 함정입니다! 탈락!");

            // 3초 대기 (읽을 시간 줌)
            yield return new WaitForSeconds(3.0f);

            SceneManager.LoadScene("GameOverScene");
            yield break;
        }

        // 3. 이미 클리어한 층 체크 (재방문 금지)
        bool isAlreadyCleared = GameManager.Instance.gameState.IsFloorCleared(newFloor);
        bool isLobbyOrRest = (newFloor == 1 || newFloor == 7);

        if (!isLobbyOrRest && isAlreadyCleared)
        {
            ShowInteractionMessage($"이미 클리어한 {newFloor}층에는 다시 갈 수 없습니다!");
            OnElevatorClear();
            isMoving = false;
            yield break;
        }

        // 4. 정상 이동
        GameManager.Instance.ChangeFloor(newFloor);
        CloseAllPanels();

        // [수정됨] 문구 변경: "도착했습니다" -> "이동합니다"
        ShowInteractionMessage($"{newFloor}층으로 이동합니다.");

        // 3초 대기 (이동하는 느낌)
        yield return new WaitForSeconds(3.0f);

        // 5. 씬 이동
        if (newFloor == 1)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            SceneManager.LoadScene("FloorScene");
        }

        isMoving = false;
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

            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageCoroutine = StartCoroutine(HideMessageRoutine(3.0f));
        }
    }

    private IEnumerator HideMessageRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideInteractionMessage();
    }

    public void HideInteractionMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    public bool IsTyping()
    {
        if (answerInput != null && answerInput.isFocused) return true;
        if (memoInputField != null && memoInputField.isFocused) return true;

        return false;
    }
}