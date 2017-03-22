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
            foreach (BarSet bs in barSets)
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
            foreach (BarSet bs2 in barSets2.barSets)
            {
                if (bs2.num == 0)
                {
                    continue;
                }

                bool isExist = false;
                foreach (BarSet bs1 in barSets)
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
            foreach (BarSet bs in barSets)
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

        public BarSets sortDesc()
        {
            barSets.Sort(delegate (BarSet a, BarSet b)
            {
                return Math.Sign(b.len - a.len);
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

            double len = barSets[0].len;
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
            double totalLen = 0d;
            foreach (BarSet bs in barSets)
            {
                totalLen += bs.len * bs.num;
            }

            return totalLen;
        }

        /**
         * @return true if can subtract
         */
        public bool substractAll(BarSets substractBarSets)
        {
            List<BarSet> rstBarSets = new BarSets(this.barSets).getBarSets();
            foreach (BarSet bs in substractBarSets.barSets)
            {
                foreach (BarSet originBs in rstBarSets)
                {
                    if (originBs.len == bs.len)
                    {
                        if (originBs.num < bs.num)
                        {
                            // Invalid case
                            return false;
                        }
                        originBs.num = originBs.num - bs.num;
                    }
                }
            }

            this.barSets = removeInvalidBarSet(rstBarSets);

            return true;
        }

        private static List<BarSet> removeInvalidBarSet(List<BarSet> barSets)
        {
            List<BarSet> rstBarSets = new List<BarSet>();
            foreach (BarSet bs in barSets)
            {
                if (bs.num > 0)
                {
                    rstBarSets.Add(bs);
                }
            }

            return rstBarSets;
        }

        public bool compareEqualWith(BarSets comparedBarSets)
        {
            List<BarSet> bs1 = new BarSets(barSets).sortAsc().getBarSets();
            List<BarSet> bs2 = new BarSets(comparedBarSets.getBarSets()).sortAsc().getBarSets();

            if (bs1.Count != bs2.Count)
            {
                return false;
            }

            for (int i = 0; i < bs1.Count; i++)
            {
                if (bs1[i].len != bs2[i].len || bs1[i].num != bs2[i].num)
                {
                    return false;
                }
            }

            return true;
        }

        public int count()
        {
            int count = 0;
            foreach (BarSet bs in barSets)
            {
                count += bs.num;
            }

            return count;
        }

        public bool isEmpty()
        {
            foreach (BarSet s in barSets)
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
            List<string> pStrArr = new List<string>();
            foreach (BarSet bs in this.barSets)
            {
                pStrArr.Add(string.Format("{0}v {1}", bs.len, bs.num));
            }

            return string.Join(" + ", pStrArr.ToArray());
        }
    }
}