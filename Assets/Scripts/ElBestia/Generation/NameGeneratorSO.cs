using ElBestia.Champions;
using UnityEngine;

namespace ElBestia.Generation
{
    [CreateAssetMenu(menuName = "El Bestia/Generation/Name Generator", fileName = "ChampionNameGenerator")]
    public sealed class NameGeneratorSO : ScriptableObject
    {
        [Header("First Names")]
        [SerializeField] private string[] englishFirstNames = CreateDefaultEnglishFirstNames();
        [SerializeField] private string[] spanishFirstNames = CreateDefaultSpanishFirstNames();
        [SerializeField] private string[] frenchFirstNames = CreateDefaultFrenchFirstNames();
        [SerializeField] private string[] germanFirstNames = CreateDefaultGermanFirstNames();
        [SerializeField] private string[] nordicFirstNames = CreateDefaultNordicFirstNames();
        [SerializeField] private string[] japaneseFirstNames = CreateDefaultJapaneseFirstNames();
        [SerializeField] private string[] koreanFirstNames = CreateDefaultKoreanFirstNames();
        [SerializeField] private string[] chineseFirstNames = CreateDefaultChineseFirstNames();
        [SerializeField] private string[] indianFirstNames = CreateDefaultIndianFirstNames();
        [SerializeField] private string[] portugueseFirstNames = CreateDefaultPortugueseFirstNames();

        [Header("Crafted Surnames")]
        [SerializeField] private string[] namePrefixes = CreateDefaultPrefixes();
        [SerializeField] private string[] nameSuffixes = CreateDefaultSuffixes();

        public string GenerateName(ChampionSex sex, System.Random rng)
        {
            EnsureMinimumDefaults();

            string firstName = PickFirstName(rng);
            string lastName = CraftSurname(rng);
            return $"{firstName} {lastName}";
        }

        public void EnsureMinimumDefaults()
        {
            if (englishFirstNames == null || englishFirstNames.Length < 50)
            {
                englishFirstNames = CreateDefaultEnglishFirstNames();
            }

            if (spanishFirstNames == null || spanishFirstNames.Length < 50)
            {
                spanishFirstNames = CreateDefaultSpanishFirstNames();
            }

            if (frenchFirstNames == null || frenchFirstNames.Length < 10)
            {
                frenchFirstNames = CreateDefaultFrenchFirstNames();
            }

            if (germanFirstNames == null || germanFirstNames.Length < 10)
            {
                germanFirstNames = CreateDefaultGermanFirstNames();
            }

            if (nordicFirstNames == null || nordicFirstNames.Length < 10)
            {
                nordicFirstNames = CreateDefaultNordicFirstNames();
            }

            if (japaneseFirstNames == null || japaneseFirstNames.Length < 10)
            {
                japaneseFirstNames = CreateDefaultJapaneseFirstNames();
            }

            if (koreanFirstNames == null || koreanFirstNames.Length < 10)
            {
                koreanFirstNames = CreateDefaultKoreanFirstNames();
            }

            if (chineseFirstNames == null || chineseFirstNames.Length < 10)
            {
                chineseFirstNames = CreateDefaultChineseFirstNames();
            }

            if (indianFirstNames == null || indianFirstNames.Length < 10)
            {
                indianFirstNames = CreateDefaultIndianFirstNames();
            }

            if (portugueseFirstNames == null || portugueseFirstNames.Length < 50)
            {
                portugueseFirstNames = CreateDefaultPortugueseFirstNames();
            }

            if (namePrefixes == null || namePrefixes.Length < 100)
            {
                namePrefixes = CreateDefaultPrefixes();
            }

            if (nameSuffixes == null || nameSuffixes.Length < 100)
            {
                nameSuffixes = CreateDefaultSuffixes();
            }
        }

        public static NameGeneratorSO LoadDefault()
        {
            return Resources.Load<NameGeneratorSO>("Generators/ChampionNameGenerator");
        }

