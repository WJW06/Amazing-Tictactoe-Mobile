using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [Header("#FloorInfo")]
    public Circle circle;
    public int floorIndex;
    bool isSet = false;

    [Header("#Particle")]
    public GameObject hammerParticle;
    public GameObject shotParticle;
    public GameObject wildCardParticle;
    WaitForSeconds hammerDelay = new WaitForSeconds(1);
    WaitForSeconds shotDelay = new WaitForSeconds(0.5f);
    WaitForSeconds wildCardDelay = new WaitForSeconds(0.6f);


    public void SetCircle(int curPlayer)
    {
        circle.gameObject.SetActive(true);
        circle.SetCircleType((Circle.CircleType)curPlayer);
        isSet = true;
    }

    public int UnSetCircle()
    {
        circle.gameObject.SetActive(false);
        isSet = false;
        return (int)circle.circleType;
    }

    public void HammerParticle()
    {
        StartCoroutine(HammerCoroutine());
    }

    public void ShotParticle()
    {
        StartCoroutine(ShotCoroutine());
    }

    public void WildCardParticle()
    {
        StartCoroutine(WildCardCoroutine());
    }

    IEnumerator HammerCoroutine()
    {
        hammerParticle.gameObject.SetActive(true);
        yield return hammerDelay;
        hammerParticle.gameObject.SetActive(false);
    }

    IEnumerator ShotCoroutine()
    {
        shotParticle.gameObject.SetActive(true);
        yield return shotDelay;
        shotParticle.gameObject.SetActive(false);
    }

    IEnumerator WildCardCoroutine()
    {
        wildCardParticle.gameObject.SetActive(true);
        yield return wildCardDelay;
        wildCardParticle.gameObject.SetActive(false);
    }
}
