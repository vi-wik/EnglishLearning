using CommunityToolkit.Maui.Storage;
using EnglishLearning.App.Helper;
using EnglishLearning.BLL.Core;
using EnglishLearning.BLL.MAUI.Helper;
using EnglishLearning.BLL.MAUI.Manager;
using EnglishLearning.Model;
using System.Collections.ObjectModel;

namespace EnglishLearning.App.Views;

public partial class VOCABManage : ContentPage
{
    public VOCABManage()
    {
        InitializeComponent();
    }

    private bool IsWordVOCAB()
    {
        return this.rbWordVOVAB.IsChecked;
    }

    private bool IsPhraseVOCAB()
    {
        return this.rbPhraseVOCAB.IsChecked;
    }

    private async Task<bool> HasData()
    {
        int count = this.IsWordVOCAB()? await DataProcessor.GetEnglishWordVOCABCount() : await DataProcessor.GetEnglishPhraseVOCABCount();

        return count > 0;
    }

    private async void TapGestureRecognizer_ExportTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!(await this.HasData()))
            {
                MessageHelper.ShowToastMessage("暂无数据。");
                return;
            }

            if (!(await PermissionHelper.CheckReadWritePermission(PermissionType.Write)))
            {
                return;
            }

            IEnumerable<V_EnglishVOCAB> vocabs = this.IsWordVOCAB() ? await DataProcessor.GetVEnglishWordVOCABs() : await DataProcessor.GetVEnglishPhraseVOCABs() ;

            string fileName = $"生词本_{(this.IsWordVOCAB()?"单词":"短语")}_{DateTime.Now.ToString("yyyyMMdd")}.txt";

            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms);

                sw.WriteLine(string.Join(Environment.NewLine, vocabs.Select(item => item.Name)));

                sw.Flush();

                var res = await FileSaver.SaveAsync(fileName, ms, new CancellationToken());

                if (res.IsSuccessful)
                {
                    await DisplayAlert("提示", "导出成功", "确定");
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);

            await DisplayAlert("错误", $"导出失败：{ex.Message}", "确定");
        }
    }

    private async void TapGestureRecognizer_ImportTapped(object sender, TappedEventArgs e)
    {
        var result = await FilePicker.Default.PickAsync();

        if (result != null && result.FullPath != null)
        {
            string filePath = result.FullPath;

            try
            {
                var items = File.ReadAllLines(filePath).Select(item => item.Trim()).Where(item => item.Length > 0).Distinct();

                if (items.Count() == 0)
                {
                    await DisplayAlert("提示", "未找到任何记录！", "确定");
                    return;
                }

                IEnumerable<int> idsNotInVOCAB = null;

                if(this.IsWordVOCAB())
                {
                    var wordIds = await DataProcessor.GetEnglishWordIdsByWords(items);

                    var wordIdsInVOCAB = await DataProcessor.GetExistingWordIdsOfEnglishWordVOCAB(wordIds);

                    idsNotInVOCAB = wordIds.Where(item => !wordIdsInVOCAB.Any(t => item == t));                 
                }
                else
                {
                    var phraseIds = await DataProcessor.GetEnglishPhraseIdsByPhrases(items);

                    var phraseIdsInVOCAB = await DataProcessor.GetExistingPhraseIdsOfEnglishPhraseVOCAB(phraseIds);

                    idsNotInVOCAB = phraseIds.Where(item => !phraseIdsInVOCAB.Any(t => item == t));                   
                }              

                if (idsNotInVOCAB.Count() == 0)
                {
                    await DisplayAlert("提示", "未匹配到任何记录！", "确定");
                    return;
                }

                int affectedRows =this.IsWordVOCAB()?  await DataProcessor.BatchInsertEnglishWordVOCAB(idsNotInVOCAB):
                    await DataProcessor.BatchInsertEnglishPhraseVOCAB(idsNotInVOCAB);

                await DisplayAlert("消息", $"导入了{affectedRows}条记录。", "确定");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                await DisplayAlert("错误", ex.Message, "确定");
            }
        }
    }

    private async void TapGestureRecognizer_ClearTapped(object sender, TappedEventArgs e)
    {
        if (!(await this.HasData()))
        {
            MessageHelper.ShowToastMessage("暂无数据。");
            return;
        }

        bool confirmed = await DisplayAlert("询问?", "确定要清空生词本吗？", "是", "否");

        if (confirmed)
        {
            int affectedRows = this.IsWordVOCAB()? await DataProcessor.ClearEnglishWordVOCABs(): await DataProcessor.ClearEnglishPhraseVOCABs();

            await DisplayAlert("信息", $"已删除{affectedRows}条记录。", "确定");
        }
    }
}