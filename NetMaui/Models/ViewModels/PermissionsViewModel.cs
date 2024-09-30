using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetMaui.Page.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMaui.Models.ViewModels
{
    public partial class PermissionsViewModel : ObservableObject
    {
        [RelayCommand]
        public async Task RequestPermissions()
        {
            if (DeviceInfo.Platform != DevicePlatform.Android)
                return;

            var status = PermissionStatus.Unknown;

            if (DeviceInfo.Version.Major >= 12)
            {
                status = await Permissions.CheckStatusAsync<MyMediaPermission>();

                if (Permissions.ShouldShowRationale<MyMediaPermission>())
                    await Shell.Current.DisplayAlert("Needs permission", "Because the application needs this permission to work", "Ok!");

                status = await Permissions.RequestAsync<MyMediaPermission>();
            }
            else
            {
                status = await Permissions.CheckStatusAsync<Permissions.Media>();

                if (Permissions.ShouldShowRationale<Permissions.Media>())
                    await Shell.Current.DisplayAlert("Needs permission", "Because the application needs this permission to work", "Ok!");

                status = await Permissions.RequestAsync<Permissions.Media>();
            }

            if(status != PermissionStatus.Granted)
                await Shell.Current.DisplayAlert("Permission Denied", "The application will not work without this permission", "Osk!");
        }
    }
}
