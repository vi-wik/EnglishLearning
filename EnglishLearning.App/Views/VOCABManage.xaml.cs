using CommunityToolkit.Maui.Storage;
using EnglishLearning.App.Helper;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;
using EnglishLearning.Model;
using System.Collections.ObjectModel;

namespace EnglishLearning.App.Views;

public partial class VOCABManage : ContentPage
{
    public VOCABManage()
    {
        InitializeComponent();
    }

    private async Task<bool> HasData()
    {
        int count = await DataProcessor.GetVOCABCount();

        return count > 0;
    }

    private async void TapGestureRecognizer_ExportTapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!(await this.HasData()))
            {
                MessageHelper.ShowToastMessage("�������ݡ�");
                return;
            }

            if (!(await PermissionHelper.CheckPermission(PermissionType.Write)))
            {
                return;
            }

            var vocabs = await DataProcessor.GetVVOCABs();

            string fileName = $"���ʱ�_{DateTime.Now.ToString("yyyyMMdd")}.txt";

            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms);

                sw.WriteLine(string.Join(Environment.NewLine, vocabs.Select(item => item.Name)));

                sw.Flush();

                var res = await FileSaver.SaveAsync(fileName, ms, new CancellationToken());

                if (res.IsSuccessful)
                {
                    await DisplayAlert("��ʾ", "�����ɹ�", "ȷ��");
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.LogException(ex);

            await DisplayAlert("����", $"����ʧ�ܣ�{ex.Message}", "ȷ��");
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
                    await DisplayAlert("��ʾ", "δ�ҵ��κμ�¼��", "ȷ��");
                    return;
                }

                var wordIds = await DataProcessor.GetEnglishWordIdsByWords(items);

                var wordIdsInVOCAB = await DataProcessor.GetExistingWordIdsOrPhraseIdsOfVOCAB(EnglishObjectType.Word, wordIds);

                var wordIdsNotInVOCAB = wordIds.Where(item => !wordIdsInVOCAB.Any(t => item == t));

                var phraseIds = await DataProcessor.GetEnglishPhraseIdsByPhrases(items);

                var phraseIdsInVOCAB = await DataProcessor.GetExistingWordIdsOrPhraseIdsOfVOCAB(EnglishObjectType.Phrase, phraseIds);

                var phraseIdsNotInVOCAB = phraseIds.Where(item => !phraseIdsInVOCAB.Any(t => item == t)).Where(item => !wordIdsNotInVOCAB.Any(t => item == t));

                if (wordIdsNotInVOCAB.Count() == 0 && phraseIdsNotInVOCAB.Count() == 0)
                {
                    await DisplayAlert("��ʾ", "δƥ�䵽�κμ�¼��", "ȷ��");
                    return;
                }

                int affectedRows = await DataProcessor.BatchInsertVOCAB(wordIdsNotInVOCAB, phraseIdsNotInVOCAB);

                await DisplayAlert("��Ϣ", $"������{affectedRows}����¼��", "ȷ��");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                await DisplayAlert("����", ex.Message, "ȷ��");
            }
        }
    }

    private async void TapGestureRecognizer_ClearTapped(object sender, TappedEventArgs e)
    {
        if (!(await this.HasData()))
        {
            MessageHelper.ShowToastMessage("�������ݡ�");
            return;
        }

        bool confirmed = await DisplayAlert("ѯ��?", "ȷ��Ҫ������ʱ���", "��", "��");

        if (confirmed)
        {
            int affectedRows = await DataProcessor.ClearVOCABs();

            await DisplayAlert("��Ϣ", $"��ɾ��{affectedRows}����¼��", "ȷ��");
        }
    }
}