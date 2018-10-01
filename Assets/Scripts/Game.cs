using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

enum keyEnum { Down, Left, Right, Pause, Rotate, Counterrotate, Invert };
enum soundEnum {clear1, clear4, crash, keep, music, music_fast, levelup, move, pause, rotate, slam };

public class Game : MonoBehaviour {

    public GameObject UIController;

    public Board board;
    Board dummyboard;
    Camera cam;
    public AudioClip[] clips;
    public AudioSource musicSystem;
    AudioSource soundSystem;
    public UnityEngine.UI.Text scoreText;

    int DAS_NEGATIVE_EDGE=10;
    int DAS_MAX=16;
    int GRAVITY_START_DELAY=97;
    int LINECLEAR_STEPS=5;
    // not expecting to go past level 30?
    int[] speedLevels = { 48, 43, 38, 33, 28, 23, 18, 13, 8, 6, 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1 };
    int[] scoreVals = { 0, 40, 100, 300, 1200 };
    delegate void taskDelegate();
    taskDelegate currentTask;
    public Queue<Zoid> previewZoidQueue;
    [SerializeField] private LineRenderer nextLine;

    [SerializeField] private GameObject zoidSprite;

  
    private GameObject[,] renderedSprites;
    private SpriteRenderer[,] spriteRenderers;

    private GameObject nextSpriteParent;
    private SpriteRenderer[,] nextRenderers;
    private GameObject[,] nextSprites;

    private Sprite[] boardSprites; // will be 0-29, every group of three is for one level, times 10 level color schemes

    SpriteRenderer rend;

    Dictionary<string, int> keys = new Dictionary<string, int> { { "LEFT", 0 }, { "RIGHT", 1 }, { "DOWN", 2 }, { "ROTATE", 3 }, { "COUNTERROTATE", 4 }, {"PAUSE",5 } };
    Color darkBackgroundColor = Color.black; // new Color(.31f, .31f, .31f, 1f);
    Color lightBackgroundColor = Color.white;
    int frames = 0;
    bool alive = true;
    bool paused = false;
    bool inverted = false;
   
    int are = 0;
    int _49 = 0;
    int vx = 0;
    int vy = 0;
    int vr = 0;
    public float gameStartTime;
    
    int das = 0;
    int softdrop_timer = 0;
    int drop = 0;
    public int startlevel;

    public int level = 0;
    public int lines = 0;
    public int score = 0;
    public int episode = 0;
    public List<string> zoidBuff = new List<string>();
    public Zoid zoid;
    Zoid nextZoid;

    int drop_points = 0;
    int lines_this = 0;
    int lines_flagged = 0;

    public int curr = 0;
    public int next = 0;

    public bool useKeyboard = true;
    public bool useTomee = false;
    public bool useRetro = false;
    int piece_count = 0;
    bool leftCurr = false;
    bool leftPrev = false;
    bool rightCurr = false;
    bool rightPrev = false;
    bool downCurr = false;
    bool downPrev = false;
    bool pauseCurr = false;
    bool pausePrev = false;
    bool rotateCurr = false;
    bool rotatePrev = false;
    bool counterRotateCurr = false;
    bool counterRotatePrev = false;
    bool invertCurr;
    bool invertPrev;
    bool fastMusic = false;
    public int gameNumber = 1;
    List<int> rowsToClear = new List<int>();
    public Log log;
    public StreamWriter writer;


    UIControllerScript uiContrl;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        // either seed with fixed num from settings or the first four digits after the decimal point of the current time
        Random.InitState(Settings.randomSeed > 0? Settings.randomSeed : (int)((Time.time - (int)Time.time) * 1000));
        softdrop_timer = -GRAVITY_START_DELAY;
        cam = Camera.main;
        log = GetComponent<Log>();
        log.game = this;
        //Reset(); // create boards
        board = new Board();
        dummyboard = new Board();


