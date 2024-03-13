using System;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.ViewModels;

public class BreadcrumbViewModel : ViewModelBase
{
    public BreadcrumbViewModel(string titleKey, Type pageType)
    {
        TitleKey = titleKey;
        PageType = pageType;
        Loc.LanguageUpdated += OnLanguageUpdated;
    }

    private void OnLanguageUpdated()
    {
        RaisePropertyChanged(nameof(Title));
    }

    public Type PageType { get; set; }
    public string TitleKey { get; }
    public string Title => Loc.Resolve(TitleKey);
}