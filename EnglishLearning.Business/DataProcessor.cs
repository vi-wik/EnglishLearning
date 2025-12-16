using EnglishLearning.Business.Manager;
using EnglishLearning.DataAccess;
using EnglishLearning.Model;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EnglishLearning.Business
{
    public class DataProcessor
    {
        static DataProcessor()
        {
            DbUtitlity.DataFilePath = DataFileManager.DataFilePath;
        }

        public static async Task<EnglishSubject> GetEnglishSubjectByEnName(string enName)
        {
            return await DbObjectsFetcher.GetEnglishSubjectByEnName(enName);
        }

        public static async Task<V_EnglishWord> GetVEnglishWord(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWord(wordId);
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWords(EnglishWordFilter filter = null)
        {
            return await DbObjectsFetcher.GetEnglishWords(filter);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeanings(string keyword)
        {
            return await DbObjectsFetcher.GetEnglishWordMeanings(keyword);
        }

        public static async Task<IEnumerable<EnglishWordMeaning>> GetEnglishWordMeanings(int wordId, EnglishWordMeaningFilter filter = null)
        {
            return await DbObjectsFetcher.GetEnglishWordMeanings(wordId, filter);
        }

        public static async Task<IEnumerable<V_EnglishWordVOCAB>> GetEnglishWordVOCABSuggestions(string keyword)
        {
            return await DbObjectsFetcher.GetEnglishWordVOCABSuggestions(keyword);
        }

        public static async Task<IEnumerable<V_EnglishPhraseVOCAB>> GetEnglishPhraseVOCABSuggestions(string keyword)
        {
            return await DbObjectsFetcher.GetEnglishPhraseVOCABSuggestions(keyword);
        }

        public static async Task<IEnumerable<V_EnglishSubjectMedia>> GetVEnglishSubjectMedias(int subjectId)
        {
            return await DbObjectsFetcher.GetVEnglishSubjectMedias(subjectId);
        }

        public static async Task<IEnumerable<V_EnglishWordMedia>> GetVEnglishWordMedias(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordMedias(wordId);
        }

        public static async Task<EnglishSubject> GetEnglishSubject(int subjectId)
        {
            return await DbObjectsFetcher.GetEnglishSubject(subjectId);
        }

        public static async Task<EnglishMediaExtraInfo> GetEnglishMediaExtraInfo(int mediaId)
        {
            return await DbObjectsFetcher.GetEnglishMediaExtraInfo(mediaId);
        }

        public static async Task<int> UpdateMediaSource(int id, string source)
        {
            return await DbExecuter.UpdateMediaSource(id, source);
        }

        public static async Task<IEnumerable<EnglishTopic>> GetEnglishTopics(int subjectId)
        {
            return await DbObjectsFetcher.GetEnglishTopics(subjectId);
        }

        public static async Task<IEnumerable<EnglishTopicDetail>> GetEnglishTopicDetails(int topicId, string keyword = null)
        {
            return await DbObjectsFetcher.GetEnglishTopicDetails(topicId, keyword);
        }

        public static async Task<IEnumerable<V_EnglishTopicDetailMedia>> GetVEnglishTopicDetailMedias(int topicId, string keyword = null)
        {
            return await DbObjectsFetcher.GetVEnglishTopicDetailMedias(topicId, keyword);
        }

        public static async Task<IEnumerable<V_EnglishWordMediaPlayTime>> GetVEnglishWordMediaPlayTimes(int wordMediaId)
        {
            return await DbObjectsFetcher.GetVEnglishWordMediaPlayTimes(wordMediaId);
        }

        public static async Task<IEnumerable<V_EnglishConsonantMediaPlayTime>> GetVEnglishConsonantMediaPlayTimes(int consonantMediaId)
        {
            return await DbObjectsFetcher.GetVEnglishConsonantMediaPlayTimes(consonantMediaId);
        }

        public static async Task<IEnumerable<V_EnglishVowelMediaPlayTime>> GetVEnglishVowelMediaPlayTimes(int vowelMediaId)
        {
            return await DbObjectsFetcher.GetVEnglishVowelMediaPlayTimes(vowelMediaId);
        }

        public static async Task<IEnumerable<V_EnglishConsonantMedia>> GetVEnglishConsonantMedias(int consonantId)
        {
            return await DbObjectsFetcher.GetVEnglishConsonantMedias(consonantId);
        }

        public static async Task<IEnumerable<V_EnglishVowelMedia>> GetVEnglishVowelMedias(int vowelId)
        {
            return await DbObjectsFetcher.GetVEnglishVowelMedias(vowelId);
        }

        public static async Task<IEnumerable<EnglishConsonant>> GetEnglishConsonants()
        {
            return await DbObjectsFetcher.GetEnglishConsonants();
        }

        public static async Task<EnglishConsonant> GetEnglishConsonant(int consonantId)
        {
            return await DbObjectsFetcher.GetEnglishConsonant(consonantId);
        }

        public static async Task<IEnumerable<EnglishVowel>> GetEnglishVowels()
        {
            return await DbObjectsFetcher.GetEnglishVowels();
        }

        public static async Task<EnglishVowel> GetEnglishVowel(int vowelId)
        {
            return await DbObjectsFetcher.GetEnglishVowel(vowelId);
        }

        public static async Task<IEnumerable<V_EnglishPhraseMedia>> GetVEnglishPhraseMedias(int phraseId)
        {
            return await DbObjectsFetcher.GetVEnglishPhraseMedias(phraseId);
        }

        public static async Task<IEnumerable<V_EnglishPhraseMediaPlayTime>> GetVEnglishPhraseMediaPlayTimes(int phraseMediaId)
        {
            return await DbObjectsFetcher.GetVEnglishPhraseMediaPlayTimes(phraseMediaId);
        }

        public static async Task<V_EnglishPhrase> GetVEnglishPhrase(int phraseId)
        {
            return await DbObjectsFetcher.GetVEnglishPhrase(phraseId);
        }

        public static async Task<IEnumerable<EnglishPhrase>> GetEnglishPhrases(EnglishWordFilter filter = null, bool isByMeaning = false)
        {
            return await DbObjectsFetcher.GetEnglishPhrases(filter, isByMeaning);
        }

        public static async Task<IEnumerable<V_EnglishTopicDetailMediaPlayTime>> GetVEnglishTopicDetailMediaPlayTimes(int topicDetailMediaId)
        {
            return await DbObjectsFetcher.GetVEnglishTopicDetailMediaPlayTimes(topicDetailMediaId);
        }

        public static async Task<IEnumerable<V_MediaAccessHistory>> GetVMediaAccessHistories()
        {
            return await DbObjectsFetcher.GetVMediaAccessHistories();
        }

        public static async Task<int> RecordMediaAccessHistory(MediaAccessHistory mediaAccessHistory)
        {
            return await DbExecuter.RecordMediaAccessHistory(mediaAccessHistory);
        }

        public static async Task<int> ClearMediaAccessHistories()
        {
            return await DbExecuter.ClearMediaAccessHistories();
        }

        public static async Task<int> DeleteMediaAccessHistoriesByMediaIds(List<int> mediaIds)
        {
            return await DbExecuter.DeleteMediaAccessHistoriesByMediaIds(mediaIds);
        }

        public static async Task<EnglishWordVOCAB> GetEnglishWordVOCAB(int wordId)
        {
            return await DbObjectsFetcher.GetEnglishWordVOCAB(wordId);
        }

        public static async Task<EnglishPhraseVOCAB> GetEnglishPhraseVOCAB(int phraseId)
        {
            return await DbObjectsFetcher.GetEnglishPhraseVOCAB(phraseId);
        }

        public static async Task<bool> AddEnglishWordVOCAB(int wordId)
        {
            return await DbExecuter.AddEnglishWordVOCAB(wordId);
        }

        public static async Task<bool> DeleteEnglishWordVOCAB(int id)
        {
            return await DbExecuter.DeleteEnglishWordVOCAB(id);
        }

        public static async Task<bool> AddEnglishPhraseVOCAB(int phraseId)
        {
            return await DbExecuter.AddEnglishPhraseVOCAB(phraseId);
        }

        public static async Task<bool> DeleteEnglishPhraseVOCAB(int id)
        {
            return await DbExecuter.DeleteEnglishPhraseVOCAB(id);
        }

        public static async Task<IEnumerable<V_EnglishWordVOCAB>> GetVEnglishWordVOCABs(EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {
            return await DbObjectsFetcher.GetVEnglishWordVOCABs(filter, sortInfo);
        }

        public static async Task<IEnumerable<V_EnglishPhraseVOCAB>> GetVEnglishPhraseVOCABs(EnglishWordFilter filter = null, DataSortInfo sortInfo = null)
        {
            return await DbObjectsFetcher.GetVEnglishPhraseVOCABs(filter, sortInfo);
        }

        public static async Task<bool> IsEnglishWordVOCAB(int id)
        {
            return await DbObjectsFetcher.IsEnglishWordVOCAB(id);
        }

        public static async Task<bool> IsEnglishPhraseVOCAB(int id)
        {
            return await DbObjectsFetcher.IsEnglishPhraseVOCAB(id);
        }

        public static async Task<int> GetEnglishWordVOCABCount()
        {
            return await DbObjectsFetcher.GetEnglishWordVOCABCount();
        }

        public static async Task<int> GetEnglishPhraseVOCABCount()
        {
            return await DbObjectsFetcher.GetEnglishPhraseVOCABCount();
        }

        public static async Task<MediaFavorite> GetMediaFavoriteByMediaId(int mediaId)
        {
            return await DbObjectsFetcher.GetMediaFavoriteByMediaId(mediaId);
        }

        public static async Task<bool> AddMediaFavorite(int mediaId, int categoryId)
        {
            return await DbExecuter.AddMediaFavorite(mediaId, categoryId);
        }

        public static async Task<bool> DeleteMediaFavorite(int id)
        {
            return await DbExecuter.DeleteMediaFavorite(id);
        }

        public static async Task<IEnumerable<V_MediaFavorite>> GetVMediaFavorites()
        {
            return await DbObjectsFetcher.GetVMediaFavorites();
        }

        public static async Task<IEnumerable<MediaFavoriteCategory>> GetMediaFavoriteCategories()
        {
            return await DbObjectsFetcher.GetMediaFavoriteCategories();
        }

        public static async Task<bool> IsMediaFavoriteCategoryBeRefered(List<int> ids)
        {
            return await DbObjectsFetcher.IsMediaFavoriteCategoryBeRefering(ids);
        }

        public static async Task<bool> AddMediaFavoriteCategory(MediaFavoriteCategory category)
        {
            return await DbExecuter.AddMediaFavoriteCategory(category);
        }

        public static async Task<int> DeleteMediaFavoriteCategoriesByIds(List<int> ids)
        {
            return await DbExecuter.DeleteMediaFavoriteCategoriesByIds(ids);
        }

        public static async Task<bool> IsMediaFavoriteCategoryNameExisting(bool isAdd, string name, int? id)
        {
            return await DbObjectsFetcher.IsMediaFavoriteCategoryNameExisting(isAdd, name, id);
        }

        public static async Task<bool> RenameMediaFavoriteCategory(int id, string name)
        {
            return await DbExecuter.RenameMediaFavoriteCategory(id, name);
        }

        public static async Task<int> GetMediaFavoriteCategoriesCount()
        {
            return await DbObjectsFetcher.GetMediaFavoriteCategoriesCount();
        }

        public static async Task<IEnumerable<int>> GetEnglishWordIdsByWords(IEnumerable<string> words)
        {
            return await DbObjectsFetcher.GetEnglishWordIdsByWords(words);
        }

        public static async Task<IEnumerable<int>> GetEnglishPhraseIdsByPhrases(IEnumerable<string> phrases)
        {
            return await DbObjectsFetcher.GetEnglishPhraseIdsByPhrases(phrases);
        }

        public static async Task<IEnumerable<int>> GetExistingWordIdsOfEnglishWordVOCAB(IEnumerable<int> ids)
        {
            return await DbObjectsFetcher.GetExistingWordIdsOfEnglishWordVOCAB(ids);
        }

        public static async Task<IEnumerable<int>> GetExistingPhraseIdsOfEnglishPhraseVOCAB(IEnumerable<int> ids)
        {
            return await DbObjectsFetcher.GetExistingPhraseIdsOfEnglishPhraseVOCAB(ids);
        }

        public static async Task<int> BatchInsertEnglishWordVOCAB(IEnumerable<int> wordIds)
        {
            return await DbExecuter.BatchInsertEnglishWordVOCAB(wordIds);
        }

        public static async Task<int> BatchInsertEnglishPhraseVOCAB(IEnumerable<int> phraseIds)
        {
            return await DbExecuter.BatchInsertEnglishPhraseVOCAB(phraseIds);
        }

        public static async Task<int> ClearEnglishWordVOCABs()
        {
            return await DbExecuter.ClearEnglishWordVOCABs();
        }

        public static async Task<int> ClearEnglishPhraseVOCABs()
        {
            return await DbExecuter.ClearEnglishPhraseVOCABs();
        }

        public static async Task<IEnumerable<EnglishExamType>> GetEnglishExamTypes()
        {
            return await DbObjectsFetcher.GetEnglishExamTypes();
        }

        public static async Task<int?> GetEnglishWordNotLearnedNextId(EnglishExamType examType, bool isForNonExamType = false, bool isForVOCAB = false,
                                                                     EnglishVOCABLearnSortMode sortMode = EnglishVOCABLearnSortMode.AlphabetAsc)
        {
            return await DbObjectsFetcher.GetEnglishWordNotLearnedNextId(examType, isForNonExamType, isForVOCAB, sortMode);
        }

        public static async Task<bool> SaveEnglishWordLearnedHistory(V_EnglishWord word)
        {
            return await DbExecuter.SaveEnglishWordLearnedHistory(word);
        }

        public static async Task<IEnumerable<EnglishExamTypeWordLearnedStatisticInfo>> GetEnglishExamTypeWordLearnedStatistics()
        {
            return await DbObjectsFetcher.GetEnglishExamTypeWordLearnedStatistics();
        }

        public static async Task<IEnumerable<EnglishExamTypeWordLearnedStatisticInfo>> GetEnglishWordVOCABLearnedStatistics()
        {
            return await DbObjectsFetcher.GetEnglishWordVOCABLearnedStatistics();
        }

        public static async Task<int> ClearEnglishWordLearnedHistories(List<int?> examTypeIds = null)
        {
            return await DbExecuter.ClearEnglishWordLearnedHistories(examTypeIds);
        }

        public static async Task<int?> GetEnglishWordLearnedPreviousWordId(int? examTypeId, int wordId, bool isForNonExamType = false, bool isForVOCAB = false)
        {
            return await DbObjectsFetcher.GetEnglishWordLearnedPreviousWordId(examTypeId, wordId, isForNonExamType, isForVOCAB);
        }

        public static async Task<int?> GetEnglishPhraseNotLearnedNextId(bool isForVOCAB = false, EnglishVOCABLearnSortMode sortMode = EnglishVOCABLearnSortMode.AlphabetAsc)
        {
            return await DbObjectsFetcher.GetEnglishPhraseNotLearnedNextId(isForVOCAB, sortMode);
        }

        public static async Task<int?> GetEnglishPhraseLearnedPreviousPhraseId(int phraseId, bool isForVOCAB = false)
        {
            return await DbObjectsFetcher.GetEnglishPhraseLearnedPreviousPhraseId(phraseId, isForVOCAB);
        }

        public static async Task<bool> SaveEnglishPhraseLearnedHistory(V_EnglishPhrase phrase)
        {
            return await DbExecuter.SaveEnglishPhraseLearnedHistory(phrase);
        }

        public static async Task<int> ClearEnglishPhraseLearnedHistories()
        {
            return await DbExecuter.ClearEnglishPhraseLearnedHistories();
        }

        public static async Task<IEnumerable<V_EnglishWordExample>> GetVEnglishWordExamples(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordExamples(wordId);
        }

        public static async Task<IEnumerable<V_EnglishPhraseExample>> GetVEnglishPhraseExamples(int phraseId)
        {
            return await DbObjectsFetcher.GetVEnglishPhraseExamples(phraseId);
        }

        public static async Task<bool> HasUserDataTable(string dbFilePath)
        {
            return await DbObjectsFetcher.HasUserDataTable(dbFilePath, "MediaAccessHistory");
        }

        public static async Task<UserData> GetUserData(string dbFilePath)
        {
            UserData userData = new UserData();

            userData.MediaFavoriteCategories = await DbObjectsFetcher.GetMediaFavoriteCategories(dbFilePath);
            userData.MediaFavorites = await DbObjectsFetcher.GetMediaFavorites(dbFilePath);
            userData.MediaAccessHistories = await DbObjectsFetcher.GetMediaAccessHistories(dbFilePath);
            userData.EnglishWordVOCABs = await DbObjectsFetcher.GetEnglishWordVOCABs(dbFilePath);
            userData.EnglishPhraseVOCABs = await DbObjectsFetcher.GetEnglishPhraseVOCABs(dbFilePath);
            userData.WordLearnedHistories = await DbObjectsFetcher.GetEnglishWordLearnHistories(dbFilePath);
            userData.PhraseLearnedHistories = await DbObjectsFetcher.GetEnglishPhraseLearnHistories(dbFilePath);

            return userData;
        }

        public static async Task<int> ImportUserData(string sourceDbFilePath)
        {
            string targetDbFilePath = DataFileManager.DataFilePath;

            if (!File.Exists(targetDbFilePath))
            {
                return 0;
            }

            if (await DataProcessor.HasUserDataTable(targetDbFilePath))
            {
                UserData userData = await GetUserData(sourceDbFilePath);

                ClearSqliteAllPools();

                return await DbExecuter.KeepUserData(userData, targetDbFilePath);
            }

            return 0;
        }

        public static void ClearSqliteAllPools()
        {
            SqliteConnection.ClearAllPools();
        }

        public static async Task<IEnumerable<V_EnglishWordInflection>> GetVEnglishWordInflections(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordInflections(wordId);
        }

        public static async Task<IEnumerable<EnglishWordSyllable>> GetEnglishWordSyllables(int wordId)
        {
            return await DbObjectsFetcher.GetEnglishWordSyllables(wordId);
        }

        public static async Task<IEnumerable<EnglishWordInflectionType>> GetEnglishWordInflectionTypes()
        {
            return await DbObjectsFetcher.GetEnglishWordInflectionTypes();
        }

        public static async Task<IEnumerable<string>> GetEnglishPhraseAlphabets()
        {
            return await DbObjectsFetcher.GetEnglishPhraseAlphabets();
        }

        public static async Task<IEnumerable<EnglishPhrase>> GetEnglishPhrasesByAlphabet(string alphabet)
        {
            return await DbObjectsFetcher.GetEnglishPhrasesByAlphabet(alphabet);
        }

        public static async Task<IEnumerable<V_EnglishWordInflection>> GetVEnglishWordInflectionsByTargetWordId(int targetWordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordInflectionsByTargetWordId(targetWordId);
        }

        public static async Task<IEnumerable<V_EnglishWordVariant>> GetVEnglishWordVariants(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordVariants(wordId);
        }

        public static async Task<EnglishWordMeaningSpecialStatistic> GetEnglishWordMeaningSpecialStatistic(int wordId)
        {
            return await DbObjectsFetcher.GetEnglishWordMeaningSpecialStatistic(wordId);
        }

        public static async Task<IEnumerable<EnglishWordPrefix>> GetEnglishWordPrefixes(string keyword, bool excludeHidden = false)
        {
            return await DbObjectsFetcher.GetEnglishWordPrefixes(keyword, excludeHidden);
        }

        public static async Task<IEnumerable<EnglishWordSuffix>> GetEnglishWordSuffixes(string keyword, bool excludeHidden = false)
        {
            return await DbObjectsFetcher.GetEnglishWordSuffixes(keyword, excludeHidden);
        }

        public static async Task<IEnumerable<EnglishWordElement>> GetEnglishWordRoots(string keyword)
        {
            return await DbObjectsFetcher.GetEnglishWordRoots(keyword);
        }

        public static async Task<IEnumerable<EnglishWordPrefixStatistic>> GetEnglishWordPrefixStatisticsByAffixName(string affixName)
        {
            return await DbObjectsFetcher.GetEnglishWordPrefixStatisticsByAffixName(affixName);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningByPrefixDetail(EnglishWordAffixStatistic detail, string affixName)
        {
            return await DbObjectsFetcher.GetEnglishWordMeaningByPrefixStatistic(detail, affixName);
        }

        public static async Task<EnglishWordPrefix> GetEnglishWordPrefixByName(string name)
        {
            return await DbObjectsFetcher.GetEnglishWordPrefixByName(name);
        }

        public static async Task<EnglishWordSuffix> GetEnglishWordSuffixByName(string name)
        {
            return await DbObjectsFetcher.GetEnglishWordSuffixByName(name);
        }

        public static async Task<IEnumerable<EnglishWordSuffixStatistic>> GetEnglishWordSuffixStatisticsByAffixName(string affixName)
        {
            return await DbObjectsFetcher.GetEnglishWordSuffixStatisticsByAffixName(affixName);
        }

        public static async Task<IEnumerable<V_EnglishWordMeaning>> GetEnglishWordMeaningBySuffixDetail(EnglishWordAffixStatistic detail, string affixName)
        {
            return await DbObjectsFetcher.GetEnglishWordMeaningBySuffixStatistic(detail, affixName);
        }

        public static async Task<IEnumerable<V_EnglishWordForm>> GetVEnglishWordForms(int wordId)
        {
            return await DbObjectsFetcher.GetVEnglishWordForms(wordId);
        }

        public static async Task<IEnumerable<V_EnglishWordRootMeaning>> GetVEnglishWordRootMeanings(string keyword)
        {
            return await DbObjectsFetcher.GetVEnglishWordRootMeanings(keyword);
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWordByRootAffix(int id, EnglishWordElementType type)
        {
            return await DbObjectsFetcher.GetEnglishWordByRootAffix(id, type);
        }

        public static async Task<IEnumerable<V_EnglishWordWithMeaning>> GetEnglishWordsByForm(string affix, EnglishWordElementType type, int limitCount = 0)
        {
            return await DbObjectsFetcher.GetEnglishWordsByForm(affix, type, limitCount);
        }

        public static async Task<Dictionary<int, IEnumerable<V_EnglishWordStructure>>> GetEnglishWordStructures(int wordId)
        {
            Dictionary<int, IEnumerable<V_EnglishWordStructure>> dict = new Dictionary<int, IEnumerable<V_EnglishWordStructure>>();

            var structures = await DbObjectsFetcher.GetVEnglishWordStructures(wordId);

            if (structures.Count() == 0)
            {
                var forms = await DbObjectsFetcher.GetVEnglishWordFormByTargetWordId(wordId);

                int count = 0;

                Action<List<V_EnglishWordStructure>, int?, string> insertStructure = (list, prefixId, prefix) =>
                {
                    list.Insert(0, new V_EnglishWordStructure() { PrefixId = prefixId, Prefix = prefix, Priority = 1 });
                    list[1].Priority = 2;
                };

                Action<List<V_EnglishWordStructure>, int?, string> appendStructure = async (list, suffixId, suffix) =>
                {
                    if (!suffixId.HasValue)
                    {
                        var result = await DbObjectsFetcher.GetEnglishWordSuffixByName(suffix);

                        if (result != null)
                        {
                            suffixId = result.Id;
                        }
                    }

                    list.Add(new V_EnglishWordStructure() { SuffixId = suffixId, Suffix = suffix, Priority = 2 });
                    list[0].Priority = 1;
                };

                foreach (var form in forms)
                {
                    count++;

                    List<V_EnglishWordStructure> list = new List<V_EnglishWordStructure>();

                    if (form.RuleId > 0)
                    {
                        list.Add(new V_EnglishWordStructure() { SubWordId = form.WordId, SubWord = form.Word });

                        var rule = await DbObjectsFetcher.GetVEnglishWordFormRule(form.RuleId.Value);

                        if (rule.Prefix != null)
                        {
                            insertStructure(list, rule.PrefixId, rule.Prefix);
                        }
                        else if (rule.Suffix != null)
                        {
                            appendStructure(list, rule.SuffixId, rule.Suffix);
                        }
                        else if (rule.IsInsertBefore)
                        {
                            insertStructure(list, null, rule.InsertBeforeContent);
                        }
                        else if (rule.IsAppend)
                        {
                            appendStructure(list, null, rule.AppendContent);
                        }
                        else if (rule.IsChangeEnd)
                        {
                            appendStructure(list, null, rule.ChangeNewContent);
                            list[1].ChangeEndOldContent = rule.ChangeOldContent;
                        }

                        dict.Add(count, list);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < form.Word.Length; i++)
                        {
                            if (i < form.TargetWord.Length && form.Word[i] == form.TargetWord[i])
                            {
                                sb.Append(form.Word[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (sb.Length > 0 && sb.Length < form.TargetWord.Length)
                        {
                            list.Add(new V_EnglishWordStructure() { SubWordId = form.WordId, SubWord = form.Word });

                            appendStructure(list, null, form.TargetWord.Substring(sb.Length));
                            list[1].ChangeEndOldContent = form.Word.Substring(sb.Length);

                            dict.Add(1, list);
                        }
                    }
                }
            }
            else
            {
                dict.Add(1, structures);
            }

            return dict;
        }
    }
}
