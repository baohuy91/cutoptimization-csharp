namespace CutOptimization
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


        override public string ToString()
        {
            return string.Format("{0}v {1}", this.num, this.len);
        }
    }
}