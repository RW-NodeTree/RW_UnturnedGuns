
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RW_UnturnedWeapon;

public class Projectile_ExplosiveStickiness : Projectile_Explosive
{
    private Vector2 relativePos;
    private LocalTargetInfo stickingTarget;

    public override Vector3 DrawPos
    {
        get
        {
            if (stickingTarget.IsValid && stickingTarget.HasThing && stickingTarget != this)
            {
                return stickingTarget.Thing.DrawPos + new Vector3(relativePos.x, 1, relativePos.y);
            }
            return base.DrawPos;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref relativePos, "relativePos");
        Scribe_TargetInfo.Look(ref stickingTarget, "stickingTarget");
    }


#if V10 || V11 || V12 || V13 || V14 || V15
    public override void Tick()
    {
        base.Tick();
#else
    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
#endif
        if (stickingTarget.IsValid && stickingTarget.HasThing && stickingTarget != this)
        {
            Position = stickingTarget.Cell;
        }
    }

#if V10 || V11 || V12 || V13
    protected override void Impact(Thing hitThing)
    {
        base.Impact(hitThing);
        if(!Spawned || hitThing == null)
        {
            return;
        }
#else
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        base.Impact(hitThing, blockedByShield);
        if(blockedByShield || !Spawned || hitThing == null)
        {
            return;
        }
#endif
        float size = hitThing.def.fillPercent;
        if (hitThing is Pawn pawn)
        {
            size = pawn.BodySize;
        }
        Vector3 vector = base.DrawPos - hitThing.DrawPos;
        relativePos = new Vector2(vector.x, vector.z);
        if (Vector3.SqrMagnitude(relativePos) <= size * size)
        {
            stickingTarget = hitThing;
        }
    }
}