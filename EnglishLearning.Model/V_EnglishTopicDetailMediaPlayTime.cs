using System;
using System.Collections.Generic;
using System.Text;

namespace EnglishLearning.Model
{
    public class V_EnglishTopicDetailMediaPlayTime : EnglishMediaPlayTime
    {
        public int Id { get; set; }
        public string TopicDetailMediaId { get; set; }
        public string TopicDetailId     { get; set; }
        public int MediaId { get; set; }
    }
}
