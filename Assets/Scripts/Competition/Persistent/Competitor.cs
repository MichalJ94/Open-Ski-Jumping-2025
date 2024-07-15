using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenSkiJumping.Competition.Persistent
{
    [Serializable]
    public class Competitor
    {
        public string id = "";
        public string lastName = "";
        public string firstName = "";
        public string countryCode = "";

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender gender;
        public Control control;

        public DateTime birthdate = new DateTime(1999, 8, 22);
        public string imagePath = "";
        public string helmetColor = "000000";
        public string suitTopFrontColor = "000000";
        public string suitTopBackColor = "000000";
        public string suitBottomFrontColor = "000000";
        public string suitBottomBackColor = "000000";
        public string skisColor = "000000";
        public int normalHillSkill;
        public int largeHillSkill;
        public int skiFlyingHillSkill;
    }
}