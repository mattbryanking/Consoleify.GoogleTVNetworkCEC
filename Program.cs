namespace Consoleify.GoogleTVNetworkCEC
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isSilent = args.Contains("--silent");
            Application.Run(new BackgroundApplicationContext(isSilent));
        }
    }
}