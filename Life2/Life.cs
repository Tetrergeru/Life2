using System.Windows.Forms;

namespace Life2
{
    static class Life
    {
        public const int BotSize = 2;
        public const int Width = 200;
        public const int Height = 200;
        public const int StartupBotCount = 20;
        
        private static LifeForm _form;

        static void Main(string[] args)
        {
            _form = new LifeForm(Width,Height);
            Application.Run(_form);
        }
    }
}
