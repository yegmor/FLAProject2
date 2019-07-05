using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLAProject2
{
    public class CYKParser
    {
        public bool[,,] Table;
        public List<List<char>> ProductionRules;
        public string InputWord;
        public int ProductionRulesCount;

        public bool Parse(char initialState, string inputWord, List<List<char>> rules)
        {
            InputWord = inputWord;
            ProductionRules = rules;
            ProductionRulesCount = ProductionRules.Count;

            Table = new bool[InputWord.Length, InputWord.Length, ProductionRulesCount];


            //Initialize first row of table
            for (int i = 0; i < InputWord.Length; i++)
            {
                for (int j = 0; j < ProductionRulesCount; j++)
                {
                    if (Check(ProductionRules[j], InputWord[i]))
                        Table[i, 0, j] = true;
                }
            }


            //filling table
            for (int i = 1; i < InputWord.Length; i++)//تقریبا ردیف
            {
                for (int j = 0; j < InputWord.Length - i; j++)//تقریبا ستون
                {
                    for (int k = 0; k < i; k++)//تقریبا ترکیب حروف کا به علاوه یک، تایی که میشه تقریبا ردیف
                    {
                        for (int x = 0; x < ProductionRulesCount; x++)
                        {
                            for (int y = 0; y < ProductionRulesCount; y++)
                            {
                                if (Table[j, k, x] && Table[j + k + 1, i - k - 1, y])
                                {
                                    for (int z = 0; z < ProductionRulesCount; z++)
                                    {
                                        if (Check(ProductionRules[z], ProductionRules[x][0], ProductionRules[y][0]))
                                            Table[j, i, z] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            // determine result
            for (int i = 0; i < ProductionRulesCount; i++)
                if (Table[0, InputWord.Length - 1, i] && ProductionRules[i][0] == initialState)
                    return true;

            return false;
        }

        private bool Check(List<char> production, char rightHandVar1, char rightHandVar2)
        {
            if (production.Count <= 2)
                return false;

            if (production[1] == rightHandVar1 && production[2] == rightHandVar2)
                return true;

            return false;
        }

        private bool Check(List<char> production, char ch)
        {
            for (int i = 1; i < production.Count; i++)// بعد از اون سمت چپیه
                if (ch == production[i])
                    return true;

            return false; ;
        }
    }
}
