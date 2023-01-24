using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    public Animator anim;
    public Transform frontWallCheck;
    public Transform backWallCheck;
    BoxCollider2D bc;

    private enum WallSlideStates {
        Left,
        Right,
        None
    }

    private enum Direction {
        Left, 
        Right
    }

    public float speed = 1f;
    public float jumpStrength = 1f;
    public float gravityStrength = .1f;
    public float slidingDamp = .95f;
    public float slideSpeedFinish = .5f;
    public float dashBoost = 1.25f;
    public float wallSlideSpeed = -0.5f;
    public float wallJumpBoost = 1f;
    public float airMovement = 0.1f;

    private Rigidbody2D rb;
    private Vector2 velocity = new Vector2(0f, 0f);

    private bool wasFacingRight = true;
    private bool sliding = false;
    private bool wallSliding = false;
    private bool firstAttackTrigger = false;
    private bool secondAttackTrigger = false;
    private bool dashTrigger = false;
    private bool dashAttackTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Dash Logic
        if (Input.GetKeyDown("k")) {
            if (isGrounded() && !isAttacking()) dashTrigger = true; 
        }

        //Attack Logic
        if (Input.GetKeyDown("j")) {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash")) dashAttackTrigger = true;
            else if (canFirstAttack()) firstAttackTrigger = true;
            else if (anim.GetCurrentAnimatorStateInfo(0).IsName("First Attack")) secondAttackTrigger = true;
        }

        anim.SetBool("Dash", dashTrigger);
        anim.SetBool("Dash Attack", dashAttackTrigger);

        anim.SetBool("First Attack", firstAttackTrigger);
        anim.SetBool("Second Attack", secondAttackTrigger);

        if (Input.GetKeyDown("s") && velocity.x != 0 && isGrounded() && !isAttacking() && !isDashing()) sliding = true;

        //Other animation settings
        //Debug.Log(Time.time + " " + isFacingRight());
        setDirection(isFacingRight() ? Direction.Right : Direction.Left);
        anim.SetBool("Running", isRunning() && !firstAttackTrigger && !dashTrigger);
        anim.SetBool("InAir", !isGrounded());
        anim.SetBool("Sliding", sliding);
        anim.SetBool("Wall Slide", wallSliding);

        //Reset triggers
        if (firstAttackTrigger) firstAttackTrigger = false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Second Attack")) secondAttackTrigger = false;
        if (dashTrigger) dashTrigger = false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash Attack")) dashAttackTrigger = false;

        wasFacingRight = isFacingRight();
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime); //position is updated first so collider has time to update with animation

        //Horizontal Logic
        if (sliding && 
            (((velocity.x * velocity.x) > slideSpeedFinish * speed * speed) || Input.GetKey("s"))) 
            velocity.x *= slidingDamp;
        else if (sliding) sliding = false;
        else if (isAttacking()) velocity.x = 0f;
        else if (isDashing()) {
            if (isFacingRight()) velocity.x = speed * dashBoost;
            else velocity.x = -speed * dashBoost;
        }
        else if (canChangeDirection()) velocity.x = Input.GetAxisRaw("Horizontal") * speed;
        else if (wallSliding) velocity.x = 0f;
        else velocity.x += Input.GetAxisRaw("Horizontal") * airMovement * speed;

        //Jump/Gravity Logic
        if (Input.GetKey("space") && (isGrounded() || wallSliding) && !sliding && !isAttacking() && !isDashing()) {
            velocity.y = jumpStrength;

            if (wallSliding) {
                if (Input.GetAxisRaw("Horizontal") < 0) {
                    velocity.x = speed * wallJumpBoost;
                    wasFacingRight = true;
                } else if (Input.GetAxisRaw("Horizontal") > 0) {
                    velocity.x = -speed * wallJumpBoost;
                    wasFacingRight = false;
                }
                wallSliding = false;
            }
        }
        
        if (!isGrounded() && !Input.GetKey("space")) {
            WallSlideStates wallSlideState = canWallSlide();

            if (Input.GetAxisRaw("Horizontal") != 0) {
                if (wallSlideState == WallSlideStates.Left && Input.GetAxisRaw("Horizontal") < 0) {
                    wallSliding = true;
                    wasFacingRight = false;
                } else if (wallSlideState == WallSlideStates.Right && Input.GetAxisRaw("Horizontal") > 0) {
                    wallSliding = true;
                    wasFacingRight = true;
                } else {
                    wallSliding = false;
                }
            } else if (wallSliding) {
                if (wallSlideState == WallSlideStates.Left) wasFacingRight = true;
                else wasFacingRight = false;

                wallSliding = false;
            }
        } else {
            wallSliding = false;
        }

        if (wallSliding) velocity.y = wallSlideSpeed;
        else if (!isGrounded()) velocity.y -= gravityStrength;
        else if (isGrounded() && velocity.y < 0f) velocity.y = 0f;
    }

    bool isFacingRight() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        
        if (canChangeDirection()) {
            if (horizontal > 0) { return true; }
            else if (horizontal < 0) { return false; }
        }

        return wasFacingRight;
    }

    void setDirection(Direction dir) {
        gameObject.transform.localScale = dir == Direction.Right ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
    }

    bool canChangeDirection() {
        if (!isGrounded()) return false;

        if (sliding) return false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Slide")) return false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Slide Loop")) return false;
        if (isAttacking()) return false;
        if (isDashing()) return false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Wall Slide")) return false;
        return true;
    }

    bool canFirstAttack() {
        return canChangeDirection();
    }

    bool isAttacking() {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("First Attack")) return true;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Second Attack")) return true;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash Attack")) return true;
        return false;
    }

    bool isDashing() {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dash")) return true;
        return false;
    }

    bool isRunning() {
        return (velocity.x != 0f && isGrounded() && !sliding);
    }

    bool isGrounded() {
        float distanceToCheck = .01f;

        Bounds colBound = bc.bounds;
        Vector2 left = new Vector2(colBound.min.x, colBound.min.y);
        Vector2 right = new Vector2(colBound.max.x, colBound.max.y - 2f * colBound.extents.y);

        RaycastHit2D hitLeft = Physics2D.Raycast(
            left, -Vector2.up,
            distanceToCheck, LayerMask.GetMask("Ground"));

        //Vector3 leftDebug = new Vector3(left.x, left.y, 0f);
        //Debug.DrawLine(leftDebug, leftDebug - (new Vector3(0f, distanceToCheck, 0f)), hitLeft.collider != null ? Color.green : Color.red);

        RaycastHit2D hitRight = Physics2D.Raycast(
            right, -Vector2.up,
            distanceToCheck, LayerMask.GetMask("Ground"));

        //Vector3 rightDebug = new Vector3(right.x, right.y, 0f);
        //Debug.DrawLine(rightDebug, rightDebug - (new Vector3(0f, distanceToCheck, 0f)), hitRight.collider != null ? Color.green : Color.red);

        return (hitLeft.collider != null) || (hitRight.collider != null);
    }

    WallSlideStates canWallSlide() {
        if (isGrounded()) return WallSlideStates.None;

        float distanceToCheck = .05f;

        RaycastHit2D hitFront = Physics2D.Raycast(
            new Vector2(frontWallCheck.position.x, frontWallCheck.position.y),
            Vector2.right * (isFacingRight() ? 1 : -1), distanceToCheck, LayerMask.GetMask("Wall"));

        Debug.DrawLine(frontWallCheck.position, frontWallCheck.position + (new Vector3(distanceToCheck * (isFacingRight() ? 1 : -1), 0f, 0f)), hitFront.collider != null ? Color.green : Color.red);

        RaycastHit2D hitBack = Physics2D.Raycast(
            new Vector2(backWallCheck.position.x, backWallCheck.position.y),
            Vector2.right * (isFacingRight() ? -1 : 1), distanceToCheck, LayerMask.GetMask("Wall"));

        Debug.DrawLine(backWallCheck.position, backWallCheck.position + (new Vector3(distanceToCheck * (isFacingRight() ? -1 : 1), 0f, 0f)), hitBack.collider != null ? Color.green : Color.red);

        if (hitFront.collider != null) {
            if (isFacingRight()) return WallSlideStates.Right;
            else return WallSlideStates.Left;
        } else if (hitBack.collider != null) {
            if (isFacingRight()) return WallSlideStates.Left;
            else return WallSlideStates.Right;
        }
        else return WallSlideStates.None;
    }
}