        private string PickFirstName(System.Random rng)
        {
            int total = englishFirstNames.Length + spanishFirstNames.Length + frenchFirstNames.Length
                + germanFirstNames.Length + nordicFirstNames.Length + japaneseFirstNames.Length
                + koreanFirstNames.Length + chineseFirstNames.Length + indianFirstNames.Length
                + portugueseFirstNames.Length;

            int roll = rng.Next(0, total);
            return PickFromWeightedArrays(roll);
        }

        private string PickFromWeightedArrays(int roll)
        {
            if (TryPick(englishFirstNames, ref roll, out string name)) return name;
            if (TryPick(spanishFirstNames, ref roll, out name)) return name;
            if (TryPick(frenchFirstNames, ref roll, out name)) return name;
            if (TryPick(germanFirstNames, ref roll, out name)) return name;
            if (TryPick(nordicFirstNames, ref roll, out name)) return name;
            if (TryPick(japaneseFirstNames, ref roll, out name)) return name;
            if (TryPick(koreanFirstNames, ref roll, out name)) return name;
            if (TryPick(chineseFirstNames, ref roll, out name)) return name;
            if (TryPick(indianFirstNames, ref roll, out name)) return name;
            if (TryPick(portugueseFirstNames, ref roll, out name)) return name;
            return "Luchador";
        }

        private static bool TryPick(string[] values, ref int roll, out string result)
        {
            if (values == null || values.Length == 0)
            {
                result = null;
                return false;
            }

            if (roll < values.Length)
            {
                result = values[roll];
                return true;
            }

            roll -= values.Length;
            result = null;
            return false;
        }

        private string CraftSurname(System.Random rng)
        {
            string prefix = Pick(namePrefixes, rng, "lucha");
            string suffix = Pick(nameSuffixes, rng, "dor");
            return Capitalize(prefix + suffix);
        }

