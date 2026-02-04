using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public float MinX => origin.x; //vị trí biên nhỏ nhất của trục Ox
    public float MaxX => origin.x + (columns - 1) * cellSize; //vị trí biên lớn nhất của trục Ox
    public Vector2 GridSize =>
    new Vector2(columns * cellSize, rows * cellSize);

    [Header("Grid Size")]   //khai báo 6 hàng 5 cột.
    public int columns = 5;
    public int rows = 6;

    [Header("Layout")]  //khai báo kích thước và vị trí của grid
    public float cellSize = 1.6f;       //khoảng cách giữa tâm các ô
    public Vector2 origin = new Vector2(-3.2f, -4.0f);      //tọa độ ô (0,0)

    public MonsterHealth[,] grid;   //mảng 2 chiều grid

    void Awake()
    {
        Instance = this;
        grid = new MonsterHealth[columns, rows];
        origin = new Vector2(-(columns - 1) * cellSize / 2f, -(rows - 1) * cellSize / 2f);
    }

    public Vector3 GetWorldPos(int x, int y)        
    {
        return new Vector3(origin.x + x * cellSize, origin.y + y * cellSize-0.3f, 0);
    }

    public bool IsValid(int x, int y)   //kiểm tra xem ô có nằm trong grid không
    {
        return (x >= 0) && (x < columns) && (y >= 0) && (y < rows);
    }

    public bool IsEmpty(int x, int y)   //kiểm tra ô có trống hay không
    {
        return IsValid(x, y) && (grid[x, y] == null);
    }

    public MonsterHealth GetUnit(int x, int y)   //lấy unit tại vị trí (x,y)
    {
        return IsValid(x, y) ? grid[x, y] : null;
    }
    public Vector2 ConvertToPosGrid(float x, float y)
    {
        int x1 = Mathf.RoundToInt((x - origin.x) / cellSize);   //tính tọa độ (x,y) của vị trí mới
        int y1 = Mathf.RoundToInt((y - origin.y) / cellSize);
        return new Vector2(x1, y1);
    }

    public void Place(MonsterHealth unit, int x, int y)  //đặt unit vào vị trí (x,y)
    {
        if (!IsEmpty(x, y)) return;
        unit.GetComponent<SortingGroup>().sortingOrder = -y;
        grid[x, y] = unit;
        unit.SetGridPos(x, y);
        unit.transform.position = GetWorldPos(x, y);
    }

    public void Remove(int x, int y)    //đánh dấu vị trí cũ của unit là null
    {
        if (!IsValid(x, y)) return;
        grid[x, y] = null;
    }
    public void CLear(int toX, int toY) //Đặt toàn bộ vị trí trong grid về null
    {
        for(int i = 0; i <= toY; i++)
        {
            for (int j = 0; j <= toX; j++)
            {
                grid[j, i] = null;
            }
        }
    }
    public void CLearEnemy(int toX, int toY) //Đặt toàn bộ vị trí trong grid về null
    {
        for (int i = 5; i >= toY; i--)
        {
            for (int j = 0; j <= toX; j++)
            {
                grid[j, i] = null;
            }
        }
    }

    void OnDrawGizmos()     //vẽ grid trên Scene để xem
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Gizmos.DrawWireCube(
                    GetWorldPos(x, y),
                    new Vector3(cellSize, cellSize, 0)
                );
            }
        }
    }
    public Vector2 ClampToGrid(Vector2 worldPos) //Kiểm tra vị trí nếu nằm ngoài grid thì sẽ trả về vị trí ở biên
    {
        float minX = origin.x;
        float maxX = origin.x + (columns - 1) * cellSize;

        float minY = origin.y;
        float maxY = origin.y + (rows - 1) * cellSize;

        float clampedX = Mathf.Clamp(worldPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(worldPos.y, minY, maxY);

        return new Vector2(clampedX, clampedY);
    }
    public bool IsNearEdgeX(float x, float epsilon = 0.05f) //Xét trục Ox kiểm tra xem vị trí x có ở gần biên không với khoảng cách an toàn là epsilon
    {
        float minX = origin.x;
        float maxX = origin.x + (columns - 1) * cellSize;

        return x <= minX + epsilon || x >= maxX - epsilon;
    }
    public bool isFull()
    {
        for (int y = 0; y < rows/2; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (IsEmpty(x, y)) return false;
            }
        }
        return true;
    }
}
