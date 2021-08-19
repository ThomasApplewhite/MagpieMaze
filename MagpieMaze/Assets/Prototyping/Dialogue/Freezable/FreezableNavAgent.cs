/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/
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