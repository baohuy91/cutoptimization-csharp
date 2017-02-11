namespace CutOptimzation
{
    public class Pair<A, B>
    {
        public A fst;
        public B snd;

        public Pair(A var1, B var2)
        {
            this.fst = var1;
            this.snd = var2;
        }

        public string toString()
        {
            return "Pair[" + this.fst + "," + this.snd + "]";
        }
    }
}