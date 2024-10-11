using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAI : MonoBehaviour
{
    public static BattleAI battleAI;
    public GameManager gameManager;

    [Header("#Field")]
    int index;
    int maxCount_Player1;
    int maxCount_Player2;
    int maxCase_Player1;
    int maxCase_Player2;
    int enemy_Count;

    [Header("#Item")]
    int item_Index;
    int hammer_Index;
    int handGun_Index;
    int shotGun_Index;
    int wildCard_Index;


    void Awake()
    {
        battleAI = this;
    }

    public void Think()
    {
        index = -1;
        item_Index = -1;

        CountingPlayer1();
        CountingPlayer2();
        CheckItems();
        FindItemIndex();

        if (wildCard_Index != -1)
        {
            UseWildCard();

            if (item_Index != -1)
            {
                UseItem(wildCard_Index);
                return;
            }
        }
        else if (gameManager.playersCount[0] >= 3 && (gameManager.playersCount[0] > gameManager.playersCount[1] || enemy_Count > 4))
        {
            FindItemIndex();

            if (hammer_Index != -1 && item_Index != -1)
            {
                UseItem(hammer_Index);
                return;
            }
            else if (shotGun_Index != -1 && item_Index != -1)
            {
                UseItem(shotGun_Index);
                return;
            }
            else if (handGun_Index != -1)
            {
                UseItem(handGun_Index);
                return;
            }
        }

        if (maxCount_Player1 >= 4 && maxCount_Player1 > maxCount_Player2)
        {
            BlockingCircle();
        }
        else if (maxCount_Player2 != 0)
        {
            ConnectionCircle();
        }
        else
        {
            RandomIndex();
        }

        gameManager.CreateCircle(index);
    }

    void CountingPlayer1()
    {
        maxCount_Player1 = 0;
        maxCase_Player1 = -1;

        for (int i = 0; i < 14; ++i)
        {
            int count_Player1 = 0;

            for (int j = 0; j < 6; ++j)
            {
                if (gameManager.field[gameManager.victoryCases[i, j]] == 2)
                {
                    count_Player1 = 0;
                    break;
                }
                else if (gameManager.field[gameManager.victoryCases[i, j]] == 1)
                {
                    ++count_Player1;
                }
            }

            if (maxCount_Player1 < count_Player1)
            {
                maxCount_Player1 = count_Player1;
                maxCase_Player1 = i;
            }
        }

        if (maxCase_Player1 == -1) return;
    }

    void CountingPlayer2()
    {
        maxCount_Player2 = 0;
        maxCase_Player2 = -1;

        for (int i = 0; i < 14; ++i)
        {
            int count_Player2 = 0;

            for (int j = 0; j < 6; ++j)
            {
                if (gameManager.field[gameManager.victoryCases[i, j]] == 1)
                {
                    count_Player2 = 0;
                    break;
                }
                else if (gameManager.field[gameManager.victoryCases[i, j]] == 2)
                {
                    ++count_Player2;
                }
            }

            if (maxCount_Player2 < count_Player2)
            {
                maxCount_Player2 = count_Player2;
                maxCase_Player2 = i;
            }
        }

        if (maxCase_Player2 == -1) return;
    }

    void CheckItems()
    {
        hammer_Index = -1;
        handGun_Index = -1;
        shotGun_Index = -1;
        wildCard_Index = -1;

        for (int i = 0; i < gameManager.player2_Items.Length; ++i)
        {
            if (gameManager.player2_Items[i].isUsed) continue;

            switch (gameManager.player2_Items[i].itemType)
            {
                case Item.Type.Hammer:
                    hammer_Index = i;
                    break;
                case Item.Type.HandGun:
                    handGun_Index = i;
                    break;
                case Item.Type.Shotgun:
                    shotGun_Index = i;
                    break;
                case Item.Type.WildCard:
                    wildCard_Index = i;
                    break;
            }
        }
    }

    void FindItemIndex()
    {
        enemy_Count = 0;

        for (int i = 0; i < 36; ++i)
        {
            int player1_Count = 0;
            int player2_Count = 0;

            for (int j = -1; j < 2; ++j)
            {
                int index = i + (6 * j);

                if (index - 1 > -1 && index - 1 < 35 && index % 6 > 0)
                {
                    if (gameManager.field[index - 1] == 1)
                        ++player1_Count;
                    else if (gameManager.field[index - 1] == 2)
                        ++player2_Count;
                }
                if (index > -1 && index < 36)
                {
                    if (gameManager.field[index] == 1)
                        ++player1_Count;
                    else if (gameManager.field[index] == 2)
                        ++player2_Count;
                }
                if (index + 1 > 0 && index + 1 < 36 && index % 6 < 5)
                {
                    if (gameManager.field[index + 1] == 1)
                        ++player1_Count;
                    else if (gameManager.field[index + 1] == 2)
                        ++player2_Count;
                }
            }

            if (player1_Count >= 3)
            {
                if (enemy_Count < player1_Count - player2_Count)
                {
                    enemy_Count = player1_Count;
                    item_Index = i;
                }
            }
        }
    }

    void RandomIndex()
    {
        while (true)
        {
            index = Random.Range(0, 36);
            if (gameManager.field[index] == 0) break;
        }
    }

    void ConnectionCircle()
    {
        for (int i = 0; i < 6; ++i)
        {
            if (gameManager.field[gameManager.victoryCases[maxCase_Player2, i]] == 2) continue;
            index = gameManager.victoryCases[maxCase_Player2, i];
            break;
        }
    }

    void BlockingCircle()
    {
        for (int i = 0; i < 6; ++i)
        {
            if (gameManager.field[gameManager.victoryCases[maxCase_Player1, i]] == 1) continue;
            index = gameManager.victoryCases[maxCase_Player1, i];
            break;
        }
    }

    void UseItem(int type)
    {
        gameManager.player2_Items[type].OnAbility(item_Index);

        gameManager.UseItem(type);
        gameManager.UsedItem(1, type);
        gameManager.ChangeTurn(1);
    }

    void UseWildCard()
    {
        item_Index = -1;
        int count_Player1;
        int count_Player2;
        int temp_Index = -1;

        for (int i = 0; i < 14; ++i)
        {
            count_Player2 = 0;

            for (int j = 0; j < 6; ++j)
            {
                if (gameManager.field[gameManager.victoryCases[i, j]] == 2)
                {
                    ++count_Player2;
                }
                else if (gameManager.field[gameManager.victoryCases[i, j]] == 1)
                {
                    temp_Index = gameManager.victoryCases[i, j];
                }
            }

            if (count_Player2 == 5)
            {
                item_Index = temp_Index;
                return;
            }
        }

        for (int i = 0; i < 14; ++i)
        {
            count_Player1 = 0;

            for (int j = 0; j < 6; ++j)
            {
                if (gameManager.field[gameManager.victoryCases[i, j]] == 1)
                {
                    ++count_Player1;
                    temp_Index = gameManager.victoryCases[i, j];
                }
            }
            
            if (count_Player1 == 5)
            {
                item_Index = temp_Index;
                return;
            }
        }
    }
}
