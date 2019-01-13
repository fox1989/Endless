using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hero : MonoBehaviour
{


    /// <summary>
    /// 选用状态，以后改为状态机
    /// </summary>
    public enum HeroState
    {
        None,
        Aim,
        LookAt,
    }

    Collider2D c2d;
    Rigidbody2D rb2d;

    public GameObject gun;
    public GameObject gunPoint;

    public Material lineMaterial;


    public LineRenderer aimLine;
    public GameObject heroTex;

    public GameObject hookGo;

    public HeroState State;


    public GameObject lookAtPoint;


    public float speed = 2;
    Vector3 mPos;


    LineRenderer hookLine;
    // Use this for initialization
    void Start()
    {
        c2d = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        hookLine = hookGo.GetComponent<LineRenderer>();

    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetMouseButtonDown(1))
        {
            startAim = true;
            aimLine.gameObject.SetActive(true);
            State = HeroState.Aim;
        }


        Vector3 hitPos;
        int dirFlag;
        GetHitDirPos(out hitPos, out dirFlag);

        if (hitPos == Vector3.one * 9999)
        {
            StarLookAt();
        }

        switch (State)
        {
            case HeroState.None:
                None();
                break;
            case HeroState.Aim:
                OnAim();
                break;
            case HeroState.LookAt:
                LookAt();
                break;
            default:
                break;
        }



        //if (!isFire)
        //    Move();
        //UpdateMove();
        //RotateGun();
        //Fire();
    }



    #region 描准

    bool startAim;
    bool isFireHook;

    public float aimLineDis = 100;

    /// <summary>
    /// 描准
    /// </summary>
    void OnAim()
    {
        if (Input.GetMouseButton(1) && startAim)
        {
            Vector3 mouseDir = GetMouserDir();

            RaycastHit2D hit = Physics2D.Raycast(transform.position, mouseDir, aimLineDis, ~(1 << LayerMask.NameToLayer("Player")));

            Vector3 aimLinePos2;
            if (hit.collider != null)
            {
                aimLinePos2 = hit.point;
            }
            else
            {
                aimLinePos2 = mouseDir * aimLineDis;
            }
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, aimLinePos2);
        }





        if (Input.GetMouseButtonUp(1))
        {
            startAim = false;
            aimLine.gameObject.SetActive(false);
            isFireHook = false;
            FireHook();
        }



        if (Input.GetMouseButtonUp(0))
        {
            startAim = false;
            isFireHook = false;
            aimLine.gameObject.SetActive(false);
            hookGo.SetActive(false);
            State = HeroState.None;
        }




    }

    private Vector3 GetMouserDir()
    {

        Vector3 tempPos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = tempPos.z;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 mouseDir = mousePos - transform.position;
        return mouseDir;
    }

    Coroutine corHaulBack = null;

    void FireHook()
    {
        Vector3 mouseDir = GetMouserDir();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, mouseDir, aimLineDis, ~(1 << LayerMask.NameToLayer("Player")));

        Vector3 hitPos = mouseDir * aimLineDis;
        Vector3 hitLocalPos = Vector3.zero;
        GameObject hitGo;


        bool isHit = false; ;
        if (hit.collider != null)
        {
            hitGo = hit.collider.gameObject;
            hitPos = hit.point;
            hitLocalPos = hit.transform.InverseTransformPoint(hit.point);
            isHit = true;

        }

        hookGo.transform.localPosition = Vector3.zero;

        hookGo.SetActive(true);
        hookGo.transform.DOMove(hitPos, 0.5f).OnUpdate(UpdateHookLine).OnComplete(() =>
        {
            isFireHook = true;

            if (isHit)
            {
                StartCoroutine(HaulBack(hit.collider.gameObject, hitLocalPos));
            }
            else
            {
                hookGo.transform.DOMove(transform.position, 0.2f).OnUpdate(UpdateHookLine).OnComplete(() =>
                {
                    isFireHook = false;
                    aimLine.gameObject.SetActive(false);
                });

            }
        });
    }

    /// <summary>
    /// 更新绳子
    /// </summary>
    void UpdateHookLine()
    {
        if (hookLine != null)
        {

            hookLine.SetPosition(0, transform.position);
            hookLine.SetPosition(1, hookGo.transform.position);
        }
    }



    IEnumerator HaulBack(GameObject rock, Vector3 rockLocalPos)
    {
        TargetJoint2D heroTj2d = gameObject.AddComponent<TargetJoint2D>();
        TargetJoint2D rockTj2d = rock.AddComponent<TargetJoint2D>();

        heroTj2d.anchor = Vector2.zero;
        heroTj2d.frequency = 1;
        heroTj2d.maxForce = 3;

        rockTj2d.anchor = rockLocalPos;
        rockTj2d.frequency = 1;
        rockTj2d.maxForce = 3;

        while (true)
        {
            Vector3 pos = rock.transform.TransformPoint(rockLocalPos);

            hookGo.transform.position = pos;
            Vector3 v3 = (gameObject.transform.position + pos) * 0.5f;
            UpdateHookLine();

            heroTj2d.target = v3;
            rockTj2d.target = v3;

            float dis = Vector3.Distance(gameObject.transform.position, pos);

            if (dis < 4f || !isFireHook)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        Destroy(heroTj2d);
        Destroy(rockTj2d);
        hookGo.SetActive(false);
        isFireHook = false;
    }


    #endregion





    #region 看

    public float lookAtSpeed = 10;

    bool isLookAt;

    void StarLookAt()
    {

        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.S))
        {
            State = HeroState.LookAt;
        }

    }
    Tweener lookAtMove = null;

    //向什么方向看
    void LookAt()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x -= lookAtSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += lookAtSpeed * Time.deltaTime;

        }
        else if (Input.GetKey(KeyCode.W))
        {
            moveDir.y += lookAtSpeed * Time.deltaTime;

        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDir.y -= lookAtSpeed * Time.deltaTime;
        }



        if (lookAtPoint.transform.localPosition.magnitude < 5)
        {

            if (lookAtMove != null)
            {
                lookAtMove.Kill(true);
                lookAtMove = null;
            }

            lookAtPoint.transform.position += moveDir;
        }

        if (moveDir == Vector3.zero)
        {
            State = HeroState.None;
            lookAtMove = lookAtPoint.transform.DOLocalMove(Vector3.zero, lookAtPoint.transform.localPosition.magnitude / 50);
        }
    }
    #endregion



    #region None
    void None()
    {
        Vector3 hitPos;
        int dirFlag;
        GetHitDirPos(out hitPos, out dirFlag);

        if (hitPos != Vector3.one * 9999)
        {
            Vector3 mPos = hitPos - transform.position;


            mPos = mPos.normalized * 10;
            float tempSpeed = speed * Time.deltaTime;

            if (dirFlag >= 0 && dirFlag <= 1)//上下
            {

                if (Input.GetKey(KeyCode.D))
                {
                    mPos.x += tempSpeed;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    mPos.x -= tempSpeed;
                }

            }
            else if (dirFlag > 1 && dirFlag <= 3)//左右
            {

                if (Input.GetKey(KeyCode.W))
                {
                    mPos.y += tempSpeed;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    mPos.y -= tempSpeed;
                }
            }

            rb2d.AddForce(mPos);

        }
    }

    private void GetHitDirPos(out Vector3 hitPos, out int dirFlag)
    {
        Vector3[] dirs = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

        hitPos = Vector3.one * 9999;
        dirFlag = -1;
        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = dirs[i];

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 4.5f, 1 << LayerMask.NameToLayer("Wall"));

            if (hit.collider != null)
            {
                float tempDis = Vector3.Distance(Vector3.zero, hit.point);
                float oldDis = Vector3.Distance(Vector3.zero, hitPos);
                if (tempDis < oldDis)
                {
                    hitPos = hit.point;
                    dirFlag = i;
                }
            }
        }
    }


    #endregion

    private void Move()
    {
        mPos = Vector3.zero;

        float tempSpeed = speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
        {
            mPos.x += tempSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            mPos.x -= tempSpeed;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            mPos.y += tempSpeed * 30;
        }



        if (Input.GetKey(KeyCode.W))
        {
            mPos.y += tempSpeed;
        }


        if (Input.GetKey(KeyCode.S))
        {
            mPos.y -= tempSpeed;
        }


    }

    void UpdateMove()
    {
        //rb2d.velocity = mPos;

        rb2d.AddForce(mPos);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.name == "wl")
        {

            rb2d.gravityScale = 0;
            rb2d.drag = 2;
        }

    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "wl")
        {

            rb2d.gravityScale = 1;
            rb2d.drag = 1;

        }
    }




    public void RotateGun()
    {


        //if (Input.GetKey(KeyCode.Q))
        //{
        //    gun.transform.Rotate(Vector3.forward * Time.deltaTime * 60);
        //}
        //if (Input.GetKey(KeyCode.E))
        //{
        //    gun.transform.Rotate(Vector3.back * Time.deltaTime * 60);

        //}




        if (Input.GetMouseButton(0))
        {


            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;

            Vector3 mouseDir = mousePos - transform.position;










        }




    }


    bool isFire;








}
