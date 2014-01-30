using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace chortle
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            Dictionary<string, string> questionData = new Dictionary<string, string> ();
            Dictionary<string, string> responseData = new Dictionary<string, string> ();
            Dictionary<string, string> vocabularyData = new Dictionary<string, string> ();
            Dictionary<string, string> phraseData = new Dictionary<string, string> ();
            String response;
            
            // init questionData
            // this data represents what questions the chatbot has previously "learned" how to ask from a teacher
            questionData.Add ("name", "What is your name?");
            questionData.Add ("favorite color", "What is your favorite color?");
            questionData.Add ("favorite food", "What is your favorite food?");
            questionData.Add ("like {{favorite color}} {{favorite food}}", "Do you like {{favorite color}} {{favorite food}}?");
            
            // init responseData
            // the keys in this data represent concepts that the chatbot has previously "learned" from a teacher
            responseData.Add ("name", "");
            responseData.Add ("favorite color", "");
            responseData.Add ("favorite food", "");
            responseData.Add ("like {{favorite color}} {{favorite food}}", "");
            
            // init vocabularyData
            // this data represents what vocabulary the chatbot has previously "learned" from a teacher
            vocabularyData.Add ("my", "");
            vocabularyData.Add ("name", "");
            vocabularyData.Add ("is", "");
            vocabularyData.Add ("favorite", "");
            vocabularyData.Add ("color", "");
            vocabularyData.Add ("i", "");
            vocabularyData.Add ("like", "");
            vocabularyData.Add ("to", "");
            vocabularyData.Add ("eat", "");
            vocabularyData.Add ("food", "");
            vocabularyData.Add ("do", "");
            vocabularyData.Add ("not", "");
            vocabularyData.Add ("it", "");
            vocabularyData.Add ("that", "");
            
            // init phraseData
            // this data represents what phrases the chatbot has previously "learned" from a teacher
            phraseData.Add("response", "I see");
            
            
            Boolean firstTime = true;
            List<string> questionKeyList = new List<string> (questionData.Keys);
            List<string> phraseKeyList = new List<string> (phraseData.Keys);
            Random randomNumber = new Random ();
            
            for (int i = 0; i < 10; i++) {
                
                string randomKey = questionKeyList [randomNumber.Next(questionKeyList.Count)];
                Boolean validQuestion = true;
                Boolean questionNeedsInterpolation = false;
                
                // make sure to ask name at start
                if (firstTime) {
                    randomKey = "name";
                    firstTime = false;
                }
                
                String connectedResponse = "";
                
                // check if we've already asked question
                if (responseData[randomKey] == "")
                {
                    // check for "dynamic" patterns in question
                    // requires that the bot has already asked about related patterns
                    string pattern = @"({{[\w\s]+}})";
                    foreach (Match match in Regex.Matches (questionData[randomKey], pattern, RegexOptions.IgnoreCase))
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
                        if (responseData[item] == "")
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
                            foreach (Match match in Regex.Matches (questionData[randomKey], patternItem, RegexOptions.IgnoreCase))
                            {
                                String itemKey = match.Groups[0].ToString();
                                itemKey = itemKey.Replace("{", "").Replace("}", "");
                                interpolatedString = interpolatedString.Replace (match.Groups[0].ToString(), responseData[itemKey]);
                            }
                            
                            questionText = interpolatedString;
                            questionNeedsInterpolation = false;
                            
                        }
                        
                        // bot asks question and gets human response
                        Console.WriteLine ("bot    > " + questionText);
                        Console.Write ("human  > ");
                        response = Console.ReadLine ();
                        
                        Random randomPhraseNumber = new Random ();
                        string randomPhraseKey = phraseKeyList [randomPhraseNumber.Next(phraseKeyList.Count)];
                        Console.WriteLine ("bot    > " + phraseData[randomPhraseKey]);
                        
                        string[] responseWords = response.Split (' ');
                        foreach (string word in responseWords) {
                            if (!vocabularyData.ContainsKey (word.ToLower ())) {
                                connectedResponse = connectedResponse + word.ToLower ();
                                //Console.WriteLine ("found: " + word.ToLower ());
                            }
                        }
                        
                        // save response data to dictionary
                        responseData [randomKey] = connectedResponse.ToLower ();
                    }
                }
            }
            
            // print out learned values
            Console.WriteLine ("\n\nlearned information:");
            foreach(var key in responseData.Keys){
                Console.WriteLine("{0} - {1}", key, responseData[key]);      
            }
        }
    }
}
