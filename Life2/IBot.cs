using System.Drawing;

namespace Life2
{
    public interface IBot
    {
        void Live(int iteration);
        
        int LastIteration();
        
        string GetInfo();

        Color GetColor(DrawType drawStyle);

        void Attack(int damage);

        int GetKinship(IBot bot);

        bool IsKiller();
    }
}