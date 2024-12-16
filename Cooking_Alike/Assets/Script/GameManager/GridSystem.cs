using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System;
public class GridSystem : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int rows = 10;
    public int columns = 10;
    public float cellSize = 1f;
    public Vector2 gridOffset = Vector2.zero;

    [Header("Game Objects")]
    public GameObject[] characterPrefabs;

    [Header("UI Elements")]
    public Text timerText;
    public Text scoreText;

    [Header("Game Settings")]
    public float gameDuration = 180f;

    // Quản lý trạng thái game
    private float currentTime;
    private bool isGameRunning = false;
    private int score = 0;

    // Ma trận grid
    [SerializeField] private GameObject[,] gridItems;
    [SerializeField] private Vector2[,] gridPositions;

    // Quản lý chọn character
    [SerializeField] private GameObject firstSelectedCharacter = null;
    private Vector2Int firstSelectedPosition;
    private bool isFirstCharacterSelected = false;

    void Start()
    {
        InitializeGrid();
        StartGame();
    }

    // Khởi tạo grid với offset tuỳ chỉnh
    void InitializeGrid()
    {
        gridItems = new GameObject[rows, columns];
        gridPositions = new Vector2[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Tính toán vị trí chính xác của mỗi ô
                Vector2 cellPosition = new Vector2(
                    gridOffset.x + col * cellSize,
                    gridOffset.y + row * cellSize
                );
                gridPositions[row, col] = cellPosition;
            }
        }
    }

    void Update()
    {
        if (isGameRunning)
        {
            UpdateGameTime();
            HandleCharacterSelection();
        }
    }

    void UpdateGameTime()
    {
        currentTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (currentTime <= 0)
        {
            EndGame(false);
        }
    }

    void HandleCharacterSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int selectedPosition = GetGridPositionFromMouseClick();

            if (IsValidGridPosition(selectedPosition))
            {
                GameObject selectedCharacter = gridItems[selectedPosition.y, selectedPosition.x];

                if (selectedCharacter != null)
                {
                    ProcessCharacterSelection(selectedPosition, selectedCharacter);
                }
            }
        }
    }

    // Chuyển đổi toạ độ chuột sang vị trí grid
    Vector2Int GetGridPositionFromMouseClick()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return FindNearestGridCell(mousePosition);
    }

    // Tìm ô grid gần nhất với vị trí chuột
    Vector2Int FindNearestGridCell(Vector3 mousePosition)
    {
        float minDistance = float.MaxValue;
        Vector2Int nearestCell = new Vector2Int(-1, -1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 cellCenter = gridPositions[row, col];

                float distance = Vector2.Distance(
                    new Vector2(mousePosition.x, mousePosition.y),
                    cellCenter
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCell = new Vector2Int(col, row);
                }
            }
        }

        return nearestCell;
    }

    void ProcessCharacterSelection(Vector2Int selectedPosition, GameObject selectedCharacter)
    {
       
        if (!isFirstCharacterSelected)
        {
            firstSelectedCharacter = selectedCharacter;
            firstSelectedPosition = selectedPosition;
            isFirstCharacterSelected = true;

            HighlightCharacter(firstSelectedCharacter, true);
        }
        else
        {
            HighlightCharacter(firstSelectedCharacter, true);

            if (CanMatchCharacters(firstSelectedPosition, selectedPosition))
            {
                RemoveMatchedCharacters(firstSelectedPosition, selectedPosition);
                UpdateScore();
            }
            else
            {
                HighlightCharacter(firstSelectedCharacter, false);
            }

            firstSelectedCharacter = null;
            isFirstCharacterSelected = false;
        }
    }

    // Kiểm tra xem có thể match 2 character không
    bool CanMatchCharacters(Vector2Int pos1, Vector2Int pos2)
    {
        // Kiểm tra cùng loại character
        if (gridItems[pos1.y, pos1.x] != gridItems[pos2.y, pos2.x])
            return false;

        // Kiểm tra đường đi giữa 2 điểm
        return FindConnectionPath(pos1, pos2);
    }

    // Loại bỏ các character đã match
    void RemoveMatchedCharacters(Vector2Int pos1, Vector2Int pos2)
    {
        Debug.Log("Destroy");
        // Phá huỷ game object
        Destroy(gridItems[pos1.y, pos1.x]);
        Destroy(gridItems[pos2.y, pos2.x]);

        // Đánh dấu ô trống trong grid
        gridItems[pos1.y, pos1.x] = null;
        gridItems[pos2.y, pos2.x] = null;

        // Kiểm tra kết thúc game nếu không còn character
        CheckGameCompletion();
    }

    // Highlight character khi được chọn
    void HighlightCharacter(GameObject character, bool isHighlighted)
    {
        // Thay đổi màu sắc hoặc scale để highlight
        if (isHighlighted)
        {
            character.transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            character.transform.localScale = Vector3.one;
        }
    }

    // Bắt đầu game
    public void StartGame()
    {
        // Reset thời gian
        currentTime = gameDuration;
        isGameRunning = true;

        // Tạo map
        GenerateRandomMap(1);
    }
    // Cập nhật điểm số
    void UpdateScore()
    {
        score += 10;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    // Kiểm tra kết thúc game
    void CheckGameCompletion()
    {
        bool allCharactersMatched = true;

        // Kiểm tra xem còn character nào không
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridItems[row, col] != null)
                {
                    allCharactersMatched = false;
                    break;
                }
            }

            if (!allCharactersMatched) break;
        }

        // Nếu đã match hết tất cả
        if (allCharactersMatched)
        {
            EndGame(true);
        }
    }

    // Kiểm tra vị trí grid có hợp lệ không
    bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < columns &&
               position.y >= 0 && position.y < rows;
    }

    // Cập nhật hiển thị thời gian
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Kết thúc game
    private void EndGame(bool isWin)
    {
        isGameRunning = false;

        if (isWin)
        {
            Debug.Log("Chiến thắng!");
            // Xử lý khi thắng game
        }
        else
        {
            Debug.Log("Hết giờ!");
            // Xử lý khi thua game
        }
    }

    // Thuật toán tạo map với các hình dạng grid linh hoạt
    
    // Sinh map với hỗ trợ offset
    public void GenerateRandomMap(int difficulty)
    {
        // Xoá các item cũ nếu có
        ClearExistingGrid();

        List<GameObject> availableItems = new List<GameObject>();
        int totalPairs = (rows * columns) / 2;

        // Tạo các cặp character
        for (int i = 0; i < totalPairs; i++)
        {
            GameObject selectedCharacter = characterPrefabs[UnityEngine.Random.Range(0, characterPrefabs.Length)];

            availableItems.Add(selectedCharacter);
            availableItems.Add(selectedCharacter);
        }

        // Trộn ngẫu nhiên
        availableItems = availableItems.OrderBy(x => UnityEngine.Random.value).ToList();

        // Điền vào grid
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridItems[row, col] == null && availableItems.Count > 0)
                {
                    GameObject itemToPlace = availableItems[0];

                    // Sử dụng vị trí từ gridPositions
                    Vector2 spawnPosition = gridPositions[row, col];

                    gridItems[row, col] = Instantiate(
                        itemToPlace,
                        spawnPosition,
                        Quaternion.identity
                    );

                    availableItems.RemoveAt(0);
                }
            }
        }
    }

    // Xoá các item cũ trên grid
    void ClearExistingGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridItems[row, col] != null)
                {
                    Destroy(gridItems[row, col]);
                    gridItems[row, col] = null;
                }
            }
        }
    }

    // Thuật toán đảm bảo số thẻ luôn chẵn và có đủ cặp
    private void FillGridWithMatchingPairs()
    {
        List<GameObject> availableItems = new List<GameObject>();

        int totalPairs = (rows * columns) / 2;

        for (int i = 0; i < totalPairs; i++)
        {
            GameObject selectedCharacter = characterPrefabs[UnityEngine.Random.Range(0, characterPrefabs.Length)];

            availableItems.Add(selectedCharacter);
            availableItems.Add(selectedCharacter);
        }

        availableItems = availableItems.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridItems[row, col] == null && availableItems.Count > 0)
                {
                    GameObject itemToPlace = availableItems[0];
                    gridItems[row, col] = Instantiate(
                        itemToPlace,
                        new Vector2(col, row),
                        Quaternion.identity
                    );
                    availableItems.RemoveAt(0);
                }
            }
        }
    }

    // Thuật toán tìm đường đi không giới hạn turn
    public bool FindConnectionPath(Vector2Int start, Vector2Int end)
    {
        // Kiểm tra điều kiện ban đầu
        if (start == end || gridItems[start.y, start.x] != gridItems[end.y, end.x])
            return false;

        // Các hướng di chuyển
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        // Thuật toán BFS để tìm đường đi không giới hạn turn
        Queue<List<Vector2Int>> paths = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        paths.Enqueue(new List<Vector2Int> { start });

        while (paths.Count > 0)
        {
            List<Vector2Int> currentPath = paths.Dequeue();
            Vector2Int currentPos = currentPath[currentPath.Count - 1];

            if (currentPos == end)
                return true;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;

                // Kiểm tra điều kiện di chuyển
                if (IsValidMove(nextPos, currentPath))
                {
                    List<Vector2Int> newPath = new List<Vector2Int>(currentPath);
                    newPath.Add(nextPos);
                    paths.Enqueue(newPath);
                }
            }
        }

        return false;
    }

    // Kiểm tra tính hợp lệ của nước đi
    private bool IsValidMove(Vector2Int pos, List<Vector2Int> currentPath)
    {
        // Kiểm tra trong biên grid
        if (pos.x < 0 || pos.x >= columns || pos.y < 0 || pos.y >= rows)
            return false;

        // Kiểm tra ô trống hoặc điểm cuối
        return gridItems[pos.y, pos.x] == null ||
               (pos == currentPath[0] || pos == currentPath[currentPath.Count - 1]);
    }

    // Tạo chướng ngại và vùng trống tuỳ độ khó
    private void CreateMapObstacles(int difficulty)
    {
        int obstacleCount = difficulty * 5;

        for (int i = 0; i < obstacleCount; i++)
        {
            int randomRow = UnityEngine.Random.Range(0, rows);
            int randomCol = UnityEngine.Random.Range(0, columns);

            // Đánh dấu là chướng ngại
            gridItems[randomRow, randomCol] = null;
        }
    }
}
