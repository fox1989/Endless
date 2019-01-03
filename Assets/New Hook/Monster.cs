using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{

    Rigidbody2D r2d;

    public enum MonsterState
    {
        Walk,
        Hooked,

    }

    public MonsterState state;
    // Start is called before the first frame update
    void Start()
    {
        r2d = GetComponent<Rigidbody2D>();
    }


    bool left = false;

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case MonsterState.Walk:
                Move();
                break;
            case MonsterState.Hooked:
                break;
            default:
                break;
        }

    }

    float moveDis = 0;

    float speed = 0.2f;

    private void Move()
    {
        Vector3 v3 = transform.position; ;
        if (left)
        {
            v3 += Vector3.left * Time.deltaTime * speed;
            moveDis += Time.deltaTime;
        }
        else
        {
            v3 += Vector3.right * Time.deltaTime * speed;
            moveDis += Time.deltaTime;
        }

        if (moveDis > 4)
        {
            moveDis = 0;
            left = !left;
            Vector3 scale = transform.localScale;
            scale.x = scale.y * (left ? -1 : 1);
            transform.localScale = scale;
        }

        transform.position = v3;
    }

}
