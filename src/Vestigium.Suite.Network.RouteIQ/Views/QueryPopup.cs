using System.Windows;
using System.Windows.Controls.Primitives;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public static class QueryPopup
{
    public static CustomPopupPlacementCallback Place { get; } = PlaceOnBar;

    private static CustomPopupPlacement[] PlaceOnBar(Size popupSize, Size targetSize, Point offset)
    {
        var x = popupSize.Width > targetSize.Width ? targetSize.Width - popupSize.Width : 0;
        return [new CustomPopupPlacement(new Point(x, targetSize.Height + 2), PopupPrimaryAxis.Horizontal)];
    }
}
