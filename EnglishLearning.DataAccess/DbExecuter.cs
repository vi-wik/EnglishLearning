using Dapper;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using System.Text;

namespace EnglishLearning.DataAccess
{
    public class DbExecuter
    {
        public static async Task<int> UpdateMediaSource(int id, string source)
        {
            string sql = $"update EnglishMedia set Source =@Source where Id={id}";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Source", source);

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql, parameters));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<int> KeepUserData(UserData userData, string dbFilePath = null)
        {
            using (var connection = DbUtitlity.CreateDbConnection(dbFilePath))
            {
                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                var existingFavoriteCategories = await DbObjectsFetcher.GetMediaFavoriteCategories(dbFilePath);

                StringBuilder sb = new StringBuilder("insert into MediaFavoriteCategory(Id,Name,Priority,CanDelete)values");

                bool needExectue = false;

                foreach (var category in userData.MediaFavoriteCategories.Where(item => !existingFavoriteCategories.Any(t => t.Id == item.Id)))
                {
                    sb.AppendLine($"({category.Id},'{category.Name}',{category.Priority},{(category.CanDelete ? "1" : "0")}),");

                    needExectue = true;
                }

                int affectedRows = 0;

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into MediaFavorite(Id,MediaId,CategoryId,CreateTime)values");

                foreach (var favorite in userData.MediaFavorites)
                {
                    sb.AppendLine($"({favorite.Id},{favorite.MediaId},{favorite.CategoryId},'{favorite.CreateTime}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into MediaAccessHistory(Id,MediaId,PositionTime,LastAccessTime)values");

                foreach (var history in userData.MediaAccessHistories)
                {
                    sb.AppendLine($"({history.Id},{history.MediaId},'{history.PositionTime}','{DateTimeHelper.GetStandardFormattedDateTimeString(history.LastAccessTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into EnglishWordVOCAB(Id,WordId,CreateTime)values");

                foreach (var vocab in userData.EnglishWordVOCABs)
                {
                    sb.AppendLine($"({vocab.Id},{DbUtitlity.GetHandledNullValue(vocab.WordId)},'{DateTimeHelper.GetStandardFormattedDateTimeString(vocab.CreateTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into EnglishPhraseVOCAB(Id,PhraseId,CreateTime)values");

                foreach (var vocab in userData.EnglishPhraseVOCABs)
                {
                    sb.AppendLine($"({vocab.Id},{DbUtitlity.GetHandledNullValue(vocab.PhraseId)},'{DateTimeHelper.GetStandardFormattedDateTimeString(vocab.CreateTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into EnglishWordLearnedHistory(Id,WordId,CreateTime)values");
                foreach (var wh in userData.WordLearnedHistories)
                {
                    sb.AppendLine($"({wh.Id},{wh.WordId},'{DateTimeHelper.GetStandardFormattedDateTimeString(wh.CreateTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into EnglishPhraseLearnedHistory(Id,PhraseId,CreateTime)values");
                foreach (var wh in userData.PhraseLearnedHistories)
                {
                    sb.AppendLine($"({wh.Id},{wh.PhraseId},'{DateTimeHelper.GetStandardFormattedDateTimeString(wh.CreateTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<int> RecordMediaAccessHistory(MediaAccessHistory mediaAccessHistory)
        {
            string now = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);
            string positionTime = mediaAccessHistory.PositionTime;

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"select * from MediaAccessHistory where MediaId={mediaAccessHistory.MediaId}";

                var record = (await connection.QueryAsync<MediaAccessHistory>(sql))?.FirstOrDefault();

                if (record != null)
                {
                    sql = $"update MediaAccessHistory set PositionTime='{positionTime}',LastAccessTime='{now}' where Id ={record.Id}";
                }
                else
                {
                    sql = $"insert into MediaAccessHistory(MediaId,PositionTime,LastAccessTime) values({mediaAccessHistory.MediaId},'{positionTime}','{now}')";
                }

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<int> ClearMediaAccessHistories()
        {
            return await ClearTableData("MediaAccessHistory");
        }

        public static async Task<int> ClearEnglishWordVOCABs()
        {
            return await ClearTableData("EnglishWordVOCAB");
        }

        public static async Task<int> ClearEnglishPhraseVOCABs()
        {
            return await ClearTableData("EnglishPhraseVOCAB");
        }

        public static async Task<int> DeleteMediaAccessHistoriesByMediaIds(List<int> mediaIds)
        {
            if (mediaIds == null || mediaIds.Count == 0)
            {
                return 0;
            }

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from MediaAccessHistory where MediaId in({string.Join(",", mediaIds)})";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<bool> AddEnglishWordVOCAB(int wordId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"insert into EnglishWordVOCAB(WordId,CreateTime) values({wordId}, '{DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now)}')";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> AddEnglishPhraseVOCAB(int phraseId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"insert into EnglishPhraseVOCAB(PhraseId,CreateTime) values({phraseId}, '{DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now)}')";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> DeleteEnglishWordVOCAB(int id)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from EnglishWordVOCAB where Id={id}";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> DeleteEnglishPhraseVOCAB(int id)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from EnglishPhraseVOCAB where Id={id}";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> AddMediaFavorite(int mediaId, int categoryId)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"insert into MediaFavorite(MediaId,CategoryId,CreateTime) values({mediaId},{categoryId}, '{DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now)}')";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> DeleteMediaFavorite(int id)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from MediaFavorite where Id={id}";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> AddMediaFavoriteCategory(MediaFavoriteCategory category)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = "select ifnull(max(Priority),0) as MaxPriority from MediaFavoriteCategory";

                int? maxPriority = (await connection.QueryAsync<int>(sql))?.FirstOrDefault();

                if (maxPriority.HasValue == false)
                {
                    maxPriority = 0;
                }

                sql = $"insert into MediaFavoriteCategory(Name,Priority)values(@Name,{(maxPriority + 1)})";

                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Name", category.Name);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql, para));

                await transaction.CommitAsync();

                return affectedRows > 0;
            }
        }


        public static async Task<int> DeleteMediaFavoriteCategoriesByIds(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return 0;
            }

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from MediaFavoriteCategory where Id in({string.Join(",", ids)})";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<bool> RenameMediaFavoriteCategory(int id, string name)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"update MediaFavoriteCategory set Name=@Name where Id={id}";

                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Name", name);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql, para));

                await transaction.CommitAsync();

                return affectedRows > 0;
            }
        }

        public static async Task<int> BatchInsertEnglishWordVOCAB(IEnumerable<int> wordIds)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string createTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                StringBuilder sb = new StringBuilder("insert into EnglishWordVOCAB(WordId,CreateTime)values");

                foreach (var wordId in wordIds)
                {
                    sb.AppendLine($"({wordId},'{createTime}'),");
                }

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<int> BatchInsertEnglishPhraseVOCAB(IEnumerable<int> phraseIds)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string createTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                StringBuilder sb = new StringBuilder("insert into EnglishPhraseVOCAB(PhraseId,CreateTime)values");

                foreach (var phraseId in phraseIds)
                {
                    sb.AppendLine($"({phraseId},'{createTime}'),");
                }

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<bool> SaveEnglishWordLearnedHistory(V_EnglishWord word)
        {
            int wordId = word.Id;          

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string dateTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();
    
                string sql = $"insert into EnglishWordLearnedHistory(WordId,CreateTime) values({wordId},'{dateTime}')";               

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows ==1;
            }
        }

        public static async Task<bool> SaveEnglishPhraseLearnedHistory(V_EnglishPhrase phrase)
        {
            int phraseId = phrase.Id;           

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string dateTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                string sql = $"insert into EnglishPhraseLearnedHistory(PhraseId,CreateTime) values({phraseId},'{dateTime}')";

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<int> ClearEnglishWordLearnedHistories(List<int?> examTypeIds = null)
        {
            if (examTypeIds == null)
            {
                return await ClearTableData("EnglishWordLearnedHistory");
            }
            else
            {
                using (var connection = DbUtitlity.CreateDbConnection())
                {
                    var exIds = examTypeIds.Where(item => item > 0);
                    IEnumerable<int> examTypeWeights = Enumerable.Empty<int>();

                    if (exIds.Any())
                    {
                        string strIds = string.Join(",", exIds);
                        examTypeWeights = await connection.QueryAsync<int>($"select Weight from EnglishExamType where Id in({strIds})");
                    }

                    string condition = "";

                    if (examTypeWeights.Any())
                    {
                        condition += string.Join(" or ", examTypeWeights.Select(item => $"ExamType&{item}={item}"));
                    }

                    if (examTypeIds.Any(item => item == null))
                    {
                        condition += $" {(examTypeWeights.Any() ? "or " : "")}ExamType is null";
                    }

                    string sql = $"delete from EnglishWordLearnedHistory where WordId in(select Id from EnglishWord where {condition})";

                    await connection.OpenAsync();

                    var transaction = await connection.BeginTransactionAsync();

                    int affectedRows = (await connection.ExecuteAsync(sql));

                    await transaction.CommitAsync();

                    return affectedRows;
                }
            }
        }

        public static async Task<int> ClearEnglishPhraseLearnedHistories()
        {
            return await ClearTableData("EnglishPhraseLearnedHistory");
        }

        private static async Task<int> ClearTableData(string tableName)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from {tableName}";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }
    }
}
