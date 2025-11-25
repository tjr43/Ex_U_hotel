using UnityEngine;
using TMPro; // 텍스트 입력을 위해 필수
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField memoInputField; // 플레이어가 메모를 적을 입력창

    [Header("설정 (인스펙터에서 직접 적으세요)")]
    public string resultStatus; // "success" 또는 "fail"을 적을 변수

    // '저장하고 나가기' 버튼에 연결할 함수
    public void OnSubmitButtonPressed()
    {
        string memo = "";

        // 입력창에 내용이 있다면 가져오기
        if (memoInputField != null)
        {
            memo = memoInputField.text;
        }

        // GameManager에게 저장 요청
        if (GameManager.Instance != null)
        {
            Debug.Log($"[EndGameUI] 메모 저장 시도: {memo}, 상태: {resultStatus}");
            GameManager.Instance.SaveMemoAndExit(memo, resultStatus);
        }
        else
        {
            // 비상시 (GameManager가 없을 때)
            Debug.LogError("GameManager가 없습니다! 그냥 종료 씬으로 이동합니다.");
            SceneManager.LoadScene("GoodbyeScene");
        }
    }
}