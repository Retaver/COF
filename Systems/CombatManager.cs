using UnityEngine;
public class CombatManager:MonoBehaviour{
 bool combatActive=false;
 public void BeginCombat(string enemyId){ combatActive=true; Debug.Log($"[CombatManager] BeginCombat '{enemyId}' (stub)."); }
 public void TriggerPlayerAction(int index){ Debug.Log($"[CombatManager] TriggerPlayerAction {index} (stub)."); }
 public void TriggerPlayerAction(int index,object payload){ Debug.Log($"[CombatManager] TriggerPlayerAction {index} with payload (stub)."); }

    // Overload that accepts action names as strings.  This allows UI controllers
    // to call into the combat manager without requiring a numeric index.  At
    // present this simply logs the action; you can add your own mapping logic
    // here if desired.
    public void TriggerPlayerAction(string action, string payload)
    {
        Debug.Log($"[CombatManager] TriggerPlayerAction '{action}' with payload '{payload}' (stub).");
        // Optionally, map string actions to integer indices or handle them directly.
    }
 public void DealDamageToEnemy(int rawDamage,out int finalDamage){ finalDamage=Mathf.Max(0,rawDamage); Debug.Log($"[CombatManager] DealDamageToEnemy {finalDamage} (stub)."); }
 public void EnemyAttack(){ if(!combatActive) return; Debug.Log("[CombatManager] EnemyAttack (stub)."); }
}
