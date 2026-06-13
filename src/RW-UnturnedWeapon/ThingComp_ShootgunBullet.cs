
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RW_UnturnedWeapon;

public class ThingComp_ShootgunBullet : ThingComp
{
    private bool exctraBulletSpawned;

    private delegate void Projectile_InfoCopyLaunch(Projectile _this, Projectile copySrc);
    private static Projectile_InfoCopyLaunch InfoCopyLaunch;
    static ThingComp_ShootgunBullet()
    {
#if !V10 && !V11 && !V12
        Type[] parmTypes = [typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef)];
        FieldInfo Projectile_preventFriendlyFire = typeof(Projectile).GetField("preventFriendlyFire", BindingFlags.Instance | BindingFlags.NonPublic);
#else
        Type[] parmTypes = [typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(Thing), typeof(ThingDef)];
#endif
        FieldInfo Projectile_launcher = typeof(Projectile).GetField("launcher", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo Projectile_origin = typeof(Projectile).GetField("origin", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo Projectile_usedTarget = typeof(Projectile).GetField("usedTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo Projectile_intendedTarget = typeof(Projectile).GetField("intendedTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo Projectile_equipment = typeof(Projectile).GetField("equipment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo Projectile_targetCoverDef = typeof(Projectile).GetField("targetCoverDef", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MethodInfo Projectile_Lanuch = typeof(Projectile).GetMethod("Launch", parmTypes);
        //        projectile.Launch(
        //            Projectile_launcher(parentAsProjectile),
        //            Projectile_origin(parentAsProjectile),
        //            Projectile_usedTarget(parentAsProjectile),
        //            Projectile_intendedTarget(parentAsProjectile),
        //            ProjectileHitFlags.All,
        //#if !V10 && !V11 && !V12
        //                Projectile_preventFriendlyFire(parentAsProjectile),
        //#endif
        //            Projectile_equipment(parentAsProjectile),
        //            Projectile_targetCoverDef(parentAsProjectile)
        //        );
        DynamicMethod dynamicMethod = new DynamicMethod("InfoCopyLaunch", typeof(void), [typeof(Projectile), typeof(Projectile)], typeof(Projectile));
        ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_launcher);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_origin);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_usedTarget);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_intendedTarget);
        iLGenerator.Emit(OpCodes.Ldc_I4_M1);
#if !V10 && !V11 && !V12
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_preventFriendlyFire);
#endif
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_equipment);
        iLGenerator.Emit(OpCodes.Ldarg_1);
        iLGenerator.Emit(OpCodes.Ldfld, Projectile_targetCoverDef);
        iLGenerator.EmitCall(OpCodes.Callvirt, Projectile_Lanuch, parmTypes);
        iLGenerator.Emit(OpCodes.Ret);

        InfoCopyLaunch = (Projectile_InfoCopyLaunch)dynamicMethod.CreateDelegate(typeof(Projectile_InfoCopyLaunch));
    }
    public CompProperties_ShootgunBullet Properties_ShootgunBullet
    {
        get
        {
            if (props == null || props is not CompProperties_ShootgunBullet)
            {
                props = new CompProperties_ShootgunBullet();
            }
            return (CompProperties_ShootgunBullet)props;
        }
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look<bool>(ref exctraBulletSpawned, "exctraBulletSpawned");
    }

    public override void CompTick()
    {
        if (exctraBulletSpawned || parent is not Projectile || !typeof(Projectile).IsAssignableFrom(parent.def.thingClass))
        {
            return;
        }
        exctraBulletSpawned = true;
        if (parent.def.comps == null || parent.def.comps.Find(x => x?.compClass != null && typeof(ThingComp_ShootgunBullet).IsAssignableFrom(x.compClass)) == null)
        {
            return;
        }
        Projectile parentAsProjectile = (Projectile)parent;
        for (int i = 0; i < Properties_ShootgunBullet.exctraBulletCount; i++)
        {
            Projectile projectile = (Projectile)ThingMaker.MakeThing(parent.def, parent.Stuff);
            ThingComp_ShootgunBullet thingComp = projectile.GetComp<ThingComp_ShootgunBullet>();
            if(thingComp != null)
            {
                thingComp.exctraBulletSpawned = true;
            }
            projectile.Position = parent.Position;
            projectile.SpawnSetup(parent.MapHeld, false);
            InfoCopyLaunch(projectile, parentAsProjectile);
        }
    }

}

public class CompProperties_ShootgunBullet : CompProperties
{
    public CompProperties_ShootgunBullet()
    {
        compClass = typeof(ThingComp_ShootgunBullet);
    }
    public uint exctraBulletCount = 0;
}