//To freeze an NPC that moves using a NavMeshAgent, just turn the agent on/off
public class FreezableNavAgent : FreezableNPC
{
    //The NPC's NavMeshAgent
    private UnityEngine.AI.NavMeshAgent movement;

    void Start()
    {
        movement = this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public override void Freeze()
    {
        ToggleSystem(false);
    }

    public override void Unfreeze()
    {
        ToggleSystem(true);
    }

    void ToggleSystem(bool on)
    {
        movement.enabled = on;
    }
}