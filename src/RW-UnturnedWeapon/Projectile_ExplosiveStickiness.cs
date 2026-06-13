
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RW_UnturnedWeapon;

public class Projectile_ExplosiveStickiness : Projectile_Explosive
{
    private Vector3 relativePos;
    private LocalTargetInfo stickingTarget;

    public override Vector3 DrawPos
    {
        get
        {
            if (stickingTarget.IsValid && stickingTarget.HasThing && stickingTarget != this)
            {
                return stickingTarget.Thing.DrawPos + relativePos;
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
        relativePos = base.DrawPos - hitThing.DrawPos;
        if (Vector3.SqrMagnitude(relativePos) <= size * size)
        {
            stickingTarget = hitThing;
        }
    }
}