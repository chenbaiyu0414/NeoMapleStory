using System;
using System.Data.Entity;
using System.Windows.Forms;
using NeoMapleStory.Core.Database;

namespace NeoMapleStory
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<NeoMapleStoryDatabase>());
            Application.Run(new MainForm());
        }
    }
}