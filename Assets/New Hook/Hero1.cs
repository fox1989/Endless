﻿using System.Collections;
using UnityEngine;
using DG.Tweening;
public class Hero1 : MonoBehaviour
{
    Collider2D c2d;
    Rigidbody2D rb2d;


    public GameObject gun;
    public GameObject gunPoint;

    public Material lineMaterial;


    // Use this for initialization
    void Start()
    {
        c2d = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {

        if (!isFire)
            Move();
        UpdateMove();
        RotateGun();
        Fire();
    }


    public float speed = 2;
    Vector3 mPos;
    private void Move()
    {
        mPos = Vector3.zero;

        float tempSpeed = speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            mPos.x += tempSpeed;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            mPos.x -= tempSpeed;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            mPos.y += tempSpeed * 30;
        }



        if (Input.GetKey(KeyCode.UpArrow))
        {
            mPos.y += tempSpeed;
        }


        if (Input.GetKey(KeyCode.DownArrow))
        {
            mPos.y -= tempSpeed;
        }


    }

    void UpdateMove()
    {
        //rb2d.velocity = mPos;

        rb2d.AddForce(mPos);
    }


    public void RotateGun()
    {


        if (Input.GetKey(KeyCode.Q))
        {
            gun.transform.Rotate(Vector3.forward * Time.deltaTime * 60);
        }
        if (Input.GetKey(KeyCode.E))
        {
            gun.transform.Rotate(Vector3.back * Time.deltaTime * 60);

        }


    }


    bool isFire;


    void Fire()
    {

        if (Input.GetKey(KeyCode.F) && !isFire)
        {
            Debug.LogError("ddddd");
            Vector3 dir = gunPoint.transform.position - gun.transform.position;
            Ray2D ray2D = new Ray2D(gun.transform.position, dir * 999);


            RaycastHit2D hit = Physics2D.Raycast(gun.transform.position, dir, float.MaxValue, 1 << LayerMask.NameToLayer("Enemy"));

            if (hit.collider != null)
            {
                Vector2 pos = hit.transform.InverseTransformPoint(hit.point);
                isFire = true;

                Monster monster = hit.collider.gameObject.GetComponent<Monster>();
                monster.state = Monster.MonsterState.Hooked;

                dddd(gameObject, hit.collider.gameObject);

                //StartCoroutine(HaulBack(gameObject, hit.collider.gameObject, pos));
            }
        }
    }



    void dddd(GameObject hero, GameObject monster)
    {

        GameObject lrgo = new GameObject("lr");

        LineRenderer lr = lrgo.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = lineMaterial;

        lr.SetPosition(0, hero.transform.position);
        lr.SetPosition(1, monster.transform.position);

        Vector3 pos = (hero.transform.position + monster.transform.position) * 0.5f;

        float dis = Vector2.Distance(hero.transform.position, monster.transform.position) / 5f;

        monster.transform.DOMove(pos, dis);

        transform.DOMove(pos, dis).OnUpdate(() =>
       {

          float heroOffset = gun.transform.position.y- hero.transform.position.y;
           Vector3 monsterPos = monster.transform.position;
           monsterPos.y += heroOffset;


           lr.SetPosition(0, gun.transform.position);
           lr.SetPosition(1, monsterPos);

       }).OnComplete(() =>
         {
             Monster m = monster.GetComponent<Monster>();
             m.state = Monster.MonsterState.Walk;
             Destroy(lrgo);
             isFire = false;
         });

    }



    IEnumerator HaulBack(GameObject hero, GameObject rock, Vector2 rockLocalPos)
    {

        TargetJoint2D heroTj2d = hero.AddComponent<TargetJoint2D>();

        TargetJoint2D rockTj2d = rock.AddComponent<TargetJoint2D>();

        heroTj2d.anchor = Vector2.zero;
        heroTj2d.frequency = 1;
        heroTj2d.maxForce = 3;


        rockTj2d.anchor = rockLocalPos;
        rockTj2d.frequency = 1;
        rockTj2d.maxForce = 3;

        GameObject lrgo = new GameObject("lr");

        LineRenderer lr = lrgo.AddComponent<LineRenderer>();
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;

        lr.material = lineMaterial;


        while (true)
        {



            Vector3 pos = rock.transform.TransformPoint(rockLocalPos);

            Vector3 v3 = (hero.transform.position + pos) * 0.5f;

            lr.SetPosition(0, hero.transform.position);
            lr.SetPosition(1, pos);


            heroTj2d.target = v3;
            rockTj2d.target = v3;

            float dis = Vector3.Distance(hero.transform.position, pos);

            if (dis < 4f)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(heroTj2d);
        Destroy(rockTj2d);
        Destroy(lrgo);
        isFire = false;
    }
}
