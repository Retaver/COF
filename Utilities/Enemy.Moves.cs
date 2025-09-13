using System.Collections.Generic;

public partial class Enemy
{
    [System.Serializable]
    public class EnemyMove
    {
        public string id;
        public int weight = 1;
        public int cooldown = 0;            // reserved for future use
        [System.NonSerialized] public int currentCd = 0;
    }

    // Runtime move set (populated by EnemyDatabase.CreateEnemy)
    public List<EnemyMove> moves = new List<EnemyMove>();
}