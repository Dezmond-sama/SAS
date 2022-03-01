using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Unknown, Left, Right, Top, Bottom };
public class MazeCell
{
    public Vector2Int Coords { get; set; }
    public int X { get => Coords.x; }
    public int Y { get => Coords.y; }
    public bool HasBottomWall { get; set; } = true;
    public bool HasLeftWall { get; set; } = true;
    public bool HasRightWall { get; set; } = true;
    public bool HasTopWall { get; set; } = true;

    public bool Visited { get; set; } = false;
    public bool Room { get; set; } = false;
    public int RoomNumber { get; set; } = 0;
    public bool Locked { get; set; } = false;
    public bool SpawnEnemy { get; set; } = false;
    public bool SpawnTreasure { get; set; } = false;
    public int KeyNumber { get; set; } = 0;
    public bool SpawnPortal { get; set; } = false;
    public Vector2Int PortalDirection { get; set; }
}

public class MazeDoor
{
    public int DoorIndex { get; set; } = 0;
    public MazeCell FirstCell { get; set; }
    public MazeCell SecondCell { get; set; }
    public bool Locked { get; set; }
}

public class Maze
{
    private const int tryingPlaceRoomIterations = 5;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private MazeCell[,] _cells;
    private int _firstRoomNumber = 1;
    public MazeCell this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
            return _cells[x, y];
        }
    }

    private List<MazeDoor> _doors;
    public int DoorsCount()
    {
        return _doors.Count;
    }
    public MazeDoor Door(int index)
    {
        if (index < 0 || index >= _doors.Count) return null;
        return _doors[index];
    }
    private Vector2Int _startPoint;
    private List<Vector2Int> filledPoints = new List<Vector2Int>();
    public int GetExitCount(int x, int y)
    {
        return GetExitCount(this[x, y]);
    }


    private int GetExitCount(MazeCell cell)
    {
        if (cell == null) return 0;
        int count = 4;
        if (cell.HasLeftWall || (cell.X > 0 && _cells[cell.X - 1, cell.Y].HasRightWall)) count--;
        if (cell.HasRightWall || (cell.X < Width - 1 && _cells[cell.X + 1, cell.Y].HasLeftWall)) count--;
        if (cell.HasBottomWall || (cell.Y > 0 && _cells[cell.X, cell.Y - 1].HasTopWall)) count--;
        if (cell.HasTopWall || (cell.Y < Height - 1 && _cells[cell.X, cell.Y + 1].HasBottomWall)) count--;

        return count;
    }

    static public Maze CreateMaze(int width, int height)
    {
        Maze maze = new Maze();
        
        maze.Width = width;
        maze.Height = height;

        maze._doors = new List<MazeDoor>();
        maze._cells = new MazeCell[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                maze._cells[x, y] = new MazeCell();
                maze._cells[x, y].Coords = new Vector2Int(x, y);
            }
        }

        return maze;
    }
    public void SetSeed(int seed)
    {
        UnityEngine.Random.InitState(seed);
    }
    public void GenerateMaze(int roomCount, int lockedCount, Vector2Int minimumRoomSize, Vector2Int maximumRoomSize, Vector2Int startPoint,
                             bool createOuterWalls = true, int cyclesCount = 0, bool removeDeadEnds = false, bool singleWalls = false,int straightCoef = 0)
    {
        //for (int y = -1; y < 2; ++y)
        //{
        //    for (int x = -1; x < 2; ++x)
        //    {
        //        filledPoints.Add(new Vector2Int(startPoint.x + x, startPoint.y + y));
        //    }
        //}
        _startPoint = startPoint;
        _cells[startPoint.x, startPoint.y].Visited = true;

        //int iter = 1000;

        //while (iter > 0)
        //{
        //    if (AddRoom(new Vector2Int(2, 1), -1, true, 0)) break;
        //    else iter--;

        //}

        int iter = tryingPlaceRoomIterations;

        int number = _firstRoomNumber;
        while (roomCount > 0 && iter > 0)
        {
            if (AddRoom(new Vector2Int(
                UnityEngine.Random.Range(minimumRoomSize.x, maximumRoomSize.x + 1),
                UnityEngine.Random.Range(minimumRoomSize.y, maximumRoomSize.y + 1)), number, lockedCount > 0))
            {
                lockedCount--;
                roomCount--;
                number++;
                iter = tryingPlaceRoomIterations;
            }
            else
            {
                iter--;
            }
        }

        for (int x = 0; x < Width; ++x)
        {
            _cells[x, Height - 1].HasTopWall = createOuterWalls;
            _cells[x, 0].HasBottomWall = createOuterWalls;
        }

        for (int y = 0; y < Height; ++y)
        {
            _cells[Width - 1, y].HasRightWall = createOuterWalls;
            _cells[0, y].HasLeftWall = createOuterWalls;
        }

        RemoveWallsWithBacktracker(startPoint, straightCoef);
        MakeCycles(cyclesCount);
        if (removeDeadEnds) RemoveDeadEnds();


        if (singleWalls)
        {
            for (int x = 0; x < Width - 1; ++x)
            {
                for (int y = 0; y < Height - 1; ++y)
                {
                    if (GetExitCount(_cells[x, y + 1]) > 0) _cells[x, y].HasTopWall = false;
                    if (GetExitCount(_cells[x + 1, y]) > 0) _cells[x, y].HasRightWall = false;
                }
            }
        }
    }

    private bool AddRoom(Vector2Int roomSize, int roomNumber, bool locked, int perimeter = 1)
    {
        List<Vector2Int> roomStartCoords = new List<Vector2Int>();

        for (int x = perimeter; x < Width - perimeter - roomSize.x; ++x)
        {
            for (int y = perimeter; y < Height - perimeter - roomSize.y; ++y)
            {
                Vector2Int roomCoords = new Vector2Int(x, y);
                if (CanPlaceRoom(roomCoords, roomSize, perimeter))
                {
                    roomStartCoords.Add(roomCoords);
                }
            }
        }
        //Debug.Log(roomStartCoords.Count);
        if (roomStartCoords.Count > 0)
        {
            Vector2Int roomCoords = roomStartCoords[UnityEngine.Random.Range(0, roomStartCoords.Count)];
            PlaceRoom(roomCoords, roomSize, roomNumber, locked, new Vector2Int(-1,-1));
            return true;
        }
        return false;
    }

    private bool CanPlaceRoom(Vector2Int roomCoords, Vector2Int roomSize, int perimeter)
    {
        for (int x = roomCoords.x - perimeter; x < roomCoords.x + roomSize.x + perimeter; ++x)
        {
            for (int y = roomCoords.y - perimeter; y < roomCoords.y + roomSize.y + perimeter; ++y)
            {
                if (x >= Width || y >= Height || x < 0 || y < 0 || _cells[x, y] == null || _cells[x, y].Room || _cells[x, y].Visited) return false;
            }
        }
        return true;
    }

    public bool PlaceRoom(Vector2Int roomCoords, Vector2Int roomSize, int roomNumber, bool locked, Vector2Int exitCoords)
    {
        _firstRoomNumber = Mathf.Max(_firstRoomNumber, roomNumber + 1);
        if (!CanPlaceRoom(roomCoords, roomSize, 0)) return false;
        List<MazeCell> exitList = new List<MazeCell>();
        bool hasExit = (exitCoords.x == 0 || exitCoords.y == 0 || exitCoords.x == roomSize.x - 1 || exitCoords.y == roomSize.y - 1);
        if(hasExit) exitList.Add(_cells[roomCoords.x + exitCoords.x, roomCoords.y + exitCoords.y]);
        for (int x = roomCoords.x; x < roomCoords.x + roomSize.x; ++x)
        {
            for (int y = roomCoords.y; y < roomCoords.y + roomSize.y; ++y)
            {
                _cells[x, y].Room = true;
                _cells[x, y].Visited = true;

                _cells[x, y].HasBottomWall = y == roomCoords.y;
                _cells[x, y].HasTopWall = y == roomCoords.y + roomSize.y - 1;
                _cells[x, y].HasLeftWall = x == roomCoords.x;
                _cells[x, y].HasRightWall = x == roomCoords.x + roomSize.x - 1;
                _cells[x, y].RoomNumber = roomNumber;
                _cells[x, y].Locked = locked;
                if (hasExit) continue;
                if (x == roomCoords.x || y == roomCoords.y || x == roomCoords.x + roomSize.x - 1 || y == roomCoords.y + roomSize.y - 1)
                {
                    exitList.Add(_cells[x, y]);
                }
            }
        }
        
        if (exitList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, exitList.Count);
            List<MazeCell> neighbours = new List<MazeCell>();

            int x = exitList[index].X;
            int y = exitList[index].Y;
            if (x > 0 && !_cells[x - 1, y].Room) neighbours.Add(_cells[x - 1, y]);
            if (y > 0 && !_cells[x, y - 1].Room) neighbours.Add(_cells[x, y - 1]);
            if (x < Width - 1 && !_cells[x + 1, y].Room) neighbours.Add(_cells[x + 1, y]);
            if (y < Height - 1 && !_cells[x, y + 1].Room) neighbours.Add(_cells[x, y + 1]);

            if (neighbours.Count > 0)
            {
                MazeCell cell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                RemoveWall(exitList[index], cell);
            }
            exitList.Remove(exitList[index]);
        }
        return true;
    }

    public void InitKeys()
    {
        foreach (MazeDoor door in _doors)
        {
            if (door.DoorIndex > 0) InitKey(door.DoorIndex, false);
        }
    }

    public void InitEnemies(int enemyCount)
    {
        List<Vector2Int> emptyPoints = new List<Vector2Int>();


        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                if (_cells[x, y] == null || GetExitCount(_cells[x, y]) == 0 || _cells[x, y].RoomNumber == -1) continue;
                Vector2Int temp = new Vector2Int(x, y);
                if (filledPoints.Contains(temp)) continue;
                emptyPoints.Add(temp);
            }
        }

        for (int i = 0; i < enemyCount; ++i)
        {
            if (emptyPoints.Count == 0) break;
            Vector2Int v = emptyPoints[UnityEngine.Random.Range(0, emptyPoints.Count)];
            _cells[v.x, v.y].SpawnEnemy = true;
            emptyPoints.Remove(v);
            filledPoints.Add(v);
        }

    }
    public void InitTreasures(int coinsInOpenCount, int coinsInLockedCount)
    {
        List<Vector2Int> openPoints = new List<Vector2Int>();
        List<Vector2Int> lockedPoints = new List<Vector2Int>();

        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                if (_cells[x, y] == null || GetExitCount(_cells[x, y]) == 0 || _cells[x, y].RoomNumber == -1) continue;
                Vector2Int temp = new Vector2Int(x, y);
                if (filledPoints.Contains(temp)) continue;
                if (_cells[x, y].Locked) lockedPoints.Add(temp);
                else openPoints.Add(temp);
            }
        }

        while (coinsInOpenCount > 0 && openPoints.Count > 0)
        {
            Vector2Int v = openPoints[UnityEngine.Random.Range(0, openPoints.Count)];
            _cells[v.x, v.y].SpawnTreasure = true;
            openPoints.Remove(v);
            coinsInOpenCount--;
        }

        while (coinsInLockedCount > 0 && lockedPoints.Count > 0)
        {
            Vector2Int v = openPoints[UnityEngine.Random.Range(0, lockedPoints.Count)];
            _cells[v.x, v.y].SpawnTreasure = true;
            lockedPoints.Remove(v);
            coinsInLockedCount--;
        }
    }

    private bool InitKey(int doorIndex, bool canBePlacedInTunnel)
    {
        List<Vector2Int> emptyPoints = new List<Vector2Int>();
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                if (_cells[x, y] == null || GetExitCount(_cells[x, y]) == 0) continue;
                if (_cells[x, y].RoomNumber > doorIndex || (_cells[x, y].RoomNumber == 0 && canBePlacedInTunnel))
                {
                    Vector2Int temp = new Vector2Int(x, y);
                    if (!filledPoints.Contains(temp)) emptyPoints.Add(temp);
                }
            }
        }

        if (emptyPoints.Count > 0)
        {
            Vector2Int p = emptyPoints[UnityEngine.Random.Range(0, emptyPoints.Count)];

            _cells[p.x, p.y].KeyNumber = doorIndex;
            filledPoints.Add(p);
            return true;
        }
        else if (!canBePlacedInTunnel)
        {
            return InitKey(doorIndex, true);
        }
        return false;
    }

    private void RemoveWallsWithBacktracker(Vector2Int startPoint, int straightCoef)
    {
        MazeCell current = _cells[startPoint.x, startPoint.y];
        current.Visited = true;

        Stack<MazeCell> stack = new Stack<MazeCell>();
        Direction direction = Direction.Unknown;
        do
        {
            List<MazeCell> neighbours = new List<MazeCell>();
            if (!current.Room)
            {
                int x = current.X;
                int y = current.Y;
                if (x > 0 && !_cells[x - 1, y].Visited)
                {
                    neighbours.Add(_cells[x - 1, y]);
                    if (direction == Direction.Left)
                    {
                        for (int i = 0; i < straightCoef; i++) neighbours.Add(_cells[x - 1, y]);
                    }
                }
                if (y > 0 && !_cells[x, y - 1].Visited)
                {
                    neighbours.Add(_cells[x, y - 1]);
                    if (direction == Direction.Bottom)
                    {
                        for (int i = 0; i < straightCoef; i++) neighbours.Add(_cells[x, y - 1]);
                    }
                }
                if (x < Width - 1 && !_cells[x + 1, y].Visited)
                {
                    neighbours.Add(_cells[x + 1, y]);
                    if (direction == Direction.Right)
                    {
                        for (int i = 0; i < straightCoef; i++) neighbours.Add(_cells[x + 1, y]);
                    }
                }
                if (y < Height - 1 && !_cells[x, y + 1].Visited)
                {
                    neighbours.Add(_cells[x, y + 1]);
                    if (direction == Direction.Top)
                    {
                        for (int i = 0; i < straightCoef; i++) neighbours.Add(_cells[x, y + 1]);
                    }
                }
            }
            if (neighbours.Count > 0)
            {
                MazeCell cell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                if (cell.X == current.X - 1) direction = Direction.Left;
                else if (cell.X == current.X + 1) direction = Direction.Right;
                else if (cell.Y == current.Y - 1) direction = Direction.Bottom;
                else if (cell.Y == current.Y + 1) direction = Direction.Top;
                else direction = Direction.Unknown;

                RemoveWall(current, cell);
                cell.Visited = true;
                stack.Push(cell);
                current = cell;
            }
            else
            {
                current = stack.Pop();
            }
        } while (stack.Count > 0);
    }

    private void MakeCycles(int count)
    {
        if (count == 0) return;

        List<MazeCell> points = new List<MazeCell>();
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                if (_cells[x, y] == null || _cells[x, y].Room) continue;
                points.Add(_cells[x, y]);

            }
        }

        while (count > 0 && points.Count > 0)
        {
            MazeCell cell = points[UnityEngine.Random.Range(0, points.Count)];
            int i = 0;
            if (cell.HasLeftWall && cell.X > 0 && !_cells[cell.X - 1, cell.Y].Room) i += 1;
            if (cell.HasBottomWall && cell.Y > 0 && !_cells[cell.X, cell.Y - 1].Room) i += 2;

            switch (i)
            {
                case 1:
                    RemoveWall(cell, _cells[cell.X - 1, cell.Y]);
                    count--;
                    break;
                case 2:
                    RemoveWall(cell, _cells[cell.X, cell.Y - 1]);
                    count--;
                    break;
                case 3:
                    RemoveWall(cell, UnityEngine.Random.Range(0, 2) == 1 ? _cells[cell.X, cell.Y - 1] : _cells[cell.X - 1, cell.Y]);
                    count--;
                    break;
            }
            points.Remove(cell);
        }

    }

    private void RemoveDeadEnds()
    {
        for (int x = 0; x < Width; ++x)
        {
            for (int y = 0; y < Height; ++y)
            {
                RemoveDeadEndCell(_cells[x, y]);
            }
        }
    }

    private MazeCell RemoveDeadEndCell(MazeCell cell)
    {
        if (cell == null || GetExitCount(cell) != 1 || cell.Room || (cell.X == _startPoint.x && cell.Y == _startPoint.y))
        {
            return null;
        }
        else
        {
            int x = cell.X;
            int y = cell.Y;
            if (!cell.HasBottomWall && y > 0)
            {
                cell.HasBottomWall = true;
                _cells[x, y - 1].HasTopWall = true;

                return RemoveDeadEndCell(_cells[x, y - 1]);
            }
            else if (!cell.HasTopWall && y < Height - 1)
            {
                cell.HasTopWall = true;
                _cells[x, y + 1].HasBottomWall = true;
                return RemoveDeadEndCell(_cells[x, y + 1]);
            }
            else if (!cell.HasLeftWall && x > 0)
            {
                cell.HasLeftWall = true;
                _cells[x - 1, y].HasRightWall = true;
                return RemoveDeadEndCell(_cells[x - 1, y]);
            }
            else if (!cell.HasRightWall && x < Width - 1)
            {
                cell.HasRightWall = true;
                _cells[x + 1, y].HasLeftWall = true;
                return RemoveDeadEndCell(_cells[x + 1, y]);
            }
            else return null;
        }
    }

    public void RemoveWall(Vector2Int a, Vector2Int b)
    {
        RemoveWall(_cells[a.x, a.y], _cells[b.x, b.y]);
    }
    private void RemoveWall(MazeCell a, MazeCell b)
    {
        if (a == null || b == null || Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) > 1) return;
        if (a.Room || b.Room)
        {
            MazeDoor door = new MazeDoor();
            door.FirstCell = a;
            door.SecondCell = b;
            door.Locked = a.Locked || b.Locked;
            if (b.RoomNumber == 0) door.DoorIndex = a.RoomNumber;
            else door.DoorIndex = b.RoomNumber;
            _doors.Add(door);
        }
        if (a.X == b.X)
        {
            if (a.Y > b.Y)
            {
                a.HasBottomWall = false;
                b.HasTopWall = false;
            }
            else
            {
                b.HasBottomWall = false;
                a.HasTopWall = false;
            }
        }
        else
        {
            if (a.X > b.X)
            {
                a.HasLeftWall = false;
                b.HasRightWall = false;
            }
            else
            {
                b.HasLeftWall = false;
                a.HasRightWall = false;
            }
        }
    }
}
