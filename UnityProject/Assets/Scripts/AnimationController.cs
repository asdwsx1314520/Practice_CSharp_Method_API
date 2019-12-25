using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator hero;

    private Player player;

    float angle;

    void Awake()
    {
        player = gameObject.GetComponent<Player>();

    }

    void Update()
    {
        
    }
}
