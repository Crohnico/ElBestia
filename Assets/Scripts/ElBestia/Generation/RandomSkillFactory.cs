using System;
using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Skills;

namespace ElBestia.Generation
{
    public static class RandomSkillFactory
    {
        public static SkillData Create(System.Random rng)
        {
            WeaponType weapon = RandomWeapon(rng);
            return CreateElaborateSkill(weapon, 1, rng);
        }

        public static SkillData[] CreateEquippedElaborateSkills(int championLevel, WeaponType weapon, System.Random rng)
        {
            int unlockedSlots = championLevel / 5;
            if (championLevel <= 1 && rng.NextDouble() < 0.5)
            {
                unlockedSlots = 1;
            }

            unlockedSlots = Math.Min(3, Math.Max(0, unlockedSlots));
            var skills = new List<SkillData>();

            for (int i = 0; i < unlockedSlots; i++)
            {
                int skillLevel = Math.Max(1, championLevel - i * 5);
                skills.Add(CreateElaborateSkill(weapon, skillLevel, rng));
            }

            return skills.ToArray();
        }

        public static SkillData CreateBaseSkill(WeaponType weapon, System.Random rng)
        {
            var skill = new SkillData
            {
                isBaseSkill = true,
                quality = GrowthGrade.F,
                level = 1,
                cooldownSeconds = 0f,
                element = SkillElement.None,
                energyCost = 0,
                range = weapon == WeaponType.Spear ? 2f : 1f,
                weapons = new[] { weapon },
                statScaling = CreateBaseScaling(weapon),
                cast = new[] { CreateBaseCastAction(weapon) },
                preCast = Array.Empty<SkillAction>(),
                postCast = Array.Empty<SkillAction>(),
                iconSeed = rng.Next(1, int.MaxValue)
            };

            SkillNameResult name = SkillNameGenerator.Generate(skill);
            skill.skillName = GetBaseSkillName(weapon);
            skill.skillNameId = "skill_name.base." + GetBaseSkillId(weapon);
            skill.description = SkillDescriptionGenerator.Generate(skill);
            return skill;
        }

        private static string GetBaseSkillName(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Axe:
                    return "Base Axe Cleave";
                case WeaponType.Sword:
                    return "Base Sword Slash";
                case WeaponType.Spear:
                    return "Base Spear Thrust";
                case WeaponType.Staff:
                    return "Base Staff Strike";
                case WeaponType.Fists:
                    return "Base Punch";
                default:
                    return "Base Strike";
            }
        }

