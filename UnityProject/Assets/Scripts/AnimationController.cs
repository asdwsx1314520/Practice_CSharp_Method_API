using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator fox;
    private Player player;
    private Controller2D controller;

    void Awake()
    {
        player = gameObject.GetComponent<Player>();
        controller = gameObject.GetComponent<Controller2D>();
    }

    void Update()
    {
        behavior();
    }

    /// <summary>
    /// 角色動作判斷
    /// </summary>
    void behavior() 
    {
        fox.SetBool("Run", false);

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector2 v2Anim = player.animType;

        if(input.x == 0) v2Anim.x = input.x;

        if (controller.collisions.below) v2Anim.y = 0;

        if (v2Anim.x != 0)
        {
            fox.SetBool("Run", true);
        }

        if (!controller.collisions.below && v2Anim.y > 0)
        {
            fox.SetTrigger("Jump_ent");
        }
        else if(controller.collisions.below)
        {
            fox.SetTrigger("Jump_end");
        }

        jump(v2Anim.y);
    }

    /// <summary>
    /// 跳躍
    /// </summary>
    /// <param name="vec2Y">判斷上升或下落</param>
    void jump(float vec2Y)
    {
        fox.SetFloat("Jump", vec2Y);
    }
}
