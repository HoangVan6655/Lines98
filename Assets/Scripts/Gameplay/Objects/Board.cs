using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class Board : MonoBehaviour
{
    public static readonly int MAX_UNDO_STEPS = 10;
    public RuntimeBoard runtimeBoard;

    //
    protected Collider m_Collider;
    protected Vector3 m_BoardSize;
    protected Vector2 m_CellSize;

    protected Vector2Int m_SelectedIndex = Vector2Int.left;
    protected List<Vector2Int> m_PointsToCheck;
    protected List<Vector2Int> m_MovePath;
    protected Stack<int[]> m_UndoStack;

    void Awake() => Initialize();

    private readonly CompositeDisposable _mDisposables = new CompositeDisposable();

    protected virtual void Initialize()
    {
        runtimeBoard = new RuntimeBoard();
        m_PointsToCheck = new List<Vector2Int>();
        m_MovePath = new List<Vector2Int>();
        m_UndoStack = new Stack<int[]>();
        m_Collider = GetComponent<Collider>();
        if (m_Collider)
        {
            Debug.Log(" --- " + m_Collider.bounds.size.x);
            Debug.Log(" --- " + m_Collider.bounds.size.y);
            Debug.Log(" --- " + m_Collider.bounds.size.z);
            m_BoardSize = m_Collider.bounds.size;
            m_CellSize = new Vector2(m_BoardSize.x / RuntimeBoard.BOARD_SIZE, m_BoardSize.z / RuntimeBoard.BOARD_SIZE);
        }
    }

    void Start()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchDown).AddTo(_mDisposables);

        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchUp).AddTo(_mDisposables);
    }

    public void StartGame()
    {
        SpawnDot();
        CheckBoard();
        m_UndoStack.Clear();
        SpawnDot();
        SaveBoardState();
    }

    public void EndGame()
    {
        _mDisposables.Clear();
    }

    public IEnumerator StartDebug()
    {
        yield return new WaitForSeconds(1.0f);
        StartGame();
    }

    public void OnTouchDown(Vector3 position)
    {
        Ray raycast = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(raycast, out var castHit))
        {
            if (castHit.collider.GetComponent<Ball>())
            {
                var ball = castHit.collider.GetComponent<Ball>();
                Vector2Int offset = GetBoardOffset(ball.transform.position);
                Debug.Log("offset : " + offset.x + " | " + offset.y);
                if (runtimeBoard.cells[offset.x, offset.y].selectable)
                {
                    if (m_SelectedIndex != Vector2Int.left)
                        runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y]?.SetSelection(false);
                    runtimeBoard.cells[offset.x, offset.y].SetSelection(true);
                    m_SelectedIndex = offset;
                }
            }
            else if (castHit.collider.GetComponent<Board>())
            {
                Vector2Int offset = GetBoardOffset(castHit.point);
                Debug.Log("offset : " + offset.x + " | " + offset.y);
                if (runtimeBoard.cells[offset.x, offset.y].available)
                {
                    if (m_SelectedIndex != Vector2Int.left)
                    {
                        runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y].SetSelection(false);
                        CheckMovePath(m_SelectedIndex, offset, runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y].ball.color == Ball.Color.Ghost);
                        if (!m_MovePath.IsNullOrEmpty())
                        {
                            SaveBoardState();
                            runtimeBoard.cells[offset.x, offset.y].AttachBall(runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y].DettachBall());
                            m_PointsToCheck.Clear();
                            m_PointsToCheck.Add(offset);
                            //
                            MoveBall(m_SelectedIndex, offset, runtimeBoard.cells[offset.x, offset.y].ball.color == Ball.Color.Ghost);
                        }

                        m_SelectedIndex = Vector2Int.left;
                    }
                }
            }
        }
    }

    private void SpawnDot()
    {
        var colors = new int[3];
        var emptyCells = runtimeBoard.GetRandomCells(3);
        if (emptyCells.Count < 3)
        {
            GameManager.instance.ChangeGamestate(GameManager.GameState.EndGame);
            return;
        }
        int maxRange = (int)(GameManager.instance.hardMode == GameManager.HardMode.Normal ? Ball.Color.Magenta :
            GameManager.instance.hardMode == GameManager.HardMode.Hard ? Ball.Color.Brown :
            Ball.Color.Ghost) + 1;
        for (int i = 0; i < emptyCells.Count; i++)
        {
            var cell = emptyCells[i];
            var randomColor = (Ball.Color)Random.Range(0, maxRange);
            var pos = ComputePosition(cell.rowOffset, cell.colOffset);
            var ball = Pooler.instance.Spawn<Ball>(randomColor == Ball.Color.Ghost ? "ghost_ball" : "ball", pos, transform);
            ball.SetColor(randomColor);
            cell.ball = ball;

            colors[i] = ball.GetColor32Code();
        }
        EventDispatcher.Dispatch(GameEvent.GE_ADD_DOT_COLOR, colors[0], colors[1], colors[2]);
    }

    public void SaveBoardState()
    {
        m_UndoStack.Push(runtimeBoard.BoardData());
        if (m_UndoStack.Count > MAX_UNDO_STEPS) m_UndoStack.Peek();
    }

    public void RestorePreBoardState()
    {
        if (m_UndoStack.Count == 0) return;

        var boardState = m_UndoStack.Pop();
        int count = 0;
        foreach (var cell in runtimeBoard.cells)
        {
            if (runtimeBoard.BoardCellToValue(cell) != boardState[count])
            {
                Debug.Log("111 : " + runtimeBoard.BoardCellToValue(cell));
                Debug.Log("222 : " + boardState[count]);
                if (!runtimeBoard.cells[cell.rowOffset, cell.colOffset].empty)
                {
                    runtimeBoard.cells[cell.rowOffset, cell.colOffset].ball.Destroy();
                    runtimeBoard.cells[cell.rowOffset, cell.colOffset].ball = null;
                }

                if (boardState[count] != 0)
                {
                    var ballDefine = runtimeBoard.ValueToBoardCell(boardState[count]);
                    var ball = Pooler.instance.Spawn<Ball>(ballDefine.Item1 == Ball.Color.Ghost ? "ghost_ball" : "ball", ComputePosition(cell.rowOffset, cell.colOffset), transform);
                    ball.SetColor(ballDefine.Item1);
                    ball.SetSize(ballDefine.Item2);
                    runtimeBoard.cells[cell.rowOffset, cell.colOffset].ball = ball;
                }
            }
            count++;
        }
    }

    private Vector2Int GetBoardOffset(Vector3 point)
    {
        var boardPoint = transform.InverseTransformPoint(point);
        return new Vector2Int((int)((boardPoint.x + m_BoardSize.x / 2) / m_CellSize.x), (int)((boardPoint.z + m_BoardSize.z / 2) / m_CellSize.y));
    }

    public void OnTouchUp(Vector3 position)
    {
        //Debug.Log("MouseUpPos : " + Camera.main.ScreenToWorldPoint(position));
    }

    private Vector3 ComputePosition(int row, int col)
    {
        return new Vector3(-m_BoardSize.x * 0.5f + row * m_CellSize.x + m_CellSize.x / 2 + transform.position.x, transform.position.y + 0.5f, -m_BoardSize.z * 0.5f + col * m_CellSize.y + m_CellSize.y / 2 + transform.position.z);
    }

    public void CheckMovePath(Vector2Int from, Vector2Int to, bool isGhostBall = false)
    {
        m_MovePath = runtimeBoard.BuildPath(from, to, isGhostBall);
    }

    protected void MoveBall(Vector2Int from, Vector2Int to, bool isGhostBall = false)
    {
        if (!m_MovePath.IsNullOrEmpty())
        {
            var worldMovePath = new List<Vector3>();
            foreach (var point in m_MovePath)
            {
                worldMovePath.Add(ComputePosition(point.x, point.y));
            }
            runtimeBoard.cells[to.x, to.y].ball.transform.DOPath(worldMovePath.ToArray(), 0.3f).OnComplete(() => OnMovePathDone());
        }
    }

    public void OnMovePathDone()
    {
        Debug.Log("mOnMoveDone");

        CheckBoard();
        SpawnDot();

        if (m_PointsToCheck.Count > 0)
        {
            foreach (var point in m_PointsToCheck)
            {
                List<Vector2Int> points = runtimeBoard.CheckLines(point);

                if (points != null && points.Count > 0)
                {
                    Debug.Log("add score");
                    EventDispatcher.Dispatch(GameEvent.GE_ADD_SCORE, points.Count);
                    foreach (var p in points) ExplodeBall(p);
                }
            }

            m_PointsToCheck.Clear();
        }
        //m_PrevSelected = Vector2Int.left;
    }

    public void ChangDot()
    {
        var availableCells = runtimeBoard.GetDotCells();
        foreach (var cell in availableCells)
        {
            if (!runtimeBoard.cells[cell.rowOffset, cell.colOffset].empty)
            {
                runtimeBoard.cells[cell.rowOffset, cell.colOffset].ball.Destroy();
                runtimeBoard.cells[cell.rowOffset, cell.colOffset].ball = null;
            }
        }

        SpawnDot();

        if (m_UndoStack.Count > 0) m_UndoStack.Pop();
        SaveBoardState();
    }

    public void CheckBoard()
    {
        var availableCells = runtimeBoard.GetDotCells();
        foreach (var cell in availableCells)
        {
            cell.ball.SetSize(Ball.Size.Ball);
            m_PointsToCheck.Add(new Vector2Int(cell.rowOffset, cell.colOffset));
        }
    }

    protected void ExplodeBall(Vector2Int cellIndex)
    {
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball.Explosive();
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball.Destroy();
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball = null;
    }
}