        private static string GetBaseSkillId(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Axe:
                    return "axe_cleave";
                case WeaponType.Sword:
                    return "sword_slash";
                case WeaponType.Spear:
                    return "spear_thrust";
                case WeaponType.Staff:
                    return "staff_strike";
                case WeaponType.Fists:
                    return "punch";
                default:
                    return "strike";
            }
        }

        private static SkillData CreateElaborateSkill(WeaponType weapon, int skillLevel, System.Random rng)
        {
            var quality = ParetoRoller.RollGrade(rng);
            var element = RandomEnum<SkillElement>(rng);
            var mainStat = RandomEnum<ChampionStatType>(rng);
            int scalingCount = rng.NextDouble() < 0.3 ? 2 : 1;

            var skill = new SkillData
            {
                quality = quality,
                level = Math.Max(1, skillLevel),
                element = element,
                energyCost = rng.Next(5, 21),
                range = RandomRange(rng),
                weapons = new[] { weapon },
                statScaling = CreateScaling(rng, mainStat, scalingCount, quality),
                iconSeed = rng.Next(1, int.MaxValue)
            };

            AssignActions(skill, rng);
            skill.cooldownSeconds = CalculateCooldown(skill);
            SkillNameResult name = SkillNameGenerator.Generate(skill);
            skill.skillName = name.EnglishName;
            skill.skillNameId = name.LocalizationId;
            skill.description = SkillDescriptionGenerator.Generate(skill);
            return skill;
        }

        private static StatScaling[] CreateBaseScaling(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Axe:
                    return new[] { new StatScaling { stat = ChampionStatType.Strength, usesGradeScaling = false } };
                case WeaponType.Spear:
                    return new[]
                    {
                        new StatScaling { stat = ChampionStatType.Agility, usesGradeScaling = false },
                        new StatScaling { stat = ChampionStatType.Strength, usesGradeScaling = false }
                    };
                case WeaponType.Sword:
                    return new[]
                    {
                        new StatScaling { stat = ChampionStatType.Strength, usesGradeScaling = false },
                        new StatScaling { stat = ChampionStatType.Agility, usesGradeScaling = false }
                    };
                default:
                    return new[] { new StatScaling { stat = ChampionStatType.Strength, usesGradeScaling = false } };
            }
        }

        private static SkillAction CreateBaseCastAction(WeaponType weapon)
        {
            int damage = weapon == WeaponType.Axe ? 14 : weapon == WeaponType.Spear ? 12 : 8;
            return new SkillAction
            {
                target = SkillTarget.Enemy,
                action = SkillActionType.DoDamage,
                charge = ChargeType.None,
                amount = damage
            };
        }

        private static StatScaling[] CreateScaling(System.Random rng, ChampionStatType mainStat, int count, GrowthGrade quality)
        {
            var result = new List<StatScaling>
            {
                new StatScaling
                {
                    stat = mainStat,
                    scaling = quality
                }
            };

            if (count > 1)
            {
                ChampionStatType secondStat = RandomEnum<ChampionStatType>(rng);
                if (secondStat == mainStat)
                {
                    secondStat = (ChampionStatType)(((int)mainStat + 1) % Enum.GetValues(typeof(ChampionStatType)).Length);
                }

                result.Add(new StatScaling
                {
                    stat = secondStat,
                    scaling = RandomEnum<GrowthGrade>(rng)
                });
            }

            return result.ToArray();
        }

        private static WeaponType RandomWeapon(System.Random rng)
        {
            WeaponType[] weapons =
            {
                WeaponType.Fists,
                WeaponType.Sword,
                WeaponType.Axe,
                WeaponType.Spear,
                WeaponType.Staff
            };

            return weapons[rng.Next(0, weapons.Length)];
        }

        private static float RandomRange(System.Random rng)
        {
            return (float)Math.Round(rng.NextDouble() * 4f, 1);
        }

        private static void AssignActions(SkillData skill, System.Random rng)
        {
            int phaseCount = GetPhaseCount(skill.quality);
            var phases = PickPhases(rng, phaseCount);

            skill.preCast = phases.Contains(0) ? new[] { CreatePreAction(rng) } : Array.Empty<SkillAction>();
            skill.cast = phases.Contains(1) ? new[] { CreateCastAction(rng) } : Array.Empty<SkillAction>();
            skill.postCast = phases.Contains(2) ? new[] { CreatePostAction(rng) } : Array.Empty<SkillAction>();
        }

        private static float CalculateCooldown(SkillData skill)
        {
            int actionCount = CountActions(skill.preCast) + CountActions(skill.cast) + CountActions(skill.postCast);
            int effectCount = CountEffects(skill.preCast) + CountEffects(skill.cast) + CountEffects(skill.postCast);
            float baseCooldown = Math.Min(10f, Math.Max(2f, 2f + actionCount * 1.75f + effectCount * 0.5f));
            float levelReduction = Math.Min(0.6f, (skill.level - 1) * 0.03f);
            return (float)Math.Round(Math.Max(1f, baseCooldown * (1f - levelReduction)), 1);
        }

        private static int CountActions(SkillAction[] actions)
        {
            return actions != null ? actions.Length : 0;
        }

        private static int CountEffects(SkillAction[] actions)
        {
            if (actions == null)
            {
                return 0;
            }

            int count = 0;
            foreach (SkillAction action in actions)
            {
                if (action != null && action.action != SkillActionType.DoDamage)
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetPhaseCount(GrowthGrade quality)
        {
            switch (quality)
            {
                case GrowthGrade.F:
                case GrowthGrade.E:
                case GrowthGrade.D:
                    return 1;
                case GrowthGrade.C:
                case GrowthGrade.B:
                    return 2;
                default:
                    return 3;
            }
        }

        private static HashSet<int> PickPhases(System.Random rng, int count)
        {
            var phases = new HashSet<int>();
            while (phases.Count < count)
            {
                phases.Add(rng.Next(0, 3));
            }

            return phases;
        }

        private static SkillAction CreateCastAction(System.Random rng)
        {
            bool isHeal = rng.NextDouble() < 0.2;
            return new SkillAction
            {
                target = isHeal ? SkillTarget.Self : SkillTarget.Enemy,
                action = SkillActionType.DoDamage,
                charge = ChargeType.None,
                amount = isHeal ? -rng.Next(8, 21) : rng.Next(8, 26)
            };
        }

        private static SkillAction CreatePreAction(System.Random rng)
        {
            int roll = rng.Next(0, 3);
            switch (roll)
            {
                case 0:
                    return new SkillAction
                    {
                        target = SkillTarget.Self,
                        action = SkillActionType.IncreaseDamage,
                        amount = rng.Next(1, 4)
                    };
                case 1:
                    return new SkillAction
                    {
                        target = SkillTarget.Enemy,
                        action = SkillActionType.WeakenEnemy,
                        amount = rng.Next(1, 4)
                    };
                default:
                    return new SkillAction
                    {
                        target = SkillTarget.Enemy,
                        action = SkillActionType.ApplyCharges,
                        charge = RandomNegativeCharge(rng),
                        amount = rng.Next(1, 4)
                    };
            }
        }

        private static SkillAction CreatePostAction(System.Random rng)
        {
            int roll = rng.Next(0, 3);
            switch (roll)
            {
                case 0:
                    return new SkillAction
                    {
                        target = SkillTarget.Self,
                        action = SkillActionType.GainCharges,
                        charge = RandomPositiveCharge(rng),
                        amount = rng.Next(1, 4)
                    };
                case 1:
                    return new SkillAction
                    {
                        target = SkillTarget.Enemy,
                        action = SkillActionType.ApplyCharges,
                        charge = RandomNegativeCharge(rng),
                        amount = rng.Next(1, 4)
                    };
                default:
                    return new SkillAction
                    {
                        target = SkillTarget.Enemy,
                        action = SkillActionType.WeakenEnemy,
                        amount = rng.Next(1, 4)
                    };
            }
        }

        private static ChargeType RandomPositiveCharge(System.Random rng)
        {
            var values = new[]
            {
                ChargeType.Haste,
                ChargeType.LifeSteal,
                ChargeType.Counterattack,
                ChargeType.Regeneration,
                ChargeType.Fortified,
                ChargeType.Thorns,
                ChargeType.FireFortified,
                ChargeType.WaterFortified,
                ChargeType.ElectricityFortified,
                ChargeType.PoisonFortified,
                ChargeType.EarthFortified,
                ChargeType.AirFortified,
                ChargeType.WoodFortified
            };
            return values[rng.Next(0, values.Length)];
        }

        private static ChargeType RandomNegativeCharge(System.Random rng)
        {
            var values = new[]
            {
                ChargeType.Poison,
                ChargeType.Burn,
                ChargeType.Drowning,
                ChargeType.Shock,
                ChargeType.Splinter,
                ChargeType.Crush,
                ChargeType.WindShear,
                ChargeType.Slow,
                ChargeType.Vulnerable,
                ChargeType.Bleed,
                ChargeType.Caltrops,
                ChargeType.Recoil,
                ChargeType.Dizzle
            };
            return values[rng.Next(0, values.Length)];
        }

        private static T RandomEnum<T>(System.Random rng) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(rng.Next(0, values.Length));
        }

    }
}
