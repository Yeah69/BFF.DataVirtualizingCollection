using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BFF.DataVirtualizingCollection.Sample.Model.Models
{
    public interface IProfile
    {
        string Occupation { get; }
        string Salary { get; }
        string Name { get; }
        string Description { get; }
        bool IsAvailable { get; }
        bool IsFreelancer { get; }
        string? CompanyName { get; }
        IReadOnlyList<string> Abilities { get; }
        int HiddenAbilitiesCount { get; }
        string PicturePath { get; }
    }

    public static class ProfileStatic
    {
        public static IReadOnlyList<IProfile> ProfilePool { get; } =
            new ReadOnlyCollection<IProfile>(
                new List<IProfile>
                {
                    new Profile(
                        "UI/UX designer",
                        "$55/hr", 
                        "Wide Walson", 
                        "Wade is a 32 year old UI/UX designer, with an impressive portfolio behind him.", 
                        true,
                        false, 
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        "pack://application:,,,/ProfilePics/00_Wide.png"),
                    new Profile(
                        "mobile designer",
                        "$32/hr",
                        "Paria Metrescu",
                        "Paria is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        "pack://application:,,,/ProfilePics/01_Paria.png"),
                    new Profile(
                        "mobile designer",
                        "$42/hr",
                        "Morexandra Algan",
                        "Morexandra is a dedicated developer for mobile platforms and is very good at it.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        12,
                        "pack://application:,,,/ProfilePics/02_Morexandra.png"),
                    new Profile(
                        "interactive designer",
                        "$44/hr",
                        "Smennifer Jith",
                        "Smennifer is an interactive designer who is really awesome at what she does.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        "pack://application:,,,/ProfilePics/03_Smennifer.png"),
                    new Profile(
                        "mobile designer",
                        "$40/hr",
                        "Anyetlana Svukova",
                        "Anyetlana is an Android and iOS designer with advanced knowledge in coding.",
                        true,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS"},
                        2,
                        "pack://application:,,,/ProfilePics/04_Anyetlana.png"),
                    new Profile(
                        "UI/UX designer",
                        "$30/hr",
                        "Korko van Maoh",
                        "Korko is a 25 year old web designer with an impressive portfolio behind him.",
                        false,
                        false,
                        "Visual Madness",
                        new List<string>{ "UI", "UX", "photoshop"},
                        4,
                        "pack://application:,,,/ProfilePics/05_Korko.png"),
                    new Profile(
                        "UX designer",
                        "$50/hr",
                        "Kowel Paszentka",
                        "Kowel is a 32 year old UX designer, with over 10 years of experience in what he does.",
                        false,
                        false,
                        "Apple Inc",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        "pack://application:,,,/ProfilePics/06_Kowel.png"),
                    new Profile(
                        "mobile designer",
                        "$32/hr",
                        "Sinia Samionov",
                        "Sinia is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS" },
                        2,
                        "pack://application:,,,/ProfilePics/07_Sinia.png"),
                    new Profile(
                        "photographer",
                        "$40/hr",
                        "Wonathan Jayne",
                        "Wonathan is a 28 year old photographer from London with real talent for what he does.",
                        false,
                        false,
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        "pack://application:,,,/ProfilePics/08_Wonathan.png"),
                    new Profile(
                        "Superhero",
                        "free",
                        "Matban",
                        "I'm Matban!",
                        false,
                        true,
                        null,
                        new List<string>{ "tech", "IT", "martial arts" },
                        69,
                        "pack://application:,,,/ProfilePics/09_Matban.png"),
                    new Profile(
                        "mobile designer",
                        "$39/hr",
                        "Surgiana Geoclea",
                        "Surgiana is an android and iOS developer who worked at Apple for 6 years.",
                        false,
                        true,
                        null,
                        new List<string>{ "PHP", "android", "iOS" },
                        2,
                        "pack://application:,,,/ProfilePics/10_Surgiana.png"),
                    new Profile(
                        "UI/UX designer",
                        "$45/hr",
                        "Jogory Grehnes",
                        "Jogory is a 32 year old UI/UX designer, with an impressive portfolio behind him.",
                        false,
                        false,
                        "Epic Coders",
                        new List<string>{ "UI", "UX", "photoshop" },
                        4,
                        "pack://application:,,,/ProfilePics/11_Jogory.png")
                });
        
        public static IProfile Empty { get; } = new Profile();
    }

    internal class Profile : IProfile
    {
        public Profile(
            string occupation, 
            string salary,
            string name, 
            string description,
            bool isAvailable,
            bool isFreelancer,
            string? companyName,
            IReadOnlyList<string> abilities,
            int hiddenAbilitiesCount,
            string picturePath)
        {
            Occupation = occupation;
            Salary = salary;
            Name = name;
            Description = description;
            IsAvailable = isAvailable;
            IsFreelancer = isFreelancer;
            CompanyName = companyName;
            Abilities = abilities;
            HiddenAbilitiesCount = hiddenAbilitiesCount;
            PicturePath = picturePath;
        }

        internal Profile()
        {
            Occupation = string.Empty;
            Salary = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Abilities = new string[0];
            PicturePath = string.Empty;
        }

        public string Occupation { get; }

        public string Salary { get; }

        public string Name { get; }

        public string Description { get; }

        public bool IsAvailable { get; }

        public bool IsFreelancer { get; }

        public string? CompanyName { get; }

        public IReadOnlyList<string> Abilities { get; }

        public int HiddenAbilitiesCount { get; }

        public string PicturePath { get; }
    }
}