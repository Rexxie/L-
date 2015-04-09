using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace RexxieAIO.Plugins
{
    class Ahri
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        private static Obj_AI_Hero Hero;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public Ahri()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Hero = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 875);
            R = new Spell(SpellSlot.R, 850);

            Q.SetSkillshot(0.25f, 100, 1600, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60, 1200, true, SkillshotType.SkillshotLine);

            Config = new Menu("Ahri", "Ahri", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassQ", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassW", "Use W")).SetValue(false);
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassE", "Use E")).SetValue(false);

            Config.AddSubMenu(new Menu("Lane Clear", "Wave"));
            Config.SubMenu("Wave").AddItem(new MenuItem("UseQWave", "Use Q")).SetValue(true);
            Config.SubMenu("Wave").AddItem(new MenuItem("QMode", "Q to Mouse")).SetValue(true);
            Config.SubMenu("Wave").AddItem(new MenuItem("UseWWave", "Use W")).SetValue(true);

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KS", "Kill Steal").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("KSq", "Killsteal Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KSe", "Killsteal E").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("KSr", "Killsteal R").SetValue(false));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawAA", "Draw AA Range")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += On_InterrupteptableTarget;

            MenuItem drawComboDamageMenu = new MenuItem("DrawDmg", "Draw Combo Damage", true).SetValue(true);
            MenuItem drawFill = new MenuItem("DrawFill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, System.Drawing.Color.FromArgb(90, 255, 169, 4)));
            Config.SubMenu("Drawings").AddItem(drawComboDamageMenu);
            Config.SubMenu("Drawings").AddItem(drawFill);
            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
            drawComboDamageMenu.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            drawFill.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };
        }

        private static float GetComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0d;

            if (Q.IsReady())
                damage += Hero.GetSpellDamage(hero, SpellSlot.Q);

            if (W.IsReady())
                damage += Hero.GetSpellDamage(hero, SpellSlot.W);

            if (E.IsReady())
                damage += Hero.GetSpellDamage(hero, SpellSlot.E);

            if (R.IsReady())
                damage += Hero.GetSpellDamage(hero, SpellSlot.R) * R.Instance.Ammo;

            return (float)damage;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Hero.Position, Q.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }

                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Hero.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }

                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Hero.Position, R.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }

                if (Config.Item("DrawAA").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Hero.Position, W.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(Hero.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawAA").GetValue<bool>())
                {
                    Drawing.DrawCircle(Hero.Position, W.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(Hero.Position, E.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(Hero.Position, R.Range, System.Drawing.Color.White);
                }

            }
        }

        private static void On_InterrupteptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            CastOnUnit(E, sender, HitChance.High);
            // All the checks in function
        }

        private static void OnGameUpdate(EventArgs argum)
        {
            Hero = ObjectManager.Player;
            if (Hero.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                if (target.HasBuffOfType(BuffType.SpellImmunity))
                    return;

                if (Config.Item("UseECombo").GetValue<bool>())
                    CastOnUnit(E, target, HitChance.High);

                if (Config.Item("UseQCombo").GetValue<bool>())
                    CastOnUnit(Q, target, HitChance.High);

                if (Config.Item("UseWCombo").GetValue<bool>())
                    CastSpell(W, target);
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    bool qMouse = Config.Item("QMode").GetValue<bool>();
                    var minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

                    var minion = minions[0];
                    if (minions.Count > 1)
                    {
                        if (qMouse && Config.Item("UseQWave").GetValue<bool>())
                            Q.Cast(Game.CursorPos, true);
                        else if (!qMouse && Config.Item("UseQWave").GetValue<bool>())
                            CastOnUnit(Q, minion, HitChance.High);
                        if (Config.Item("UseWWave").GetValue<bool>())
                            CastSpell(W, minion);
                    }
                }

                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                    if (Config.Item("HarassE").GetValue<bool>())
                        CastOnUnit(E, target, HitChance.High);

                    if (Config.Item("HarassQ").GetValue<bool>())
                        CastOnUnit(Q, target, HitChance.High);

                    if (Config.Item("HarassW").GetValue<bool>())
                        CastSpell(W, target);
                }
            }

            if (Config.Item("KS").GetValue<bool>())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.Team != Hero.Team &&
                !target.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    float health = target.Health;

                    var direction = Hero.Position.Extend(target.Direction, R.Range);

                    double QDMG = Q.GetDamage(target);
                    double EDMG = E.GetDamage(target);
                    double RDMG = R.GetDamage(target);

                    if (Config.Item("KSq").GetValue<bool>() && QDMG + 10 > health)
                        CastOnUnit(Q, target, HitChance.High);
                    else if (Config.Item("KSq").GetValue<bool>() && QDMG + 10 > health && target.IsValidTarget(Q.Range) && R.IsReady())
                        R.Cast(direction);

                    if (Config.Item("KSe").GetValue<bool>() && EDMG + 10 > health)
                        CastOnUnit(E, target, HitChance.High);
                    else if (Config.Item("KSe").GetValue<bool>() && EDMG + 10 > health && target.IsValidTarget(E.Range) && R.IsReady())
                        R.Cast(direction);

                    if (Config.Item("KSr").GetValue<bool>() && RDMG * R.Instance.Ammo + 10 > health)
                        CastSpell(R, target);
                }
            }
        }

        private static void CastSpell(Spell spell, Obj_AI_Base hero)
        {
            if (spell.IsReady() && hero.IsValidTarget(spell.Range))
                spell.Cast();
        }

        private static void CastOnUnit(Spell spell, Obj_AI_Base hero, HitChance hit)
        {
            if (spell.IsReady() && hero.IsValidTarget(spell.Range))
            {
                if (spell.IsSkillshot)
                    spell.CastIfHitchanceEquals(hero, hit, true);
                else
                    spell.Cast(hero);
            }
        }
    }
}
