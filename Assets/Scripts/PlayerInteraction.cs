using UnityEngine;
using TMPro; // UIManager의 텍스트를 사용하기 위해 필요할 수 있음

public class PlayerInteraction : MonoBehaviour
{
    // 1인칭 카메라를 연결할 슬롯 (레이를 쏘기 위해)
    [SerializeField] private Camera playerCamera;

    // 레이(Ray)의 최대 사정거리
    [SerializeField] private float interactionDistance = 3f;

    // 상호작용 메시지를 띄울 UIManager 참조
    private UIManager uiManager;

    private void Start()
    {
        // 씬(Scene)에 있는 UIManager를 찾음 (GameScene의 Canvas에 있어야 함)
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (playerCamera == null || uiManager == null) return;

        // 카메라 정중앙에서 레이(Ray)를 쏨
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo; // 레이에 맞은 물체 정보

        bool hitSomething = Physics.Raycast(ray, out hitInfo, interactionDistance);

        if (hitSomething)
        {
            // 레이가 QuizTrigger 태그가 붙은 물체에 닿았는지 확인
            if (hitInfo.collider.CompareTag("QuizTrigger"))
            {
                // UIManager에 메시지 표시 요청
                uiManager.ShowInteractionMessage("E 키를 눌러 퀴즈 시작");

                // 'E' 키를 눌렀는지 확인
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // UIManager에 퀴즈 패널을 띄우라고 명령
                    uiManager.ShowQuizPanel();
                }
            }
            else
            {
                // 다른 물체에 닿았을 때는 메시지 숨김
                uiManager.HideInteractionMessage();
            }
        }
        else
        {
            // 레이가 아무것에도 닿지 않았을 때 메시지 숨김
            uiManager.HideInteractionMessage();
        }
    }
}