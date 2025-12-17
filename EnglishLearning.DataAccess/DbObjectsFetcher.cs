using Dapper;
using EnglishLearning.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishLearning.DataAccess
{
    public class DbObjectsFetcher
    {
        public static async Task<EnglishSubject> GetEnglishSubjectByEnName(string enName)
        {
            string sql = "select * from EnglishSubject where Name_EN=@EnName";

            Dictionary<string, object> para = new Dictionary<string, object>();
            para.Add("EnName", enName);

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishSubject>(sql, para)).FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishMediaType>> GetEnglishMediaTypes()
        {
            string sql = "select * from EnglishMediaType";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishMediaType>(sql));
            }
        }

        public static async Task<IEnumerable<EnglishPlatform>> GetEnglishPlatforms()
        {
            string sql = "select * from EnglishPlatform";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishPlatform>(sql));
            }
        }

        public static async Task<IEnumerable<EnglishTeacher>> GetEnglishTeachers()
        {
            string sql = "select * from EnglishTeacher";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishTeacher>(sql));
            }
        }

        public static async Task<V_EnglishPhrase> GetVEnglishPhrase(int phraseId)
        {
            string sql = $"select * from V_EnglishPhrase where Id={phraseId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishPhrase>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<V_EnglishPhrase> GetVEnglishPhrase(string phrase)
        {
            string sql = $"select * from V_EnglishPhrase where Phrase=@Phrase";

            Dictionary<string, object> para = new Dictionary<string, object>() { { "@Phrase", phrase } };

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishPhrase>(sql, para))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishPhrase>> GetEnglishPhrases(EnglishWordFilter filter = null, bool isByMeaning = false)
        {
            string condition = "";
            string keyword = filter?.Keyword;
            bool fullMatch = filter?.FullMatch ?? false;
            bool needMeaning = filter?.NeedMeaning ?? false;

            string cleanKeyword = "REPLACE(REPLACE(REPLACE(REPLACE(LOWER(@Keyword),'somebody','sb'),'someone','sb'),'one','sb'),'something','sth')";
            string cleanFieldFormat = "REPLACE(REPLACE(REPLACE(LOWER({0}),'someone','sb'),'one','sb'),'something','sth')";

            if (!string.IsNullOrEmpty(keyword))
            {
                if (!isByMeaning)
                {
                    if (fullMatch)

                    {
                        condition = $"{string.Format(cleanFieldFormat, "Phrase")}={cleanKeyword}";
                    }
                    else
                    {
                        condition = $@"INSTR({string.Format(cleanFieldFormat, "Phrase")}, {cleanKeyword})>0
                          or (Abbreviation is not null and INSTR(LOWER(Abbreviation),LOWER(@Keyword))>0)
                          or (Synonym is not null and INSTR({string.Format(cleanFieldFormat, "Synonym")},{cleanKeyword})>0)";
                    }
                }
                else
                {
                    string value = DbUtitlity.GetSafeValue(keyword);

                    condition = $"Meaning like '%{value}%'";
                }
            }

            string where = !string.IsNullOrEmpty(condition) ? $" where {condition}" : "";
            string columns = "Id,Phrase";

            if (needMeaning)
            {
                columns += ",Meaning";
            }

            string sql = $@"select {columns} from EnglishPhrase {where}
                          order by Lower(Phrase)";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishPhrase>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishWord>> GetVEnglishWords()
        {
            string sql = "select * from V_EnglishWord";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWord>(sql);
            }
        }

        public static async Task<V_EnglishWord> GetVEnglishWord(int wordId)
        {
            string sql = $"select * from V_EnglishWord where Id={wordId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishWord>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<V_EnglishWord> GetVEnglishWord(string word)
        {
            string sql = $"select * from V_EnglishWord where Word=@Word";

            Dictionary<string, object> para = new Dictionary<string, object> { { "@Word", word } };

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishWord>(sql, para))?.FirstOrDefault();
            }
        }

        private static string GetWordMatchCondition(string keyword, bool fullMatch, bool ignoreCase = true, bool isMatchPrefix = false, bool isMatchSuffix = false)
        {
            string condition = "";
            string cleanKeyword = keyword;

            if (!string.IsNullOrEmpty(keyword))
            {
                if (fullMatch)
                {
                    if (ignoreCase)
                    {
                        condition = $"REPLACE(REPLACE(LOWER(Word),' ',''),'-','')=REPLACE(REPLACE(LOWER('{keyword}'),' ',''),'-','')";
                    }
                    else
                    {
                        condition = $"REPLACE(REPLACE(Word,' ',''),'-','')=REPLACE(REPLACE('{keyword}',' ',''),'-','')";
                    }
                }
                else if (isMatchPrefix)
                {
                    if (ignoreCase)
                    {
                        condition = $"LOWER(Word) like LOWER('{keyword}%')";
                    }
                    else
                    {
                        condition = $"INSTR(Word, '{keyword}')=1";
                    }

                    condition += $" and LOWER(Word)<> LOWER('{keyword}')";
                }
                else if (isMatchSuffix)
                {
                    condition = $"LOWER(Word) like LOWER('%{keyword}')";
                }
                else
                {
                    condition = $"REPLACE(REPLACE(LOWER(Word),' ',''),'-','') like REPLACE(REPLACE(LOWER('{keyword}%'),' ',''),'-','')";
                }
            }

            return condition;
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWords(EnglishWordFilter filter = null)
        {
            string keyword = filter?.Keyword;
            bool ignoreCase = filter?.IgnoreCase ?? true;
            bool fullMatch = filter?.FullMatch ?? false;
            bool needMeaning = filter?.NeedMeaning ?? false;
            bool mustHaveMeaning = filter?.MustHaveMeaning ?? false;
            bool isMatchPrefix = filter?.IsMatchPrefix ?? false;
            bool isMatchSuffix = filter?.IsMatchSuffix ?? false;
            int limitCount = filter?.LimitCount ?? 100;
            string notBeginWith = filter?.NotBeginWith;
            string notEndWith = filter?.NotEndWith;

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = DbUtitlity.GetSafeValue(keyword);
            }

            string condition = GetWordMatchCondition(keyword, fullMatch, ignoreCase, isMatchPrefix, isMatchSuffix);
            string limitCondition = filter?.NoLimit == true ? "" : $"limit {limitCount}";

            if (!string.IsNullOrEmpty(notBeginWith))
            {
                condition += GetEnglishWordNotBeginWithCondition(notBeginWith);
            }

            if (!string.IsNullOrEmpty(notEndWith))
            {
                condition += GetEnglishWordNotEndWithCondition(notEndWith);
            }

            string tableName = needMeaning ? "V_EnlishWordSimpleMeaning" : "EnglishWord";

            string columns = "Id,Word,ExamType";

            if (needMeaning)
            {
                columns += ",CommonMeaning,SpecialMeaning";

                if (mustHaveMeaning)
                {
                    condition += $" and (CommonMeaning is not null)";
                }
            }

            string where = !string.IsNullOrEmpty(condition) ? $" where {condition}" : "";

            string sql = $@"select {columns} from {tableName} {where}                          
                           {limitCondition}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordWithMeaning>(sql);
            }
        }

        private static string GetEnglishWordNotBeginWithCondition(string notBeginWith)
        {
            return GetEnglishWordNotBeginWithOrEndWithCondition(notBeginWith, true);
        }

        private static string GetEnglishWordNotEndWithCondition(string notEndWith)
        {
            return GetEnglishWordNotBeginWithOrEndWithCondition(notEndWith, false);
        }

        private static string GetEnglishWordNotBeginWithOrEndWithCondition(string affix, bool isBeginWith)
        {
            if (affix == null)
            {
                return string.Empty;
            }

            var items = affix.Split(',');

            StringBuilder sb = new StringBuilder();

            foreach (var item in items)
            {
                sb.AppendLine($" and LOWER(Word) not like LOWER('{(isBeginWith ? "" : "%")}{DbUtitlity.GetSafeValue(item)}{(isBeginWith ? "%" : "")}')");
            }

            return sb.ToString();
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeanings(string keyword)
        {
            string value = DbUtitlity.GetSafeValue(keyword);

            string sql = $@"select * from V_EnglishWordMeaning where CommonMeaning like '%{value}%' or SpecialMeaning like '%{value}%' ";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordMeaning>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordMeaning>> GetEnglishWordMeanings(int wordId, EnglishWordMeaningFilter filter = null)
        {
            string fields = "Id,WordId,Word,ExamType,PartOfSpeechId,PartOfSpeech,CommonMeaning,Comment,IsFromWeb,IsOld, Priority";

            string condition = "";

            if (filter == null || filter.ShowSpecialMeaning)
            {
                fields += ",SpecialMeaning";
            }
            else
            {
                condition = " and IsOld=0 and Special=0 and Informal=0 and Obsolete=0";
            }

            string sql = $@"select {fields} from V_EnglishWordMeaning where WordId={wordId} {condition} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordMeaning>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWord>> GetEnglishWords()
        {
            string sql = "select * from EnglishWord";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishWord>(sql));
            }
        }

        public static async Task<IEnumerable<EnglishConsonant>> GetEnglishConsonants()
        {
            string sql = "select * from EnglishConsonant order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishConsonant>(sql));
            }
        }

        public static async Task<EnglishConsonant> GetEnglishConsonant(int consonantId)
        {
            string sql = $"select * from EnglishConsonant where Id={consonantId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishConsonant>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishVowel>> GetEnglishVowels()
        {
            string sql = "select * from EnglishVowel  where IsHidden=0 order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishVowel>(sql));
            }
        }

        public static async Task<EnglishVowel> GetEnglishVowel(int vowelId)
        {
            string sql = $"select * from EnglishVowel where Id={vowelId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishVowel>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<V_EnglishSubjectMedia>> GetVEnglishSubjectMedias(int subjectId)
        {
            string sql = $"select * from V_EnglishSubjectMedia where SubjectId={subjectId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishSubjectMedia>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishWordMedia>> GetVEnglishWordMedias(int wordId)
        {
            string sql = $"select * from V_EnglishWordMedia where WordId={wordId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishWordMedia>(sql));
            }
        }

        public static async Task<IEnumerable<EnglishSubject>> GetEnglishSubjects()
        {
            string sql = $"select * from EnglishSubject order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishSubject>(sql));
            }
        }

        public static async Task<EnglishSubject> GetEnglishSubject(int subjectId)
        {
            string sql = $"select * from EnglishSubject where Id={subjectId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishSubject>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<EnglishMediaExtraInfo> GetEnglishMediaExtraInfo(int medialId)
        {
            string sql = $"select * from EnglishMediaExtraInfo where MediaId={medialId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishMediaExtraInfo>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<EnglishMedia> GetEnglishMedia(int id)
        {
            string sql = $"select * from EnglishMedia where Id={id}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishMedia>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishTopic>> GetEnglishTopics(int subjectId)
        {
            string sql = $"select * from EnglishTopic where SubjectId={subjectId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishTopic>(sql));
            }
        }

        public static async Task<IEnumerable<EnglishTopicDetail>> GetEnglishTopicDetails(int topicId, string keyword = null)
        {
            keyword = DbUtitlity.GetSafeValue(keyword);

            string keywordCondition = string.IsNullOrEmpty(keyword) ? "" : $"and Name like '%{keyword}%'";

            string sql = $"select * from EnglishTopicDetail where TopicId={topicId} {keywordCondition} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishTopicDetail>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishTopicDetailMedia>> GetVEnglishTopicDetailMedias(int topicId, string keyword = null)
        {
            keyword = DbUtitlity.GetSafeValue(keyword);

            string keywordCondition = string.IsNullOrEmpty(keyword) ? "" : $"and ((MediaTitleExt is not null and MediaTitleExt like '%{keyword}%') or (MediaTitleExt is null and MediaTitle like '%{keyword}%'))";

            string sql = $"select * from V_EnglishTopicDetailMedia where TopicId={topicId} {keywordCondition} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishTopicDetailMedia>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishWordMediaPlayTime>> GetVEnglishWordMediaPlayTimes(int wordMediaId)
        {
            string sql = $"select * from V_EnglishWordMediaPlayTime where WordMediaId={wordMediaId} order by StartTime";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishWordMediaPlayTime>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishConsonantMediaPlayTime>> GetVEnglishConsonantMediaPlayTimes(int consonantMediaId)
        {
            string sql = $"select * from V_EnglishConsonantMediaPlayTime where ConsonantMediaId={consonantMediaId} order by StartTime";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishConsonantMediaPlayTime>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishPhraseMediaPlayTime>> GetVEnglishPhraseMediaPlayTimes(int phraseMediaId)
        {
            string sql = $"select * from V_EnglishPhraseMediaPlayTime where PhraseMediaId={phraseMediaId} order by StartTime";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishPhraseMediaPlayTime>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishVowelMediaPlayTime>> GetVEnglishVowelMediaPlayTimes(int vowelMediaId)
        {
            string sql = $"select * from V_EnglishVowelMediaPlayTime where VowelMediaId={vowelMediaId} order by StartTime";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishVowelMediaPlayTime>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishConsonantMedia>> GetVEnglishConsonantMedias(int constantId)
        {
            string sql = $"select * from V_EnglishConsonantMedia where ConsonantId={constantId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishConsonantMedia>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishVowelMedia>> GetVEnglishVowelMedias(int vowelId)
        {
            string sql = $"select * from V_EnglishVowelMedia where VowelId={vowelId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishVowelMedia>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishPhraseMedia>> GetVEnglishPhraseMedias(int phraseId)
        {
            string sql = $"select * from V_EnglishPhraseMedia where PhraseId={phraseId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishPhraseMedia>(sql));
            }
        }

        public static async Task<IEnumerable<V_EnglishTopicDetailMediaPlayTime>> GetVEnglishTopicDetailMediaPlayTimes(int topicDetailMediaId)
        {
            string sql = $"select * from V_EnglishTopicDetailMediaPlayTime where TopicDetailMediaId={topicDetailMediaId} order by StartTime";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishTopicDetailMediaPlayTime>(sql));
            }
        }

        public static async Task<bool> HasUserDataTable(string dbFilePath, string tableName)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = $"SELECT 1 FROM sqlite_schema WHERE type= 'table' AND name=@Name";

                Dictionary<string, object> para = new Dictionary<string, object>();

                para.Add("@Name", DbUtitlity.GetParameterValue(tableName));

                bool? existing = (await connection.QueryAsync<bool>(sql, para))?.FirstOrDefault();

                return existing == true;
            }
        }

        public static async Task<IEnumerable<MediaFavoriteCategory>> GetMediaFavoriteCategories(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from MediaFavoriteCategory order by Priority";

                return await connection.QueryAsync<MediaFavoriteCategory>(sql);
            }
        }

        public static async Task<IEnumerable<MediaFavorite>> GetMediaFavorites(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from MediaFavorite";

                return await connection.QueryAsync<MediaFavorite>(sql);
            }
        }

        public static async Task<IEnumerable<MediaAccessHistory>> GetMediaAccessHistories(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from MediaAccessHistory";

                return await connection.QueryAsync<MediaAccessHistory>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordVOCAB>> GetEnglishWordVOCABs(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from EnglishWordVOCAB";

                return await connection.QueryAsync<EnglishWordVOCAB>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishPhraseVOCAB>> GetEnglishPhraseVOCABs(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from EnglishPhraseVOCAB";

                return await connection.QueryAsync<EnglishPhraseVOCAB>(sql);
            }
        }

        public static async Task<IEnumerable<V_MediaAccessHistory>> GetVMediaAccessHistories()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = "SELECT * from V_MediaAccessHistory";

                return await connection.QueryAsync<V_MediaAccessHistory>(sql);
            }
        }

        public static async Task<EnglishWordVOCAB> GetEnglishWordVOCAB(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"SELECT * from EnglishWordVOCAB where WordId={wordId}";

                return (await connection.QueryAsync<EnglishWordVOCAB>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<EnglishPhraseVOCAB> GetEnglishPhraseVOCAB(int phraseId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"SELECT * from EnglishPhraseVOCAB where PhraseId={phraseId}";

                return (await connection.QueryAsync<EnglishPhraseVOCAB>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<V_EnglishWordVOCAB>> GetVEnglishWordVOCABs(EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string keyword = filter?.Keyword;

                string sql = GetVOCABCondition(EnglishObjectType.Word, keyword, filter, sortInfo);

                Dictionary<string, object> para = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(keyword))
                {
                    para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
                }

                return await connection.QueryAsync<V_EnglishWordVOCAB>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishPhraseVOCAB>> GetVEnglishPhraseVOCABs(EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string keyword = filter?.Keyword;

                string sql = GetVOCABCondition(EnglishObjectType.Phrase, keyword, filter, sortInfo);

                Dictionary<string, object> para = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(keyword))
                {
                    para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
                }

                return await connection.QueryAsync<V_EnglishPhraseVOCAB>(sql, para);
            }
        }

        private static string GetVOCABCondition(EnglishObjectType type, string keyword, EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {

            bool fullMatch = filter?.FullMatch ?? false;
            bool needMeaning = filter?.NeedMeaning ?? false;

            string tableName = type == EnglishObjectType.Word ? (needMeaning ? "V_EnglishWordVOCABWithMeaning" : "V_EnglishWordVOCAB") :
               (needMeaning ? "V_EnglishPhraseVOCABWithMeaning" : "V_EnglishPhraseVOCAB");

            string sql = $"select * from {tableName}";

            if (!string.IsNullOrEmpty(keyword))
            {
                string condition = "";

                if (fullMatch)
                {
                    condition = "LOWER(Name)=LOWER(@Keyword)";
                }
                else
                {
                    condition = "INSTR(LOWER(Name), LOWER(@Keyword))>0";
                }

                sql += $" where {condition}";
            }

            string order = "";

            if (sortInfo != null)
            {
                string fieldName = sortInfo.FieldName;

                DataSortType sortType = sortInfo.SortType;

                order = $"Lower({fieldName}) {sortType.ToString()}";
            }
            else
            {
                order = "Lower(Name)";
            }

            sql += $" order by {order}";

            return sql;
        }

        public static async Task<IEnumerable<V_EnglishWordVOCAB>> GetEnglishWordVOCABSuggestions(string keyword)
        {
            string sql = "select * from V_EnglishWordVOCAB where INSTR(LOWER(Name),LOWER(@Keyword))=1 order by Name limit 50";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordVOCAB>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishPhraseVOCAB>> GetEnglishPhraseVOCABSuggestions(string keyword)
        {
            string sql = "select * from V_EnglishPhraseVOCAB where INSTR(LOWER(Name),LOWER(@Keyword))=1 order by Name limit 50";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishPhraseVOCAB>(sql, para);
            }
        }

        public static async Task<int> GetEnglishWordVOCABCount()
        {
            string sql = "select count(1) from EnglishWordVOCAB";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<int>(sql))?.FirstOrDefault() ?? 0;
            }
        }

        public static async Task<int> GetEnglishPhraseVOCABCount()
        {
            string sql = "select count(1) from EnglishPhraseVOCAB";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<int>(sql))?.FirstOrDefault() ?? 0;
            }
        }

        public static async Task<bool> IsEnglishWordVOCAB(int id)
        {
            string sql = $"select 1 from EnglishWordVOCAB where Id={id}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<bool>(sql))?.FirstOrDefault() == true;
            }
        }

        public static async Task<bool> IsEnglishPhraseVOCAB(int id)
        {
            string sql = $"select 1 from EnglishPhraseVOCAB where Id={id}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<bool>(sql))?.FirstOrDefault() == true;
            }
        }


        public static async Task<MediaFavorite> GetMediaFavoriteByMediaId(int mediaId)
        {
            string sql = $"select * from MediaFavorite where MediaId={mediaId}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<MediaFavorite>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<V_MediaFavorite>> GetVMediaFavorites()
        {
            string sql = $"select * from V_MediaFavorite";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_MediaFavorite>(sql);
            }
        }

        public static async Task<bool> IsMediaFavoriteCategoryBeRefering(List<int> ids)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select count(1) as Num from MediaFavorite where CategoryId in({string.Join(",", ids)})";

                int? num = (await connection.QueryAsync<int>(sql))?.FirstOrDefault();

                return num > 0;
            }
        }

        public static async Task<bool> IsMediaFavoriteCategoryNameExisting(bool isAdd, string name, int? id)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select 1 from MediaFavoriteCategory where Name=@Name";

                if (!isAdd)
                {
                    sql += $" and Id<>{id}";
                }

                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Name", name);

                return (await connection.QueryAsync<bool>(sql, para))?.FirstOrDefault() == true;
            }
        }

        public static async Task<int> GetMediaFavoriteCategoriesCount()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select count(1) as Num from MediaFavoriteCategory";


                return (await connection.QueryAsync<int>(sql))?.FirstOrDefault() ?? 0;
            }
        }

        public static async Task<IEnumerable<int>> GetEnglishWordIdsByWords(IEnumerable<string> words)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string strWords = string.Join(",", words.Select(item => $"'{DbUtitlity.GetSafeValue(item.ToLower())}'"));

                string sql = $"select Id from EnglishWord where LOWER(Word) in({strWords})";

                return await connection.QueryAsync<int>(sql);
            }
        }

        public static async Task<IEnumerable<int>> GetEnglishPhraseIdsByPhrases(IEnumerable<string> phrases)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string strPhrases = string.Join(",", phrases.Select(item => $"'{DbUtitlity.GetSafeValue(item.ToLower())}'"));

                string sql = $"select Id from EnglishPhrase where LOWER(Phrase) in({strPhrases})";

                return await connection.QueryAsync<int>(sql);
            }
        }

        public static async Task<IEnumerable<int>> GetExistingWordIdsOfEnglishWordVOCAB(IEnumerable<int> ids)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string strIds = string.Join(",", ids);

                string sql = $"select WordId from EnglishWordVOCAB where WordId in({strIds})";

                return await connection.QueryAsync<int>(sql);
            }
        }

        public static async Task<IEnumerable<int>> GetExistingPhraseIdsOfEnglishPhraseVOCAB(IEnumerable<int> ids)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string strIds = string.Join(",", ids);

                string sql = $"select PhraseId from EnglishPhraseVOCAB where PhraseId in({strIds})";

                return await connection.QueryAsync<int>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishExamType>> GetEnglishExamTypes()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from EnglishExamType order by Priority";

                return await connection.QueryAsync<EnglishExamType>(sql);
            }
        }

        public static async Task<int?> GetEnglishWordNotLearnedNextId(EnglishExamType examType, bool isForNonExamType = false, bool isForVOCAB = false,
                                                                     EnglishVOCABLearnSortMode sortMode = EnglishVOCABLearnSortMode.AlphabetAsc)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {

                string sql = null;
                string condition = null;
                string orderBy = "Lower(w.Word)";

                if (examType != null)
                {
                    int weight = examType.Weight;
                    int examTypeId = examType.Id;

                    condition = $"ExamType & {examType.Weight} = {weight} ";
                }
                else if (isForNonExamType)
                {
                    condition = "ExamType is null";
                }

                if (!isForVOCAB)
                {
                    sql = $@"select w.Id from EnglishWord w
                            where {condition}
                            and w.Id not in(select WordId from EnglishWordLearnedHistory)
                            order by {orderBy}
                            limit 1";
                }
                else
                {
                    if (sortMode == EnglishVOCABLearnSortMode.CreateTimeAsc)
                    {
                        orderBy = "v.CreateTime";
                    }
                    else if (sortMode == EnglishVOCABLearnSortMode.CreateIimeDesc)
                    {
                        orderBy = "v.CreateTime desc";
                    }

                    sql = $@"select w.Id from EnglishWord w
                             join EnglishWordVOCAB v on v.WordId=w.Id
                             where {condition}
                             and w.Id not in(select WordId from EnglishWordLearnedHistory)
                             order by {orderBy}
                             limit 1";
                }

                return (await connection.QueryAsync<int?>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishExamTypeWordLearnedStatisticInfo>> GetEnglishExamTypeWordLearnedStatistics()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = @"select et.Id,et.Name,Count(distinct w.Id) as Total,Count(distinct h.WordId) as LearnedCount
                                from EnglishExamType et
                                left join EnglishWord w on w.ExamType is not null and (et.Weight & w.ExamType=et.Weight)
                                left join EnglishWordLearnedHistory h on h.WordId=w.Id
                                group by et.Id,et.Name";

                return await connection.QueryAsync<EnglishExamTypeWordLearnedStatisticInfo>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishExamTypeWordLearnedStatisticInfo>> GetEnglishWordVOCABLearnedStatistics()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = @"select t.Id,t.Name,Count(distinct v.WordId) as Total,Count(distinct h.WordId) as LearnedCount
                                from EnglishWordVOCAB v
                                left join EnglishWordLearnedHistory h on v.WordId=h.WordId
                                left join EnglishWord w on w.Id=v.WordId
                                left join EnglishExamType t on w.ExamType is not null and (t.Weight & w.ExamType=t.Weight)
                                group by t.Id,t.Name";

                return await connection.QueryAsync<EnglishExamTypeWordLearnedStatisticInfo>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordLearnedHistory>> GetEnglishWordLearnHistories(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = $"select * from EnglishWordLearnedHistory";

                return await connection.QueryAsync<EnglishWordLearnedHistory>(sql);
            }
        }

        public static async Task<int?> GetEnglishWordLearnedPreviousWordId(int? examTypeId, int wordId, bool isForNonExamType = false, bool isForVOCAB = false)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string joinCondition = null;
                string condition = "";

                if (examTypeId > 0)
                {
                    joinCondition = $" join EnglishExamType et on et.Id={examTypeId} and (et.Weight & w.ExamType=et.Weight)";
                }

                if(isForNonExamType)
                {
                    condition = " and w.ExamType is null";
                }

                if(isForVOCAB)
                {
                    joinCondition += " join EnglishWordVOCAB v on v.WordId = w.Id";
                }

                string sql = $@"select h.WordId from EnglishWordLearnedHistory h
                                join EnglishWord w on h.WordId=w.Id
                                {joinCondition}
                                where h.CreateTime<IFNULL((select CreateTime from EnglishWordLearnedHistory where WordId={wordId}),STRFTIME('%Y-%m-%d %H:%M:%f','NOW','LOCALTIME'))
                                {condition}
                                order by h.CreateTime desc
                                limit 1";

                return (await connection.QueryAsync<int?>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<int?> GetEnglishPhraseLearnedPreviousPhraseId(int phraseId, bool isForVOCAB = false)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string joinCondition = null;
                string condition = "";               

                if (isForVOCAB)
                {
                    joinCondition += " join EnglishPhraseVOCAB v on v.PhraseId = p.Id";
                }

                string sql = $@"select h.PhraseId from EnglishPhraseLearnedHistory h
                                join EnglishPhrase p on h.PhraseId=p.Id
                                {joinCondition}
                                where h.CreateTime<IFNULL((select CreateTime from EnglishPhraseLearnedHistory where PhraseId={phraseId}),STRFTIME('%Y-%m-%d %H:%M:%f','NOW','LOCALTIME'))
                                {condition}
                                order by h.CreateTime desc
                                limit 1";

                return (await connection.QueryAsync<int?>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<int?> GetEnglishPhraseNotLearnedNextId(bool isForVOCAB = false, EnglishVOCABLearnSortMode sortMode = EnglishVOCABLearnSortMode.AlphabetAsc)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = null;
                string orderBy = "Lower(p.Phrase)";

                if(!isForVOCAB)
                {
                    sql = $@"select p.Id from EnglishPhrase p                           
                             where p.Id not in(select PhraseId from EnglishPhraseLearnedHistory)
                             order by {orderBy}
                             limit 1";
                }
                else
                {
                    if (sortMode == EnglishVOCABLearnSortMode.CreateTimeAsc)
                    {
                        orderBy = "v.CreateTime";
                    }
                    else if (sortMode == EnglishVOCABLearnSortMode.CreateIimeDesc)
                    {
                        orderBy = "v.CreateTime desc";
                    }

                    sql = $@"select p.Id from EnglishPhrase p
                             join EnglishPhraseVOCAB v on v.PhraseId=p.Id
                             where p.Id not in(select PhraseId from EnglishPhraseLearnedHistory)
                             order by {orderBy}
                             limit 1";
                }               

                return (await connection.QueryAsync<int?>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishPhraseLearnedHistory>> GetEnglishPhraseLearnHistories(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = $"select * from EnglishPhraseLearnedHistory";

                return await connection.QueryAsync<EnglishPhraseLearnedHistory>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordPartOfSpeech>> GetEnglishWordPartOfSpeeches()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = "select * from EnglishWordPartOfSpeech";

                return await connection.QueryAsync<EnglishWordPartOfSpeech>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordExample>> GetVEnglishWordExamples(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordExample where WordId={wordId} order by Priority";

                return await connection.QueryAsync<V_EnglishWordExample>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishPhraseExample>> GetVEnglishPhraseExamples(int phraseId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishPhraseExample where PhraseId={phraseId} order by Priority";

                return await connection.QueryAsync<V_EnglishPhraseExample>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordInflection>> GetVEnglishWordInflections(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordInflection where WordId={wordId}";

                return await connection.QueryAsync<V_EnglishWordInflection>(sql);
            }
        }


        public static async Task<IEnumerable<EnglishWordSyllable>> GetEnglishWordSyllables(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from EnglishWordSyllable where WordId={wordId} order by Priority";

                return await connection.QueryAsync<EnglishWordSyllable>(sql);
            }
        }

        public static async Task<int> GetEnglishWordSyllableCount(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select count(1) from EnglishWordSyllable where WordId={wordId}";

                return (await connection.QueryAsync<int?>(sql)).FirstOrDefault()??0;
            }
        }

        public static async Task<IEnumerable<EnglishWordInflectionType>> GetEnglishWordInflectionTypes()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = "select * from EnglishWordInflectionType";

                return await connection.QueryAsync<EnglishWordInflectionType>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordInflection>> GetVEnglishWordInflectionsByTargetWordId(int targetWordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordInflection where TargetWordId={targetWordId}";

                return await connection.QueryAsync<V_EnglishWordInflection>(sql);
            }
        }

        public static async Task<IEnumerable<string>> GetEnglishPhraseAlphabets()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = @"select Alphabet from
(
select UPPER(substring(Phrase,1,1)) as Alphabet from EnglishPhrase
where cast(substring(Phrase,1,1) as numberic)=0
group by UPPER(substring(Phrase,1,1))
) t order by Alphabet";

                return await connection.QueryAsync<string>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishPhrase>> GetEnglishPhrasesByAlphabet(string alphabet)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Alphabet", alphabet);

                string sql = "select * from EnglishPhrase where UPPER(substring(Phrase,1,1))=@Alphabet order by UPPER(Phrase)";

                return await connection.QueryAsync<EnglishPhrase>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordVariant>> GetVEnglishWordVariants(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordVariant where WordId={wordId} order by Priority";

                return await connection.QueryAsync<V_EnglishWordVariant>(sql);
            }
        }

        public static async Task<EnglishWordMeaningSpecialStatistic> GetEnglishWordMeaningSpecialStatistic(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $@"select sum(case when (Special = 1 or Informal=1 or IsOld=1 or Obsolete=1)=1 then 1 else 0 end) as SpecialRowCount
,sum(case when SpecialMeaning is not null then 1 else 0 end)  as SpecialColumnCount
from EnglishWordMeaning
where WordId = {wordId} ";

                return (await connection.QueryAsync<EnglishWordMeaningSpecialStatistic>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishWordPrefix>> GetEnglishWordPrefixes(string keyword = null, bool excludeHidden = false)
        {
            string sql = "select * from EnglishWordPrefix where 1=1";

            if (excludeHidden)
            {
                sql += " and Hidden=0";
            }

            Dictionary<string, object> para = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " and INSTR(LOWER(Name),LOWER(@Keyword))>0";
                para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
            }

            sql += " order by name";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefix>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordSuffix>> GetEnglishWordSuffixes(string keyword, bool excludeHidden = false)
        {
            string sql = "select * from EnglishWordSuffix where 1=1";

            if (excludeHidden)
            {
                sql += " and Hidden=0";
            }

            Dictionary<string, object> para = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " and INSTR(LOWER(Name),LOWER(@Keyword))>0";
                para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
            }

            sql += " order by name";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordSuffix>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordElement>> GetEnglishWordRoots(string keyword)
        {
            string sql = "select * from EnglishWordRoot";

            Dictionary<string, object> para = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " where INSTR(LOWER(Name),LOWER(@Keyword))>0";

                para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
            }

            sql += " order by Name";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordElement>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordRootMeaning>> GetVEnglishWordRootMeanings(string keyword)
        {
            string sql = @"select * from V_EnglishWordRootMeaning";

            Dictionary<string, object> para = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(keyword))
            {
                sql += " where INSTR(LOWER(RootName),LOWER(@Keyword))>0";

                para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
            }

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordRootMeaning>(sql, para);
            }
        }

        public static async Task<EnglishWordPrefix> GetEnglishWordPrefixById(int id)
        {
            string sql = $"select * from EnglishWordPrefix where Id={id}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishWordPrefix>(sql)).FirstOrDefault();
            }
        }

        public static async Task<EnglishWordPrefix> GetEnglishWordPrefixByName(string name)
        {
            string sql = "select * from EnglishWordPrefix where LOWER(Name)=LOWER(@Name)";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(name));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishWordPrefix>(sql, para)).FirstOrDefault();
            }
        }

        public static async Task<EnglishWordSuffix> GetEnglishWordSuffixByName(string name)
        {
            string sql = "select * from EnglishWordSuffix where LOWER(Name)=LOWER(@Name)";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(name));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishWordSuffix>(sql, para)).FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<EnglishWordPrefixStatistic>> GetEnglishWordPrefixStatisticsByAffixName(string affixName)
        {
            string sql = "select d.* from EnglishWordPrefixStatistic d join EnglishWordPrefix p on p.Id=d.AffixId where LOWER(p.Name)=LOWER(@Name) order by Priority";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(affixName));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefixStatistic>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordSuffixStatistic>> GetEnglishWordSuffixStatisticsByAffixName(string affixName)
        {
            string sql = "select d.* from EnglishWordSuffixStatistic d join EnglishWordSuffix s on s.Id=d.AffixId where LOWER(s.Name)=LOWER(@Name) order by Priority";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(affixName));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordSuffixStatistic>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordPrefixStatistic>> GetEnglishWordPrefixStatisticsByAffixId(int affixId)
        {
            string sql = $"select d.* from EnglishWordPrefixStatistic d join EnglishWordPrefix p on p.Id=d.AffixId where p.Id={affixId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefixStatistic>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningByPrefixStatistic(EnglishWordAffixStatistic statistic, string affixName)
        {
            return await GetEnglishWordMeaningByAffixStatistic(statistic, affixName, true);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningBySuffixStatistic(EnglishWordAffixStatistic statistic, string affixName)
        {
            return await GetEnglishWordMeaningByAffixStatistic(statistic, affixName, false);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningByAffixStatistic(EnglishWordAffixStatistic statistic, string affixName, bool isPrefix)
        {
            bool isOthers = statistic.Id < 0;

            string wordCondition = "";
            string contentCondition = "";

            EnglishWordAffix affix = null;

            if (isPrefix)
            {
                affix = await GetEnglishWordPrefixByName(affixName);
            }
            else
            {
                affix = await GetEnglishWordSuffixByName(affixName);
            }

            if (!isOthers)
            {
                if (!statistic.Content.Contains("...") || statistic.Content.StartsWith("...") || statistic.Content.EndsWith("..."))
                {
                    contentCondition = "and (instr(m.CommonMeaning,@Content)>0 or instr(m.SpecialMeaning,@Content)>0)";
                }
                else
                {
                    string likeContent = statistic.Content.Replace("...", "%");

                    contentCondition = $"and (m.CommonMeaning like '%{likeContent}%' or ifnull(m.SpecialMeaning,'') like '%{likeContent}%')";
                }
            }
            else
            {
                IEnumerable<EnglishWordAffixStatistic> statistics = null;

                if (isPrefix)
                {
                    statistics = await GetEnglishWordPrefixStatisticsByAffixName(affixName);
                }
                else
                {
                    statistics = await GetEnglishWordSuffixStatisticsByAffixName(affixName);
                }

                StringBuilder sb = new StringBuilder();

                foreach (var s in statistics)
                {
                    string content = s.Content;

                    if (!content.Contains("...") || content.StartsWith("...") || content.EndsWith("..."))
                    {
                        string trimedContent = content.Replace("...", "");

                        sb.AppendLine($"and (instr(m.CommonMeaning,'{trimedContent}')=0 and ifnull(instr(m.SpecialMeaning,'{trimedContent}'),0)=0)");
                    }
                    else
                    {
                        string likeContent = content.Replace("...", "%");

                        sb.AppendLine($"and (m.CommonMeaning not like '%{likeContent}%' and ifnull(m.SpecialMeaning,'') not like '%{likeContent}%')");
                    }
                }

                contentCondition = sb.ToString();
            }

            if (statistic.ExcludeContent != null)
            {
                var items = statistic.ExcludeContent.Split(',', '，');

                StringBuilder sb = new StringBuilder();

                foreach (var item in items)
                {
                    sb.AppendLine($" and (instr(m.CommonMeaning,'{item}')=0 and ifnull(instr(m.SpecialMeaning,'{item}'),0)=0)");
                }

                contentCondition += sb.ToString();
            }

            if (affix != null)
            {
                if (affix.ExcludeName != null)
                {
                    if (isPrefix)
                    {
                        wordCondition = GetEnglishWordNotBeginWithCondition(affix.ExcludeName);
                    }
                    else
                    {
                        wordCondition = GetEnglishWordNotEndWithCondition(affix.ExcludeName);
                    }
                }
            }

            string sql = "";

            if (isPrefix)
            {
                sql = $@"select m.Id,m.WordId,w.Word,m.CommonMeaning,m.SpecialMeaning,w.ExamType
from EnglishWordMeaning m
join Englishword w on m.WordId=w.Id
where INSTR(w.Word,'{affixName}')=1 and LOWER(w.Word)<> '{affixName}' {wordCondition} {contentCondition}";
            }
            else
            {
                sql = $@"select m.Id,m.WordId,w.Word,m.CommonMeaning,m.SpecialMeaning,w.ExamType
from EnglishWordMeaning m
join Englishword w on m.WordId=w.Id
where w.Word like '%{affixName}' and LOWER(w.Word)<> '{affixName}' {wordCondition} {contentCondition}";
            }

            if (affix != null && affix.OnlyShowWithExamType)
            {
                sql += " and w.ExamType is not null";
            }

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Content", DbUtitlity.GetParameterValue(statistic.Content.Replace("...", "")));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordMeaning>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordForm>> GetVEnglishWordForms(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordForm where WordId={wordId} order by Priority";

                return await connection.QueryAsync<V_EnglishWordForm>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordForm>> GetVEnglishWordFormByTargetWordId(int targetWordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordForm where TargetWordId={targetWordId}";

                return await connection.QueryAsync<V_EnglishWordForm>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordStructureType>> GetEnglishWordStructureTypes()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = "select * from EnglishWordStructureType";

                return await connection.QueryAsync<EnglishWordStructureType>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordStructure>> GetVEnglishWordStructures(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from V_EnglishWordStructure where WordId={wordId} order by Priority";

                return await connection.QueryAsync<V_EnglishWordStructure>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordRoot>> GetEnglishWordRoots()
        {
            string sql = "select * from EnglishWordRoot order by Name";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordRoot>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWordByRootAffix(int id, EnglishWordElementType type)
        {
            string condition = "";

            if (type == EnglishWordElementType.Prefix)
            {
                condition = $"s.PrefixId={id}";
            }
            else if (type == EnglishWordElementType.Suffix)
            {
                condition = $"s.SuffixId={id}";
            }
            else if (type == EnglishWordElementType.WordRoot)
            {
                condition = $"s.RootId={id}";
            }

            string sql = $@"select v.Id,v.Word,v.CommonMeaning,v.SpecialMeaning 
from EnglishWordStructure s
join V_EnlishWordSimpleMeaning v on s.WordId=v.Id
where {condition}
group by v.Id,v.Word,v.CommonMeaning,v.SpecialMeaning";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordWithMeaning>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWordsByForm(string affix, EnglishWordElementType type, int limitCount = 0)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $@"select TargetWordId as Id, TargetWord as Word, CommonMeaning, SpecialMeaning, 
case when TargetExamType is null then 100000 else 
(
   case when TargetExamType &2=2 then 2 when TargetExamType &4=4 then 4 when TargetExamType &8=8 then 8 when TargetExamType &16=16 then 16 when TargetExamType &32=32 then 32 else TargetExamType end
) end as Priority 
from V_EnglishWordForm";

                string condition = "";

                if (type == EnglishWordElementType.Prefix)
                {
                    condition = $"'{affix}' || Word=TargetWord";
                }
                else if (type == EnglishWordElementType.Suffix)
                {
                    condition = $"Word || '{affix}'=TargetWord";
                }

                sql += $" where {condition}";

                sql = $@"select * from({sql}) order by Priority,Word";

                if (limitCount > 0)
                {
                    sql += $" limit {limitCount}";
                }

                return await connection.QueryAsync<V_EnglishWordWithMeaning>(sql);
            }
        }

        public static async Task<V_EnglishWordFormRule> GetVEnglishWordFormRule(int id)
        {
            string sql = $"select * from V_EnglishWordFormRule where Id={id}";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<V_EnglishWordFormRule>(sql)).FirstOrDefault();
            }
        }      
    }
}
