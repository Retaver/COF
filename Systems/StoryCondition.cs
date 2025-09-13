using System; using UnityEngine;
[Serializable] public class StoryCondition{
 public string type; public string flag; public string stat; public string itemId; public string value; public int minValue; public int maxValue; public int quantity; public string @operator=">="; public bool negate=false;
 public bool IsConditionMet(){ bool ok=(type??"").ToLowerInvariant() switch { "flag"=>CheckFlag(), "item"=>CheckItem(), "stat"=>CheckStat(), "personality"=>CheckPersonality(), _=>true }; return negate? !ok: ok; }
 bool CheckFlag(){ if(string.IsNullOrEmpty(flag)) return false; var sm=global::StoryManager.Instance; var v=sm!=null?sm.GetFlag(flag):null; if(v==null) return false; if(string.IsNullOrEmpty(value)) return true; return string.Equals(v,value,StringComparison.OrdinalIgnoreCase); }
 bool CheckItem(){ return true;} bool CheckStat(){ return true;} bool CheckPersonality(){ return true;}
 public StoryCondition Clone()=> (StoryCondition)this.MemberwiseClone();
}
