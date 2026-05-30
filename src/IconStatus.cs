using Microsoft.Win32;

public class IconStatus {
    public static bool AreVisible() {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
        return (int)(key?.GetValue("HideIcons") ?? 0) == 0;
    }
}