        soundSystem = GetComponent<AudioSource>();
        renderedSprites = new GameObject[board.boardHeight, board.boardWidth];
        boardSprites = Resources.LoadAll<Sprite>("zoidtex");
        spriteRenderers = new SpriteRenderer[board.boardHeight, board.boardWidth];

        // instantiate all zoidSprite GameObjects
        // Phaser is a y-down coordinate system, so to keep compatibility with its board structure,
        // we're going to start with a high y-value and subtract spacing, drawing from the top down
        for (int j = 0; j < board.boardHeight; j++)
        {
            for (int i = 0; i < board.boardWidth; i++)
            {
                GameObject go = Instantiate(zoidSprite);
                rend = go.GetComponent<SpriteRenderer>();
                //rend.color = Color.black;
                rend.enabled = false;
                rend.sprite = boardSprites[0];
                renderedSprites[j, i] = go;
                spriteRenderers[j, i] = rend;
                go.transform.position = new Vector3(board.xStart + i * board.spacing, board.yStart - j * board.spacing, 0f);
                go.transform.parent = transform;
            }
        }
       

        // create sprites for Next window
        nextSpriteParent = new GameObject("Next");
        nextSprites = new GameObject[14, 4];  // for up to 5 preview pieces, 2 rows each with an empey row in between
        nextRenderers = new SpriteRenderer[14, 4];

