using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBehaviour : MonoBehaviour
{

    public Transform player;

    private Animator anim;
    private Rigidbody2D rb;

    enum SlimeAction {
        Idle,
        Move,
        Chase,
        Attack,
        Hurt,
        Die
    }

    enum Direction {
        Left,
        Right
    }

    private static readonly string Idle = "Idle";
    private static readonly string Move = "Move";
    private static readonly string Hurt = "Hurt";
    private static readonly string Die  = "Die";

    private SlimeAction currentAction = SlimeAction.Idle;
    private string currentAnimState = Idle;

    private float timeSinceActionStart = 0f;

    private float stateLockedTill = 0f;

    public float speed = 0.5f;
    public float chaseBonus = 2f;
    public float hitBackSpeed = 3f;
    public float hitBackFriction = .95f;

    public float idleTime = 1f;
    public float moveTime = 1f;

    public float startingHealth = 3f;
    private float currentHealth;

    private Vector2 velocity = new Vector2(0f, 0f);
    private Direction dir = Direction.Right;

    private bool seesPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();

        currentHealth = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        currentAction = decideAction();

        string animState = GetAnimState();
        if (animState != currentAnimState) {
            anim.CrossFade(animState, 0, 0);
            currentAnimState = animState;
        }

        transform.localScale = dir == Direction.Left ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
    }

    void FixedUpdate() {
        switch (currentAction) {
            case SlimeAction.Move:
                velocity.x = speed * (dir == Direction.Right ? 1 : -1);
                break;
            case SlimeAction.Idle:
            case SlimeAction.Die:
                velocity.x = 0f;
                break;
            case SlimeAction.Chase:
                velocity.x = speed * chaseBonus * (dir == Direction.Right ? 1 : -1);
                break;
            case SlimeAction.Hurt:
                velocity.x *= hitBackFriction;
                break;
        }

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private string GetAnimState() {
        switch (currentAction) {
            case SlimeAction.Idle:
                return Idle;
            case SlimeAction.Move:
            case SlimeAction.Chase:
                return Move;
            case SlimeAction.Hurt:
                return Hurt;
            case SlimeAction.Die:
                return Die;
        }

        return Idle;
    }

    SlimeAction decideAction() {
        if (Time.time < stateLockedTill) return currentAction;

        if (currentAction == SlimeAction.Die) {
            Destroy(gameObject);
            return SlimeAction.Die;
        }

        if (seesPlayer) {//check if player is in sight
            dir = (player.position.x > transform.position.x) ? Direction.Right : Direction.Left;

            return SlimeAction.Chase;
        } else {
            timeSinceActionStart += Time.deltaTime; //update this to fixedDeltaTime if called from FixedUpdate
            switch (currentAction) {
                case SlimeAction.Idle:
                    if (timeSinceActionStart > idleTime) {
                        timeSinceActionStart = 0f;
                        dir = dir == Direction.Left ? Direction.Right : Direction.Left;
                        return SlimeAction.Move;
                    } else {
                        return SlimeAction.Idle;
                    }
                case SlimeAction.Move:
                    if (timeSinceActionStart > moveTime) {
                        timeSinceActionStart = 0f;
                        return SlimeAction.Idle;
                    } else {
                        return SlimeAction.Move;
                    }
                case SlimeAction.Chase:
                case SlimeAction.Hurt:
                    timeSinceActionStart = 0f;
                    return SlimeAction.Idle;
            }
        }

        return SlimeAction.Idle;
    }

    public void setSeesPlayer(bool sees) {
        seesPlayer = sees;
    }

    public void hitByPlayer(float damage) {
        currentHealth -= damage;

        if (currentHealth <= 0f) {
            currentAction = LockState(SlimeAction.Die, 0.333f);
            velocity.x = 0f;
        } else {
            currentAction = LockState(SlimeAction.Hurt, 0.333f);
            velocity.x = hitBackSpeed * (dir == Direction.Right ? -1f : 1f);
        }
    }

    SlimeAction LockState(SlimeAction state, float lockTime) {
        stateLockedTill = Time.time + lockTime;
        return state;
    }
}
