using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using Settings = Hellsing.Kalista.Config.Modes.WaveClear;

namespace Hellsing.Kalista.Modes
{
    public class WaveClear : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
        }

        public override void Execute()
        {
            if (Player.ManaPercent < Settings.MinMana)
            {
                return;
            }

            // Precheck
            if (!(Settings.UseQ && Q.IsReady()) &&
                !(Settings.UseE && E.IsReady()))
            {
                return;
            }

            // Minions around
            var minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition.To2D(), Q.Range);
            if (minions.Count == 0)
            {
                return;
            }

            // TODO: Readd Q logic once Collision is added

            #region E usage

            if (Settings.UseE && E.IsReady())
            {
                // Get minions in E range
                var minionsInRange = minions.Where(m => E.IsInRange(m)).ToArray();

                // Validate available minions
                if (minionsInRange.Length >= Settings.MinNumberE)
                {
                    // Check if enough minions die with E
                    var killableNum = 0;
                    foreach (var minion in minionsInRange)
                    {
                        if (minion.IsRendKillable())
                        {
                            // Increase kill number
                            killableNum++;

                            // Cast on condition met
                            if (killableNum >= Settings.MinNumberE)
                            {
                                E.Cast();
                                break;
                            }
                        }
                    }
                }
            }

            #endregion
        }
    }
}
