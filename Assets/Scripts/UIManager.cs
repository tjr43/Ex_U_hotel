using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Buttons")]
    public Button submitButton;
    public Button changeFloorButton;

    [Header("Panel References")]
    public GameObject quizPanel; // 퀴즈 패널 (메인 2D UI)
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
    }

    private void Start()
    {
        // 씬 로드 후 UI 초기화 (GameScene에 붙어있다고 가정)
        UpdateUI();
    }

    // --- 게임 상태 갱신 ---
    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        // 1. 공통 정보 업데이트
        playerNameText.text = state.currentPlayerId;
        attemptsText.text = state.attemptsLeft.ToString();

        // 2. 퀴즈/이동 패널 상태 결정
        int currentFloor = state.currentFloor;

        // 층 데이터가 유효한지 확인
        if (currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

        Floor currentFloorData = state.gameFloors[currentFloor - 1];
        bool isCleared = state.IsFloorCleared(state.currentPlayerId, currentFloor);
        bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

        // 3. 퀴즈/이동 UI 텍스트 업데이트
        if (isLobbyOrRest)
        {
            quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
            quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요. (여기서는 정답을 제출할 수 없습니다.)";
        }
        else if (isCleared)
        {
            quizDescriptionText.text = $"{currentFloor}층 (클리어)";
            quizRiddleText.text = "이미 클리어한 층입니다. 다른 층으로 이동하세요.";
        }
        else
        {
            // 퀴즈 층 (미클리어)
            Trap trap = currentFloorData.traps[0];
            quizDescriptionText.text = $"--- {currentFloor}층입니다. --- [방송] {trap.description}";
            quizRiddleText.text = $"[문제] {trap.riddle}";
        }

        // 4. 버튼 활성화/비활성화 (로비/휴식/클리어 상태에서는 퀴즈 제출 비활성화)
        bool canSubmit = !isCleared && !isLobbyOrRest;
        answerInput.gameObject.SetActive(canSubmit);
        submitButton.gameObject.SetActive(canSubmit);

        // 1층, 7층, 클리어 층에서는 층 이동 가능
        bool canChangeFloor = isCleared || isLobbyOrRest;
        floorInput.gameObject.SetActive(canChangeFloor);
        changeFloorButton.gameObject.SetActive(canChangeFloor);
    }

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }

    // --- 버튼 이벤트 핸들러 (유니티 버튼 OnClick에 연결) ---
    public void OnStartGameButton(TMP_InputField inputField)
    {
        string playerName = inputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            // 이 메시지는 Start 씬의 UI에 표시되어야 합니다.
            Debug.Log("이름을 입력해주세요.");
            return;
        }

        // 로드 및 초기화는 이미 GameManager의 Awake에서 호출되었다고 가정합니다.
        // Start 씬 전용 GameManager를 만들어 LoadGameAndInitialize()를 호출하는 것이 좋습니다.

        GameManager.Instance.StartNewPlayer(playerName);
    }

    public void OnSubmitAnswerButton()
    {
        string answer = answerInput.text;
        GameManager.Instance.SubmitAnswer(answer);
        answerInput.text = ""; // 입력 필드 초기화
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
        floorInput.text = ""; // 입력 필드 초기화
    }

    public void OnExitGameButton()
    {
        GameManager.Instance.SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    // --- 모달/패널 제어 ---
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
            TransitionManager.Instance.SetUIMode(false);
        }
    }

    // --- 메모 리스트 렌더링 ---
    private void UpdateMemoList()
    {
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

            // 메모 항목 프리팹이 미리 준비되어 있어야 합니다.
            GameObject item = Instantiate(memoItemPrefab, memoListContent);

            // 프리팹 내의 텍스트 컴포넌트들을 찾아서 설정합니다.
            // 프리팹 구조에 따라 GetComponentsInChildren 대신 직접 참조하는 변수를 사용하는 것이 더 좋습니다.
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = record.playerId;
                texts[1].text = $"\"{record.memo}\"";
            }
        }
    }
}
