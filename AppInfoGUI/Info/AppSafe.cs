namespace AppInfoGUI.Info
{
    internal static class Safe
    {
        public static string Text(Func<string?> lookup)
        {
            try
            {
                string? value = lookup();
                return string.IsNullOrWhiteSpace(value) ? "Not available" : value;
            }
            catch (Exception)
            {
                return "Not available";
            }
        }
    }
}
