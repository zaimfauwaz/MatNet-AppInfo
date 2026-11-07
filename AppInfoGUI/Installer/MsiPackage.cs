namespace AppInfoGUI.Installer;

// The tables inside an .msi package (read only)
internal sealed class MsiPackage : IDisposable
{
    readonly IntPtr _database;

    public MsiPackage(string path)
    {
        // 0 = open read only
        if (MsiNative.MsiOpenDatabase(path, IntPtr.Zero, out _database) != 0)
            throw new InvalidOperationException("Could not open the package database");
    }

    // Name and value of every property, like ProductName and Manufacturer
    public Dictionary<string, string> Properties()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string[] row in Query("SELECT `Property`, `Value` FROM `Property`", 2))
            result[row[0]] = row[1];

        return result;
    }

    public int CountRows(string table, string column) =>
        Query($"SELECT `{column}` FROM `{table}`", 1).Count();

    // A missing table gives no rows
    internal IEnumerable<string[]> Query(string sql, uint columns)
    {
        if (MsiNative.MsiDatabaseOpenView(_database, sql, out IntPtr view) != 0)
            yield break;

        try
        {
            MsiNative.MsiViewExecute(view, IntPtr.Zero);

            while (MsiNative.MsiViewFetch(view, out IntPtr record) == 0)
            {
                try
                {
                    var row = new string[columns];
                    for (uint i = 0; i < columns; i++)
                        row[i] = Field(record, i + 1);
                    yield return row;
                }
                finally
                {
                    MsiNative.MsiCloseHandle(record);
                }
            }
        }
        finally
        {
            MsiNative.MsiCloseHandle(view);
        }
    }

    static string Field(IntPtr record, uint index)
    {
        const uint MoreData = 234;
        var buffer = new System.Text.StringBuilder(2048);
        uint size = 2048;

        uint result = MsiNative.MsiRecordGetString(record, index, buffer, ref size);

        // A long value needs a bigger buffer. Windows tells us how big.
        if (result == MoreData)
        {
            buffer = new System.Text.StringBuilder((int)size + 1);
            size++;
            result = MsiNative.MsiRecordGetString(record, index, buffer, ref size);
        }

        return result == 0 ? buffer.ToString() : "";
    }

    public void Dispose() => MsiNative.MsiCloseHandle(_database);
}
