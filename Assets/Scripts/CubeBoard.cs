using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeBoard : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject square;
    public double fallSpeed = 0.5; //下落速度
    private double timeOffset = 0.0; //统计游戏进行总时间
    public int score = 0;
    public int removeLineCount = 0;
    private int stamp; //时间戳，每隔一秒下落一次
    public Color[] colorList = new Color[]{Color.blue, Color.yellow, Color.green, Color.red};
    public GameObject[, ] mySquare = new GameObject[30, 30];
    private int cubeKind = 0;
    private int colorKind = 0;
    private int xCore;
    private int yCore;
    private GameObject scoreTextUI;
    private Text scoreText;
    private int[,,] cubeShape = new int[15, 3, 3]{
        {{0, 0, 0}, {1, 1, 0}, {1, 1, 0}},//方块 0
        {{0, 0, 0}, {1, 1, 1}, {1, 0, 0}},//反向L 1-4
        {{1, 0, 0}, {1, 0, 0}, {1, 1, 0}},
        {{0, 0, 0}, {0, 0, 1}, {1, 1, 1}},
        {{0, 1, 1}, {0, 0, 1}, {0, 0, 1}},
        {{0, 0, 0}, {0, 1, 1}, {1, 1, 0}},//反向Z 5-6
        {{0, 1, 0}, {0, 1, 1}, {0, 0, 1}},
        {{0, 0, 0}, {1, 1, 0}, {0, 1, 1}},//Z形 7-8
        {{0, 0, 1}, {0, 1, 1}, {0, 1, 0}},
        {{0, 0, 0}, {1, 0, 0}, {1, 1, 1}},//L形 9-12
        {{1, 1, 0}, {1, 0, 0}, {1, 0, 0}},
        {{0, 0, 0}, {1, 1, 1}, {0, 0, 1}},
        {{0, 0, 1}, {0, 0, 1}, {0, 1, 1}},
        {{0, 1, 0}, {0, 1, 0}, {0, 1, 0}},//1形13-14
        {{0, 0, 0}, {1, 1, 1}, {0, 0, 0}}
    };

    private Dictionary<int, int> shapeMap = new Dictionary<int, int>{
        {0, 0}, {1, 2}, {2, 3}, {3, 4}, {4, 1}, {5, 6}, {6, 5}, {7, 8}, {8, 7},
        {9, 10}, {10, 11}, {11, 12}, {12, 9}, {13, 14}, {14, 13}
    };
    public int width = 8;
    public int hegiht = 20;
    void Start()
    {
        stamp = 0;
        xCore = width / 2 - 1;
        yCore = hegiht - 3;
        for(int j = 0; j < hegiht; j++) {
            for(int i = 0; i < width; i++) {
                mySquare[i, j] = Instantiate(square, transform.position + new Vector3(i, j, 0), Quaternion.identity);
            }
        }
        scoreTextUI = GameObject.Find("Canvas/scoreText");
        scoreText = scoreTextUI.GetComponent<Text>();
        generateNewCube();
    }

    void generateCube() {
        for(int i = 0; i < 3; i++) {
            for(int j = 0; j < 3; j++) {
                if(cubeShape[cubeKind, i, j] == 1) {
                    if(!cubeColorSame(xCore + i, yCore + j, Color.white)) {
                        scoreText.text = "Game Over! Score is " + score.ToString();
                        Time.timeScale = 0;
                        return;
                    }
                    setCubeColor(xCore + i, yCore + j, colorList[colorKind % 4]);
                }
            }
        }
    }


    void generateNewCube() { //生成真正的新方块，同时需要先消除能消失的行
        //消除行
        removeLineCount = 0;
        int i, j, k;
        for(i = 0; i < width; i++) setCubeColor(i, hegiht - 1, Color.white);
        for(j = hegiht - 2; j > -1; j--) {
            for(i = 0; i < width; i++) {
                if(cubeColorSame(i, j, Color.white)) {
                    break;
                }
            }
            if(i == width) {
                removeLineCount++;
                for(k = j; k < hegiht - 2; k++) {
                    for(i = 0; i < width; i++) {
                        setCubeColor(i, k, mySquare[i, k + 1].GetComponent<SpriteRenderer>().color);
                    }
                }
            }
        }
        score += removeLineCount * width;
        scoreText.text = "Score:" + score.ToString() + ", removeLine:" + removeLineCount.ToString();
        
        // 生成新方块
        cubeKind = Random.Range(0, 15);
        colorKind += Random.Range(0, 4);
        xCore = width / 2 - 1;
        yCore = hegiht - 3;
        generateCube();
    }

    int getCubeBottom(int x) { // 根据所在列返回图形的最下方方块坐标，-1表示该列为空
        for(int i = 0; i < 3; i++) {
            if(cubeShape[cubeKind, x, i] == 1) return i;
        }
        return -1;
    }
    

    int getCubeLeft(int y) { //根据所在行返回图形最左边方块坐标，-1表示该行没有方块
        for(int i = 0; i < 3; i++) {
            if(cubeShape[cubeKind, i, y] == 1) return i;
        }
        return -1;
    }

    int getCubeRight(int y) {
        for(int i = 2; i > -1; i--) {
            if(cubeShape[cubeKind, i, y] == 1) return i;
        }
        return -1;
    }
    bool reachBottom() { //判断方块是否到达底部
        for(int i = 0; i < 3; i++) {
            if(getCubeBottom(i) == -1) continue;
            if(yCore + getCubeBottom(i) == 0) return true;
            if(!cubeColorSame(xCore + i, yCore + getCubeBottom(i) - 1, Color.white)) {// 下一格不是白色，说明已经到达底部
                return true;
            }
        }
        return false;
    }

    void clearOld() { //清除当前方块
        for(int i = 0; i < 3; i++) {
            for(int j = 0; j < 3; j++) {
                if(cubeShape[cubeKind, i, j] == 1) {
                    setCubeColor(xCore + i, yCore + j, Color.white);
                }
            }
        }
    }

    void moveCube(int right) { //right为-1表示向左移动，1为向右移动
        bool flag = true; //flag为真时，表示对应方向没有障碍物，可以移动
        int mostArix;
        for(int i = 0; i < 3; i++) {
            if(right == -1) mostArix = getCubeLeft(i); //根据移动方向选择当前行移动方向最突出的方块，用于判断是否有障碍物
            else mostArix = getCubeRight(i);
            if(mostArix == -1) continue;
            if(xCore + mostArix + right < 0 || xCore + mostArix + right >= width) {
                flag = false;
                break;
            }
            if(!cubeColorSame(xCore + mostArix + right, yCore + i, Color.white)) {
                flag = false;
                break;
            }
        }
        if(flag) { //移动方向没有障碍物，进行移动
            clearOld();
            xCore += right;
            generateCube();
        }
    }

    void setCubeColor(int i, int j, Color target) { //更改方块颜色的语法糖
        mySquare[i, j].GetComponent<SpriteRenderer>().color = target;
    }


    bool cubeColorSame(int i, int j, Color target) { //给定方块坐标，判断所在方块颜色是否和给定颜色相同
        if(i < 0 || i >= width || j < 0 || j >= hegiht)
            return false;
        if(mySquare[i, j].GetComponent<SpriteRenderer>().color == target) {
            return true;
        }
        else {
            return false;
        }
    }

    void changeCubeShape() { //改变方块的形状
        clearOld();
        int nextShape = shapeMap[cubeKind];
        int curCubeKind = cubeKind;
        cubeKind = nextShape;
        for(int i = 0; i < 3; i++) {
            for(int j = 0; j < 3; j++) {
                if(cubeShape[cubeKind, i, j] == 1) {
                    if(!cubeColorSame(xCore + i, yCore + j, Color.white)) { //如果进行图形变换后的位置已有方块占据，则不能变换
                        cubeKind = curCubeKind;
                        generateCube();
                        return;
                    }
                }
            }
        }
        generateCube();
    }

    // Update is called once per frame
    void Update()
    {
        timeOffset += fallSpeed * Time.deltaTime;
        if(timeOffset - stamp >= 1) { //每过一秒进行一次判定
            stamp++;
            if(reachBottom()) { //方块已到达当前底部，则生成新方块
                generateNewCube();
            }
            else { //方块未到达底部，继续下降
                clearOld(); //删去上一秒方块所在位置
                yCore--; //进行下降
                generateCube(); //重新生成方块
            }
        }
        // 对方块进行键盘操控
        if(Time.timeScale > 0 && Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            moveCube(-1);
        }
        else if(Time.timeScale > 0 && Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            moveCube(1);
        }
        else if(Time.timeScale > 0 && Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            clearOld();
            while(!reachBottom()) {
                yCore--;
            }
            generateCube(); //将当前方块移到底部
            generateNewCube(); //生成新方块
        }
        else if(Time.timeScale > 0 && Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            changeCubeShape();
        }
        else if(Input.GetKeyDown(KeyCode.R)) {
            Time.timeScale = 1;
            score = 0;
            for(int i = 0; i < width; i++) {
                for(int j = 0; j < hegiht; j++) {
                    setCubeColor(i, j, Color.white);
                }
            }
            generateNewCube();
        }
        else if(Input.GetKeyDown(KeyCode.Y)) { //暂停/开始功能
            Time.timeScale = 1 - Time.timeScale;
        }
    }
}
