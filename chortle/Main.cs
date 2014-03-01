﻿using System;
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

            Dictionary<string, List<String>> botLearnedResponses = new Dictionary<string, List<string>>();

            //String response;

            // init questionData
            // this data represents what questions the chatbot has previously "learned" how to ask from a teacher
            questionData.Add("your name", "What is your name?");
            questionData.Add("your favorite color", "What is your favorite color?");
            questionData.Add("your favorite food", "What is your favorite food?");
            questionData.Add("you like {{your favorite color}} {{your favorite food}}", "Do you like {{your favorite color}} {{your favorite food}}?");

            // init responseData
            // the keys in this data represent concepts that the chatbot has previously "learned" from a teacher
            responseData.Add("your name", "");
            responseData.Add("your favorite color", "");
            responseData.Add("your favorite food", "");
            responseData.Add("you like {{your favorite color}} {{your favorite food}}", "");

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


            // init phraseData
            // this data represents what phrases the chatbot has previously "learned" from a teacher
            phraseData.Add("response", "I see");


            Boolean firstTime = true;

            List<string> botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
            
            List<string> questionKeyList = new List<string>(questionData.Keys);
            List<string> phraseKeyList = new List<string>(phraseData.Keys);
            Random randomNumber = new Random();
            String teacherResponse;
            String teacherDecision;
            String botResponse;
            //String randomKey;

            int loopLimit = 10;

            for (int i = 0; i < loopLimit; i++)
            {
                botResponse = "";
                botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
                // teacher says something
                Console.Write("teacher  > ");
                teacherResponse = Console.ReadLine();

                // bot tries out a response
                if (botLearnedResponses.ContainsKey(teacherResponse))
                {
                    Console.WriteLine(botLearnedResponses.Count);
                    Console.WriteLine(randomNumber.Next(botLearnedResponses.Count));
                    //randomKey = botLearnedKeyList[randomNumber.Next(botLearnedResponses.Count)];
                    //botResponse = string.Join<string>(",", botLearnedResponses[randomKey]);
                    foreach (string item in botLearnedResponses[teacherResponse])
                    {
                        var weight = item.Split('/')[0];
                        // if weight is high enough, return found response as bot response
                        if (Convert.ToDouble(weight) >= 0.5)
                        {
                            Console.WriteLine("found a good weight response");
                            botResponse = item.Split('/')[1];
                        }
                        // no responses available, just repeat what teacher said
                        else
                        {
                            botResponse = teacherResponse;
                        }
                    }
                }
                // no responses available, just repeat what teacher said
                else
                {
                    botResponse = teacherResponse;
                }

                Console.Write("bot      > " + botResponse + "\n");
                
                // teacher tells bot 1 (yes) 0.5 (maybe) 0 (no)
                Console.Write("teacher  > 1:yes, 2:maybe, 3:no > ");
                teacherDecision = Console.ReadKey(true).KeyChar.ToString();
                Console.WriteLine();

                switch (teacherDecision)
                {
                    case "1":
                        teacherDecision = "1";
                        break;
                    case "2":
                        teacherDecision = "0.5";
                        break;
                    case "3":
                        teacherDecision = "0";
                        break;
                    default:
                        teacherDecision = "0.5";
                        break;
                }

                // bot records string key, string list with response and weight
                List<string> currentValuesList = new List<string>();
                bool firstTimeForThisTopic = true;
                if (botLearnedResponses.ContainsKey(teacherResponse))
                {
                    firstTimeForThisTopic = false;
                    currentValuesList = botLearnedResponses[teacherResponse];

                    if (currentValuesList.Count != 0)
                    {
                        // TODO: if teacher says "no", save and try another response?
                        for (int valueIndex = 0; valueIndex < botLearnedResponses[teacherResponse].Count; i++)
                        {
                            var foundResponse = botLearnedResponses[teacherResponse][valueIndex].Split('/')[1];
                            if (!botResponse.Equals(foundResponse))
                            {
                                currentValuesList.Add(teacherDecision + "/" + botResponse);
                                botLearnedResponses[teacherResponse] = currentValuesList;
                                Console.WriteLine(string.Join<string>(",", botLearnedResponses[teacherResponse]));
                                break;
                            }
                            else
                            {
                                botLearnedResponses[teacherResponse][valueIndex] = teacherDecision + "/" + botResponse;
                                Console.WriteLine("bot already knows this response... but let's update info");
                                break;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        firstTimeForThisTopic = true;
                    }
                }

                // first time learning this response
                if (firstTimeForThisTopic)
                {
                    currentValuesList.Add(teacherDecision + "/" + botResponse);
                    botLearnedResponses[teacherResponse] = currentValuesList;
                }

                /*
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

                        Random randomPhraseNumber = new Random();
                        string randomPhraseKey = phraseKeyList[randomPhraseNumber.Next(phraseKeyList.Count)];
                        Console.WriteLine("bot    > " + phraseData[randomPhraseKey]);

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
                 */
            }

            // print out learned values
            Console.WriteLine("\n\nlearned information:");
            foreach (var key in responseData.Keys)
            {
                Console.WriteLine("{0} - {1}", key, responseData[key]);
            }

            Console.WriteLine("\n\nlearned responses:");
            foreach (var key in botLearnedResponses.Keys)
            {
                Console.WriteLine("{0} - {1}", key,string.Join<string>(",", botLearnedResponses[key]));
            }

            Console.ReadLine();
        }
    }
}