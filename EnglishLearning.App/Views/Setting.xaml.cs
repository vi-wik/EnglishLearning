using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using EnglishLearning.Business;
using EnglishLearning.Business.Helper;
using EnglishLearning.Business.Manager;
using System;

namespace EnglishLearning.App.Views;

public partial class Setting : ContentPage
{
    private string projectUrl = "https://github.com/vi-wik/EnglishLearning";

    public Setting()
    {
        InitializeComponent();

        this.Init();
    }

    private void Init()
    {
        this.lblVersion.Text = AppInfo.Current.VersionString;
        this.lblProjectUrl.Text = this.projectUrl;
    }

    private async void TapGestureRecognizer_FavoriteCategoryTapped(object sender, TappedEventArgs e)
    {
        FavoriteCategorySetting setting = (FavoriteCategorySetting)Activator.CreateInstance(typeof(FavoriteCategorySetting));

        await Navigation.PushAsync(setting);
    }

    private async void TapGestureRecognizer_CommonSettingTapped(object sender, TappedEventArgs e)
    {
        CommonSetting setting = (CommonSetting)Activator.CreateInstance(typeof(CommonSetting));

        await Navigation.PushAsync(setting);
    }

    private async void TapGestureRecognizer_ClearWordLearnHistoryTapped(object sender, TappedEventArgs e)
    {
        bool confirmed = await DisplayAlert("询问?", "确定要清除背单词历史记录吗？", "是", "否");

        if (confirmed)
        {
            int affectedRows = await DataProcessor.ClearEnglishWordLearnHistories();

            if (affectedRows > 0)
            {
                await DisplayAlert("信息", $"记录已被清除。", "确定");
            }
            else
            {
                await DisplayAlert("信息", $"未清除任何信息。", "确定");
            }
        }
    }

    private async void TapGestureRecognizer_BackupDataTapped(object sender, TappedEventArgs e)
    {
        if (!(await PermissionHelper.CheckPermission(PermissionType.Write)))
        {
            return;
        }

        string targetFileName = $"english_{DateTime.Now.ToString("yyyyMMdd")}.db3";

        string dataFilePath = DataFileManager.DataFilePath;

        if (File.Exists(dataFilePath))
        {
            try
            {
                DataProcessor.ClearSqliteAllPools();

                using (FileStream fs = File.OpenRead(dataFilePath))
                {
                    var res = await FileSaver.SaveAsync(targetFileName, fs, new CancellationToken());

                    if (res.IsSuccessful)
                    {
                        await DisplayAlert("提示", "备份完成", "确定");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                await DisplayAlert("发生错误", ex.Message, "确定");
            }
        }
        else
        {
            await DisplayAlert("提示", "数据文件不存在，无法备份！", "确定");
        }
    }

    private async void TapGestureRecognizer_ImportDataTapped(object sender, TappedEventArgs e)
    {
        if (!(await PermissionHelper.CheckPermission(PermissionType.Read)))
        {
            return;
        }

        var result = await FilePicker.Default.PickAsync();

        if (result != null && result.FullPath != null)
        {
            string filePath = result.FullPath;

            try
            {
                int affectedRows = await DataProcessor.ImportUserData(filePath);

                if (affectedRows > 0)
                {
                    await DisplayAlert("信息", "导入成功。", "确定");
                }
                else
                {
                    await DisplayAlert("信息", "未导入任何数据。", "确定");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                await DisplayAlert("导入失败", ex.Message, "确定");
            }
        }
    }

    private async void TapGestureRecognizer_ViewLogTapped(object sender, TappedEventArgs e)
    {
        if (!(await PermissionHelper.CheckPermission(PermissionType.Read)))
        {
            return;
        }

        FileList fileList = (FileList)Activator.CreateInstance(typeof(FileList), LogManager.LogFolder);

        await Navigation.PushAsync(fileList);
    }

    private async void TapGestureRecognizer_ClearLogTapped(object sender, TappedEventArgs e)
    {
        bool confirmed = await DisplayAlert("询问?", "确定要清除所有日志信息吗？", "是", "否");

        if (confirmed)
        {
            if (!(await PermissionHelper.CheckPermission(PermissionType.Write)))
            {
                await DisplayAlert("提示", "无写入权限！", "确定");
                return;
            }

            try
            {
                string folder = LogManager.LogFolder;

                var files = new DirectoryInfo(folder).GetFiles();

                foreach(var file in files)
                {
                    file.Delete();
                }

                await DisplayAlert("信息", "清除成功。", "确定");
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);

                await DisplayAlert("清除日志失败！", ex.Message, "确定");
            }
        }
    }

    private async void TapGestureRecognizer_ProjectUrlTapped(object sender, TappedEventArgs e)
    {
        var options = new BrowserLaunchOptions()
        {
            LaunchMode = BrowserLaunchMode.SystemPreferred
        };

        await Browser.Default.OpenAsync(this.projectUrl, options);
    }
}