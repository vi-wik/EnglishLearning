using System;
using System.Collections.Generic;
using System.Text;

namespace EnglishLearning.Model
{
    public class V_EnglishWordRootMeaning
    {
        public int Id { get; set; }
        public string RootName { get; set; }
        public int RootId { get; set; }
        public int MeaningId { get; set; }
        public string Meaning { get; set; }
        public string OtherMeaning { get; set; }

        public string FullMeaning
        {
            get
            {
                if(this.OtherMeaning == null)
                {
                    return this.Meaning;
                }
                else
                {
                    return this.Meaning + "；" + this.OtherMeaning;
                }
            }
        }
    }
}
