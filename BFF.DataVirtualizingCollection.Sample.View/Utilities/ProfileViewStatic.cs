using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;
using MahApps.Metro.IconPacks;

namespace BFF.DataVirtualizingCollection.Sample.View.Utilities
{
    public static class ProfileViewStatic
    {
        public static IValueConverter ToCompanyBrush =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, Brush>(
                e => e.Value.IsFreelancer ? Brushes.Green : Brushes.Blue);

        public static IValueConverter ToCompanyText =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, string>(
                e => e.Value.IsFreelancer ? "Freelancer" : e.Value.CompanyName ?? string.Empty);

        public static IValueConverter ToCompanyIcon =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, PackIconMaterialKind>(
                e => e.Value.IsFreelancer ? PackIconMaterialKind.AccountOutline : PackIconMaterialKind.City);

        public static IValueConverter ToImageSource =
            LambdaConverters.ValueConverter.Create<ProfileViewModel, ImageSource>(
                e => new BitmapImage(new Uri(e.Value.PicturePath)));
        
        public static IValueConverter PrefixedHiddenAbilitiesCount =
            LambdaConverters.ValueConverter.Create<int, string>(
                e => $"+{e.Value}");

        public static IValueConverter ProfilesTitle =
            LambdaConverters.ValueConverter.Create<int, string>(
                e => $"Profiles ({e.Value})");
    }
}