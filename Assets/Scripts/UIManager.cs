using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using UnityEngine.UI; // Button을 사용하기 위해 필요

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Components (HUD)")]
    public TMP_Text playerNameText;
    public TMP_Text attemptsText;
    public TMP_Text messageText; // 각종 알림 메시지

    [Header("UI Text Components (Quiz Panel)")]
    public TMP_Text quizRiddleText;
    public TMP_Text quizDescriptionText;

    [Header("Input Fields")]
    // public TMP_InputField floorInput; // [삭제됨]
    public TMP_InputField answerInput;
    public TMP_InputField memoInputField; // Win/GameOver 씬에서 사용

    [Header("Buttons")]
    public Button submitButton;
    // public Button changeFloorButton; // [삭제됨]

    // --- ▼ [새로 추가된 부분] 엘리베이터 UI ---
    [Header("Elevator UI")]
    [Tooltip("숫자패드, 이동 버튼 등을 포함하는 부모 패널")]
    public GameObject elevatorPanel;
    [Tooltip("입력한 층 번호가 표시될 텍스트")]
    public TMP_Text elevatorDisplay;
    private string currentElevatorInput = ""; // 현재 입력된 층 번호
    // --- ▲ [새로 추가된 부분] ---

    [Header("Panel References")]
    public GameObject quizPanel;
    public GameObject memoPanel;
    public GameObject rulesPanel;

    [Header("Memo List Components")]
    public RectTransform memoListContent;
    public GameObject memoItemPrefab; // 메모 항목 프리팹 (이름, 메모 텍스트 포함)

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

        // 모든 패널을 숨기고 시작
        if (quizPanel != null) quizPanel.SetActive(false);
        if (memoPanel != null) memoPanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);

        // --- ▼ [새로 추가된 부분] 엘리베이터 UI 초기화 ---
        if (elevatorDisplay != null) elevatorDisplay.text = "";
        currentElevatorInput = "";
        // elevatorPanel 자체는 UpdateUI에서 상태에 따라 켜고 끕니다.
        // --- ▲ [새로 추가된 부분] ---
    }

    // --- 게임 상태 갱신 ---
    public void UpdateUI() {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        // 1. 공통 정보 (HUD) 업데이트
        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        // 2. 퀴즈/이동 패널 상태 결정 (GameScene 전용)
        int currentFloor = state.currentFloor;
        if (quizRiddleText == null) return; // GameScene이 아니면 중단

        // (수정) 층 정보가 로드되지 않았을 수 있으므로 방어 코드 추가
        if (state.gameFloors == null || state.gameFloors.Count == 0 || currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

        Floor currentFloorData = state.gameFloors[currentFloor - 1];

        // --- ▼ [CS1501 오류 수정] ---
        // 'IsFloorCleared' 함수는 1개의 인수(currentFloor)만 받습니다. (DataModels.cs 기준)
        // 2개의 인수를 전달하던 부분을 수정합니다.
        bool isCleared = state.IsFloorCleared(currentFloor); // <-- (state.currentPlayerId 제거)
        // --- ▲ [오류 수정 완료] ---

        bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

        // 3. 퀴즈 UI 텍스트 업데이트
        if (isLobbyOrRest) {
            if (quizDescriptionText != null) quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
            if (quizRiddleText != null) quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요. (여기서는 정답을 제출할 수 없습니다.)";
        } else if (isCleared) {
            if (quizDescriptionText != null) quizDescriptionText.text = $"{currentFloor}층 (클리어)";
            if (quizRiddleText != null) quizRiddleText.text = "이미 클리어한 층입니다. 다른 층으로 이동하세요.";
        } else {
            // 퀴즈 층 (미클리어)
            if (currentFloorData.traps != null && currentFloorData.traps.Count > 0) {
                Trap trap = currentFloorData.traps[0];
                if (quizDescriptionText != null) quizDescriptionText.text = $"--- {currentFloor}층입니다. --- [방송] {trap.description}";
                if (quizRiddleText != null) quizRiddleText.text = $"[문제] {trap.riddle}";
            }
        }

        // 4. 버튼 활성화/비활성화 (로비/휴식/클리어 상태에서는 퀴즈 제출 비활성화)
        bool canSubmit = !isCleared && !isLobbyOrRest;
        if (answerInput != null) answerInput.gameObject.SetActive(canSubmit);
        if (submitButton != null) submitButton.gameObject.SetActive(canSubmit);

        // --- ▼ [수정된 부분] 엘리베이터 UI 활성화/비활성화 ---
        // 1층, 7층, 클리어 층에서는 층 이동(엘리베이터) 가능
        bool canChangeFloor = isCleared || isLobbyOrRest;
        if (elevatorPanel != null) elevatorPanel.SetActive(canChangeFloor);
        // --- ▲ [수정된 부분] ---
    }

    public void ShowMessage(string msg) {
        if (messageText != null) {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }

    // --- 버튼 이벤트 핸들러 (유니티 버튼 OnClick에 연결) ---

    public void OnSubmitAnswerButton() {
        if (GameManager.Instance == null || answerInput == null) return;
        string answer = answerInput.text;
        GameManager.Instance.SubmitAnswer(answer);
        if (answerInput != null) answerInput.text = ""; // 입력 필드 초기화
    }

    // [삭제됨] public void OnChangeFloorButton() { ... }

    // --- ▼ [새로 추가된 부분] 엘리베이터 버튼 함수 ---

    [Tooltip("엘리베이터 숫자(0~9) 버튼을 눌렀을 때 호출됩니다.")]
    public void OnElevatorNumpadPress(string digit) {
        // 층 번호는 2자리까지만 입력받음 (1~30층)
        if (currentElevatorInput.Length < 2) {
            currentElevatorInput += digit;
            if (elevatorDisplay != null) elevatorDisplay.text = currentElevatorInput;
        }
    }

    [Tooltip("엘리베이터 'C' (지우기) 버튼을 눌렀을 때 호출됩니다.")]
    public void OnElevatorClear() {
        currentElevatorInput = "";
        if (elevatorDisplay != null) elevatorDisplay.text = "";
    }

    [Tooltip("엘리베이터 'GO' (이동) 버튼을 눌렀을 때 호출됩니다.")]
    public void OnElevatorGo() {
        if (GameManager.Instance == null) return;

        if (int.TryParse(currentElevatorInput, out int newFloor)) {
            GameManager.Instance.ChangeFloor(newFloor);
        } else {
            ShowMessage("유효한 층 번호를 입력하세요.");
        }

        // 입력 초기화
        OnElevatorClear();
    }
    // --- ▲ [새로 추가된 부분] ---


    public void OnExitGameButton() {
        if (GameManager.Instance == null) return;
        GameManager.Instance.SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    // --- WinScene / GameOverScene 버튼 핸들러 ---

    public void OnSubmitSuccessMemo() {
        if (GameManager.Instance == null) return;
        string memo = (memoInputField != null) ? memoInputField.text : "";
        GameManager.Instance.SaveMemoAndExit(memo, "success");
    }

    public void OnSubmitFailMemo() {
        if (GameManager.Instance == null) return;
        string memo = (memoInputField != null) ? memoInputField.text : "";
        GameManager.Instance.SaveMemoAndExit(memo, "fail");
    }


    // --- 모달/패널 제어 ---

    public void ShowQuizPanel() {
        if (quizPanel != null) {
            quizPanel.SetActive(true);
            UpdateUI();
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(true);
        }
    }

    public void HideQuizPanel() {
        if (quizPanel != null) {
            quizPanel.SetActive(false);
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(false);
        }
    }

    public void OnShowMemoButton() {
        if (memoPanel != null) {
            memoPanel.SetActive(true);
            UpdateMemoList();
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(true);
        }
    }

    public void OnShowRulesButton() {
        if (rulesPanel != null) {
            rulesPanel.SetActive(true);
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(true);
        }
    }

    public void OnHideModalButton(GameObject panel) {
        if (panel != null) {
            panel.SetActive(false);

            // 퀴즈 패널, 메모 패널, 규칙 패널 중 하나라도 닫히면
            // 1인칭 모드로 돌아가도록 시도
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.SetUIMode(false);
        }
    }

    // --- 메모 리스트 렌더링 ---
    private void UpdateMemoList() {
        if (memoListContent == null || memoItemPrefab == null) return;
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        foreach (Transform child in memoListContent) {
            Destroy(child.gameObject);
        }

        var history = GameManager.Instance.gameState.playerHistory;
        if (history == null || history.Count == 0) return;

        for (int i = history.Count - 1; i >= 0; i--) {
            PlayerRecord record = history[i];
            if (record == null || string.IsNullOrEmpty(record.memo)) continue;

            GameObject item = Instantiate(memoItemPrefab, memoListContent);

            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2) {
                texts[0].text = record.playerId; // 이름
                texts[1].text = $"\"{record.memo}\""; // 메모 내용
            }
        }
    }

    // --- 1인칭 상호작용 메시지 (PlayerInteraction.cs에서 호출) ---
    public void ShowInteractionMessage(string msg) {
        if (messageText != null) {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }

    public void HideInteractionMessage() {
        if (messageText != null) {
            // 비워두기만 하고 끄지는 않습니다.
            messageText.text = "";
        }
    }
}