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

            string columns = "Id,Word";

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
            string fields = "Id,WordId,Word,ExamType,PartOfSpeechId,PartOfSpeech,CommonMeaning,IsFromWeb,IsOld, Priority";

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
            string sql = "select * from EnglishVowel order by Priority";

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

        public static async Task<IEnumerable<EnglishVowel>> GetEnglishVowelsHaveMedias()
        {
            string sql = $@"select v.Id,v.Vowel,v.USVowel,v.UKVowel,v.Description,v.Priority from EnglishVowel v 
                            join EnglishVowelMedia m on v.Id=m.VowelId 
                            group by v.Id,v.Vowel,v.USVowel,v.UKVowel,v.Description,v.Priority
                            having count(1)>0
                            order by v.Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<EnglishVowel>(sql));
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

        public static async Task<IEnumerable<VOCAB>> GetVOCABs(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = "SELECT * from VOCAB";

                return await connection.QueryAsync<VOCAB>(sql);
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

        public static async Task<VOCAB> GetVOCAB(EnglishObjectType objectType, int objectId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"SELECT * from VOCAB where {(objectType == EnglishObjectType.Word ? "WordId" : "PhraseId")}={objectId}";

                return (await connection.QueryAsync<VOCAB>(sql))?.FirstOrDefault();
            }
        }

        public static async Task<IEnumerable<V_VOCAB>> GetVVOCABs(EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string keyword = filter?.Keyword;
                bool fullMatch = filter?.FullMatch ?? false;
                bool needMeaning = filter?.NeedMeaning ?? false;

                string tableName = needMeaning ? "V_VOCABWithMeaning" : "V_VOCAB";

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

                Dictionary<string, object> para = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(keyword))
                {
                    para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));
                }

                return await connection.QueryAsync<V_VOCAB>(sql, para);
            }
        }

        public static async Task<IEnumerable<V_VOCAB>> GetVOCABSuggestions(string keyword)
        {
            string sql = "select * from V_VOCAB where INSTR(LOWER(Name),LOWER(@Keyword))=1 order by Name limit 50";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_VOCAB>(sql, para);
            }
        }

        public static async Task<int> GetVOCABCount()
        {
            string sql = "select count(1) from VOCAB";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return (await connection.QueryAsync<int>(sql))?.FirstOrDefault() ?? 0;
            }
        }

        public static async Task<bool> IsVOCAB(int id)
        {
            string sql = $"select 1 from V_VOCAB where Id={id}";

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

        public static async Task<IEnumerable<int>> GetExistingWordIdsOrPhraseIdsOfVOCAB(EnglishObjectType objectType, IEnumerable<int> ids)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string strIds = string.Join(",", ids);

                string fieldName = objectType == EnglishObjectType.Word ? "WordId" : "PhraseId";

                string sql = $"select {fieldName} from VOCAB where {fieldName} in({strIds})";

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

        public static async Task<int> GetEnglishWordLearnNextId(EnglishExamType examType)
        {
            int weight = examType.Weight;
            int examTypeId = examType.Id;

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $@"select w.Id from EnglishWord w
                             where ExamType & {examType.Weight} = {weight} 
                             and Id not in(select WordId from EnglishWordLearnHistory where ExamTypeId={examTypeId})
                             order by w.Id
                             limit 1";

                return (await connection.QueryAsync<int>(sql))?.FirstOrDefault() ?? 0;
            }
        }

        public static async Task<IEnumerable<EnglishExamStatisticInfo>> GetEnglishExamStatistics()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = @"select et.Id,et.Name,Count(1) as Total,Count(h.WordId) as LearnedCount
                                from EnglishExamType et
                                left join EnglishWord w on w.ExamType is not null and (et.Weight & w.ExamType=et.Weight)
                                left join EnglishWordLearnHistory h on h.ExamTypeId=et.Id and h.WordId=w.Id
                                group by et.Id,et.Name";

                return await connection.QueryAsync<EnglishExamStatisticInfo>(sql);
            }
        }

        public static async Task<IEnumerable<EnglishWordLearnHistory>> GetEnglishWordLearnHistories(string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                string sql = $"select * from EnglishWordLearnHistory";

                return await connection.QueryAsync<EnglishWordLearnHistory>(sql);
            }
        }

        public static async Task<int?> GetPreviousEnglishLearnedWordId(int examTypeId, int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $@"select WordId from EnglishWordLearnHistory
                                where ExamTypeId={examTypeId} and  CreateTime<IFNULL((select CreateTime from EnglishWordLearnHistory where ExamTypeId={examTypeId} and WordId={wordId}),DATETIME('NOW','LOCALTIME'))
                                order by CreateTime desc
                                limit 1";

                return (await connection.QueryAsync<int?>(sql))?.FirstOrDefault();
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

        public static async Task<IEnumerable<EnglishWordPrefix>> GetEnglishWordPrefixSuggestions(string keyword)
        {
            string sql = "select * from EnglishWordPrefix where Hidden=0 and INSTR(LOWER(Name),LOWER(@Keyword))=1 order by Name";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefix>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordSuffix>> GetEnglishWordSuffixSuggestions(string keyword)
        {
            string sql = "select * from EnglishWordSuffix where Hidden=0 and INSTR(LOWER(Name),LOWER(@Keyword))=1 order by Name";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Keyword", DbUtitlity.GetParameterValue(keyword));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordSuffix>(sql, para);
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

        public static async Task<IEnumerable<EnglishWordPrefixDetail>> GetEnglishWordPrefixDetailsByAffixName(string affixName)
        {
            string sql = "select d.* from EnglishWordPrefixDetail d join EnglishWordPrefix p on p.Id=d.AffixId where LOWER(p.Name)=LOWER(@Name) order by Priority";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(affixName));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefixDetail>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordSuffixDetail>> GetEnglishWordSuffixDetailsByAffixName(string affixName)
        {
            string sql = "select d.* from EnglishWordSuffixDetail d join EnglishWordSuffix s on s.Id=d.AffixId where LOWER(s.Name)=LOWER(@Name) order by Priority";

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Name", DbUtitlity.GetParameterValue(affixName));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordSuffixDetail>(sql, para);
            }
        }

        public static async Task<IEnumerable<EnglishWordPrefixDetail>> GetEnglishWordPrefixDetailsByAffixId(int affixId)
        {
            string sql = $"select d.* from EnglishWordPrefixDetail d join EnglishWordPrefix p on p.Id=d.AffixId where p.Id={affixId} order by Priority";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<EnglishWordPrefixDetail>(sql);
            }
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningByPrefixDetail(EnglishWordAffixDetail detail, string affixName)
        {
            return await GetEnglishWordMeaningByAffixDetail(detail, affixName, true);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningBySuffixDetail(EnglishWordAffixDetail detail, string affixName)
        {
            return await GetEnglishWordMeaningByAffixDetail(detail, affixName, false);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningByAffixDetail(EnglishWordAffixDetail detail, string affixName, bool isPrefix)
        {
            bool isOthers = detail.Id < 0;

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
                if(!detail.Content.Contains("...") || detail.Content.StartsWith("...") || detail.Content.EndsWith("..."))
                {
                    contentCondition = "and (instr(m.CommonMeaning,@Content)>0 or instr(m.SpecialMeaning,@Content)>0)";
                }
                else
                {
                    string likeContent = detail.Content.Replace("...", "%");

                    contentCondition = $"and (m.CommonMeaning like '%{likeContent}%' or ifnull(m.SpecialMeaning,'') like '%{likeContent}%')";
                }
            }
            else
            {
                IEnumerable<EnglishWordAffixDetail> details = null;

                if (isPrefix)
                {
                    details = await GetEnglishWordPrefixDetailsByAffixName(affixName);
                }
                else
                {
                    details = await GetEnglishWordSuffixDetailsByAffixName(affixName);
                }

                StringBuilder sb = new StringBuilder();

                foreach (var d in details)
                {
                    string content = d.Content;

                    if(!content.Contains("...") || content.StartsWith("...") || content.EndsWith("..."))
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

            if (detail.ExcludeContent != null)
            {
                var items = detail.ExcludeContent.Split(',', '，');

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
                    if(isPrefix)
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

            if(affix!=null && affix.OnlyShowWithExamType)
            {
                sql += " and w.ExamType is not null";
            }

            Dictionary<string, object> para = new Dictionary<string, object>();

            para.Add("@Content", DbUtitlity.GetParameterValue(detail.Content.Replace("...","")));

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                return await connection.QueryAsync<V_EnglishWordMeaning>(sql, para);
            }
        }
    }
}
