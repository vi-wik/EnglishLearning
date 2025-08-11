using Dapper;
using EnglishLearning.Model;
using EnglishLearning.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                sb = new StringBuilder("insert into VOCAB(Id,WordId,PhraseId,CreateTime)values");

                foreach (var vocab in userData.VOCABs)
                {
                    sb.AppendLine($"({vocab.Id},{DbUtitlity.GetHandledNullValue(vocab.WordId)},{DbUtitlity.GetHandledNullValue(vocab.PhraseId)},'{DateTimeHelper.GetStandardFormattedDateTimeString(vocab.CreateTime)}'),");
                    needExectue = true;
                }

                if (needExectue)
                {
                    affectedRows += (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));
                }

                needExectue = false;
                sb = new StringBuilder("insert into EnglishWordLearnHistory(Id,ExamTypeId,WordId,CreateTime)values");
                foreach (var wh in userData.WordLearnHistories)
                {
                    sb.AppendLine($"({wh.Id},{wh.ExamTypeId},{wh.WordId},'{DateTimeHelper.GetStandardFormattedDateTimeString(wh.CreateTime)}'),");
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
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from MediaAccessHistory";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<int> ClearVOCABs()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from VOCAB";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
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

        public static async Task<bool> AddVOCAB(EnglishObjectType objectType, int objectId)
        {
            string idField = objectType == EnglishObjectType.Word ? "WordId" : "PhraseId";

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"insert into VOCAB({idField},CreateTime) values({objectId}, '{DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now)}')";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows == 1;
            }
        }

        public static async Task<bool> DeleteVOCAB(int id)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from VOCAB where Id={id}";

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

        public static async Task<int> BatchInsertVOCAB(IEnumerable<int> wordIds, IEnumerable<int> phraseIds)
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string createTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                StringBuilder sb = new StringBuilder("insert into VOCAB(WordId,PhraseId,CreateTime)values");

                foreach (var wordId in wordIds)
                {
                    sb.AppendLine($"({wordId},null,'{createTime}'),");
                }

                foreach (var phraseId in phraseIds)
                {
                    sb.AppendLine($"(null,{phraseId},'{createTime}'),");
                }

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sb.ToString().Trim().TrimEnd(',')));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }

        public static async Task<bool> SaveWordLearnHistory(EnglishExamType examType, V_EnglishWord word)
        {
            int examTypeId = examType.Id;
            int wordId = word.Id;
            int? wordExamType = word.ExamType;

            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string dateTime = DateTimeHelper.GetStandardFormattedDateTimeString(DateTime.Now);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                string selectSql = $"select Id from EnglishWordLearnHistory where ExamTypeId={examTypeId} and WordId={wordId}";
                string insertSql = "";
                string insertPrefix = "insert into EnglishWordLearnHistory(ExamTypeId,WordId,CreateTime) values";

                int id = (await connection.QueryAsync<int>(selectSql))?.FirstOrDefault() ?? 0;

                if (id == 0)
                {
                    insertSql = $"{insertPrefix}({examTypeId}, {wordId},'{dateTime}')";
                }    
                else
                {
                    return true;
                }
                
                if(wordExamType.HasValue)
                {
                    var examTypes = await DbObjectsFetcher.GetEnglishExamTypes();

                    List<int> matchExamTypeIds = new List<int>();

                    foreach(var et in examTypes)
                    {
                        if(et.Id!= examTypeId && (et.Weight & wordExamType.Value) == et.Weight)
                        {
                            matchExamTypeIds.Add(et.Id);
                        }
                    }

                    if(matchExamTypeIds.Count>0)
                    {
                        string strExamTypeIds = string.Join(",", matchExamTypeIds);

                        selectSql = $"select ExamTypeId from EnglishWordLearnHistory where WordId={wordId} and ExamTypeId in({strExamTypeIds})";

                        var existingExamTypeIds = await connection.QueryAsync<int>(selectSql);

                        var needInsertExamTypeIds = matchExamTypeIds.Except(existingExamTypeIds).ToList();

                        if(needInsertExamTypeIds.Count>0)
                        {
                            StringBuilder sb = new StringBuilder();

                            if(insertSql.Length>0)
                            {
                                insertSql += ",";
                            }
                            else
                            {
                                sb.AppendLine(insertPrefix);
                            }

                            foreach(var needInsertExamTypeId in needInsertExamTypeIds)
                            {
                                sb.AppendLine($"({needInsertExamTypeId}, {wordId},'{dateTime}'),");
                            }

                            insertSql += sb.ToString().Trim().Trim(',');
                        }
                    }                   
                }

                int affectedRows = (await connection.ExecuteAsync(insertSql));                    

                await transaction.CommitAsync();

                return affectedRows > 0;
            }
        }

        public static async Task<int> ClearEnglishWordLearnHistories()
        {
            using (var connection = DbUtitlity.CreateDbConnection())
            {
                string sql = $"delete from EnglishWordLearnHistory";

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();

                int affectedRows = (await connection.ExecuteAsync(sql));

                await transaction.CommitAsync();

                return affectedRows;
            }
        }
    }
}
