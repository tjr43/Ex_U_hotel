using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Image 컴포넌트 제어를 위해 추가
using System.Collections;

public class EndGameUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField memoInputField;
    public TMP_Text endMessageText;
    public GameObject submitButton;

    [Header("설정")]
    public string resultStatus;

    private void Start()
    {
        if (endMessageText != null) endMessageText.gameObject.SetActive(false);
    }

    public void OnSubmitButtonPressed()
    {
        StartCoroutine(EndProcessRoutine());
    }

    IEnumerator EndProcessRoutine()
    {
        // 1. 저장 먼저 (데이터 안전 보장)
        string memo = "";
        if (memoInputField != null) memo = memoInputField.text;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveMemoAndExit(memo, resultStatus);
        }

        // 2. [수정됨] 화면 가리는 UI들 모두 숨기기!
        // 버튼 숨기기
        if (submitButton != null) submitButton.SetActive(false);

        // 입력창도 아예 안 보이게 꺼버리기
        if (memoInputField != null) memoInputField.gameObject.SetActive(false);

        // ★ 핵심: 판넬 배경 이미지도 안 보이게 끄기 (오브젝트는 켜둠)
        Image panelImage = GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.enabled = false;
        }

        // 3. 텍스트만 깔끔하게 띄우기
        if (endMessageText != null)
        {
            endMessageText.text = "호텔을 이용해주셔서 감사합니다.";
            endMessageText.gameObject.SetActive(true);
        }

        // 4. 3초 대기
        yield return new WaitForSeconds(3.0f);

        // 5. 게임 종료
        Debug.Log("게임이 종료됩니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}