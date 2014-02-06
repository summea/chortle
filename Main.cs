using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ask question -or- make a statement

namespace chortle
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Dictionary<string, string> questionData = new Dictionary<string, string>();
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            Dictionary<string, string> vocabularyData = new Dictionary<string, string>();
            Dictionary<string, string> phraseData = new Dictionary<string, string>();
            String response;
            int numberOfQuestionAttempts = 20;
<<<<<<< HEAD
            
=======

>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
            // init questionData
            // this data represents what questions the chatbot has previously "learned" how to ask from a teacher
            questionData.Add("your name", "What is your name?");
            questionData.Add("your favorite color", "What is your favorite color?");
            questionData.Add("your favorite food", "What is your favorite food?");
            questionData.Add("you like {{your favorite color}} {{your favorite food}}", "Do you like {{your favorite color}} {{your favorite food}}?");
<<<<<<< HEAD
            
=======

>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
            // init responseData
            // the keys in this data represent concepts that the chatbot has previously "learned" from a teacher
            responseData.Add("your name", "");
            responseData.Add("your favorite color", "");
            responseData.Add("your favorite food", "");
            responseData.Add("you like {{your favorite color}} {{your favorite food}}", "");
<<<<<<< HEAD
            
=======

>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
            // init vocabularyData
            // this data represents what vocabulary the chatbot has previously "learned" from a teacher
            vocabularyData.Add("my", "determiner");
            vocabularyData.Add("you", "determiner");
            vocabularyData.Add("name", "noun");
            vocabularyData.Add("is", "verb");
            vocabularyData.Add("favorite", "noun");
            vocabularyData.Add("color", "noun");
            vocabularyData.Add("i", "determiner");
            vocabularyData.Add("like", "verb");
            vocabularyData.Add("to", "determiner");
            vocabularyData.Add("eat", "verb");
            vocabularyData.Add("food", "noun");
            vocabularyData.Add("do", "verb");
            vocabularyData.Add("not", "determiner");
            vocabularyData.Add("it", "determiner");
            vocabularyData.Add("that", "determiner");
            vocabularyData.Add("who", "interrogative");
            vocabularyData.Add("what", "interrogative");
            vocabularyData.Add("why", "interrogative");
            vocabularyData.Add("when", "interrogative");
            vocabularyData.Add("how", "interrogative");
<<<<<<< HEAD
            
            
=======


>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
            // init phraseData
            // this data represents what phrases the chatbot has previously "learned" from a teacher
            phraseData.Add("response", "I see");


            Boolean firstTime = true;
            List<string> questionKeyList = new List<string>(questionData.Keys);
            List<string> phraseKeyList = new List<string>(phraseData.Keys);
            Random randomNumber = new Random();
<<<<<<< HEAD
            
            for (int i = 0; i < numberOfQuestionAttempts; i++)
            {
                
=======

            for (int i = 0; i < numberOfQuestionAttempts; i++)
            {

>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
                string randomKey = questionKeyList[randomNumber.Next(questionKeyList.Count)];
                Boolean validQuestion = true;
                Boolean questionNeedsInterpolation = false;

                // make sure to ask name at start
                if (firstTime)
                {
                    randomKey = "your name";
                    firstTime = false;
                }

                String connectedResponse = "";

                // check if we've already asked question
                if (responseData[randomKey] == "")
                {
                    // check for "dynamic" patterns in question
                    // requires that the bot has already asked about related patterns
                    string pattern = @"({{[\w\s]+}})";
                    foreach (Match match in Regex.Matches(questionData[randomKey], pattern, RegexOptions.IgnoreCase))
                    {
                        //Console.WriteLine ("found tags to replace... {0}", match.Groups[0].Value);
                        //Console.WriteLine ("passing through...");
                        String item = match.Groups[0].ToString();
                        item = item.Replace("{", "").Replace("}", "");
                        //Console.WriteLine (item);

                        // check if responseData contains required, previously-asked information
                        // #ADDME: could someday allow bot to ask questions about required information
                        //     until required information is collected... and then ask  original, "dynamic" question

                        //Console.WriteLine ("checking dictionary...");
                        if (responseData.ContainsKey(item) && responseData[item] == "")
                        {
                            validQuestion = false;
                            //Console.WriteLine ("sorry, still have to ask about this...");
                        }
                        else
                        {
                            questionNeedsInterpolation = true;
                        }
                    }


                    if (validQuestion)
                    {
                        String questionText = questionData[randomKey];

                        if (questionNeedsInterpolation)
                        {
                            // interpolate "dynamic" patterns in question
                            String interpolatedString = questionData[randomKey];
                            string patternItem = @"({{[\w\s]+}})";
                            foreach (Match match in Regex.Matches(questionData[randomKey], patternItem, RegexOptions.IgnoreCase))
                            {
                                String itemKey = match.Groups[0].ToString();
                                itemKey = itemKey.Replace("{", "").Replace("}", "");
                                interpolatedString = interpolatedString.Replace(match.Groups[0].ToString(), responseData[itemKey]);
                            }

                            questionText = interpolatedString;
                            questionNeedsInterpolation = false;

                        }

                        // bot asks question and gets human response
                        Console.WriteLine("bot    > " + questionText);
                        Console.Write("human  > ");
                        response = Console.ReadLine();
<<<<<<< HEAD
                        response = response.Replace (".", ""); // remove some punctuation
                        
                        Random randomPhraseNumber = new Random();
                        string randomPhraseKey = phraseKeyList[randomPhraseNumber.Next(phraseKeyList.Count)];
                        Console.WriteLine("bot    > " + phraseData[randomPhraseKey]);
                        
=======

                        Random randomPhraseNumber = new Random();
                        string randomPhraseKey = phraseKeyList[randomPhraseNumber.Next(phraseKeyList.Count)];
                        Console.WriteLine("bot    > " + phraseData[randomPhraseKey]);

>>>>>>> 4a768001c4313ed56d528fb906e482b2f774f35f
                        string[] responseWords = response.Split(' ');
                        foreach (string word in responseWords)
                        {
                            if (!vocabularyData.ContainsKey(word.ToLower()))
                            {
                                connectedResponse = connectedResponse + word.ToLower();
                                //Console.WriteLine ("found: " + word.ToLower ());
                            }
                        }

                        // save response data to dictionary
                        responseData[randomKey] = connectedResponse.ToLower();
                    }
                }
            }

            // print out learned values
            Console.WriteLine("\n\nlearned information:");
            foreach (var key in responseData.Keys)
            {
                Console.WriteLine("{0} - {1}", key, responseData[key]);
            }
        }
    }
}