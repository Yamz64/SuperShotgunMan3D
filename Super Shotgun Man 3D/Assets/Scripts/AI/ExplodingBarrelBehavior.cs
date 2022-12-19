using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBarrelBehavior : BaseEnemyBehavior
{
    public override void UpdateAnimationViewAngle() { }
    public override void Animate() { }

    public override void AI()
    {
        if(HP <= 0)
        {
            FXUtils.InstanceFXObject(2, transform.position, Quaternion.identity, null, true, 75, 0.5f, 4f, 1f);
            Destroy(gameObject);
        }
    }
}
