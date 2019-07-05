using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace formalLANew
{
    class Program
    {
        static void Main(string[] args)
        {





            var refinedRules = ChomskyNormalForm(null, null, null);

            Console.WriteLine("Please enter input word: ");
            string input=Console.ReadLine();

            bool result = Parse('b', input, refinedRules);

            Console.WriteLine($"output: {result}");
            Console.WriteLine($"table: ");
            WriteTable();
        }

        //preparation for part two
        public static List<List<char>> ChomskyNormalForm(List<List<char>> rules, Dictionary<char, string> states, List<char> inputAlphabet)
        {
            List<List<char>> refinedRules = new List<List<char>>();
            Dictionary<char, char> terminalRules = new Dictionary<char, char>();


            //adding terminal producitons
            foreach (char ch in inputAlphabet)
            {
                terminalRules.Add(ch, char.Parse(rules.Count.ToString()));

                rules.Add(new List<char> (){ char.Parse(rules.Count.ToString()), ch });//عدد استیت میشه اخرین عددی که تا اونجا داشتیم
                //states.Add(char.Parse(states.Count.ToString()), ch.ToString());
            }

            int i = 0;
            foreach (List<char> rule in rules)
            {
                if (rule.Count > 2)
                {
                    char terminalStateNum = terminalRules[rule[1]];

                    var partOne = new List<char>() { rule[0], terminalStateNum, char.Parse((rules.Count + i).ToString()) };
                    var partTwo = new List<char>() { char.Parse((rules.Count + i).ToString()), rule[2], rule[3] };

                    refinedRules.Add(partOne);
                    refinedRules.Add(partTwo);

                    i++;
                    //states.Add(char.Parse(states.Count.ToString()), ch.ToString());
                }

                else
                    refinedRules.Add(rule);
            }

            return refinedRules;
        }


        //part two

        public static bool[,,] Table;
        public static List<List<char>> ProductionRules;
        public static string InputWord;
        public static int ProductionRulesCount;

        public static bool Parse(char initialState, string inputWord, List<List<char>> rules)
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
            for (int i = 1; i < InputWord.Length; i++)
            {
                for (int j = 0; j < InputWord.Length - i; j++)//
                {
                    for (int k = 0; k < i; k++)//
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

        private static bool Check(List<char> production, char rightHandVar1, char rightHandVar2)
        {
            if (production.Count <= 2)
                return false;

            if (production[1] == rightHandVar1 && production[2] == rightHandVar2)
                return true;

            return false;
        }

        private static bool Check(List<char> production, char ch)
        {
            for (int i = 1; i < production.Count; i++)// بعد از اون سمت چپیه
                if (ch == production[i])
                    return true;

            return false; ;
        }

        public static void WriteTable()
        {
            for (int i = InputWord.Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < InputWord.Length; j++)
                {
                    Console.Write('|');

                    for (int k = 0; k < ProductionRulesCount; k++)
                    {
                        if (Table[j, i, k])
                            Console.Write(ProductionRules[k][0]);
                        else
                            Console.Write(' ');
                    }
                    if (j == InputWord.Length - 1)
                        Console.Write('|');
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
