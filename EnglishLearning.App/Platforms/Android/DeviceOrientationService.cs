using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishLearning.App.DeviceServices
{
    public partial class DeviceOrientationService
    {
        private static readonly IReadOnlyDictionary<DisplayOrientation, ScreenOrientation> _androidDisplayOrientationMap =
            new Dictionary<DisplayOrientation, ScreenOrientation>
            {
                [DisplayOrientation.Landscape] = ScreenOrientation.Landscape,
                [DisplayOrientation.Portrait] = ScreenOrientation.Portrait,
            };

        public partial void SetDeviceOrientation(DisplayOrientation displayOrientation)
        {
            var currentActivity = ActivityStateManager.Default.GetCurrentActivity();                       

            if (currentActivity is not null)
            {
                if (_androidDisplayOrientationMap.TryGetValue(displayOrientation, out ScreenOrientation screenOrientation))
                {
                    currentActivity.RequestedOrientation = screenOrientation;
                }
            }
        }

        public partial DisplayOrientation GetDeviceOrientation()
        {
            IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            SurfaceOrientation orientation = windowManager.DefaultDisplay.Rotation;
            
            bool isLandscape = orientation == SurfaceOrientation.Rotation90 || orientation == SurfaceOrientation.Rotation270;

            return isLandscape ? DisplayOrientation.Landscape : DisplayOrientation.Portrait;
        }
    }
}
