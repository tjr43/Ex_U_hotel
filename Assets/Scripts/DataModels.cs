using System;
using System.Collections.Generic; // List<>를 사용하기 위해 필수입니다.

// 1. GameState 클래스 (빠진 함수 추가)
[System.Serializable]
public class GameState
{
    public int currentFloor;
    public string currentPlayerId;
    public List<Floor> gameFloors;
    public int attemptsLeft;
    public List<int> clearedFloors;
    public List<PlayerRecord> playerHistory;

    // StartGame 함수가 호출할 생성자
    public GameState(int currentFloor, string currentPlayerId, List<Floor> gameFloors, int attemptsLeft) {
        this.currentFloor = currentFloor;
        this.currentPlayerId = currentPlayerId;
        this.gameFloors = gameFloors;
        this.attemptsLeft = attemptsLeft;
        this.clearedFloors = new List<int>();
        this.playerHistory = new List<PlayerRecord>();
    }

    // ▼▼▼ [오류 3 해결] UIManager.cs가 사용할 IsFloorCleared 함수 ▼▼▼
    public bool IsFloorCleared(int floorNum) {
        // clearedFloors 리스트에 이 층 번호가 포함되어 있는지 확인
        return clearedFloors != null && clearedFloors.Contains(floorNum);
    }
    // ▲▲▲ [빠진 함수 추가 완료] ▲▲▲
}

// 2. Floor 클래스 (빠진 생성자 추가)
[System.Serializable]
public class Floor
{
    public int floor;
    public string name;
    public List<Trap> traps;

    // ▼▼▼ [오류 2 해결] GameDataInitializer.cs가 사용할 생성자 ▼▼▼
    public Floor(int floorNum, List<Trap> trapsList) {
        this.floor = floorNum;
        this.name = (floorNum == 1) ? "1층 로비" : $"{floorNum}층";
        this.traps = trapsList;
    }
    // ▲▲▲ [빠진 생성자 추가 완료] ▲▲▲
}

// 3. Trap 클래스 (빠진 생성자 추가)
[System.Serializable]
public class Trap
{
    public string type;
    public string description;
    public string riddle;
    public string answer;
    public string hint;

    // ▼▼▼ [오류 2 해결] GameDataInitializer.cs가 사용할 생성자 ▼▼▼
    public Trap(string type, string riddle, string answer) {
        this.type = type;
        this.riddle = riddle;
        this.answer = answer;
        this.description = ""; // 기본값
        this.hint = ""; // 기본값
    }
    // ▲▲▲ [빠진 생성자 추가 완료] ▲▲▲
}

// 4. PlayerRecord 클래스
[System.Serializable]
public class PlayerRecord
{
    public string playerId;
    public string status; // "success" or "fail"
    public string memo;
    public string timestamp;
}