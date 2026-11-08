namespace AppInfoGUI.Capabilities;

// Every row comes from the function's own page in the Microsoft Win32 documentation (the link is in "Source").
// A function goes in only after its description was read on that page. When in doubt it stays out.
// The group is our own label for sorting. It makes no claim about what a program does with the function.
public static class CapabilityTable
{
    const string Docs = "https://learn.microsoft.com/en-us/windows/win32/api/";

    public static readonly IReadOnlyList<Capability> All = new Capability[]
    {
        // Internet
        new("Internet", "InternetOpenUrlW", "Opens a resource specified by a complete FTP or HTTP URL.", Docs + "wininet/nf-wininet-internetopenurlw"),
        new("Internet", "InternetOpenW", "Initializes an application's use of the WinINet functions.", Docs + "wininet/nf-wininet-internetopenw"),
        new("Internet", "InternetReadFile", "Reads data from a handle opened by the InternetOpenUrl, FtpOpenFile, or HttpOpenRequest function.", Docs + "wininet/nf-wininet-internetreadfile"),
        new("Internet", "HttpSendRequestW", "Sends the specified request to the HTTP server and allows the client to specify additional headers to send along with the request.", Docs + "wininet/nf-wininet-httpsendrequestw"),
        new("Internet", "WinHttpSendRequest", "Sends the specified request to the HTTP server.", Docs + "winhttp/nf-winhttp-winhttpsendrequest"),
        new("Internet", "recv", "Receives data from a connected socket.", Docs + "winsock2/nf-winsock2-recv"),

        // Files
        new("Files", "CreateFileW", "Creates or opens a file or I/O device.", Docs + "fileapi/nf-fileapi-createfilew"),
        new("Files", "DeleteFileW", "Deletes an existing file.", Docs + "fileapi/nf-fileapi-deletefilew"),
        new("Files", "MoveFileExW", "Moves an existing file or directory, including its children.", Docs + "winbase/nf-winbase-movefileexw"),

        // Registry
        new("Registry", "RegOpenKeyExW", "Opens the specified registry key.", Docs + "winreg/nf-winreg-regopenkeyexw"),
        new("Registry", "RegDeleteKeyW", "Deletes a subkey and its values.", Docs + "winreg/nf-winreg-regdeletekeyw"),
        new("Registry", "RegDeleteValueW", "Removes a named value from the specified registry key.", Docs + "winreg/nf-winreg-regdeletevaluew"),

        // Processes
        new("Processes", "CreateProcessW", "Creates a new process and its primary thread.", Docs + "processthreadsapi/nf-processthreadsapi-createprocessw"),
        new("Processes", "VirtualAllocEx", "Reserves, commits, or changes the state of a region of pages in the virtual address space of a specified process.", Docs + "memoryapi/nf-memoryapi-virtualallocex"),
        new("Processes", "ShellExecuteW", "Performs an operation on a specified file.", Docs + "shellapi/nf-shellapi-shellexecutew"),

        // Services
        new("Services", "CreateServiceW", "Creates a service object and adds it to the specified service control manager database.", Docs + "winsvc/nf-winsvc-createservicew"),

        // Encryption
        new("Encryption", "BCryptEncrypt", "Encrypts a block of data using the specified key.", Docs + "bcrypt/nf-bcrypt-bcryptencrypt"),
        new("Encryption", "BCryptDecrypt", "Decrypts a block of data.", Docs + "bcrypt/nf-bcrypt-bcryptdecrypt"),
        new("Encryption", "CryptEncrypt", "Encrypts data using the specified cryptographic key.", Docs + "wincrypt/nf-wincrypt-cryptencrypt"),
        new("Encryption", "CryptUnprotectData", "Decrypts a data BLOB that was encrypted using the CryptProtectData function.", Docs + "dpapi/nf-dpapi-cryptunprotectdata"),
        new("Encryption", "CryptAcquireContextW", "Acquires a handle to a particular key container within a particular cryptographic service provider (CSP).", Docs + "wincrypt/nf-wincrypt-cryptacquirecontextw"),

        // Keyboard and screen
        new("Keyboard", "SetWindowsHookExW", "Installs an application-defined hook procedure into a hook chain.", Docs + "winuser/nf-winuser-setwindowshookexw"),
        new("Keyboard", "GetAsyncKeyState", "Determines whether a key is up or down at the time the function is called, and whether the key has been pressed since a previous call to GetAsyncKeyState.", Docs + "winuser/nf-winuser-getasynckeystate"),
        new("Screen", "BitBlt", "Performs a bit-block transfer of the color data corresponding to a rectangle of pixels from the specified source device context into a destination device context.", Docs + "wingdi/nf-wingdi-bitblt")
    };

    static readonly Dictionary<string, Capability> ByFunction = All.ToDictionary(c => c.Function);

    public static Capability? Find(string function) => ByFunction.GetValueOrDefault(function);
}
