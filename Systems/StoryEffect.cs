using UnityEngine;
[System.Serializable] public class StoryEffect{
 public string type; public string target; public string flag; public string stat; public string itemId; public string value; public int amount; public bool flagValue;
 public void Apply(){ switch((type??"").ToLowerInvariant()){ case "flag": ApplyFlag(); break; case "node": if(!string.IsNullOrEmpty(target)) global::StoryManager.Instance?.GoToNode(target); break; } }
 void ApplyFlag(){ string name=!string.IsNullOrEmpty(flag)?flag:target; if(string.IsNullOrEmpty(name)) return; string v=!string.IsNullOrEmpty(value)?value:(flagValue?"true":"true"); global::StoryManager.Instance?.SetFlag(name,v); GameEventSystem.Instance?.RaiseGameFlagSet(name,v); }
 public StoryEffect Clone()=> (StoryEffect)this.MemberwiseClone();
}