        for (int j = 0; j < 14; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject go = Instantiate(zoidSprite);
                rend = go.GetComponent<SpriteRenderer>();
                rend.enabled = false;
                rend.sprite = boardSprites[0];
                nextSprites[j, i] = go;
                nextRenderers[j, i] = rend;
                go.transform.position = new Vector3(i * board.spacing, -j * board.spacing, 0f); // -j to work down
                go.transform.parent = nextSpriteParent.transform;
            }
        }
        // move parent after parenting! subsprites are instantiated in world position
        nextSpriteParent.transform.position = new Vector3(2f, 1f, 0f);

        if (Settings.numPreviewZoids == 0){
            nextSpriteParent.SetActive(false);
            nextLine.gameObject.SetActive(false);
        }
        else{
            // with one, the y-value of elements 2 and 3 of the "next" box is set to 0.6. We work our way down based on i*board.spacing
            Vector3 pos2 = nextLine.GetPosition(2);
            Vector3 pos3 = nextLine.GetPosition(3);
            nextLine.SetPosition(2, new Vector3(pos2.x, pos2.y - (2.3f * board.spacing * Settings.numPreviewZoids), pos2.z));
            nextLine.SetPosition(3, new Vector3(pos3.x, pos3.y - (2.3f * board.spacing * Settings.numPreviewZoids), pos3.z));
        }

        uiContrl = UIController.GetComponent<UIControllerScript>();

    }

    // Use this for initialization
    public void Reset () {
        //print(Application.persistentDataPath);
        System.DateTime timestamp = System.DateTime.Now;
        writer = new StreamWriter(Application.persistentDataPath + "/" + string.Format("{0}_{1}", Settings.subjectID, timestamp.ToString("yyyy-MM-dd_HH-mm-ss")) + ".tsv", true);
        writer.WriteLine(log.logHeader);
        board = new Board();
        dummyboard = new Board();
        frames = 0;
        alive = true;
        paused = false;
        gameStartTime = Time.time;
        cam.backgroundColor = darkBackgroundColor;

        are = 0;
        _49 = 0;
        vx = 0;
        vy = 0;
        vr = 0;

        das = 0;
        softdrop_timer = 0;
        drop = 0;

        level = startlevel;
        lines = 0;
        score = 0;
        episode = 0;
        zoidBuff = new List<string>();

        drop_points = 0;
        lines_this = 0;
        lines_flagged = 0;

        curr = 0;
        next = 0;

        piece_count = 0;
        leftCurr = false;
        leftPrev = false;
        rightCurr = false;
        rightPrev = false;
        downCurr = false;
        downPrev = false;
        pauseCurr = false;
        pausePrev = false;
        rotateCurr = false;
        rotatePrev = false;
        counterRotateCurr = false;
        counterRotatePrev = false;
        fastMusic = false;
        rowsToClear = new List<int>();
       
        currentTask = Active;
        previewZoidQueue = new Queue<Zoid>();
        curr = Mathf.FloorToInt(Random.value * 7);
        zoid = Zoid.Spawn(curr);

        next = Mathf.FloorToInt(Random.value * 7);
        nextZoid = Zoid.Spawn(next);
        previewZoidQueue.Enqueue(nextZoid);
        zoidBuff.Add(Zoid.names[curr].ToString());

        //generate any further next pieces based on preview size
        if (Settings.numPreviewZoids > 1)
        {
            for (int i = 0; i < (Settings.numPreviewZoids - 1); i++)
            {
                previewZoidQueue.Enqueue(Zoid.Spawn(Mathf.FloorToInt(Random.value * 7)));
            }
        }

        // making sure the list of preview pieces is working correctly
        //string debugNames = "";
        //for (int i = 0; i < Settings.numPreviewZoids; i++)
        //{
        //    debugNames += Zoid.names[previewZoidQueue.ElementAt<Zoid>(i).zoidType].ToString();
        //}
        //print(debugNames);
        log.LogEvent("GAME", "BEGIN", "");

        musicSystem.clip = clips[(int)soundEnum.music];
        musicSystem.Play();
       
	}
	
	// Update is called once per frame
	void Update () {
        Poll();
        //
        if (JustPressed(keyEnum.Pause) && (!paused))
        {
            paused = true;
            musicSystem.Stop();
            soundSystem.PlayOneShot(clips[(int)soundEnum.pause], 1f);
        }
        else if (JustPressed(keyEnum.Pause) && (paused))
        {
            paused = false;
            musicSystem.Play();
        }
        if (JustPressed(keyEnum.Invert) && (!inverted))
        {
            inverted = true;
            // invertris? lines don't quite work perfectly
            transform.localScale = new Vector3(1f, -1f, 1f);
            transform.position = new Vector3(0f, 0.06f, 0f);
        }
        else if (JustPressed(keyEnum.Invert) && (inverted))
        {
            inverted = false;
            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;
        }
        //
        if (alive && !paused)
        {
            Sub_94ee(); // a housekeeping function, named after its original ROM address
            drop++;
            currentTask(); // only one real "job" is handled every frame, 
                           // this can be moving the zoid, checking for line clears, or updating the score
                           // currentTask is a function pointer
            //foreach (taskDelegate subscriber in currentTask.GetInvocationList())
            //{
            //    print(subscriber.Method.Name);
            //}
            frames++;
            log.LogWorld();
        }
        RenderBoard();
	}

  // Possibly the most delicate and important function in the game, translating user input into game commands.
  // This logic is critical for certain advanced strategies once the game becomes super-fast, including charging DAS
  // and allowing for the "Chinese Fireball" maneuver. PLEASE don't modify unless you're explicitly trying to trip up the experts
  // as an experimental condition

    void Control(){
        if (!downCurr)
        {
            if (JustPressed(keyEnum.Right) || JustPressed(keyEnum.Left))
            {
                das = 0;
            }
            if (rightCurr) { vx = 1; }
            else if (leftCurr) { vx = -1; }
            else { vx = 0; }
        }
        else { vx = 0; }

        if (softdrop_timer < 0)
        {
            if (JustPressed(keyEnum.Down))
            {
                softdrop_timer = 0;
            }
            else
            {
                softdrop_timer++;
            }
        }

        if (softdrop_timer >= 0)
        {
            if (softdrop_timer == 0)
            {
                if (leftCurr || rightCurr) { } //do nothing - "regular" gravity
                else if (OnlyDownHit())
                {
                    softdrop_timer = 1;
                }
            }
            else
            {
                if (OnlyDown())
                {
                    softdrop_timer++;
                    if (softdrop_timer > 2)
                    {
                        softdrop_timer = 1;
                        drop_points++;
                        vy = 1;
                    }
                    else
                    {
                        vy = 0;
                    }
                }
                else
                {
                    softdrop_timer = 0;
                    vy = 0;
                    drop_points = 0;
                }
            }
        }

        if (JustPressed(keyEnum.Rotate))
        {
            vr = 1;
        }
        else if (JustPressed(keyEnum.Counterrotate))
        {
            vr = -1;
        }
        else
        {
            vr = 0;
        }
    }

    void Move(){
        if (vx != 0)
        {
            bool shift = false;
            if (das == 0)
            {
                shift = true;
            }
            if (das >= DAS_MAX)
            {
                shift = true;
                das = DAS_NEGATIVE_EDGE;
            }
            das++;

            if (shift)
            {
                if (!zoid.Collide(board, vx, 0, 0))
                {
                    zoid.x += vx;
                    log.LogEvent("ZOID", "TRANSLATE", vx.ToString()); 
                    soundSystem.PlayOneShot(clips[(int)soundEnum.move], 1f);
                }
                else
                {
                    das = DAS_MAX;
                }
            }
        }
    }

    void Rotate(){
        if (vr != 0)
        {
            if (!zoid.Collide(board, 0, 0, vr))
            {
                soundSystem.PlayOneShot(clips[(int)soundEnum.rotate], 1f);

                log.LogEvent("ZOID", "ROTATE", vr.ToString());

                zoid.r += vr;
                zoid.r = zoid.r & 3;
            }

        }
    }

    void Gravity(){
        if (softdrop_timer < 0)
        {
            return;
        }
        if ((vy != 0) || (drop >= speedLevels[level < 29 ? level : 29]))
        {
            if (vy != 0) { log.LogEvent("ZOID", "U-DOWN", ""); }
            else { log.LogEvent("ZOID", "DOWN", ""); }
            vy = 0;
            drop = 0;
            if (!zoid.Collide(board, 0, 1, 0))
            {
                zoid.y++;
            }
            else
            {
                // we're playing the "lock" sound now, but technically the piece doesn't commit until the next frame (in updateTask)
                Sub_9caf();
                currentTask = UpdateTask;
                log.LogEvent("PLACED", Zoid.names[curr].ToString(), "");
                if (drop_points >= 2)
                {
                    soundSystem.PlayOneShot(clips[(int)soundEnum.slam], 1f);

                    log.LogEvent("ZOID", "SLAMMED", "");
                }
                else
                {
                    soundSystem.PlayOneShot(clips[(int)soundEnum.keep], 1f);

                }
            }
        }
    }

    void Active(){
        Control();
        Move();
        Rotate();
        Gravity();
    }

    void UpdateTask(){
        if (are == 0)
        {
            are = 1;
            if (board.Commit(zoid))
            {
                //GAME OVER
                alive = false;
                musicSystem.Stop();
                soundSystem.PlayOneShot(clips[(int)soundEnum.crash], 1f);

                //// LOG END-OF-GAME INFO
                log.LogGameSumm();
                writer.Close();

                //todo:display score
                uiContrl.FinishGame();
            }
        }

        else if ((!fastMusic) && (PileHeight() >= 15))
        {
            fastMusic = true;
            musicSystem.Stop();
            musicSystem.clip = clips[(int)soundEnum.music_fast];
            musicSystem.Play();
        }

        if (_49 < 0x20)
        {
            return;
        }

        are = 0;
        Sub_9caf();
        currentTask = LineCheck;
        //copy board contents into the backup board, in case there are lines to clear
        System.Array.Copy(board.contents, dummyboard.contents, board.contents.Length);
    }

    void LineCheck(){
        if (_49 < 0x20)
        {
            return;
        }
        int row = Mathf.Max(0, zoid.y);

        row += are;

        if ((row < dummyboard.boardHeight) && (dummyboard.LineCheck(row)))
        {
            dummyboard.LineDrop(row); //clear lines from "backup" board, copy to actual board at end of animation
            lines_this++;
            rowsToClear.Add(row);
        }

        are++;

        if (are >= 4)
        {
            _49 = 0;
            are = 0;
            if (lines_this != 0)
            {
                currentTask = LineAnim;
                // PLAY LINE SOUND
                if ((lines_this > 0) && (lines_this < 4))
                {
                    soundSystem.PlayOneShot(clips[(int)soundEnum.clear1], 1f);

                }
                if (lines_this == 4)
                {
                    soundSystem.PlayOneShot(clips[(int)soundEnum.clear4], 1f);

                }
            }
            else
            {
                currentTask = ScoreUpdate;

            }
        }
    }

    void LineAnim(){
        // all handled in 94ee, but checked elsewhere
    }

    void ScoreUpdate(){
        int lines_before = lines;
        lines += lines_this;
        int hex_trick = 0;
        if (Mathf.FloorToInt(lines / 10) > Mathf.FloorToInt(lines_before / 10))
        {
            hex_trick = Mathf.FloorToInt(lines / 10);
            hex_trick = System.Convert.ToInt32(hex_trick.ToString(), 16);
            if (hex_trick > level)
            {
                level++;
                soundSystem.PlayOneShot(clips[(int)soundEnum.levelup], 1f);

            }
        }
        level = level & 255;

        score += (level + 1) * scoreVals[lines_this];
        // To replicate the drop score bug, we need to convert the last 
        // two digits to packed binary coded decimal.
        if (drop_points >= 2)
        {
            int modScore = score % 100;
            hex_trick = System.Convert.ToInt32(modScore.ToString(), 16);
            hex_trick--;
            hex_trick += drop_points;
            if ((hex_trick & 0x0F) >= 0x0A)
            {
                hex_trick += 0x06;
            }
            if ((hex_trick & 0xF0) >= 0xA0)
            {
                hex_trick = hex_trick & 0xF0;
                hex_trick += 0x60;
            }
            score -= modScore;
            score += System.Convert.ToInt32(hex_trick.ToString("X"), 10);
        }
        if ((fastMusic) && (PileHeight() < 15))
        {
            fastMusic = false;
            musicSystem.Stop();
            musicSystem.clip = clips[(int)soundEnum.music];
            musicSystem.Play();
        }
        // LOG EPISODE info
        log.LogEpisode();
        log.LogEvent("EPISODE", "END", "");
        currentTask = GoalCheck;
    }

    void GoalCheck(){
        //not applicable in A-Type MetaTWO
        currentTask = Dummy;
    }

    void Dummy(){
        //skipped frame for unimplemented 2-player code
        currentTask = Prep;
    }

    void Prep(){
        if (_49 < 0x20)
        {
            return;
        }
        episode++;
        are = 0;
        lines_this = 0;
        drop_points = 0;
        softdrop_timer = 0;
        drop = 0;
        vy = 0;

        // dequeue, then peek so that "next" stays in the queue
        previewZoidQueue.Dequeue();

        curr = nextZoid.zoidType;
        //next = Mathf.FloorToInt(Random.value * 7);
        zoid = Zoid.Spawn(curr);
        previewZoidQueue.Enqueue(Zoid.Spawn(Mathf.FloorToInt(Random.value * 7)));
        nextZoid = previewZoidQueue.Peek();
        //nextZoid = Zoid.Spawn(next);

        string debugNames = "";
        for (int i = 0; i < Settings.numPreviewZoids; i++)
        {
            debugNames += Zoid.names[previewZoidQueue.ElementAt<Zoid>(i).zoidType].ToString();
        }
        //print(debugNames);

        zoidBuff.Add(Zoid.names[curr].ToString());
        log.LogEvent("ZOID", "NEW", Zoid.names[curr].ToString());
        log.LogEvent("EPISODE", "BEGIN", "");
        currentTask = Active;
    }

    void Sub_94ee(){
        if (currentTask == LineAnim)
        {
            if ((frames & 3) == 0)
            {
                are++;
                //advance through line animation
                for (int i = 0; i < rowsToClear.Count; i++)
                {
                    switch (are)
                    {
                        case 1:
                            board.contents[rowsToClear[i] + 3, 4] = 0;
                            board.contents[rowsToClear[i] + 3, 5] = 0;
                            break;
                        case 2:
                            board.contents[rowsToClear[i] + 3, 3] = 0;
                            board.contents[rowsToClear[i] + 3, 6] = 0;
                            //if (rowsToClear.Count == 4) { stage.backgroundColor = 0xffffff; }
                            if (rowsToClear.Count == 4) { cam.backgroundColor = lightBackgroundColor; }
                            break;
                        case 3:
                            board.contents[rowsToClear[i] + 3, 2] = 0;
                            board.contents[rowsToClear[i] + 3, 7] = 0;
                            cam.backgroundColor = darkBackgroundColor;
                            // stage.backgroundColor = 0x050505;
                            break;
                        case 4:
                            board.contents[rowsToClear[i] + 3, 1] = 0;
                            board.contents[rowsToClear[i] + 3, 8] = 0;
                            //if (rowsToClear.Count == 4) { stage.backgroundColor = 0xffffff; }
                            if (rowsToClear.Count == 4) { cam.backgroundColor = lightBackgroundColor; }

                            break;
                        case 5:
                            board.contents[rowsToClear[i] + 3, 0] = 0;
                            board.contents[rowsToClear[i] + 3, 9] = 0;
                            break;
                    }
                }
            }
           
            if (are >= LINECLEAR_STEPS)
            {
                are = 0;
                currentTask = ScoreUpdate;
                rowsToClear = new List<int>();
                //stage.backgroundColor = 0x050505;
                cam.backgroundColor = darkBackgroundColor;
                System.Array.Copy(dummyboard.contents, board.contents, dummyboard.contents.Length);
                //board.contents = dummyboard.contents.JSON.parse(JSON.stringify(dummyBoard.contents)); //animation is done, copy cleared lines to board for rendering
            }
            _49 = 0;
        }
        else{
            for (int i = 0; i < 4; i++)
            {
                this.Sub_9725();
            }
        }
    }

    void Sub_9725(){
        if (_49 > 0x15)
        {
            return;
        }
        _49++;
        if (_49 < 0x14)
        {
            return;
        }
        _49 = 0x20;
    }

    void Sub_9caf(){
        _49 = zoid.y;
        if (_49 < 0)
        {
            _49 = 0;
        }
    }

    void Poll(){
        leftPrev = leftCurr;
        rightPrev = rightCurr;
        downPrev = downCurr;
        rotatePrev = rotateCurr;
        counterRotatePrev = counterRotateCurr;
        pausePrev = pauseCurr;
        invertPrev = invertCurr;

        if (useKeyboard){
            leftCurr = Input.GetButton("Left");
            rightCurr = Input.GetButton("Right");
            downCurr = Input.GetButton("Down");
            rotateCurr = Input.GetButton("Rotate");
            counterRotateCurr = Input.GetButton("Counterrotate");
            pauseCurr = Input.GetButton("Pause");
            invertCurr = Input.GetButton("Invert");
        }
        /*
        Tomee converted gamepad
        MetaTWO.config.AButton = Phaser.Gamepad.BUTTON_0;
        MetaTWO.config.BButton = Phaser.Gamepad.BUTTON_1;
        MetaTWO.config.leftButton = Phaser.Gamepad.BUTTON_5;
        MetaTWO.config.rightButton = Phaser.Gamepad.BUTTON_6;
        MetaTWO.config.downButton = Phaser.Gamepad.BUTTON_4;
        MetaTWO.config.startButton = Phaser.Gamepad.BUTTON_3;
        */
        else if (useTomee){
            //for (int i =0; i<20; i++)
            //{
            //    if (Input.GetKey("joystick button " + i.ToString())) print("button " + i.ToString());
            //}
            //print("H " + Input.GetAxis("Horizontal").ToString());

            leftCurr = Input.GetAxis("Horizontal") == -1; // Input.GetKey("joystick button 5");
            rightCurr = Input.GetAxis("Horizontal") == 1; // Input.GetKey("joystick button 6");
            downCurr = Input.GetAxis("Vertical") == -1; //Input.GetKey("joystick button 4");
            rotateCurr = Input.GetKey("joystick button 0");
            counterRotateCurr = Input.GetKey("joystick button 1");
            pauseCurr = Input.GetKey("joystick button 3");
            invertCurr = Input.GetButton("Invert");
        }
        /*
        NES-Retro gamepad
        MetaTWO.config.AButton = Phaser.Gamepad.BUTTON_1;
        MetaTWO.config.BButton = Phaser.Gamepad.BUTTON_0;
        MetaTWO.config.leftButton = Phaser.Gamepad.BUTTON_4;
        MetaTWO.config.rightButton = Phaser.Gamepad.BUTTON_6;
        MetaTWO.config.downButton = Phaser.Gamepad.BUTTON_5;
        MetaTWO.config.startButton = Phaser.Gamepad.BUTTON_3;
        }
        */
        else{
            leftCurr = Input.GetKey("joystick button 4");
            rightCurr = Input.GetKey("joystick button 6");
            downCurr = Input.GetKey("joystick button 5");
            rotateCurr = Input.GetKey("joystick button 1");
            counterRotateCurr = Input.GetKey("joystick button 0");
            pauseCurr = Input.GetKey("joystick button 3");
            invertCurr = Input.GetButton("Invert");
        }

    }

    int PileHeight(){
        for (int iy = 0; iy < board.boardHeight; iy++)
        {
            for (int ix = 0; ix < board.boardWidth; ix++)
            {
                if (board.IsFilled(ix, iy))
                {
                    return board.boardHeight - iy;
                }
            }
        }
        return 0;
    }

    bool OnlyDown(){
        //special function to determine if the down key is the only one down right now

        if (downCurr && !leftCurr && !rightCurr && !rotateCurr && !counterRotateCurr && !invertCurr)
        { return true; }
        else { return false; }
    }

    bool JustPressed(keyEnum k){
        switch (k){
            case keyEnum.Left:
                if ((leftCurr && !leftPrev)) { return true; }
                break;
            case keyEnum.Right:
                if ((rightCurr && !rightPrev)) { return true; }
                break;
            case keyEnum.Down:
                if ((downCurr && !downPrev)) { return true; }
                break;
            case keyEnum.Pause:
                if ((pauseCurr && !pausePrev)) { return true; }
                break;
            case keyEnum.Rotate:
                if ((rotateCurr && !rotatePrev)) { return true; }
                break;
            case keyEnum.Counterrotate:
                if ((counterRotateCurr && !counterRotatePrev)) { return true; }
                break;
            case keyEnum.Invert:
                if ((invertCurr && !invertPrev)) { return true; }
                break;
        }
        return false;
    }

    bool OnlyDownHit(){
        if (JustPressed(keyEnum.Down) &&
            !JustPressed(keyEnum.Left) &&
            !JustPressed(keyEnum.Right) &&
            !JustPressed(keyEnum.Rotate) &&
            !JustPressed(keyEnum.Counterrotate) &&
            !JustPressed(keyEnum.Invert))
        {
            return true;
        }
        else { return false; }
    }

    public void ClearBoard(){
        for (int j = 0; j < board.boardHeight; j++)
        {
            for (int i = 0; i < board.boardWidth; i++)
            {
                spriteRenderers[j, i].color = Color.black;
                spriteRenderers[j, i].enabled = false;
            }
        }
        for (int i = 0; i < 14; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                nextRenderers[i, j].color = Color.black;
                nextRenderers[i, j].enabled = false;
            }
        }
        paused = true;
    }

    private void RenderBoard()
    {
        if (!paused)
        {
            for (int j = 0; j < board.boardHeight; j++)
            {
                for (int i = 0; i < board.boardWidth; i++)
                {
                    rend = spriteRenderers[j, i];
                    if (board.IsFilled(i, j))
                    {
                        switch (board.GetStyle(i, j))
                        {
                            case 0:
                                rend.color = Color.white;
                                rend.sprite = boardSprites[3 * (level % 10)];
                                rend.enabled = true;
                                break;
                            case 1:
                                rend.color = Color.white;
                                rend.sprite = boardSprites[3 * (level % 10) + 1];
                                rend.enabled = true;
                                break;
                            case 2:
                                rend.color = Color.white;
                                rend.sprite = boardSprites[3 * (level % 10) + 2];
                                rend.enabled = true;
                                break;
                        }
                    }
                    else
                    {
                        rend.color = Color.black;
                        rend.enabled = false;
                    }
                }
            }
            // zoid
            Coordinates blocks = zoid.GetBlocks();
            for (int i = 0; i < 4; i++){
                if (blocks.coords[i,1] >= 0){
                    rend = spriteRenderers[blocks.coords[i, 1],blocks.coords[i, 0]];
                    switch(zoid.style)
                    {
                        case 0:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10)];
                            rend.enabled = true;
                            break;
                        case 1:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10) + 1];
                            rend.enabled = true;
                            break;
                        case 2:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10) + 2];
                            rend.enabled = true;
                            break;
                    }
                }
            }

           
            // clear nextRenderers;
            for (int i = 0; i < 14; i++){
                for (int j = 0; j < 4; j++)
                {
                    nextRenderers[i, j].color = Color.black;
                    nextRenderers[i, j].enabled = false;
                }
            }

            // render all "next" zoids
            for (int i = 0; i < Settings.numPreviewZoids; i++)
            {

                Coordinates nextBlocks = previewZoidQueue.ElementAt<Zoid>(i).GetBlocks();// nextZoid.GetBlocks();

                // an 'L' comes in as {1, 2}, {2, 2}, {3, 2}, {1, 3}
                // x coord can range from 0-3, y coord is either 2 or 3
                // y is down, L is: * * *
                //                  *
                // we use a 4x2 grid of sprite renderers for the next block

                //cycle through the four sets of coordinates in this zoid
                for (int j = 0; j < 4; j++)
                {
                    // zoid initial position is x=3, y = -2, so we can leave the y coord alone but need to substract 3 from the x
                    // drawing backwards!? rotating won't help. 1 - y is doing it, not sure why  ¯\_(ツ)_/¯
                    rend = nextRenderers[nextBlocks.coords[j, 1] + (i * 3), nextBlocks.coords[j, 0] - 3];
                    switch (previewZoidQueue.ElementAt<Zoid>(i).style)
                    {
                        case 0:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10)];
                            rend.enabled = true;
                            break;
                        case 1:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10) + 1];
                            rend.enabled = true;
                            break;
                        case 2:
                            rend.color = Color.white;
                            rend.sprite = boardSprites[3 * (level % 10) + 2];
                            rend.enabled = true;
                            break;
                    }

                }
            }

            scoreText.text = string.Format("Score: {0}\nLines: {1}\nLevel: {2}", score, lines, level);
        }
        else{
            // clear all sprites!
            for (int j = 0; j < board.boardHeight; j++)
            {
                for (int i = 0; i < board.boardWidth; i++)
                {
                    spriteRenderers[j, i].enabled = false;
                    spriteRenderers[j, i].color = Color.black;
                }
            }
            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextRenderers[i, j].color = Color.black;
                    nextRenderers[i, j].enabled = false;
                }
            }

            scoreText.text = "Paused";
        }

    }

   
}

//IMMEDIATE
// 5 preview zoids

//FOR TOURNAMENT
//local data logging
//local controllers
//send eyetrack tick
//experimental condition (invert when?)

//MEDIUM-TERM
//remote data logging