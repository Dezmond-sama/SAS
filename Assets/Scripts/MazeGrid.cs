using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public int RoomNumber { get; private set; }
    public bool Walkable;
    public GridCell(int room)
    {
        RoomNumber = room;
    }

    public bool CameraSlot = false;
}
public class MazeGrid
{
    private GridCell[,] _cells;
    private List<GridCell> _cameraSlots = new List<GridCell>();
    public int CameraSlotsCount()
    {
        return _cameraSlots.Count;
    }
    public GridCell CameraSlot(int index)
    {
        if (index < 0 || index >= _cameraSlots.Count) return null;
        return _cameraSlots[index];
    }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public GridCell this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
            return _cells[x, y];
        }
    }

    static public MazeGrid CreateGrid(Maze maze, int scaleCoef)
    {
        MazeGrid grid = new MazeGrid();

        grid.Width = maze.Width * scaleCoef;
        grid.Height = maze.Height * scaleCoef;
        grid._cells = new GridCell[grid.Width, grid.Height];

        for (int x = 0; x < maze.Width; ++x)
        {
            for (int y = 0; y < maze.Height; ++y)
            {
                for (int inner_x = 0; inner_x < scaleCoef; ++inner_x)
                {
                    for (int inner_y = 0; inner_y < scaleCoef; ++inner_y)
                    {
                        bool walkable = false;
                        bool cameraSlot = false;
                        bool left = inner_x == 0;
                        bool right = inner_x == scaleCoef - 1;
                        bool top = inner_y == scaleCoef - 1;
                        bool bottom = inner_y == 0;
                        if (maze[x, y].HasLeftWall && maze[x, y].HasRightWall && maze[x, y].HasTopWall && maze[x, y].HasBottomWall)
                        {
                            walkable = false;
                        }
                        else if (left || right || top || bottom)
                        {
                            if(left && !maze[x, y].HasLeftWall || right && !maze[x, y].HasRightWall)
                            {
                                if (top) walkable = !maze[x, y].HasTopWall;
                                else if (bottom) walkable = !maze[x, y].HasBottomWall;
                                else walkable = true;
                            }else if (top && !maze[x, y].HasTopWall || bottom && !maze[x, y].HasBottomWall)
                            {
                                if (left) walkable = !maze[x, y].HasLeftWall;
                                else if (right) walkable = !maze[x, y].HasRightWall;
                                else walkable = true;
                            }
                            cameraSlot = left && top && maze[x, y].HasLeftWall && maze[x, y].HasTopWall && !maze[x, y].HasBottomWall && !maze[x, y].HasRightWall ||
                                left && bottom && maze[x, y].HasLeftWall && maze[x, y].HasBottomWall && !maze[x, y].HasTopWall && !maze[x, y].HasRightWall ||
                                right && top && maze[x, y].HasRightWall && maze[x, y].HasTopWall && !maze[x, y].HasBottomWall && !maze[x, y].HasLeftWall ||
                                right && bottom && maze[x, y].HasRightWall && maze[x, y].HasBottomWall && !maze[x, y].HasTopWall && !maze[x, y].HasLeftWall;
                        }
                        else
                        {
                            walkable = true;
                        }

                        grid._cells[x * scaleCoef + inner_x, y * scaleCoef + inner_y] = new GridCell(maze[x,y].RoomNumber);
                        grid._cells[x * scaleCoef + inner_x, y * scaleCoef + inner_y].Walkable = walkable;
                        grid._cells[x * scaleCoef + inner_x, y * scaleCoef + inner_y].CameraSlot = cameraSlot;

                    }
                }
            }
        }
        for (int i = 0; i < maze.DoorsCount(); i++)
        {
            MazeDoor door = maze.Door(i);
            Vector2 coords = (Vector2)(door.FirstCell.Coords + door.SecondCell.Coords) / 2f;
            Debug.Log(door.FirstCell.Coords + " " + door.SecondCell.Coords + " " + coords);
            float x_border = coords.x - (int)coords.x < 0.1f ? scaleCoef - 1f : scaleCoef / 2f;
            float x_start = coords.x - (int)coords.x < 0.1f ? 0f : scaleCoef / 2f - 1f;
            float y_border = coords.y - (int)coords.y < 0.1f ? scaleCoef - 1f : scaleCoef / 2f;
            float y_start = coords.y - (int)coords.y < 0.1f ? 0f : scaleCoef / 2f - 1f;
            for (float inner_x = x_start; inner_x <= x_border; inner_x += 1f)
            {
                for (float inner_y = y_start; inner_y <= y_border; inner_y += 1f)
                {
                    bool walkable = (inner_x - x_start > 0.1 && x_border - inner_x > 0.1f && Mathf.Abs((x_start + x_border) / 2 - inner_x) < 0.7f ||
                        inner_y - y_start > 0.1 && y_border - inner_y > 0.1f && Mathf.Abs((y_start + y_border) / 2 - inner_y) < 0.7f);
                    grid._cells[(int)(coords.x * scaleCoef + inner_x), (int)(coords.y * scaleCoef + inner_y)].Walkable = walkable;
                }
            }
        }
        return grid;
    }
}
