using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- [수정됨] 이 줄이 추가되어 'Button' 오류를 해결합니다.

// [최종 수정] WinScene/GameOverScene의 메모 입력을 처리하는 기능이 추가되었습니다.

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Components")]
    public TMP_Text playerNameText;
    public TMP_Text attemptsText;
    public TMP_Text quizRiddleText;
    public TMP_Text quizDescriptionText;
    public TMP_Text messageText; // 게임 메시지 출력용

    [Header("Input Fields")]
    public TMP_InputField floorInput;
    public TMP_InputField answerInput;
    public TMP_InputField memoInputField; // [추가됨] Win/GameOverScene의 메모 입력창

    [Header("Buttons")]
    public Button submitButton; // <-- 이제 이 줄의 오류가 사라집니다.
    public Button changeFloorButton;

    [Header("Panel References")]
    public GameObject quizPanel; // 퀴즈 패널 (메인 2D UI)
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

        // 씬이 바뀌어도 UIManager가 파괴되지 않도록 설정 (GameManager와 동일하게)
        // 단, 씬마다 UI가 다르다면 이 로직은 빼고 씬마다 Canvas를 두는 것이 나을 수 있습니다.
        // 여기서는 씬마다 Canvas가 있다고 가정하고 DontDestroyOnLoad를 사용하지 않습니다.
    }

    private void Start() {
        // 씬이 로드될 때마다 UI를 갱신하도록 시도
        // (GameScene이 아닐 경우 playerNameText 등이 null일 수 있으므로 null 체크 필수)
        if (quizPanel == null) // GameScene이 아님 (예: StartScene, WinScene...)
        {
            // WinScene/GameOverScene의 퀴즈 패널은 비활성화
            // (만약 GameScene의 Canvas를 공유한다면)
            if (quizPanel != null) quizPanel.SetActive(false);
            if (memoPanel != null) memoPanel.SetActive(false);
            if (rulesPanel != null) rulesPanel.SetActive(false);
        } else // GameScene일 경우 UI 초기화
          {
            UpdateUI();
            quizPanel.SetActive(false);
            memoPanel.SetActive(false);
            rulesPanel.SetActive(false);
        }
    }

    // --- 게임 상태 갱신 ---
    public void UpdateUI() {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;
        GameState state = GameManager.Instance.gameState;

        // [수정] null 체크 추가 (GameScene이 아닐 때 오류 방지)
        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        int currentFloor = state.currentFloor;
        if (currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

        Floor currentFloorData = state.gameFloors[currentFloor - 1];
        bool isCleared = state.IsFloorCleared(state.currentPlayerId, currentFloor);
        bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

        if (quizDescriptionText == null || quizRiddleText == null) return; // 퀴즈 패널 UI가 없으면 중단

        if (isLobbyOrRest) {
            quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
            quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요. (여기서는 정답을 제출할 수 없습니다.)";
        } else if (isCleared) {
            quizDescriptionText.text = $"{currentFloor}층 (클리어)";
            quizRiddleText.text = "이미 클리어한 층입니다. 다른 층으로 이동하세요.";
        } else if (currentFloorData.traps.Count > 0) // 퀴즈 층 (미클리어)
          {
            Trap trap = currentFloorData.traps[0];
            quizDescriptionText.text = $"--- {currentFloor}층입니다. --- [방송] {trap.description}";
            quizRiddleText.text = $"[문제] {trap.riddle}";
        }

        // 버튼 활성화/비활성화 (null 체크 추가)
        bool canSubmit = !isCleared && !isLobbyOrRest && currentFloorData.traps.Count > 0;
        if (answerInput != null) answerInput.gameObject.SetActive(canSubmit);
        if (submitButton != null) submitButton.gameObject.SetActive(canSubmit);

        bool canChangeFloor = isCleared || isLobbyOrRest;
        if (floorInput != null) floorInput.gameObject.SetActive(canChangeFloor);
        if (changeFloorButton != null) changeFloorButton.gameObject.SetActive(canChangeFloor);
    }

    public void ShowMessage(string msg) {
        if (messageText != null) messageText.text = msg;
    }

    // --- 버튼 이벤트 핸들러 (유니티 버튼 OnClick에 연결) ---
    public void OnStartGameButton(TMP_InputField inputField) {
        string playerName = inputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) {
            ShowMessage("이름을 입력해주세요.");
            return;
        }
        GameManager.Instance.StartNewPlayer(playerName);
    }

    public void OnSubmitAnswerButton() {
        if (answerInput == null) return;
        string answer = answerInput.text;
        GameManager.Instance.SubmitAnswer(answer);
        answerInput.text = ""; // 입력 필드 초기화
    }

    public void OnChangeFloorButton() {
        if (floorInput == null) return;
        if (int.TryParse(floorInput.text, out int newFloor)) {
            GameManager.Instance.ChangeFloor(newFloor);
        } else {
            ShowMessage("유효한 층 번호를 숫자로 입력하세요.");
        }
        floorInput.text = ""; // 입력 필드 초기화
    }

    public void OnExitGameButton() {
        // OnClick()에서 직접 호출 가능 (매개변수 없음)
        GameManager.Instance.SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    // --- [새로 추가된 함수 1] ---
    // WinScene의 "기록 남기기" 버튼이 호출할 함수
    public void OnSubmitSuccessMemo() {
        if (memoInputField != null && GameManager.Instance != null) {
            GameManager.Instance.SaveMemoAndExit(memoInputField.text, "success");
        }
    }

    // --- [새로 추가된 함수 2] ---
    // GameOverScene의 "기록 남기기" 버튼이 호출할 함수
    public void OnSubmitFailMemo() {
        if (memoInputField != null && GameManager.Instance != null) {
            GameManager.Instance.SaveMemoAndExit(memoInputField.text, "fail");
        }
    }


    // --- 모달/패널 제어 ---

    // [수정됨] 1인칭 상호작용이 호출할 퀴즈 패널 '열기' 함수
    public void ShowQuizPanel() {
        if (quizPanel != null) {
            quizPanel.SetActive(true);
            UpdateUI(); // 퀴즈 패널 열 때 최신 정보로 갱신
            if (TransitionManager.Instance != null) {
                TransitionManager.Instance.SetUIMode(true); // 2D UI 모드 활성화
            }
        }
    }

    // [수정됨] 퀴즈 제출 시 호출할 퀴즈 패널 '닫기' 함수
    public void HideQuizPanel() {
        if (quizPanel != null) {
            quizPanel.SetActive(false);
            if (TransitionManager.Instance != null) {
                TransitionManager.Instance.SetUIMode(false); // 1인칭 탐험 모드 복귀
            }
        }
    }

    public void OnShowMemoButton() {
        if (memoPanel != null) {
            memoPanel.SetActive(true);
            UpdateMemoList();
            if (TransitionManager.Instance != null) {
                TransitionManager.Instance.SetUIMode(true);
            }
        }
    }

    public void OnShowRulesButton() {
        if (rulesPanel != null) {
            rulesPanel.SetActive(true);
            if (TransitionManager.Instance != null) {
                TransitionManager.Instance.SetUIMode(true);
            }
        }
    }

    // 이 함수는 모달 닫기 버튼에 연결됩니다.
    public void OnHideModalButton(GameObject panel) {
        if (panel != null) {
            panel.SetActive(false);
            // [수정] 퀴즈 패널이 아닐 때만 1인칭 모드로 복귀하도록 확인
            if (TransitionManager.Instance != null && !quizPanel.activeSelf) {
                TransitionManager.Instance.SetUIMode(false);
            }
        }
    }

    // --- 메모 리스트 렌더링 ---
    private void UpdateMemoList() {
        if (memoListContent == null || memoItemPrefab == null) {
            Debug.LogWarning("Memo List Content 또는 Prefab이 UIManager에 연결되지 않았습니다.");
            return;
        }

        // 기존 메모 아이템 제거
        foreach (Transform child in memoListContent) {
            Destroy(child.gameObject);
        }

        var history = GameManager.Instance.gameState.playerHistory;
        if (history == null || history.Count == 0) return;

        // 최신순으로 표시 (역순)
        for (int i = history.Count - 1; i >= 0; i--) {
            PlayerRecord record = history[i];
            GameObject item = Instantiate(memoItemPrefab, memoListContent);

            // 프리팹 구조에 맞게 텍스트 컴포넌트 설정 (자식에서 2개 찾는다고 가정)
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2) {
                texts[0].text = record.playerId;
                texts[1].text = $"\"{record.memo}\"";
            }
        }
    }
}