        private static string Pick(string[] values, System.Random rng, string fallback)
        {
            return values != null && values.Length > 0 ? values[rng.Next(0, values.Length)] : fallback;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Length == 1)
            {
                return value.ToUpperInvariant();
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string[] CreateDefaultEnglishFirstNames()
        {
            return new[]
            {
                "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles",
                "Christopher", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua",
                "George", "Kevin", "Brian", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob",
                "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon", "Benjamin",
                "Samuel", "Frank", "Gregory", "Raymond", "Alexander", "Patrick", "Jack", "Dennis", "Jerry", "Tyler"
            };
        }

        private static string[] CreateDefaultSpanishFirstNames()
        {
            return new[]
            {
                "Rodrigo", "Daniel", "Dionisio", "Ruben", "Isa", "Nieves", "Ana", "Lucia", "Afro", "Guillermo",
                "Jose", "Juan", "Antonio", "Manuel", "Francisco", "Javier", "Carlos", "Miguel", "David", "Alejandro",
                "Pablo", "Sergio", "Jorge", "Alberto", "Fernando", "Luis", "Angel", "Raul", "Diego", "Mario",
                "Adrian", "Marcos", "Oscar", "Victor", "Hugo", "Ivan", "Andres", "Pedro", "Rafael", "Ramon",
                "Enrique", "Alvaro", "Ismael", "Clara", "Maria", "Carmen", "Laura", "Elena", "Marta", "Paula"
            };
        }

        private static string[] CreateDefaultFrenchFirstNames()
        {
            return new[] { "Jean", "Pierre", "Michel", "Philippe", "Alain", "Nicolas", "Louis", "Henri", "Marie", "Camille" };
        }

        private static string[] CreateDefaultGermanFirstNames()
        {
            return new[] { "Hans", "Peter", "Klaus", "Thomas", "Wolfgang", "Jurgen", "Dieter", "Friedrich", "Heinrich", "Greta" };
        }

        private static string[] CreateDefaultNordicFirstNames()
        {
            return new[] { "Erik", "Lars", "Sven", "Bjorn", "Nils", "Anders", "Leif", "Ingrid", "Freya", "Astrid" };
        }

        private static string[] CreateDefaultJapaneseFirstNames()
        {
            return new[] { "Haruto", "Yuto", "Sota", "Ren", "Hiroto", "Yui", "Sakura", "Aoi", "Hina", "Akari" };
        }

        private static string[] CreateDefaultKoreanFirstNames()
        {
            return new[] { "Minjun", "Seojun", "Jiho", "Jisoo", "Seojoon", "Hana", "Seoyeon", "Jiwoo", "Minseo", "Hyunwoo" };
        }

        private static string[] CreateDefaultChineseFirstNames()
        {
            return new[] { "Wei", "Fang", "Li", "Zhang", "Ming", "Jun", "Lei", "Hao", "Xia", "Mei" };
        }

        private static string[] CreateDefaultIndianFirstNames()
        {
            return new[] { "Aarav", "Vivaan", "Aditya", "Arjun", "Krishna", "Rahul", "Priya", "Anaya", "Kavya", "Isha" };
        }

        private static string[] CreateDefaultPortugueseFirstNames()
        {
            return new[]
            {
                "Joao", "Miguel", "Francisco", "Duarte", "Afonso", "Tomas", "Martim", "Rodrigo", "Tiago", "Diogo",
                "Pedro", "Antonio", "Manuel", "Jose", "Goncalo", "Rafael", "Gabriel", "Lucas", "Dinis", "Salvador",
                "Guilherme", "Andre", "Bruno", "Carlos", "Daniel", "Eduardo", "Filipe", "Henrique", "Hugo", "Leonardo",
                "Luis", "Marco", "Nuno", "Paulo", "Ricardo", "Rui", "Vasco", "Beatriz", "Maria", "Leonor",
                "Matilde", "Mariana", "Carolina", "Ana", "Ines", "Sofia", "Clara", "Lara", "Teresa", "Vitoria"
            };
        }

        private static string[] CreateDefaultPrefixes()
        {
            return new[]
            {
                "shadow", "blind", "hand", "heart", "hearth", "fire", "iron", "steel", "blood", "bone",
                "skull", "storm", "thunder", "lightning", "dark", "night", "moon", "sun", "star", "void",
                "ash", "ember", "flame", "smoke", "frost", "ice", "snow", "stone", "rock", "earth",
                "mud", "sand", "dust", "wind", "sky", "cloud", "rain", "river", "wave", "sea",
                "mist", "venom", "toxic", "viper", "snake", "wolf", "bear", "lion", "tiger", "dragon",
                "demon", "angel", "ghost", "spirit", "grave", "crypt", "doom", "rage", "fury", "wrath",
                "blade", "spear", "axe", "hammer", "shield", "fist", "kick", "knee", "elbow", "tooth",
                "fang", "claw", "horn", "eye", "brow", "chin", "jaw", "back", "foot", "boot",
                "red", "blue", "black", "white", "green", "gold", "silver", "brass", "rust", "wild",
                "grim", "mad", "sly", "quick", "slow", "loud", "silent", "drunk", "lucky", "broken"
            };
        }

        private static string[] CreateDefaultSuffixes()
        {
            return new[]
            {
                "force", "man", "woman", "less", "cock", "hand", "heart", "fire", "blade", "born",
                "bane", "breaker", "bringer", "caller", "caster", "chaser", "crusher", "cutter", "dancer", "drinker",
                "eater", "fang", "fist", "foot", "guard", "hammer", "hunter", "keeper", "killer", "knuckle",
                "lord", "mark", "maw", "mind", "monger", "punch", "rider", "runner", "scar", "shade",
                "shard", "shield", "shot", "skin", "skull", "slayer", "smash", "song", "spark", "splitter",
                "stalker", "storm", "strike", "sword", "thorn", "throat", "tide", "tongue", "tooth", "walker",
                "ward", "watcher", "weaver", "whisper", "wind", "wing", "wound", "wraith", "wrath", "zero",
                "biter", "boiler", "burn", "burst", "chain", "clap", "coil", "crack", "crawl", "dash",
                "drop", "gaze", "grip", "grin", "grinder", "howl", "jaw", "lash", "lock", "mask",
                "nail", "piercer", "rush", "scale", "snare", "snarl", "spike", "sworn", "tear", "whip"
            };
        }
    }
}

