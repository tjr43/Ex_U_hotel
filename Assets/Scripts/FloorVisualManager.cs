using UnityEngine;
using System.Collections.Generic;

public class FloorVisualManager : MonoBehaviour
{
    [Header("1. 여기에 벽들을 다 넣으세요")]
    public List<GameObject> wallObjects;

    [Header("2. 여기에 바닥(Plane)을 넣으세요")]
    public GameObject floorObject;

    [Header("3. 원하는 색깔을 30개 정도 만드세요")]
    public List<Color> floorColors;

    void Start()
    {
        ChangeFloorColor();
    }

    void ChangeFloorColor()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        int floor = GameManager.Instance.gameState.currentFloor;

        // ▼▼▼ [핵심 수정] 1층이면 색깔 바꾸지 말고 바로 종료! ▼▼▼
        if (floor == 1) return;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        if (floorColors == null || floorColors.Count == 0) return;

        // 색상 선택 로직 (2층부터 색상이 변하도록 floor - 2 사용)
        // 2층 -> 0번 색, 3층 -> 1번 색 ...
        int colorIndex = (floor - 2) % floorColors.Count;

        // 인덱스가 음수가 되지 않도록 방어 (혹시 모를 오류 방지)
        if (colorIndex < 0) colorIndex = 0;

        Color targetColor = floorColors[colorIndex];

        // 1. 벽 색상 변경
        foreach (GameObject wall in wallObjects)
        {
            if (wall != null)
            {
                Renderer[] rends = wall.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rends)
                {
                    r.material.color = targetColor;
                }
            }
        }

        // 2. 바닥 색상 변경
        if (floorObject != null)
        {
            Renderer floorRend = floorObject.GetComponent<Renderer>();
            if (floorRend != null)
            {
                floorRend.material.color = targetColor * 0.9f;
            }
        }

        Debug.Log($"{floor}층 테마 적용 완료 (색상 인덱스: {colorIndex})");
    }
}