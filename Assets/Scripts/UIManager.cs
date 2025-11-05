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
    public TMP_InputField floorInput;
    public TMP_InputField answerInput;
    public TMP_InputField memoInputField; // Win/GameOver 씬에서 사용

    [Header("Buttons")]
    public Button submitButton;
    public Button changeFloorButton;

    [Header("Panel References")]
    public GameObject quizPanel;
    public GameObject memoPanel;
    public GameObject rulesPanel;

    [Header("Memo List Components")]
    public RectTransform memoListContent;
    public GameObject memoItemPrefab; // 메모 항목 프리팹 (이름, 메모 텍스트 포함)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 씬(Scene)이 바뀌어도 UI 관리자가 파괴되지 않도록 설정
        // (GameManager와 함께)
        // DontDestroyOnLoad(gameObject); // -> GameManager가 이미 DontDestroyOnLoad이므로, UI는 씬마다 새로 로드되는 것이 나을 수 있습니다. 씬 전환 시 오류가 나면 이 부분을 주석 처리합니다.
    }

    private void Start()
    {
        // 씬(Scene)이 로드될 때 UI를 즉시 갱신합니다.
        // 단, GameScene이 아닐 경우 (StartScene 등) GameManager가 null일 수 있습니다.
        if (GameManager.Instance != null && GameManager.Instance.gameState != null)
        {
            UpdateUI();
        }

        // 모든 패널을 숨기고 시작
        if (quizPanel != null) quizPanel.SetActive(false);
        if (memoPanel != null) memoPanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    // --- 게임 상태 갱신 ---
    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        // 1. 공통 정보 (HUD) 업데이트
        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = state.attemptsLeft.ToString();

        // 2. 퀴즈/이동 패널 상태 결정 (GameScene 전용)
        int currentFloor = state.currentFloor;
        if (quizRiddleText == null) return; // GameScene이 아니면 중단

        // 층 데이터가 유효한지 확인
        if (currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

        Floor currentFloorData = state.gameFloors[currentFloor - 1];
        bool isCleared = state.IsFloorCleared(state.currentPlayerId, currentFloor);
        bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

        // 3. 퀴즈/이동 UI 텍스트 업데이트
        if (isLobbyOrRest)
        {
            if (quizDescriptionText != null) quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
            if (quizRiddleText != null) quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요. (여기서는 정답을 제출할 수 없습니다.)";
        }
        else if (isCleared)
        {
            if (quizDescriptionText != null) quizDescriptionText.text = $"{currentFloor}층 (클리어)";
            if (quizRiddleText != null) quizRiddleText.text = "이미 클리어한 층입니다. 다른 층으로 이동하세요.";
        }
        else
        {
            // 퀴즈 층 (미클리어)
            if (currentFloorData.traps != null && currentFloorData.traps.Count > 0)
            {
                Trap trap = currentFloorData.traps[0];
                if (quizDescriptionText != null) quizDescriptionText.text = $"--- {currentFloor}층입니다. --- [방송] {trap.description}";
                if (quizRiddleText != null) quizRiddleText.text = $"[문제] {trap.riddle}";
            }
        }

        // 4. 버튼 활성화/비활성화 (로비/휴식/클리어 상태에서는 퀴즈 제출 비활성화)
        bool canSubmit = !isCleared && !isLobbyOrRest;
        if (answerInput != null) answerInput.gameObject.SetActive(canSubmit);
        if (submitButton != null) submitButton.gameObject.SetActive(canSubmit);

        // 1층, 7층, 클리어 층에서는 층 이동 가능
        bool canChangeFloor = isCleared || isLobbyOrRest;
        if (floorInput != null) floorInput.gameObject.SetActive(canChangeFloor);
        if (changeFloorButton != null) changeFloorButton.gameObject.SetActive(canChangeFloor);
    }

    public void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
    }

    // --- 버튼 이벤트 핸들러 (유니티 버튼 OnClick에 연결) ---

    // (StartSceneUI.cs로 이동됨)
    // public void OnStartGameButton(TMP_InputField inputField) { ... }

    public void OnSubmitAnswerButton()
    {
        string answer = answerInput.text;
        GameManager.Instance.SubmitAnswer(answer);
        if (answerInput != null) answerInput.text = ""; // 입력 필드 초기화
    }

    public void OnChangeFloorButton()
    {
        if (int.TryParse(floorInput.text, out int newFloor))
        {
            GameManager.Instance.ChangeFloor(newFloor);
        }
        else
        {
            ShowMessage("유효한 층 번호를 숫자로 입력하세요.");
        }
        if (floorInput != null) floorInput.text = ""; // 입력 필드 초기화
    }

    public void OnExitGameButton()
    {
        GameManager.Instance.SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    // --- WinScene / GameOverScene 버튼 핸들러 ---

    // 성공 (WinScene) 버튼 클릭 시
    public void OnSubmitSuccessMemo()
    {
        string memo = (memoInputField != null) ? memoInputField.text : "";
        GameManager.Instance.SaveMemoAndExit(memo, "success");
    }

    // 실패 (GameOverScene) 버튼 클릭 시
    public void OnSubmitFailMemo()
    {
        string memo = (memoInputField != null) ? memoInputField.text : "";
        GameManager.Instance.SaveMemoAndExit(memo, "fail");
    }


    // --- 모달/패널 제어 ---

    // (PlayerInteraction.cs에서 호출됨)
    public void ShowQuizPanel()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
            UpdateUI(); // 퀴즈 패널을 띄울 때 최신 정보로 갱신
            TransitionManager.Instance.SetUIMode(true); // 1인칭 모드 끄기
        }
    }

    // (퀴즈 제출 성공/실패 시 GameManager에서 호출됨)
    public void HideQuizPanel()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
            TransitionManager.Instance.SetUIMode(false); // 1인칭 모드 켜기
        }
    }

    public void OnShowMemoButton()
    {
        if (memoPanel != null)
        {
            memoPanel.SetActive(true);
            UpdateMemoList();
            TransitionManager.Instance.SetUIMode(true);
        }
    }

    public void OnShowRulesButton()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            TransitionManager.Instance.SetUIMode(true);
        }
    }

    // 이 함수는 모달 닫기 버튼에 연결됩니다.
    public void OnHideModalButton(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            // 퀴즈 패널이 닫힐 때만 1인칭 모드로 복귀
            if (panel == quizPanel)
            {
                TransitionManager.Instance.SetUIMode(false);
            }
            // (메모/규칙 패널이 닫힐 때는 퀴즈 패널이 뒤에 열려있을 수 있으므로, 1인칭 모드를 켜지 않음)
            // -> 만약 메모/규칙이 퀴즈와 별개로 1인칭에서 뜬다면 SetUIMode(false) 필요
        }
    }

    // --- 메모 리스트 렌더링 ---
    private void UpdateMemoList()
    {
        if (memoListContent == null || memoItemPrefab == null) return;

        // 기존 메모 아이템 제거
        foreach (Transform child in memoListContent)
        {
            Destroy(child.gameObject);
        }

        var history = GameManager.Instance.gameState.playerHistory;
        if (history == null || history.Count == 0) return;

        // 최신순으로 표시 (역순)
        for (int i = history.Count - 1; i >= 0; i--)
        {
            PlayerRecord record = history[i];
            if (record == null || string.IsNullOrEmpty(record.memo)) continue;

            GameObject item = Instantiate(memoItemPrefab, memoListContent);

            // 프리팹 내의 텍스트 컴포넌트들을 찾아서 설정합니다.
            // (프리팹 구조에 따라 GetComponentsInChildren 대신 직접 참조가 더 좋음)
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = record.playerId; // 이름
                texts[1].text = $"\"{record.memo}\""; // 메모 내용
            }
        }
    }

    // ⭐️⭐️⭐️⭐️⭐️ 여기부터가 새로 추가된 부분입니다! ⭐️⭐️⭐️⭐️⭐️
    // --- 1인칭 상호작용 메시지 (PlayerInteraction.cs에서 호출) ---

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
        if (messageText != null)
        {
            messageText.text = "";
            // (메시지 텍스트는 ShowMessage에서만 켜지고, 평소엔 꺼져있지 않도록 할 수도 있습니다)
            // (일단은 텍스트를 비우는 것으로 처리)
            // messageText.gameObject.SetActive(false); 
        }
    }
}