using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinates{
    public int[,] coords= { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
    public int style;

    public Coordinates(){
        
    }
}

public class Zoid {
    public int x = 3;
    public int y = -2;
    public int r = 0;
    public int zoidType = 0;
    public static string names = "IOTSZJL"; // Lindstedt's order - important for consistent random pieces
    int[,,,] allCoord = {
            {{{0, 2}, {1, 2}, {2, 2}, {3, 2}}, {{2, 0}, {2, 1}, {2, 2}, {2, 3}}, {{0, 2}, {1, 2}, {2, 2}, {3, 2}}, {{2, 0}, {2, 1}, {2, 2}, {2, 3}}}, //I
            {{{1, 2}, {2, 2}, {1, 3}, {2, 3}}, {{1, 2}, {2, 2}, {1, 3}, {2, 3}}, {{1, 2}, {2, 2}, {1, 3}, {2, 3}}, {{1, 2}, {2, 2}, {1, 3}, {2, 3}}}, //O
            {{{1, 2}, {2, 2}, {3, 2}, {2, 3}}, {{2, 1}, {1, 2}, {2, 2}, {2, 3}}, {{2, 1}, {1, 2}, {2, 2}, {3, 2}}, {{2, 1}, {2, 2}, {3, 2}, {2, 3}}}, //T 
            {{{2, 2}, {3, 2}, {1, 3}, {2, 3}}, {{2, 1}, {2, 2}, {3, 2}, {3, 3}}, {{2, 2}, {3, 2}, {1, 3}, {2, 3}}, {{2, 1}, {2, 2}, {3, 2}, {3, 3}}}, //S
            {{{1, 2}, {2, 2}, {2, 3}, {3, 3}}, {{3, 1}, {2, 2}, {3, 2}, {2, 3}}, {{1, 2}, {2, 2}, {2, 3}, {3, 3}}, {{3, 1}, {2, 2}, {3, 2}, {2, 3}}}, //Z
            {{{1, 2}, {2, 2}, {3, 2}, {3, 3}}, {{2, 1}, {2, 2}, {1, 3}, {2, 3}}, {{1, 1}, {1, 2}, {2, 2}, {3, 2}}, {{2, 1}, {3, 1}, {2, 2}, {2, 3}}}, //J
            {{{1, 2}, {2, 2}, {3, 2}, {1, 3}}, {{1, 1}, {2, 1}, {2, 2}, {2, 3}}, {{3, 1}, {1, 2}, {2, 2}, {3, 2}}, {{2, 1}, {2, 2}, {2, 3}, {3, 3}}} //L          
        };
    // style refers to one of the three ways a NES block is drawn: 0) large white block with primary border,
    // 1) primary bg with white highlight, or 2) secondary bg with white highlight
    // so, for IOTSZJL, it's 0001212
    public int style = 0;

    public static Zoid Spawn(int typeOfZoid){
        Zoid z = new Zoid();
        z.zoidType = typeOfZoid;
        if ((typeOfZoid == 3) || (typeOfZoid == 5)) 
            { z.style = 1; }
        else if (typeOfZoid > 2) 
            { z.style = 2; }
        return z;
    }

    public Coordinates GetBlocks(){
        int[,] originalCoords = { {allCoord[zoidType, r, 0, 0], allCoord[zoidType, r, 0, 1]},
                                  {allCoord[zoidType, r, 1, 0], allCoord[zoidType, r, 1, 1]},
                                  {allCoord[zoidType, r, 2, 0], allCoord[zoidType, r, 2, 1]},
                                  {allCoord[zoidType, r, 3, 0], allCoord[zoidType, r, 3, 1]} };
        Coordinates currentCoords = new Coordinates();
        currentCoords.style = style;
        for (int i = 0; i < 4; i++){
            currentCoords.coords[i, 0] = originalCoords[i, 0] + x;
            currentCoords.coords[i, 1] = originalCoords[i, 1] + y;
        }
        return currentCoords;
    }

    //more or less direct copy from Meta_TWO JS. This could easily be cleaned up...
    //not ideal, but given JS doesn't have keyword arguments, this prevents me
    //from having to pass in an object each time I want the blocks.
    //this should only be called from the collision function below
    Coordinates GetBlocksWithRotation(int localR)
    {
        int[,] originalCoords = { {allCoord[zoidType, localR, 0, 0], allCoord[zoidType, localR, 0, 1]},
                                  {allCoord[zoidType, localR, 1, 0], allCoord[zoidType, localR, 1, 1]},
                                  {allCoord[zoidType, localR, 2, 0], allCoord[zoidType, localR, 2, 1]},
                                  {allCoord[zoidType, localR, 3, 0], allCoord[zoidType, localR, 3, 1]} };
        Coordinates currentCoords = new Coordinates();
        for (int i = 0; i < 4; i++)
        {
            currentCoords.coords[i, 0] = originalCoords[i, 0] + x;
            currentCoords.coords[i, 1] = originalCoords[i, 1] + y;
        }
        return currentCoords;
    }

    public int[,] ZoidRep(){
        Coordinates coordinates = GetBlocks();
        int[,] tempboard = {{0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};
        for (int i = 0; i < 4; i++){
            int row = coordinates.coords[i, 1];
            int column = coordinates.coords[i, 0];
            if (row >= 0 && column >= 0)
            {
                tempboard[row, column] = zoidType + 1;
            }
        }

        return tempboard;
    }

    public bool Collide(Board board, int vx, int vy, int vr){
        vr = (r + vr) & 3;
        int ix = 0, iy = 0;
        Coordinates rotateBlocks = GetBlocksWithRotation(vr);
        for (int i = 0; i < 4; i++)
        {
            ix = rotateBlocks.coords[i, 0] + vx;
            iy = rotateBlocks.coords[i, 1] + vy;
            if ((ix < 0) || (ix >= board.boardWidth)) { return true; }
            if (iy >= board.boardHeight) { return true; }
            if (board.IsFilled(ix, iy)) { return true; }
        }
        return false;
    }

}
