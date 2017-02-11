namespace CutOptimzation
{
    public class BarSet
    {
        public double len;
        public int num;

        public BarSet(double len, int num)
        {
            this.len = len;
            this.num = num;
        }

        public string toString()
        {
            return this.len + "x" + this.num;
        }
    }
}