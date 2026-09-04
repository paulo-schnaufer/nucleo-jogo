using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nucleo.Upgrades
{
    /// <summary>Utilitário compartilhado de mira para armas automáticas (Daemon, Fork).</summary>
    public static class TargetingUtils
    {
        private static readonly Collider2D[] QueryResults = new Collider2D[64];

        public static Transform FindNearestEnemy(Vector2 origin, float range, LayerMask enemyLayer, GameObject context)
        {
            Physics2D.SyncTransforms();

            PhysicsScene2D physicsScene = context.scene.GetPhysicsScene2D();
            int hitCount = physicsScene.OverlapCircle(origin, range, QueryResults, enemyLayer);
            Transform nearest = null;
            float nearestSqrDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = QueryResults[i];
                Health health = hit.GetComponent<Health>();
                if (health == null || health.IsDead) continue;

                float sqrDist = ((Vector2)hit.transform.position - origin).sqrMagnitude;
                if (sqrDist < nearestSqrDist)
                {
                    nearestSqrDist = sqrDist;
                    nearest = hit.transform;
                }
            }
            return nearest;
        }
    }
}
