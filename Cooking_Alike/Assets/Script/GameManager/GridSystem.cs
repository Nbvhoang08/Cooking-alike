using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

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
    public TextMeshPro timerText;
    public TextMeshPro scoreText;
    public TextMeshPro highScoreText;

    [Header("Game Settings")]
    public float gameDuration = 180f;
    [SerializeField]private string currentSceneKey;
    public int difficulty;
    public bool hasObstacle;
    // Quản lý trạng thái game
    private float currentTime;
    private bool isGameRunning = false;
     private int _score = 0; // Sử dụng tiền tố "_" theo convention của bạn.

    public int getScore() {
        return _score;
    }

    public void setScore(int score) {
        // Giới hạn score không nhỏ hơn 0
        this._score = Mathf.Max(0, score);
    }

    // Ma trận grid
    [SerializeField] private GameObject[,] gridItems;
    [SerializeField] private Vector2[,] gridPositions;

    // Quản lý chọn character
    [SerializeField] private GameObject firstSelectedCharacter = null;
    private Vector2Int firstSelectedPosition;
    private bool isFirstCharacterSelected = false;
    [SerializeField] private LineRenderer lineRenderer;
    public GameObject matchEffectPrefab;  // Hiệu ứng khi match thành công
    public GameObject missMatchEffectPrefab;  // Hiệu ứng khi không match
    public GameObject celebrationEffectPrefab;
    private void Awake()
    {
        InitializeGrid();
    }
    void Start()
    {
        if (hasObstacle)
        {
            CreateMapObstacles(difficulty);
        }
        
        StartGame();
        setScore(0);
        if (scoreText != null)
        {
            scoreText.text = getScore().ToString();
        }
        if (highScoreText != null)
        {
            highScoreText.text = LoadHighScore().ToString();
        }
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
            timerText.text = "00:00";
        }
    }


    public void SaveHighScore(int score)
    {
        // Kiểm tra nếu điểm mới cao hơn điểm đã lưu
        int currentHighScore = PlayerPrefs.GetInt(currentSceneKey, 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(currentSceneKey, score);
            PlayerPrefs.Save();
            Debug.Log($"New high score for {currentSceneKey}: {score}");
        }
    }
    public int LoadHighScore()
    {
        Debug.Log("");
        int highScore = PlayerPrefs.GetInt(currentSceneKey, 0);
        Debug.Log($"High score for {currentSceneKey}: {highScore}");
        return highScore;
    }
    void HandleCharacterSelection()
{
    // Kiểm tra nếu click chuột trái
    if (Input.GetMouseButtonDown(0))
    {
        // Kiểm tra nếu chuột không đang bấm vào UI
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2Int selectedPosition = GetGridPositionFromMouseClick();

            // Kiểm tra nếu vị trí trong grid hợp lệ
            if (IsValidGridPosition(selectedPosition))
            {
                SoundManager.Instance.PlayVFXSound(1);
                GameObject selectedCharacter = gridItems[selectedPosition.y, selectedPosition.x];

                // Xử lý chọn nhân vật nếu có object tại vị trí đó
                if (selectedCharacter != null)
                {
                    ProcessCharacterSelection(selectedPosition, selectedCharacter);
                }
            }
        }
    }
}

    // Chuyển đổi toạ độ chuột sang vị trí grid
    Vector2Int GetGridPositionFromMouseClick()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Tìm chỉ số trong mảng grid
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

            HighlightCharacter(firstSelectedCharacter, true,11);
        }
        else
        {
            HighlightCharacter(selectedCharacter, true,12);
           
            if (CanMatchCharacters(firstSelectedPosition, selectedPosition))
            {
                // Vẽ đường đi giữa hai điểm
                DrawConnectionPath(firstSelectedPosition, selectedPosition);
                StartCoroutine(DestroyMatchedObjectsAfterDelay(
                    firstSelectedPosition, 
                    selectedPosition
                ));
               
            }
            else
            {
                HighlightCharacter(firstSelectedCharacter, false,10);
                HighlightCharacter(selectedCharacter, false, 10);
                Vector2 endPosition = gridPositions[selectedPosition.y, selectedPosition.x];
                Instantiate(missMatchEffectPrefab, endPosition, Quaternion.identity);
                lineRenderer.positionCount = 0;
                UpdateScore(false);
                SoundManager.Instance.PlayVFXSound(0);
            }

            firstSelectedCharacter = null;
            isFirstCharacterSelected = false;
        }
    }

    // Kiểm tra xem có thể match 2 character không
    bool CanMatchCharacters(Vector2Int pos1, Vector2Int pos2)
    {
        GameObject obj1 = gridItems[pos1.y, pos1.x];
        GameObject obj2 = gridItems[pos2.y, pos2.x];

        // Kiểm tra null
        if (obj1 == null || obj2 == null)
            return false;

        // Lấy loại của 2 object
        kitchenware data1 = obj1.GetComponent<kitchenware>();
        kitchenware data2 = obj2.GetComponent<kitchenware>();

        if (data1 == null || data2 == null)
            return false;

        // So sánh loại (enum)
        if (data1.TypeID != data2.TypeID){
            HighlightCharacter(obj1, false,1);
            HighlightCharacter(obj2,false,2);
            return false;
        }
        // Kiểm tra đường kết nối
        return FindConnectionPath(pos1, pos2);
    }

    // Loại bỏ các character đã match
    IEnumerator DestroyMatchedObjectsAfterDelay(Vector2Int pos1, Vector2Int pos2)
    {
        // Đợi 0.3 giây
        yield return new WaitForSeconds(0.3f);

        // Xóa LineRenderer để dọn đường đi
        lineRenderer.positionCount = 0;

        // Lấy các GameObject cần phá hủy
        GameObject obj1 = gridItems[pos1.y, pos1.x];
        GameObject obj2 = gridItems[pos2.y, pos2.x];

        // Hiệu ứng thu nhỏ dần
        yield return StartCoroutine(ShrinkAndDestroy(obj1));
        yield return StartCoroutine(ShrinkAndDestroy(obj2));
        firstSelectedCharacter = null;
        // Đánh dấu ô trống trong grid
        gridItems[pos1.y, pos1.x] = null;
        gridItems[pos2.y, pos2.x] = null;

        // Kiểm tra kết thúc game nếu không còn character
        CheckGameCompletion();
    }

    // Hiệu ứng thu nhỏ và phá hủy
    IEnumerator ShrinkAndDestroy(GameObject obj)
    {
        if (obj == null) yield break;

        // Thời gian thu nhỏ
        float shrinkDuration = 0.3f;

        // Bắt đầu thu nhỏ
        Vector3 initialScale = obj.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;

            // Giảm kích thước dần dần
            obj.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);

            yield return null; // Chờ frame tiếp theo
        }

        // Đặt kích thước về 0 để đảm bảo
        obj.transform.localScale = Vector3.zero;
        Vector2 startPosition = new Vector2(obj.transform.position.x, obj.transform.position.y);
        Instantiate(matchEffectPrefab, startPosition, Quaternion.identity);
        SoundManager.Instance.PlayVFXSound(3); 
        UpdateScore(true);
        // Phá hủy object
        Destroy(obj);
    }



    // Highlight character khi được chọn
    void HighlightCharacter(GameObject character, bool isHighlighted , int amount)
    {
        // Thay đổi màu sắc hoặc scale để highlight
        if(character == null) return;
        if (isHighlighted)
        {
            character.transform.localScale = Vector3.one * 1.2f;
            character.GetComponent<SpriteRenderer>().sortingOrder = amount ;
        }
        else
        {
            character.transform.localScale = Vector3.one;
            character.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
    }

    // Bắt đầu game
    public void StartGame()
    {
        // Reset thời gian
        currentTime = gameDuration;
        isGameRunning = true;

        // Tạo map
        GenerateRandomMap();
    }
    // Cập nhật điểm số
  

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
    void UpdateScore(bool increase)
    {
        if (increase)
        {
            _score += 50;
        }
        else
        {
            _score -= 50;
        }

        if (scoreText != null)
        {
            scoreText.text = getScore().ToString();
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
            
            StartCoroutine(CalculateFinalScoreAndCelebrate(currentTime));
        }
        else
        {
            Debug.Log("Hết giờ!");
            // Xử lý khi thua game
            SpawnEffects(10,missMatchEffectPrefab);
            SoundManager.Instance.PlayVFXSound(5);
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator Haswon()
    {
        yield return new WaitForSeconds(2);
        UIManager.Instance.OpenUI<Success>();
        Time.timeScale = 0;
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2);
        UIManager.Instance.OpenUI<Lose>();
        Time.timeScale = 0;
    }

    private IEnumerator CalculateFinalScoreAndCelebrate(float timeRemaining)
    {
        // Tính hệ số nhân điểm dựa trên thời gian còn lại
        float multiplier = 1.0f;
        if (timeRemaining >= 60f && timeRemaining <= 90f) // 1 đến 1.5 phút
        {
            multiplier = 1.5f;
        }
        else if (timeRemaining > 90f) // Trên 1.5 phút
        {
            multiplier = timeRemaining / 60f; // Chuyển thành phút
        }

        // Tính toán số điểm mục tiêu
        int finalScore = Mathf.RoundToInt(_score * multiplier);
        int startScore = _score;

        // Hiển thị số điểm tăng dần
        float duration = 1.5f; // Thời gian tăng dần điểm
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _score = Mathf.RoundToInt(Mathf.Lerp(startScore, finalScore, elapsed / duration));
            if (scoreText != null)
            {
                scoreText.text = getScore().ToString();
            }
            yield return null; // Chờ frame tiếp theo
        }

        // Đảm bảo hiển thị chính xác điểm cuối cùng
        _score = finalScore;
        if (scoreText != null)
        {
            scoreText.text = getScore().ToString();
        }
        SaveHighScore(_score);
        // Gọi hàm spawn hiệu ứng ăn mừng
         SoundManager.Instance.PlayVFXSound(4);
        SpawnEffects(10, celebrationEffectPrefab);
        StartCoroutine(Haswon());
    }


    private void SpawnEffects(int effectCount ,GameObject EffectPrefab)
    {
        for (int i = 0; i < effectCount; i++)
        {
            // Tạo vị trí ngẫu nhiên trên màn hình
            Vector2 randomPosition = new Vector2(
                Random.Range(0.1f, 0.9f) * Screen.width, // Đảm bảo không sát mép
                Random.Range(0.1f, 0.9f) * Screen.height
            );

            // Chuyển đổi tọa độ màn hình thành tọa độ thế giới
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(randomPosition.x, randomPosition.y, 10f));

            // Instantiate hiệu ứng (thay `celebrationEffectPrefab` bằng prefab hiệu ứng của bạn)
            GameObject effect = Instantiate(EffectPrefab, worldPosition, Quaternion.identity);
        }
    }
    // Thuật toán tạo map với các hình dạng grid linh hoạt

    // Sinh map với hỗ trợ offset
   public void GenerateRandomMap()
{
    // Xoá các item cũ nếu có
    ClearExistingGrid();

    // Tạo danh sách các cặp item
    List<GameObject> availableItems = new List<GameObject>();
    int totalPairs = (rows * columns) / 2;

    for (int i = 0; i < totalPairs; i++)
    {
        GameObject selectedCharacter = characterPrefabs[Random.Range(0, characterPrefabs.Length)];
        availableItems.Add(selectedCharacter); // Item đầu tiên trong cặp
        availableItems.Add(selectedCharacter); // Item thứ hai trong cặp
    }

    // Shuffle ngẫu nhiên danh sách các item
    availableItems = availableItems.OrderBy(x => Random.value).ToList();

    // Đặt các item vào grid
    bool mapIsValid = false;
    while (!mapIsValid)
    {
        // Reset grid trước khi spawn
        ClearExistingGrid();
        
        // Lấy danh sách vị trí trống trong lưới
        List<Vector2Int> emptyPositions = GetAllGridPositions();

        // Shuffle vị trí trống để tạo ngẫu nhiên
        emptyPositions = emptyPositions.OrderBy(x => Random.value).ToList();

        // Đặt các item ngẫu nhiên vào grid
        for (int i = 0; i < availableItems.Count; i++)
        {
            Vector2Int position = emptyPositions[i];
            GameObject itemToPlace = availableItems[i];
            Vector2 spawnPosition = gridPositions[position.y, position.x];

            gridItems[position.y, position.x] = Instantiate(
                itemToPlace,
                spawnPosition,
                Quaternion.identity
            );
        }

        // Kiểm tra xem map có ít nhất một cặp có đường đi hợp lệ hay không
        mapIsValid = CheckMapValidity();
    }
}

