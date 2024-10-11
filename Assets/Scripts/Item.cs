using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameManager gameManager;

    public enum Type { Default = -1, Hammer, HandGun, Shotgun, WildCard };
    public Type itemType;
    public bool isUsed = false;
    WaitForSeconds waitSecond = new WaitForSeconds(0.4f);
    WaitForSeconds handGunSecond = new WaitForSeconds(0.3f);

    public void OnAbility(int location)
    {
        switch (itemType)
        {
            case Type.Hammer:
                HammerAbility(location);
                break;
            case Type.HandGun:
                HandGunAbility();
                break;
            case Type.Shotgun:
                ShotgunAbility(location);
                break;
            case Type.WildCard:
                WildCardAbility(location);
                break;
        }
    }

    void HammerAbility(int location)
    {
        for (int i = -1; i < 2; ++i)
        {
            int index = location + (6 * i);

            if (index - 1 > -1 && index - 1 < 35 && index % 6 > 0 && gameManager.field[index - 1] != 0)
            {
                gameManager.DestroyCircle(index - 1);
            }
            if (index > -1 && index < 36 && gameManager.field[index] != 0)
            {
                gameManager.DestroyCircle(index);
            }
            if (index + 1 > 0 && index + 1 < 36 && index % 6 < 5 && gameManager.field[index + 1] != 0)
            {
                gameManager.DestroyCircle(index + 1);
            }

            if (index - 1 > -1 && index - 1 < 35 && index % 6 > 0) StartCoroutine(ChangeFieldColor(index - 1));
            if (index > -1 && index < 36) StartCoroutine(ChangeFieldColor(index));
            if (index + 1 > 0 && index + 1 < 36 && index % 6 < 5) StartCoroutine(ChangeFieldColor(index + 1));
        }

        AudioManager.audioManager.PlaySFX(AudioManager.SFX.Hammer);
        GameManager.gameManager.floors[location].HammerParticle();
    }

    void HandGunAbility()
    {
        int[] indexs = new int[5];

        for (int i = 0; i < 5; ++i)
        {
            int index = UnityEngine.Random.Range(0, 36);

            if (Array.IndexOf(indexs, index) != -1)
            {
                --i;
                continue;
            }

            indexs.SetValue(index, i);
        }

        StartCoroutine(HandGunCoroutine(indexs));
    }

    void ShotgunAbility(int location)
    {
        int[] indexs = new int[3];

        for (int i = 0; i < 3; ++i)
        {
            int ranV, ranH;

            if (location < 6) ranV = UnityEngine.Random.Range(0, 2);
            else if (location > 29) ranV = UnityEngine.Random.Range(-1, 1);
            else ranV = UnityEngine.Random.Range(-1, 2);

            if (location % 6 < 1) ranH = UnityEngine.Random.Range(0, 2);
            else if (location % 6 > 4) ranH = UnityEngine.Random.Range(-1, 1);
            else ranH = UnityEngine.Random.Range(-1, 2);

            int index = location + (6 * ranV) + ranH;

            if (Array.IndexOf(indexs, index) != -1)
            {
                --i;
                continue;
            }

            indexs.SetValue(index, i);

            if (index > -1 && index < 36 && gameManager.field[index] != 0)
            {
                gameManager.DestroyCircle(index);
            }

            if (index > -1 && index < 36)
            {
                StartCoroutine(ChangeFieldColor(index));
                GameManager.gameManager.floors[index].ShotParticle();
            }

            AudioManager.audioManager.PlaySFX(AudioManager.SFX.Shotgun);
        }
    }

    void WildCardAbility(int location)
    {
        if (gameManager.field[location] != 0)
        {
            gameManager.ChangeCircle(location);
        }

        AudioManager.audioManager.PlaySFX(AudioManager.SFX.WildCard);
        GameManager.gameManager.floors[location].WildCardParticle();
    }

    IEnumerator HandGunCoroutine(int[] indexs)
    {
        gameManager.isUsingHandGun = true;

        foreach (int index in indexs)
        {   
            if (gameManager.field[index] != 0)
            {
                gameManager.DestroyCircle(index);
            }
            
            StartCoroutine(ChangeFieldColor(index));
            AudioManager.audioManager.PlaySFX(AudioManager.SFX.HandGun);
            GameManager.gameManager.floors[index].ShotParticle();
            yield return handGunSecond;
        }

        gameManager.isUsingHandGun = false;
    }

    IEnumerator ChangeFieldColor(int index)
    {
        yield return new WaitForEndOfFrame();
        MeshRenderer fieldColor = gameManager.floors[index].GetComponent<MeshRenderer>();

        fieldColor.material.color = Color.red;
        yield return waitSecond;
        fieldColor.material.color = Color.white;
    }
}
