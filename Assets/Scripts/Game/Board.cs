using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board {

    public float xStart = -1f;
    public float yStart = 2.4f;
    public float spacing = .25f;

    public int boardHeight = 20;
    public int boardWidth = 10;
    public int vanish = 3;
    public int[,] contents;

	// Use this for initialization
	public Board () {
        contents = new int[boardHeight + vanish, boardWidth];

	}
	

    public int[,] GetBoard(){
        return contents;
    }

    public void PrintBoard(){
        
    }

    public int GetStyle (int x, int y) {
        int val = contents[y + vanish, x];
        if (val< 4){ return 0;}
        else if ((val == 4) || (val == 6)) {return 1;}
        else {return 2;}
    }

    public bool IsFilled(int x, int y)
    {
        return (contents[y + vanish, x] != 0);
    }

    public int GetCell(int x, int y)
    {
        return contents[y + this.vanish, x];
    }

    public void SetCell(int x, int y, int val)
    {
        contents[y + this.vanish, x] = val;
    }

    public void LineDrop (int row)
    {
        bool bug = (row <= 0);
        int top_row = 0;
        if (bug){
            row = boardHeight - 1;
            top_row = -vanish;
        }
        for (int j = row; j > top_row; j--){
            for (int i = 0; i< boardWidth; i++){
                contents[j + vanish, i] = contents[j - 1 + vanish, i];
            }
        }
        for (int i = 0; i< boardWidth; i++){
            contents[vanish, i] = 0;
        }
    }

    public bool LineCheck (int row){
        for (int i = 0; i< boardWidth; i++){
            if (!IsFilled(i, row)){
                return false;
            }
        }
        return true;
    }


    public bool Commit (Zoid zoid){
        bool collide = false;
        Coordinates blocks = zoid.GetBlocks();
        for (int i = 0; i< 4; i++){
            int x = blocks.coords[i, 0], y = blocks.coords[i, 1];
            if (this.IsFilled(x, y)){
                collide = true;
            }
            //console.log(x + " " + y + " " + zoid.zoidType + 1);
            SetCell(x, y, zoid.zoidType+1);
        }            
        return collide;
    }
     


}
