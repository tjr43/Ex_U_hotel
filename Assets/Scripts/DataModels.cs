using System;
using System.Collections.Generic;
using UnityEngine; // [System.Serializable]을 위해 필요

// [수정됨] 이 파일은 게임의 모든 데이터 구조를 정의합니다.
// GameManager.cs에서 이 클래스들을 사용합니다.

[System.Serializable]
public class Trap
{
    public string description;
    public string riddle;
    public string answer;

    public Trap(string description, string riddle, string answer)
    {
        this.description = description;
        this.riddle = riddle;
        this.answer = answer;
    }
}

[System.Serializable]
public class Floor
{
    public int floorNumber;
    public List<Trap> traps;

    public Floor(int floorNumber, List<Trap> traps)
    {
        this.floorNumber = floorNumber;
        this.traps = traps;
    }
}

[System.Serializable]
public class PlayerRecord
{
    public string playerId;
    public int floorReached;
    public string memo;
    public string status;

    public PlayerRecord(string playerId, int floorReached, string memo, string status)
    {
        this.playerId = playerId;
        this.floorReached = floorReached;
        this.memo = memo;
        this.status = status;
    }
}

[System.Serializable]
public class GameState
{
    public int currentFloor;
    public string currentPlayerId;
    public int attemptsLeft;

    public List<PlayerRecord> playerHistory;
    public List<Floor> gameFloors;
    public List<string> allPlayers;

    // [중요] JsonUtility는 Dictionary<string, Set<int>>를 직접 직렬화하지 못합니다.
    // 따라서 직렬화가 가능한 List<string> 형태로 데이터를 저장해야 합니다.
    // 예: "PlayerName,1", "PlayerName,3", "PlayerName,5"
    public List<string> clearedFloorsList;

    public GameState(int currentFloor, string currentPlayerId, List<Floor> gameFloors, int attemptsLeft)
    {
        this.currentFloor = currentFloor;
        this.currentPlayerId = currentPlayerId;
        this.gameFloors = gameFloors;
        this.attemptsLeft = attemptsLeft;

        this.playerHistory = new List<PlayerRecord>();
        this.allPlayers = new List<string>();
        this.clearedFloorsList = new List<string>(); // 초기화
    }

    // --- [추가된 함수 1] ---
    // 플레이어가 특정 층을 클리어했는지 확인하는 함수
    public bool IsFloorCleared(string playerId, int floor)
    {
        if (clearedFloorsList == null) return false;

        // "PlayerName,FloorNumber" 형태의 문자열을 찾습니다.
        string record = $"{playerId},{floor}";
        return clearedFloorsList.Contains(record);
    }

    // --- [추가된 함수 2] ---
    // (GameManager.cs가 호출하는) 클리어한 층을 기록에 추가하는 함수
    public void AddClearedFloor(string playerId, int floor)
    {
        if (clearedFloorsList == null)
        {
            clearedFloorsList = new List<string>();
        }

        string record = $"{playerId},{floor}";
        if (!clearedFloorsList.Contains(record))
        {
            clearedFloorsList.Add(record);
        }
    }
}

