//To freeze the Player, disable/enable the player's FPSSystemController
public class FreezeablePlayer : FreezableNPC
{
    //The player's FPSSystemController
    private FPSSystemController movement;

    void Start()
    {
        movement = this.gameObject.GetComponent<FPSSystemController>();
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
