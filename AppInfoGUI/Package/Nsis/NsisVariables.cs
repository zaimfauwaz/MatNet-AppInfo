namespace AppInfoGUI.Package.Nsis;

// NSIS stores $INSTDIR, $TEMP and the like as numbered variables. These are the built-in ones.
public static class NsisVariables
{
    static readonly string[] Named =
    {
        "$0", "$1", "$2", "$3", "$4", "$5", "$6", "$7", "$8", "$9",
        "$R0", "$R1", "$R2", "$R3", "$R4", "$R5", "$R6", "$R7", "$R8", "$R9",
        "$CMDLINE", "$INSTDIR", "$OUTDIR", "$EXEDIR", "$LANGUAGE",
        "$TEMP", "$PLUGINSDIR", "$EXEPATH", "$EXEFILE", "$HWNDPARENT", "$_CLICK", "$_OUTDIR"
    };

    public static string Name(int index) => index >= 0 && index < Named.Length ? Named[index] : "$variable";
}
