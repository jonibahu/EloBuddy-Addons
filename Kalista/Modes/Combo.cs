using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Settings = Hellsing.Kalista.Config.Modes.Combo;

namespace Hellsing.Kalista.Modes
{
    public class Combo : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
        }

        public override void Execute()
        {
            // Item usage
            if (Settings.UseItems && Kalista.IsAfterAttack && Kalista.AfterAttackTarget is AIHeroClient)
            {
                ItemManager.UseBotrk((AIHeroClient) Kalista.AfterAttackTarget);
                ItemManager.UseYoumuu((Obj_AI_Base) Kalista.AfterAttackTarget);
            }

            var target = TargetSelector.GetTarget((Settings.UseQ && Q.IsReady()) ? Q.Range : (E.Range * 1.2f), DamageType.Physical);
            if (target != null)
            {
                // Q usage
                if (Settings.UseQ && Q.IsReady() && Q.Cast(target))
                {
                    return;
                }

                // E usage
                var buff = target.GetRendBuff();
                if (Settings.UseE && (E.IsLearned && !E.IsOnCooldown) && buff != null && E.IsInRange(target))
                {
                    // Check if the target would die from E
                    if (!Config.Misc.UseKillsteal && target.IsRendKillable() && E.Cast())
                    {
                        return;
                    }

                    // Check if target has the desired amount of E stacks on
                    if (buff.Count >= Settings.MinNumberE)
                    {
                        // Check if target is about to leave our E range or the buff is about to run out
                        if ((target.Distance(Player, true) > (E.Range * 0.8).Pow() ||
                             buff.EndTime - Game.Time < 0.3) && E.Cast())
                        {
                            return;
                        }
                    }

                    // E to slow
                    if (!Config.Misc.UseHarassPlus && Settings.UseESlow &&
                        ObjectManager.Get<Obj_AI_Base>().Any(o => E.IsInRange(o) && o.IsRendKillable()) &&
                        E.Cast())
                    {
                        return;
                    }
                }

                // Auto attacks
                if (Settings.UseAA && Orbwalker.CanAutoAttack && !Player.IsInAutoAttackRange(target) &&
                    !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) &&
                    !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    // Force a new target for the Orbwalker
                    Orbwalker.ForcedTarget = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => !o.IsAlly && o.IsValidTarget(Player.GetAutoAttackRange()));
                }
            }
        }
    }
}
