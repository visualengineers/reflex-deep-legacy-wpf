using System.Globalization;
using WPFPhysicsControlsLib.ViewModel;

namespace WPFPhysicsControlsLib.Utilities;

public static class ConvertInfluenceExtension
{
    public static bool TryGetInfluenceAmount(object value, out double amount)
    {
        switch (value)
        {
            case null:
                amount = 0;
                return false;
            case double doubleValue:
                amount = doubleValue;
                return true;
            case float floatValue:
                amount = floatValue;
                return true;
            case int intValue:
                amount = intValue;
                return true;
            case string stringValue:
                return double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out amount)
                       || double.TryParse(stringValue, NumberStyles.Float, CultureInfo.CurrentCulture, out amount);
            default:
                amount = 0;
                return false;
        }
    }
}