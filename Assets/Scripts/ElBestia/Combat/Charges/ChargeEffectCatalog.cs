using System.Collections.Generic;
using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal static class ChargeEffectCatalog
    {
        private static readonly Dictionary<ChargeType, IChargeEffect> Effects = BuildEffects();
        private static readonly Dictionary<SkillElement, ChargeType> ElementalFortifications = BuildElementalFortifications();

        public static IChargeEffect Get(ChargeType charge)
        {
            return Effects.TryGetValue(charge, out IChargeEffect effect) ? effect : null;
        }

        public static bool IsPositive(ChargeType charge)
        {
            IChargeEffect effect = Get(charge);
            return effect != null && effect.IsPositive;
        }

        public static bool TicksOnTurnStart(ChargeType charge)
        {
            IChargeEffect effect = Get(charge);
            return effect != null && effect.TicksOnTurnStart;
        }

        public static SkillElement GetDamageElement(ChargeType charge)
        {
            IChargeEffect effect = Get(charge);
            return effect != null ? effect.DamageElement : SkillElement.None;
        }

        public static bool TryGetOpposite(ChargeType charge, out ChargeType opposite)
        {
            IChargeEffect effect = Get(charge);
            opposite = effect != null ? effect.Opposite : ChargeType.None;
            return opposite != ChargeType.None;
        }

        public static ChargeType GetElementalFortification(SkillElement element)
        {
            return ElementalFortifications.TryGetValue(element, out ChargeType charge) ? charge : ChargeType.None;
        }

        private static Dictionary<ChargeType, IChargeEffect> BuildEffects()
        {
            var effects = new IChargeEffect[]
            {
                new PoisonChargeEffect(),
                new BurnChargeEffect(),
                new DrowningChargeEffect(),
                new ShockChargeEffect(),
                new SplinterChargeEffect(),
                new CrushChargeEffect(),
                new WindShearChargeEffect(),
                new SlowChargeEffect(),
                new HasteChargeEffect(),
                new LifeStealChargeEffect(),
                new CounterattackChargeEffect(),
                new EmpoweredChargeEffect(),
                new WeakenedChargeEffect(),
                new RegenerationChargeEffect(),
                new FortifiedChargeEffect(),
                new VulnerableChargeEffect(),
                new BleedChargeEffect(),
                new ThornsChargeEffect(),
                new CaltropsChargeEffect(),
                new RecoilChargeEffect(),
                new FireFortifiedChargeEffect(),
                new WaterFortifiedChargeEffect(),
                new ElectricityFortifiedChargeEffect(),
                new PoisonFortifiedChargeEffect(),
                new EarthFortifiedChargeEffect(),
                new AirFortifiedChargeEffect(),
                new WoodFortifiedChargeEffect(),
                new DizzleChargeEffect()
            };

            var lookup = new Dictionary<ChargeType, IChargeEffect>();
            foreach (IChargeEffect effect in effects)
            {
                lookup[effect.Charge] = effect;
            }

            return lookup;
        }

        private static Dictionary<SkillElement, ChargeType> BuildElementalFortifications()
        {
            return new Dictionary<SkillElement, ChargeType>
            {
                { SkillElement.Fire, ChargeType.FireFortified },
                { SkillElement.Water, ChargeType.WaterFortified },
                { SkillElement.Electricity, ChargeType.ElectricityFortified },
                { SkillElement.Poison, ChargeType.PoisonFortified },
                { SkillElement.Earth, ChargeType.EarthFortified },
                { SkillElement.Air, ChargeType.AirFortified },
                { SkillElement.Wood, ChargeType.WoodFortified }
            };
        }
    }
}
