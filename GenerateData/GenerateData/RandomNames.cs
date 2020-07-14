using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace GenerateData
{
    // code is an abbreviated version of the code from https://github.com/duncanrhamill/RandomNameGen

    public class RandomName
    {
        Random rand;
        List<string> Male;
        List<string> Female;
        List<string> Last;

        class NameList
        {
            public string[] men { get; set; }
            public string[] women { get; set; }
            public string[] last { get; set; }

            public NameList()
            {
                men = new string[] { };
                women = new string[] { };
                last = new string[] { };
            }
        }

        /// <summary>
        /// Initialises a new instance of the RandomName class.
        /// </summary>
        /// <param name="rand">A Random that is used to pick names</param>
        public RandomName(Random rand)
        {
            this.rand = rand;
            NameList l = new NameList();

            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader reader = new StreamReader("data/names.json"))
            using (JsonReader jreader = new JsonTextReader(reader))
            {
                l = serializer.Deserialize<NameList>(jreader);
            }

            Male = new List<string>(l.men);
            Female = new List<string>(l.women);
            Last = new List<string>(l.last);
        }


        /// <summary>
        /// Returns a new random name
        /// </summary>
        /// <param name="sex">The sex of the person to be named. true for male, false for female</param>
        /// <returns>The random name as a string</returns>
        public personName GeneratePersonName(Sex sex)
        {
            string first = sex == Sex.Male ? Male[rand.Next(Male.Count)] : Female[rand.Next(Female.Count)]; // determines if we should select a name from male or female, and randomly picks
            string last = Last[rand.Next(Last.Count)]; // gets the last name

            var p = new personName();
            p.firstName = first;
            p.lastName = last;

            return p;
        }
    }

    public enum Sex
    {
        Male,
        Female
    }
}