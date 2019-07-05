using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace formalLANew
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader Input = new StreamReader("Input.txt");
            int States = int.Parse(Input.ReadLine());
            string[] inputAlphabets = Input.ReadLine().Split(',');
            string[] stackAlphabets = Input.ReadLine().Split(',');
            string startStack = Input.ReadLine();
            char initialState = '0';
            int finalState;
            List<List<string>> transitions = new List<List<string>>();
            List<string> transition = new List<string>();
            string temp;
            for (int i = 0; (temp = Input.ReadLine()) != null; i++)
            {
                transition = temp.Split(',').ToList();

                //start state
                if (transition[0].Substring(0, 2) == "->")
                {
                    initialState = transition[0].Substring(3).ToCharArray()[0];
                    transition[0] = transition[0].Substring(2);
                }

                if (transition[4].Substring(0, 1) == "*")
                {
                    finalState = int.Parse(transition[4].Substring(2));
                    transition[4] = "qf";
                }
                transitions.Add(transition);
            }
            NPDAToCFG(transitions, ref States, inputAlphabets, stackAlphabets, startStack, initialState);




            var refinedRules = ChomskyNormalForm(null, null, null);

            Console.WriteLine("Please enter input word: ");
            string input=Console.ReadLine();

            bool result = Parse('b', input, refinedRules);

            Console.WriteLine($"output: {result}");
            Console.WriteLine($"table: ");
            WriteTable();
        }

        
        //convert NPDA to CFG
        static void NPDAToCFG(List<List<string>> transitions, ref int states, string[] inputAlphabets,
            string[] stackAlphabets, string startStack, char initialState)
        {
            List<List<string>> rules = new List<List<string>>();

            for (int i = 0; i < transitions.Count; i++)
                CheckTrue(transitions[i], transitions, stackAlphabets, ref states);

            for (int i = 0; i < transitions.Count; i++)
                MakeRules(transitions[i], rules, states, initialState, inputAlphabets);

            //put grammars with similar start states in one line
            for (int i = 1; i < rules.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (rules[j][0].Equals(rules[i][0]) && rules[j][1].Equals(rules[i][1]))
                    {
                        rules[j].Add("|");
                        for (int k = 1; k < rules[i].Count; k++)
                            rules[j].Add(rules[i][k]);
                        rules.Remove(rules[i]);
                        break;
                    }
                }
            }

            // adding ()
            for (int i = 0; i < rules.Count; i++)
            {
                rules[i][0] = $"({rules[i][0]})";
                if (rules[i].Count > 2)
                {
                    rules[i][2] = $"({rules[i][2]})";
                    rules[i][3] = $"({rules[i][3]})";
                    rules[i][6] = $"({rules[i][6]})";
                    rules[i][7] = $"({rules[i][7]})";
                }
            }

            for (int i = 0; i < rules.Count; i++)
            {
                for (int j = 0; j < rules[i].Count; j++)
                    Console.Write(rules[i][j]);
                Console.WriteLine();
            }
        }

        //make transitions to normal form
        private static void CheckTrue(List<string> transition, List<List<string>> transitions,
            string[] stackAlphabets, ref int states)
        {
            if (transition[2] == "_")
            {
                transition[2] = stackAlphabets[0];
                transition[3] = transition[3] + stackAlphabets[0];
                for (int i = 1; i < stackAlphabets.Length; i++)
                {
                    List<string> temp = new List<string>();
                    temp.Add(transition[0]);
                    temp.Add(transition[1]);
                    temp.Add(stackAlphabets[i]);
                    temp.Add(transition[3] + stackAlphabets[i]);
                    temp.Add(transition[4]);
                    transitions.Add(temp);
                }
            }

            if (transition[3].Length == 1 && transition[3] != "_")
            {
                //for first alphabet
                string t1 = transition[4];
                string t3 = transition[3];
                transition[3] = stackAlphabets[0] + transition[3];
                transition[4] = "q" + $"{states}";
                states++;
                List<string> temp = new List<string>();
                temp.Add(transition[4]);
                temp.Add("_");
                temp.Add(stackAlphabets[0]);
                temp.Add("_");
                temp.Add(t1);

                //for other alphabets
                for (int i = 1; i < stackAlphabets.Length; i++)
                {
                    //first transition
                    temp = new List<string>();
                    temp.Add(transition[0]);
                    temp.Add(transition[1]);
                    temp.Add(transition[2]);
                    temp.Add(stackAlphabets[i] + t3);
                    temp.Add("q" + $"{states}");
                    states++;
                    transitions.Add(temp);

                    //second transition
                    temp = new List<string>();
                    temp.Add("q" + $"{states - 1}");
                    temp.Add("_");
                    temp.Add(stackAlphabets[i]);
                    temp.Add("_");
                    temp.Add(t1);
                    transitions.Add(temp);
                }

            }
        }

        //make rules of each transition
        private static void MakeRules(List<string> transition, List<List<string>> rules, int states,
            char initialState, string[] inputAlphabets)
        {

            //A --> lambda
            if (transition[3] == "_")
            {
                List<string> rule = new List<string>();
                rule.Add(transition[0] + transition[2] + transition[4]);
                rule.Add(transition[1]);
                if (!rules.Contains(rule))
                    rules.Add(rule);
            }

            //A --> BC
            if (transition[3].Length == 2)
            {
                for (int i = 0; i < states; i++)
                {
                    for (int j = 0; j < states; j++)
                    {

                        List<string> rule = new List<string>();
                        rule.Add(transition[0] + transition[2] + "q" + i);
                        rule.Add(transition[1]);
                        rule.Add(transition[4] + transition[3].Substring(0, 1) + "q" + j);
                        rule.Add("q" + j + transition[3].Substring(1) + "q" + i);
                        if (!rules.Contains(rule))
                            rules.Add(rule);
                    }
                }
            }

            List<List<string>> secondRules = new List<List<string>>();
            for (int i = 0; i < rules.Count; i++)
            {
                List<string> list = new List<string>();
                for (int j = 0; j < rules[i].Count; j++)
                {
                    list.Add(rules[i][j]);
                }
                secondRules.Add(list);
            }

            List<List<char>> lastRules;
            Dictionary<char, string> States;
            char InitialState;
            List<char> InputAlphabets;
            ConvertToChar(secondRules, initialState, inputAlphabets, out lastRules,
                out States, out InitialState, out InputAlphabets);
        }

        //convert to correct type
        private static void ConvertToChar(List<List<string>> rules, char initialState, string[] inputAlphabets,
            out List<List<char>> lastRules, out Dictionary<char, string> States,
            out char InitialState, out List<char> InputAlphabets)
        {
            InitialState = initialState;

            //give index to states
            List<string> ruleNum = new List<string>();
            for (int i = 0; i < rules.Count; i++)
            {
                if (!ruleNum.Contains(rules[i][0]))
                    ruleNum.Add(rules[i][0]);
            }
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].Count > 2)
                {
                    if (!ruleNum.Contains(rules[i][2]))
                        ruleNum.Add(rules[i][2]);
                    if (!ruleNum.Contains(rules[i][3]))
                        ruleNum.Add(rules[i][3]);
                }
            }

            //add states to dictionary
            States = new Dictionary<char, string>();
            for (int i = 0; i < ruleNum.Count; i++)
            {
                States.Add((char)i, ruleNum[i]);
            }

            //replace states with their number
            for (int i = 0; i < rules.Count; i++)
            {
                for (int k = 0; k < ruleNum.Count; k++)
                {
                    if (rules[i][0].Equals(ruleNum[k]))
                        rules[i][0] = $"{k}";
                    if (rules[i].Count > 2)
                    {
                        if (rules[i][2].Equals(ruleNum[k]))
                            rules[i][2] = $"{k}";
                        if (rules[i][3].Equals(ruleNum[k]))
                            rules[i][3] = $"{k}";
                    }
                }
            }

            //convert string to char
            lastRules = new List<List<char>>();
            for (int i = 0; i < rules.Count; i++)
            {
                List<char> list = new List<char>();
                for (int j = 0; j < rules[i].Count; j++)
                {
                    list.Add(rules[i][j].ToCharArray()[0]);
                }
                lastRules.Add(list);
            }

            InputAlphabets = new List<char>();
            for (int i = 0; i < inputAlphabets.Length; i++)
                InputAlphabets.Add(inputAlphabets[i].ToCharArray()[0]);

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
                for (int j = 0; j < InputWord.Length - i; j++)
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

        //check whether this production produces var1var2
        private static bool Check(List<char> production, char rightHandVar1, char rightHandVar2)
        {
            if (production.Count <= 2)
                return false;

            if (production[1] == rightHandVar1 && production[2] == rightHandVar2)
                return true;

            return false;
        }

        //check whether production produces terminal ch
        private static bool Check(List<char> production, char ch)
        {
            for (int i = 1; i < production.Count; i++)// بعد از اون سمت چپیه
                if (ch == production[i])
                    return true;

            return false; ;
        }

        //cyk table
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
