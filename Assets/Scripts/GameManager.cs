using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    [Header("#GameInfo")]
    public bool isAIBattle;
    bool isPlaying = false;
    public int turn = 0;
    WaitForSeconds startDelay = new WaitForSeconds(0.1f);

    [Header("#Field")]
    public Floor[] floors;
    public int[] field;
    public int[] player1_Arr;
    public int[] player2_Arr;
    public int[] playersCount;
    public int[,] victoryCases;

    [Header("#Item")]
    public Item[] player1_Items;
    public Item[] player2_Items;
    bool isUsingItem = false;
    bool isUsedItem = false;
    int curItemIndex;
    int curDecalIndex = -1;
    int[] playerItemCount;
    int[] createItemDelay;
    public bool isUsingHandGun = false;


    void Awake()
    {
        gameManager = this;

        field = new int[36];
        player1_Arr = new int[36];
        player2_Arr = new int[36];
        playersCount = new int[2];
        playerItemCount = new int[2];
        createItemDelay = new int[2];
        victoryCases = new int[,] {
            { 0, 1, 2, 3, 4, 5 },
            { 6, 7, 8, 9, 10, 11 },
            { 12, 13, 14, 15, 16, 17 },
            { 18, 19, 20, 21, 22, 23 },
            { 24, 25, 26, 27, 28, 29 },
            { 30, 31, 32, 33, 34, 35},
            { 0, 6, 12, 18, 24, 30 },
            { 1, 7, 13, 19, 25, 31 },
            { 2, 8, 14, 20, 26, 32 },
            { 3, 9, 15, 21, 27, 33 },
            { 4, 10, 16, 22, 28, 34 },
            { 5, 11, 17, 23, 29, 35 },
            { 0, 7, 14, 21, 28, 35 },
            { 5, 10, 15, 20, 25, 30 },
            };
    }

    void Update()
    {
        if (isPlaying)
        {
            ClickFeild();
            //DecalField(); // PC
        }
        else
        {
            if (Input.GetButtonDown("Cancel")) Application.Quit();
        }
    }

    public void InitGame()
    {
        turn = 0;

        for (int i = 0; i < 36; ++i)
        {
            field[i] = 0;
            player1_Arr[i] = 0;
            player2_Arr[i] = 0;
        }

        playersCount[0] = 0;
        playersCount[1] = 0;

        CreateItems(0);
        CreateItems(1);
        isUsedItem = false;

        StartCoroutine(StartDelay());
    }

    public void ClearField()
    {
        foreach (Floor floor in floors)
        {
            floor.UnSetCircle();
        }
    }

    public void CreateCircle(int index)
    {
        int curPlayer = turn % 2;

        if (field[index] == 0)
        {
            floors[index].SetCircle(curPlayer);
            field[index] = curPlayer + 1;

            if (curPlayer == 0) player1_Arr.SetValue(1, index);
            else player2_Arr.SetValue(1, index);

            ++playersCount[curPlayer];
            
            AudioManager.audioManager.PlaySFX((AudioManager.SFX)curPlayer);

            if (playersCount[curPlayer] > 5)
            {
                CheckVictory(curPlayer);
            }

            if (isPlaying) ChangeTurn(curPlayer);
        }
    }

    void AIThink()
    {
        BattleAI.battleAI.Think();
        CancelInvoke("AIThink");
    }

    void ClickFeild()
    {
        if (isUsingHandGun) return;

        if (isAIBattle && turn % 2 == 1)
        {
            Invoke("AIThink", 0.5f);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                int curPlayer = turn % 2;
                int index = -1;

                if (!isUsingItem)
                {
                    if (hit.transform.tag == "Floor")
                    {
                        Floor floor = hit.transform.GetComponent<Floor>();
                        index = floor.floorIndex;
                        CreateCircle(index);
                    }
                }
                else // Mobile
                {
                    if (hit.transform.tag == "Floor")
                    {
                        Floor floor = hit.transform.GetComponent<Floor>();
                        index = floor.floorIndex;
                    }
                    else if (hit.transform.tag == "Circle")
                    {
                        Floor floor = hit.transform.GetComponentInParent<Floor>();
                        index = floor.floorIndex;
                    }
                    else return;

                    if (curDecalIndex != index && curDecalIndex != -1) ClearFieldDecal();
                    curDecalIndex = index;

                    if (turn % 2 == 0)
                    {
                        switch (player1_Items[curItemIndex].itemType)
                        {
                            case Item.Type.Hammer:
                                HammerDecal(index);
                                break;
                            case Item.Type.HandGun:
                                HandGunDecal();
                                break;
                            case Item.Type.Shotgun:
                                ShotgunDecal(index);
                                break;
                            case Item.Type.WildCard:
                                WildCardDecal(index);
                                break;
                        }
                    }
                    else
                    {
                        switch (player2_Items[curItemIndex].itemType)
                        {
                            case Item.Type.Hammer:
                                HammerDecal(index);
                                break;
                            case Item.Type.HandGun:
                                HandGunDecal();
                                break;
                            case Item.Type.Shotgun:
                                ShotgunDecal(index);
                                break;
                            case Item.Type.WildCard:
                                WildCardDecal(index);
                                break;
                        }
                    }

                    UIManager.uiManager.confirm_Button.gameObject.SetActive(true);
                }
            }
            //else // PC
            //{
            //if (hit.transform.tag == "Floor" || hit.transform.tag == "Circle")
            //{
            //    if (hit.transform.tag == "Floor")
            //    {
            //        Floor floor = hit.transform.GetComponent<Floor>();
            //        index = floor.floorIndex;
            //    }
            //    else if (hit.transform.tag == "Circle")
            //    {
            //        Floor floor = hit.transform.GetComponentInParent<Floor>();
            //        index = floor.floorIndex;
            //    }

            //    if (curPlayer == 0) player1_Items[curItemIndex].OnAbility(index);
            //    else if (curPlayer == 1) player2_Items[curItemIndex].OnAbility(index);

            //    UseItem(curItemIndex);
            //    UsedItem(turn % 2, curItemIndex);
            //    if (isPlaying) ChangeTurn(turn % 2);
            //    }
            //}
        }
    }

    public void ChangeTurn(int curPlayer)
    {
        UIManager.uiManager.ChangeUI(curPlayer);

        if (curPlayer == 0 && playerItemCount[0] == 0)
        {
            if (createItemDelay[0] == 0) CreateItems(0);
            else --createItemDelay[0];
        }
        else if (curPlayer == 1 && playerItemCount[1] == 0)
        {
            if (createItemDelay[1] == 0) CreateItems(1);
            else --createItemDelay[1];
        }

        if (playersCount[0] + playersCount[1] >= 36)
        {
            UIManager.uiManager.GameEnd(2);
            AudioManager.audioManager.PlaySFX(AudioManager.SFX.Win);
            AudioManager.audioManager.PlayBGM(false);
            Debug.Log("무승부");
            isPlaying = false;
        }

        isUsedItem = false;
        ++turn;
    }

    void CheckVictory(int curPlayer)
    {
        bool victory = false;
        int[] curArr = curPlayer == 0 ? player1_Arr : player2_Arr;

        for (int i = 0; i < 14; ++i)
        {
            for (int j = 0; j < 6; ++j)
            {
                if (curArr[victoryCases[i, j]] == 1) victory = true;
                else
                {
                    victory = false;
                    break;
                }
            }
            if (victory) break;
        }

        if (victory)
        {
            UIManager.uiManager.GameEnd(curPlayer);
            AudioManager.audioManager.PlaySFX(AudioManager.SFX.Win);
            AudioManager.audioManager.PlayBGM(false);
            Debug.Log("플레이어" + curPlayer + " 승!");
            isPlaying = false;
        }
    }

    public void CreateItems(int player)
    {
        Item[] items;

        if (player == 0)
        {
            items = player1_Items;
            playerItemCount[0] = 3;
            createItemDelay[0] = 2;
        }
        else
        {
            items = player2_Items;
            playerItemCount[1] = 3;   
            createItemDelay[1] = 2;
        }

        for (int i = 0; i < 3; ++i)
        {
            int ranType = UnityEngine.Random.Range(0, 10);

            switch (ranType)
            {
                case 0:
                case 1:
                    ranType = 0; // Hammer 20%
                    break;
                case 2:
                case 3:
                case 4:
                    ranType = 1; // HandGun 30%
                    break;
                case 5:
                case 6:
                case 7:
                    ranType = 2; // ShotGun 30%
                    break;
                case 8:
                case 9:
                    ranType = 3; // WildCard 20%
                    break;
            }

            items[i].itemType = (Item.Type)ranType;
            items[i].isUsed = false;
        }
    }

    public void UseItem(int index)
    {
        if (isUsedItem || (turn % 2 == 1 && isAIBattle)) return;
        if (turn % 2 == 0 && player1_Items[index].isUsed) return;
        else if (turn % 2 == 1 && player2_Items[index].isUsed) return;

        if (isUsingItem && curItemIndex != index)
        {
            isUsingItem = !isUsingItem;
            UIManager.uiManager.UsingItem(curItemIndex, isUsingItem);
            UIManager.uiManager.confirm_Button.gameObject.SetActive(false);
        }

        isUsingItem = !isUsingItem;
        curItemIndex = index;

        UIManager.uiManager.UsingItem(index, isUsingItem);
        AudioManager.audioManager.PlaySFX(AudioManager.SFX.Item);    
        if (!isUsingItem) UIManager.uiManager.confirm_Button.gameObject.SetActive(isUsingItem);

        ClearFieldDecal();
    }

    public void UsedItem(int curPlayer,int index)
    {
        isUsedItem = true;

        if (turn % 2 == 0)
        {
            player1_Items[index].isUsed = true;
            --playerItemCount[0];
        }
        else
        {
            player2_Items[index].isUsed = true;
            --playerItemCount[1];
        }

        UIManager.uiManager.UsedItem(index);
    }

    public void ConfirmUseItem()
    {
        if (turn % 2 == 0) player1_Items[curItemIndex].OnAbility(curDecalIndex);
        else if (turn % 2 == 1) player2_Items[curItemIndex].OnAbility(curDecalIndex);

        UseItem(curItemIndex);
        UsedItem(turn % 2, curItemIndex);
        if (isPlaying) ChangeTurn(turn % 2);

        UIManager.uiManager.confirm_Button.gameObject.SetActive(false);
    }

    public void DestroyCircle(int index)
    {
        Floor floor = floors[index];
        int curPlayer = floor.UnSetCircle();

        if (curPlayer == 0) player1_Arr[index] = 0;
        else player2_Arr[index] = 0;

        field[index] = 0;
        --playersCount[curPlayer];
    }

    public void ChangeCircle(int index)
    {
        Floor floor = floors[index];
        int curPlayer = turn % 2;

        floor.SetCircle(curPlayer);
        field[index] = field[index] == 1 ? 2 : 1;

        if (curPlayer == 0)
        {
            player1_Arr[index] = 1;
            player2_Arr[index] = 0;
        }
        else
        {
            player1_Arr[index] = 0;
            player2_Arr[index] = 1;
        }

        CheckVictory(curPlayer);
    }

    void DecalField()
    {
        if (!isUsingItem) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            int index = -1;
            
            if (hit.transform.tag == "Floor" || hit.transform.tag == "Circle")
            {
                if (hit.transform.tag == "Floor")
                {
                    Floor floor = hit.transform.GetComponent<Floor>();
                    index = floor.floorIndex;
                }
                else if (hit.transform.tag == "Circle")
                {
                    Floor floor = hit.transform.GetComponentInParent<Floor>();
                    index = floor.floorIndex;
                }

                if (curDecalIndex != index && curDecalIndex != -1) ClearFieldDecal();
                curDecalIndex = index;

                if (turn % 2 == 0)
                {
                    switch (player1_Items[curItemIndex].itemType)
                    {
                        case Item.Type.Hammer:
                            HammerDecal(index);
                            break;
                        case Item.Type.HandGun:
                            HandGunDecal();
                            break;
                        case Item.Type.Shotgun:
                            ShotgunDecal(index);
                            break;
                        case Item.Type.WildCard:
                            WildCardDecal(index);
                            break;
                    }
                }
                else
                {
                    switch (player2_Items[curItemIndex].itemType)
                    {
                        case Item.Type.Hammer:
                            HammerDecal(index);
                            break;
                        case Item.Type.HandGun:
                            HandGunDecal();
                            break;
                        case Item.Type.Shotgun:
                            ShotgunDecal(index);
                            break;
                        case Item.Type.WildCard:
                            WildCardDecal(index);
                            break;
                    }
                }
            }
        }
    }

    void HammerDecal(int location)
    {
        for (int i = -1; i < 2; ++i)
        {
            int index = location + (6 * i);
            MeshRenderer fieldColor;

            if (index - 1 > -1 && index - 1 < 35 && index % 6 > 0)
            {
                fieldColor = floors[index - 1].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
            if (index > -1 && index < 36)
            {
                fieldColor = floors[index].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
            if (index + 1 > 0 && index + 1 < 36 && index % 6 < 5)
            {
                fieldColor = floors[index + 1].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
        }
    }

    void HandGunDecal()
    {
        for (int i = 0; i < 36; ++i)
        {
            MeshRenderer fieldColor;
            fieldColor = floors[i].GetComponent<MeshRenderer>();
            fieldColor.material.color = Color.blue;
        }
    }

    void ShotgunDecal(int location)
    {
        for (int i = -1; i < 2; ++i)
        {
            int index = location + (6 * i);
            MeshRenderer fieldColor;

            if (index - 1 > -1 && index - 1 < 35 && index % 6 > 0)
            {
                fieldColor = floors[index - 1].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
            if (index > -1 && index < 36)
            {
                fieldColor = floors[index].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
            if (index + 1 > 0 && index + 1 < 36 && index % 6 < 5)
            {
                fieldColor = floors[index + 1].GetComponent<MeshRenderer>();
                fieldColor.material.color = Color.blue;
            }
        }
    }

    void WildCardDecal(int location)
    {
        MeshRenderer fieldColor = floors[location].GetComponent<MeshRenderer>(); ;
        fieldColor.material.color = Color.blue;
    }

    void ClearFieldDecal()
    {
        foreach (Floor floor in floors)
        {
            MeshRenderer fieldColor = floor.GetComponent<MeshRenderer>();
            fieldColor.material.color = Color.white;
        }
    }

    IEnumerator StartDelay()
    {
        yield return startDelay;
        isPlaying = true;
    }
}
