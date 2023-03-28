using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeBoard
{
    public static readonly int BOARD_SIZE = 9;
    public static readonly int POINT_TO_SCORE = 5;

    public CellEntity[,] cells = new CellEntity[BOARD_SIZE, BOARD_SIZE];

    public RuntimeBoard()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                cells[i, j] = new CellEntity();
                cells[i, j].rowOffset = i;
                cells[i, j].colOffset = j;
            }
        }
    }

    public List<CellEntity> GetRandomCells(int NoCell)
    {
        System.Random rnd = new System.Random();
        return cells.Cast<CellEntity>().Where(x => x.empty).OrderBy(x => rnd.Next()).Take(NoCell).ToList<CellEntity>();
    }

    public List<CellEntity> GetDotCells()
    {
        return cells.Cast<CellEntity>().Where(x => x.ball != null && x.ball.size == Ball.Size.Dot).ToList();
    }

    public List<Vector2Int> CheckPath(CellEntity[,] cells, Vector2Int from, Vector2Int to, bool isGhostBall = false)
    {
        Vector2Int[,] dad = new Vector2Int[BOARD_SIZE, BOARD_SIZE];
        Vector2Int[] queue = new Vector2Int[BOARD_SIZE * BOARD_SIZE];
        Vector2Int[] trace = new Vector2Int[BOARD_SIZE * BOARD_SIZE];

        int[] u = { 1, 0, -1, 0 };
        int[] v = { 0, 1, 0, -1 };

        int fist = 0, last = 0, x, y, i, j;
        for (x = 0; x < BOARD_SIZE; x++)
            for (y = 0; y < BOARD_SIZE; y++)
            {
                dad[x, y] = new Vector2Int(-1, -1);
                trace[x * BOARD_SIZE + y] = new Vector2Int(-5, -5);
            }

        queue[0] = to;
        dad[to.x, to.y].x = -2;

        Vector2Int dir = new Vector2Int();

        while (fist <= last)
        {
            x = queue[fist].x; y = queue[fist].y;
            fist++;
            for (j = 0; j < 4; j++)
            {
                dir.x = x + u[j];
                dir.y = y + v[j];
                if (dir.x == from.x && dir.y == from.y)
                {
                    dad[from.x, from.y] = new Vector2Int(x, y);

                    i = 0;
                    while (true)
                    {
                        trace[i] = from;
                        i++;
                        j = from.x;
                        from.x = dad[from.x, from.y].x;
                        if (from.x == -2) break;
                        from.y = dad[j, from.y].y;
                    }
                    return trace.Where(p => (p.x > -5 && p.y > -5)).ToList<Vector2Int>();
                }

                if (!IsInside(dir.x, dir.y)) continue;

                if (dad[dir.x, dir.y].x == -1 && ((cells[dir.x, dir.y].available || isGhostBall)))
                {
                    last++;
                    queue[last] = dir;
                    dad[dir.x, dir.y] = new Vector2Int(x, y);
                }
            }
        }

        return trace.Where(p => (p.x > -5 && p.y > -5)).ToList<Vector2Int>();
    }

    public List<Vector2Int> CheckLines(Vector2Int point)
    {
        return CheckLines(cells, point);
    }

    public List<Vector2Int> CheckLines(CellEntity[,] cells, Vector2Int point)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        int x = (int)point.x, y = (int)point.y;
        int[] u = { 0, 1, 1, 1 };
        int[] v = { 1, 0, -1, 1 };
        int i, j, k;

        for (int t = 0; t < 4; t++)
        {
            k = 0; i = x; j = y;
            while (true)
            {
                i += u[t]; j += v[t];
                if (!IsInside(i, j))
                    break;
                if (cells[i, j].empty || cells[i, j].ball.color != cells[x, y].ball.color)
                    break;
                k++;
            }
            i = x; j = y;
            while (true)
            {
                i -= u[t]; j -= v[t];
                if (!IsInside(i, j))
                    break;
                if (cells[i, j].empty || cells[i, j].ball.color != cells[x, y].ball.color)
                    break;
                k++;
            }
            k++;
            if (k >= POINT_TO_SCORE)
                while (k-- > 0)
                {
                    i += u[t]; j += v[t];
                    if (i != x || j != y)
                        list.Add(new Vector2Int(i, j));
                }
        }

        if (list.Count > 0)
        {
            list.Add(new Vector2Int(x, y));
        }
        else list = null;
        return list;
    }

    public static bool IsInside(int x, int y)
    {
        return 0 <= x && BOARD_SIZE > x && 0 <= y && BOARD_SIZE > y;
    }

    public List<Vector2Int> BuildPath(Vector2Int from, Vector2Int to, bool isGhostBall = false)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path = CheckPath(cells, from, to, isGhostBall);
        string debugString = "";
        foreach (var node in path)
        {
            debugString += string.Format("{0}|{1} - ", node.x, node.y);
        }
        Debug.Log(debugString);

        return (path != null && path.Count > 0) ? path : null;
    }

    public int[] BoardData()
    {
        int[] data = new int[BOARD_SIZE * BOARD_SIZE];
        int count = 0;
        foreach (var cell in cells)
        {
            data[count] = BoardCellToValue(cell);
            count++;
        }

        return data;
    }

    public int BoardCellToValue(CellEntity cell)
    {
        int value = 0;
        if (!cell.empty)
        {
            value = (int)(cell.ball.color) * 10 + (cell.ball.size == Ball.Size.Dot ? 0 : 5);
        }

        return value;
    }

    public (Ball.Color, Ball.Size) ValueToBoardCell(int value)
    {
        return ((Ball.Color)(value / 10), (Ball.Size)(value % 10) == 0 ? Ball.Size.Dot : Ball.Size.Ball);
    }
}