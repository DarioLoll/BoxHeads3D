using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public static class ThemeManager
{
    public static Color GetColor(ColorType color, Theme theme)
    {
        return Colors.Find(c => c.ColorType == color && c.Theme == theme).Color;
    }
    
    public static Color GetLightColor(Color color)
    {
        ThemeColor themeColor = Colors.Find(c => c.Color == color);
        return GetColor(themeColor.ColorType, Theme.Light);
    }
    
    public static Color GetDarkColor(Color color)
    {
        ThemeColor themeColor = Colors.Find(c => c.Color == color);
        return GetColor(themeColor.ColorType, Theme.Dark);
    }
    
    public static Color GetOppositeColor(Color color)
    {
        return UIManager.Instance.currentTheme == Theme.Dark
            ? GetLightColor(color)
            : GetDarkColor(color);
    }
    

    public static List<ThemeColor> Colors = new()
    {
        new(Theme.Dark, ColorType.BaseBackground, ConvertToColor("#17191C")),
        new(Theme.Light, ColorType.BaseBackground, ConvertToColor("#e4e7ea")),

        new(Theme.Dark, ColorType.BaseForeground, ConvertToColor("#A6AEBB")),
        new(Theme.Light, ColorType.BaseForeground, ConvertToColor("#767b83")),
         
        new(Theme.Dark, ColorType.HighlightedForeground, ConvertToColor("#edf8fb")),
        new(Theme.Light, ColorType.HighlightedForeground, ConvertToColor("#1b212c")),
        
        new(Theme.Dark, ColorType.HighlightedBackground, ConvertToColor("#434956")),
        new(Theme.Light, ColorType.HighlightedBackground, ConvertToColor("#e4e7ea")),
        
        new(Theme.Dark, ColorType.DisabledForeground, ConvertToColor("#848b8e")),
        new(Theme.Light, ColorType.DisabledForeground, ConvertToColor("#949598")),
        
        new(Theme.Dark, ColorType.PlaceholderForeground, ConvertToColor("#7f8791")),
        new(Theme.Light, ColorType.PlaceholderForeground, ConvertToColor("#81838c")),
        
        new(Theme.Dark, ColorType.ElementBackground, ConvertToColor("#27292e")),
        new(Theme.Light, ColorType.ElementBackground, ConvertToColor("#fefeff")),
        
        new(Theme.Dark, ColorType.ElementBackgroundOnHover, ConvertToColor("#212228")),
        new(Theme.Light, ColorType.ElementBackgroundOnHover, ConvertToColor("#d9d9d9")),
        
        new(Theme.Dark, ColorType.DisabledElementBackground, ConvertToColor("#36393f")),
        new(Theme.Light, ColorType.DisabledElementBackground, ConvertToColor("#f2f2f5")),
        
        new(Theme.Dark, ColorType.DarkForeground, ConvertToColor("#010702")),
        new(Theme.Light, ColorType.DarkForeground, ConvertToColor("#FFFFFF")),

        new(Theme.Dark, ColorType.PrimaryBackgroundGreen, ConvertToColor("#19be5d")),
        new(Theme.Light, ColorType.PrimaryBackgroundGreen, ConvertToColor("#00af5c")),

        new(Theme.Dark, ColorType.PrimaryBackgroundHoverGreen, ConvertToColor("#149d4d")),
        new(Theme.Light, ColorType.PrimaryBackgroundHoverGreen, ConvertToColor("#01954f")),
        
        new(Theme.Dark, ColorType.PrimaryBackgroundTale, ConvertToColor("#16AABE")),
        new(Theme.Light, ColorType.PrimaryBackgroundTale, ConvertToColor("#118393")),

        new(Theme.Dark, ColorType.PrimaryBackgroundHoverTale, ConvertToColor("#248E9C")),
        new(Theme.Light, ColorType.PrimaryBackgroundHoverTale, ConvertToColor("#1b6a74")),
        
        new(Theme.Dark, ColorType.PrimaryBackgroundRed, ConvertToColor("#FF486F")),
        new(Theme.Light, ColorType.PrimaryBackgroundRed, ConvertToColor("#cb2344")),
        
        new(Theme.Dark, ColorType.PrimaryBackgroundHoverRed, ConvertToColor("#B93B56")),
        new(Theme.Light, ColorType.PrimaryBackgroundHoverRed, ConvertToColor("#ac1d3a")),
        
        new(Theme.Dark, ColorType.Transparent, ConvertToColor("#00000000")),
        new(Theme.Light, ColorType.Transparent, ConvertToColor("#00000000")),
    };
    
    private static Color ConvertToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}

public struct ThemeColor
{
    public Theme Theme { get; set; }
    public ColorType ColorType { get; set; }
    public Color Color { get; set; }

    public ThemeColor(Theme theme, ColorType colorType, Color color)
    {
        Theme = theme;
        ColorType = colorType;
        Color = color;
    }
}

public enum Theme
{
    Light,
    Dark
}

public enum ColorType
{
    BaseBackground,
    BaseForeground,
    HighlightedForeground,
    HighlightedBackground,
    DisabledForeground,
    PlaceholderForeground,
    ElementBackground,
    ElementBackgroundOnHover,
    DisabledElementBackground,
    DarkForeground,
    PrimaryBackgroundGreen,
    PrimaryBackgroundHoverGreen,
    PrimaryBackgroundTale,
    PrimaryBackgroundHoverTale,
    PrimaryBackground,
    PrimaryBackgroundHover,
    PrimaryBackgroundRed,
    PrimaryBackgroundHoverRed,
    Transparent
}