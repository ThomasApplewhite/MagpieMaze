//This class Adapter's Unity's FindObjectsOfType with any component(s) that need to be messed with
//to prevent something from moving.
public abstract class FreezableNPC : UnityEngine.MonoBehaviour
{
    public abstract void Freeze();
    public abstract void Unfreeze();
}
