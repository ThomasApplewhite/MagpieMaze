/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

//This class Adapter's Unity's FindObjectsOfType with any component(s) that need to be messed with
//to prevent something from moving.
public abstract class FreezableNPC : UnityEngine.MonoBehaviour
{
    public abstract void Freeze();
    public abstract void Unfreeze();
}
