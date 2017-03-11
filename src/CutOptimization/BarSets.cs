using System;
using System.Collections.Generic;

namespace CutOptimization
{
    /**
     * 
     */
    public class BarSets
    {
        // BarSets doesn't have barset which has len is 0
        private List<BarSet> barSets;

        public BarSets()
        {
            this.barSets = new List<BarSet>();
        }
        public BarSets(List<BarSet> barSets)
        {
            this.barSets = new List<BarSet>();
            foreach (var bs in barSets)
            {
                this.barSets.Add(new BarSet(bs.len, bs.num));
            }
        }

        public BarSets clone()
        {
            return new BarSets(this.barSets);
        }

        public List<BarSet> getBarSets()
        {
            return this.barSets;
        }

        public void addAll(BarSets barSets2)
        {
            // Check and add to existing bar set
            foreach (var bs2 in barSets2.barSets)
            {
                if (bs2.num == 0)
                {
                    continue;
                }

                var isExist = false;
                foreach (var bs1 in barSets)
                {
                    if (bs1.len == bs2.len)
                    {
                        bs1.num += bs2.num;
                        isExist = true;
                        break;
                    }
                }

                // Add new bar set if the len not exist in this BarSets
                if (!isExist)
                {
                    barSets.Add(new BarSet(bs2.len, bs2.num));
                }
            }
        }

        public void addLen(double len)
        {
            // Check and add to existing bar set
            foreach (var bs in barSets)
            {
                if (bs.len == len)
                {
                    bs.num++;
                    return;
                }
            }

            // Add new bar set if the len not exist in this BarSets
            barSets.Add(new BarSet(len, 1));
        }

        public BarSets sortAsc()
        {
            barSets.Sort(delegate (BarSet a, BarSet b)
            {
                return Math.Sign(a.len - b.len);
            });

            return this;
        }

        // Return 0 if empty
        public double popLen()
        {
            if (isEmpty())
            {
                return 0;
            }

            var len = barSets[0].len;
            barSets[0].num--;

            // There is no empty bar set in BarSets
            if (barSets[0].num == 0)
            {
                barSets.RemoveAt(0);
            }

            return len;
        }

        public double countTotalLen()
        {
            var totalLen = 0d;
            foreach (var bs in barSets)
            {
                totalLen += bs.len * bs.num;
            }

            return totalLen;
        }

        public int count()
        {
            int count = 0;
            foreach (var bs in barSets)
            {
                count += bs.num;
            }

            return count;
        }

        public bool isEmpty()
        {
            foreach (var s in barSets)
            {
                if (s.num > 0)
                {
                    return false;
                }
            }

            return true;
        }

        override public string ToString()
        {
            var pStrArr = new List<string>();
            foreach (var bs in this.barSets)
            {
                pStrArr.Add(string.Format("{0}v {1}", bs.len, bs.num));
            }

            return string.Join(" + ", pStrArr);
        }
    }
}