// Lấy tất cả các vị trí trong lưới (trả về danh sách các Vector2Int)
private List<Vector2Int> GetAllGridPositions()
{
    List<Vector2Int> positions = new List<Vector2Int>();
    for (int row = 0; row < rows; row++)
    {
        for (int col = 0; col < columns; col++)
        {
            positions.Add(new Vector2Int(col, row));
        }
    }
    return positions;
}

// Phương thức kiểm tra tính hợp lệ của map
private bool CheckMapValidity()
{
    // Duyệt qua tất cả các cặp trong grid
    for (int row1 = 0; row1 < rows; row1++)
    {
        for (int col1 = 0; col1 < columns; col1++)
        {
            if (gridItems[row1, col1] == null) continue;

            for (int row2 = 0; row2 < rows; row2++)
            {
                for (int col2 = 0; col2 < columns; col2++)
                {
                    if (gridItems[row2, col2] == null || (row1 == row2 && col1 == col2)) continue;

                    // Kiểm tra nếu hai item có thể kết nối được
                    if (gridItems[row1, col1].GetComponent<kitchenware>().TypeID ==
                        gridItems[row2, col2].GetComponent<kitchenware>().TypeID &&
                        FindConnectionPath(new Vector2Int(col1, row1), new Vector2Int(col2, row2)))
                    {
                        return true; // Có ít nhất một đường kết nối hợp lệ
                    }
                }
            }
        }
    }
    return false; // Không tìm thấy đường kết nối hợp lệ nào
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

   
    //Thuật toán tìm đường đi không giới hạn turn
    public bool FindConnectionPath(Vector2Int start, Vector2Int end)
    {
        // Kiểm tra điều kiện ban đầu
        if (start == end)
        {
            return false;
        }

        // Kiểm tra nếu `start` hoặc `end` nằm ngoài lưới
        if (!IsWithinBounds(start) || !IsWithinBounds(end))
        {
            return false;
        }

        // Kiểm tra nếu `end` không phải ô trống
        if (gridItems[end.y, end.x] != null && gridItems[start.y, start.x].GetComponent<kitchenware>().TypeID != gridItems[end.y, end.x].GetComponent<kitchenware>().TypeID)
        {
            Debug.Log(end.y +" "+ end.x   +" " +gridItems[end.y, end.x].name);
            return false;
        }

        // Các hướng di chuyển
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
        };

        // Thuật toán BFS để tìm đường đi không giới hạn turn
        Queue<List<Vector2Int>> paths = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Thêm đường đi ban đầu
        paths.Enqueue(new List<Vector2Int> { start });
        visited.Add(start);

        while (paths.Count > 0)
        {
            // Lấy đường đi hiện tại từ hàng đợi
            List<Vector2Int> currentPath = paths.Dequeue();
            Vector2Int currentPos = currentPath[currentPath.Count - 1];
            // Kiểm tra nếu đã đến điểm đích
            if (currentPos == end)
            {
                return true;
            }

            // Duyệt các hướng di chuyển
            foreach (Vector2Int dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;

                // Kiểm tra điều kiện di chuyển
                if (IsValidMove(nextPos, currentPath ,end) && !visited.Contains(nextPos))
                {
                    // Tạo đường đi mới
                    List<Vector2Int> newPath = new List<Vector2Int>(currentPath);
                    newPath.Add(nextPos);

                    // Thêm vào hàng đợi và đánh dấu đã duyệt
                    paths.Enqueue(newPath);
                    visited.Add(nextPos);
                }
            }
        }

       
        return false;
    }
 

   
    // Hàm kiểm tra tính hợp lệ của nước đi
    private bool IsValidMove(Vector2Int pos, List<Vector2Int> currentPath, Vector2Int end)
    {
       
        // Kiểm tra nếu tọa độ nằm ngoài biên grid
        if (pos.x < 0 || pos.x >= gridItems.GetLength(1) || pos.y < 0 || pos.y >= gridItems.GetLength(0))
        {
            return false;
        }
        // Kiểm tra nếu đây là điểm kết thúc (end)
        if (pos == end)
        {
            return true;
        }

        // Kiểm tra ô trống hoặc điểm bắt đầu

        bool isValid = gridItems[pos.y, pos.x] == null || pos == currentPath[0];
           
        return isValid;    
    }



    void DrawConnectionPath(Vector2Int start, Vector2Int end)
    {
        // Tìm đường đi giữa hai điểm
        List<Vector2Int> path = FindShortestPath(start, end);

        if (path != null && path.Count > 0)
        {
        // Cấu hình LineRenderer
            lineRenderer.positionCount = path.Count;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;

            // Chuyển đổi các điểm grid thành vị trí thế giới
            Vector3[] pathPositions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                pathPositions[i] = new Vector3(
                    gridPositions[path[i].y, path[i].x].x, 
                    gridPositions[path[i].y, path[i].x].y, 
                    0
                );
            }

            // Gán vị trí cho LineRenderer
            lineRenderer.SetPositions(pathPositions);
        }
        else
        {
            // Nếu không tìm thấy đường đi, reset LineRenderer
            lineRenderer.positionCount = 0;
        }
    }

    // Phương thức tìm đường đi ngắn nhất (sửa đổi từ FindConnectionPath)
    List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int end)
    {
        // Các hướng di chuyển
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        // Thuật toán BFS để tìm đường đi ngắn nhất
        Queue<List<Vector2Int>> paths = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Thêm đường đi ban đầu
        paths.Enqueue(new List<Vector2Int> { start });
        visited.Add(start);

        while (paths.Count > 0)
        {
            List<Vector2Int> currentPath = paths.Dequeue();
            Vector2Int currentPos = currentPath[currentPath.Count - 1];

            // Kiểm tra nếu đã đến điểm đích
            if (currentPos == end)
            {
                return currentPath;
            }

            // Duyệt các hướng di chuyển
            foreach (Vector2Int dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;

                // Kiểm tra điều kiện di chuyển
                if (IsValidMove(nextPos, currentPath, end) && !visited.Contains(nextPos))
                {
                    List<Vector2Int> newPath = new List<Vector2Int>(currentPath);
                    newPath.Add(nextPos);

                    paths.Enqueue(newPath);
                    visited.Add(nextPos);
                }
            }
        }

        // Không tìm thấy đường đi
        return null;
    }


    // Kiểm tra tọa độ có nằm trong lưới không
    private bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >=0 && pos.x < columns+1 && pos.y >= 0 && pos.y < rows;
    }

    // Tạo chướng ngại và vùng trống tuỳ độ khó
    private void CreateMapObstacles(int difficulty)
    {
        int obstacleCount = difficulty * 5;

        for (int i = 0; i < obstacleCount; i++)
        {
            int randomRow = Random.Range(0, rows);
            int randomCol = Random.Range(0, columns);

            // Đánh dấu là chướng ngại
            gridItems[randomRow, randomCol] = null;
        }
    }
}
