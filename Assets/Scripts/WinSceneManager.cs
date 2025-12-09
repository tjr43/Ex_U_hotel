using UnityEngine;
using TMPro;
using System.Collections;

public class WinSceneManager : MonoBehaviour
{
    [Header("UI 연결 (필수)")]
    public TMP_InputField memoInputField;

    private bool isCursorActive = false;

    IEnumerator Start()
    {
        // 1. FloorScene에서 넘어올 때의 로딩 딜레이 고려
        yield return null;

        // 2. 퀴즈 정답 시 멈췄을 수도 있는 시간을 복구 (필수)
        Time.timeScale = 1f;

        // 3. TransitionManager에게 다시 한번 "나 이제 움직일 거야!"라고 신호 보냄
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.FindPlayerReferences();
            TransitionManager.Instance.SetUIMode(false); // false = 이동 모드
        }

        // 4. 초기 상태 설정
        isCursorActive = false;
    }

    void Update()
    {
        // [Left Alt] 키: 이동 모드 <-> 마우스 모드 전환
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            isCursorActive = !isCursorActive;
            SetCursorState(isCursorActive);
        }

        // [Enter] 키: 바로 채팅창 입력 모드
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (memoInputField != null)
            {
                isCursorActive = true;
                SetCursorState(true);
                memoInputField.ActivateInputField();
            }
        }
    }

    void SetCursorState(bool isActive)
    {
        if (TransitionManager.Instance != null)
        {
            // isActive가 true면 UI모드(멈춤), false면 게임모드(이동)
            TransitionManager.Instance.SetUIMode(isActive);
        }
    }
}