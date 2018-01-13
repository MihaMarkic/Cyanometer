using System.Linq;

namespace Cynanometer.Calculator.Engine
{
    public static class Utilities
    {
        public static Rectangle[] GetAreas(string args)
        {
            Rectangle[] areas;
            if (args == null)
                areas = null;
            else
            {
                string[] parts = args.Split(';');
                var query = from p in parts
                            let c = p.Split(',')
                            select new Rectangle(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), int.Parse(c[3]));
                areas = query.ToArray();
            }
            return areas;
        }
    }
}
