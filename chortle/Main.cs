// some conversation data comes from: http://en.wikibooks.org/wiki/English_in_Use/Conversation_Pieces

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

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
            Dictionary<string, Dictionary<string, string>> botLearnedResponses = new Dictionary<string, Dictionary<string, string>>();

            // 1/...    = good response match
            // 0.5/...  = maybe a response match
            // 0/...    = bad response match (please don't use this response, bot)

            // init botLearnedResponses

            // topics
            Dictionary<string, string> topicMeetPeople = new Dictionary<string, string>
            {
                {"Oh, hey there!", "1"},
                {"Lovely day, isn't it?", "1"},
                {"Be seeing you!", "1"},
                {"See ya!", "1"},
                {"Nice to meet you!", "1"},
            };

            Dictionary<string, string> topicRespondToInformation = new Dictionary<string, string>
            {
                {"Good for you!","0.5"},
                {"I’m sorry to hear that.", "1"},
                {"Oh, how lovely!", "1"},
                {"Sounds great.", "1"},
                {"Yes, I suppose you must be.", "1"},
                {"Wow! That sounds exciting.", "1"},
            };

            Dictionary<string, string> topicRespondToThanks = new Dictionary<string, string>
            {
                {"You are welcome!", "1"},
                {"Don't mention it.", "1"},
                {"No problem.", "1"},
                {"It's okay", "1"}
            };

            Dictionary<string, string> topicAskAgain = new Dictionary<string, string>
            {
                {"What do you mean?", "1"},
                {"Hmm?", "1"},
            };

            // learned keyword response keys
            botLearnedResponses["*"] = new Dictionary<string, string>(topicAskAgain);
            botLearnedResponses["hello"] = new Dictionary<string, string>(topicMeetPeople);
            botLearnedResponses["goodbye"] = new Dictionary<string, string>(topicMeetPeople);
            botLearnedResponses["i like"] = new Dictionary<string, string>(topicRespondToInformation);
            botLearnedResponses["my name is"] = new Dictionary<string, string>(topicMeetPeople);
            botLearnedResponses["thanks"] = new Dictionary<string, string>(topicRespondToThanks);


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

            List<string> botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
            List<string> questionKeyList = new List<string>(questionData.Keys);
            List<string> phraseKeyList = new List<string>(phraseData.Keys);
            Random randomNumber = new Random();
            String teacherResponse;
            String teacherDecision;
            String botResponse;

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
                    Console.WriteLine("found key: " + teacherResponse);

                    bool checkForBest = true;
                    //List<string> botLearnedKeyValueList = new List<string>(botLearnedResponses[teacherResponse].Keys);

                    /*
                    var sortedItems = from pair in botLearnedResponses[teacherResponse]
                                      orderby pair.Value descending
                                      select pair;
                    */

                    // Add some randomness in how responses are initially found (ordered)
                    Random rndNumber = new Random();
                    var randomizedItems = from pair in botLearnedResponses[teacherResponse]
                                          orderby rndNumber.Next() descending
                                          select pair;

                    List<string> orderedKeys = new List<string>();
                    foreach (KeyValuePair<string, string> pair in randomizedItems)
                    {
                        orderedKeys.Add(pair.Key);
                        //Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
                    }

                    List<string> botLearnedKeyValueList = new List<string>(orderedKeys);

                    foreach (string keyItem in botLearnedKeyValueList)
                    {
                        var weight = botLearnedResponses[teacherResponse][keyItem];
                        Console.WriteLine("checking weight response..." + weight + " / " + keyItem);

                        // if weight is high enough, return found response as bot response
                        if (Convert.ToDouble(weight) >= 0.6 && checkForBest)
                        {
                            Console.WriteLine("found a good weight response");
                            botResponse = keyItem;
                            break;
                        }
                        else if (Convert.ToDouble(weight) >= 0.5 && Convert.ToDouble(weight) < 1.0)
                        {
                            if (checkForBest)
                            {
                                Console.WriteLine("found an okay weight response");
                                botResponse = keyItem;
                            }
                            else
                            {
                                Console.WriteLine("found an okay weight response");
                                botResponse = keyItem;
                                break;
                            }
                        }
                        // no responses available
                        else
                        {
                            Random rnd = new Random();
                            // TODO: add the random part (below) back in later...
                            // int choice = rnd.Next(1, 2);
                            int choice = 1;

                            switch (choice)
                            {
                                // randomly guess from learned words
                                case 1:
                                    // refresh bot learned key list to get recent changes
                                    botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
                                    String randomKey = botLearnedKeyList[randomNumber.Next(botLearnedResponses.Count)];

                                    if (botLearnedResponses[randomKey].Count > 0)
                                    {
                                        // Add some randomness in how guesses happen
                                        // (when bot doesn't have a response... try guessing at another topic response)
                                        Random rndNumberForGuess = new Random();
                                        var randomizedTopicsForGuess = from pair in botLearnedResponses[randomKey]
                                                                        orderby rndNumber.Next()
                                                                        select pair;

                                        List<string> orderedKeysForGuess = new List<string>();
                                        foreach (KeyValuePair<string, string> pair in randomizedTopicsForGuess)
                                        {
                                            orderedKeysForGuess.Add(pair.Key);
                                            //Console.WriteLine("{0} : {1}", pair.Key, pair.Value);
                                        }

                                        List<string> botLearnedKeyValueListForGuess = new List<string>(orderedKeysForGuess);

                                        foreach (string keyItemForGuess in botLearnedKeyValueListForGuess)
                                        {
                                            if (botLearnedResponses.ContainsKey(randomKey) && botLearnedResponses[randomKey].Count > 0)
                                            {
                                                var foundResponse = keyItemForGuess;
                                                Console.WriteLine("found response: " + foundResponse);
                                                if (!foundResponse.Equals(""))
                                                {
                                                    botResponse = foundResponse;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                // repeat what teacher said
                                                botResponse = teacherResponse;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // repeat what teacher said
                                        botResponse = teacherResponse;
                                    }
                                    break;
                                default:
                                    // repeat what teacher said
                                    botResponse = teacherResponse;
                                    break;
                            }
                        }
                    }
                }
                // no responses available (yet) so try some things
                else
                {
                    // break down teacher response key until nothing remains (i.e. nothing is found):
                    //  i like green things
                    //  i like green
                    //  i like
                    string[] teacherResponsePieces = teacherResponse.Split(' ');
                    string[] teacherResponsePiecesWorkingCopy = teacherResponse.Split(' ');
                    string shorterKey = "";
                    bool foundAResponse = false;

                    for (int teacherResponsePieceIndex = teacherResponsePieces.Length; teacherResponsePieceIndex >= 0; --teacherResponsePieceIndex)
                    {
                        if (!foundAResponse)
                        {
                            Console.WriteLine("finding response...");
                            shorterKey = "";

                            // build shorterKey from longest to shortest key possible
                            for (int buildKeyIndex = 0; buildKeyIndex < teacherResponsePieceIndex; buildKeyIndex++)
                            {
                                shorterKey += teacherResponsePieces[buildKeyIndex] + " ";
                            }
                            shorterKey = shorterKey.TrimEnd(' ');

                            Console.WriteLine("trying shorter key: " + shorterKey);

                            if (botLearnedResponses.ContainsKey(shorterKey))
                            {
                                Console.WriteLine("found this shorter key in learned responses: " + shorterKey);

                                // bot tries out a response
                                if (botLearnedResponses.ContainsKey(shorterKey))
                                {
                                    Console.WriteLine(">>>>>>>>>>>>>");
                                    //Console.WriteLine(botLearnedResponses.Count);
                                    //Console.WriteLine(randomNumber.Next(botLearnedResponses.Count));

                                    List<string> botLearnedKeyValueList = new List<string>(botLearnedResponses[shorterKey].Keys);

                                    bool checkForBest = true;
                                    foreach (string keyItem in botLearnedKeyValueList)
                                    {
                                        var weight = botLearnedResponses[shorterKey][keyItem];
                                        Console.WriteLine("checking weight response..." + weight + " / " + botLearnedResponses[shorterKey][keyItem]);

                                        // if weight is high enough, return found response as bot response
                                        if (Convert.ToDouble(weight) >= 0.6 && checkForBest)
                                        {
                                            Console.WriteLine("found a good weight response");
                                            //Console.WriteLine("should be exiting here...");
                                            botResponse = keyItem;
                                            foundAResponse = true;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= 0.5 && Convert.ToDouble(weight) < 1.0)
                                        {
                                            Console.WriteLine("checking an 'okay' response");
                                            if (checkForBest)
                                            {
                                                Console.WriteLine("found an okay weight response");
                                                botResponse = keyItem;
                                                //foundAResponse = true;
                                                //break;
                                            }
                                            else
                                            {
                                                Console.WriteLine("found an okay weight response");
                                                botResponse = keyItem;
                                                foundAResponse = true;
                                                break;
                                            }
                                        }
                                        // no responses available
                                        else
                                        {
                                            Random rnd = new Random();
                                            // TODO: add the random part (below) back in later...
                                            // int choice = rnd.Next(1, 2);
                                            int choice = 1;

                                            switch (choice)
                                            {
                                                // randomly guess from learned words
                                                case 1:
                                                    // refresh bot learned key list to get recent changes
                                                    botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
                                                    String randomKey = botLearnedKeyList[randomNumber.Next(botLearnedResponses.Count)];

                                                    botLearnedKeyValueList = new List<string>(botLearnedResponses[randomKey].Keys);
                                                    String randomValueKey = botLearnedKeyValueList[randomNumber.Next(botLearnedResponses[randomKey].Count)];

                                                    if (botLearnedResponses.ContainsKey(randomKey) && botLearnedResponses[randomKey].Count > 0)
                                                    {
                                                        var foundResponse = botLearnedResponses[randomKey][randomValueKey];
                                                        botResponse = foundResponse;
                                                    }
                                                    else
                                                    {
                                                        // repeat what teacher said
                                                        botResponse = shorterKey;
                                                    }
                                                    foundAResponse = true;
                                                    break;
                                                default:
                                                    // repeat what teacher said
                                                    botResponse = shorterKey;
                                                    foundAResponse = true;
                                                    break;
                                            }
                                        }
                                    }
                                    Console.WriteLine("||||" + botResponse + "||||");
                                }
                            }

                            Console.WriteLine("found a response: " + foundAResponse);
                            
                            if (!foundAResponse)
                            {
                                Console.WriteLine("picking from grab bag...");

                                // save new key first
                                botLearnedResponses[teacherResponse] = new Dictionary<string, string>();

                                // otherwise... pick from the * (grab bag) responses
                                if (botLearnedResponses.ContainsKey("*"))
                                {
                                    //teacherResponse = "*";

                                    List<string> botLearnedKeyValueList = new List<string>(botLearnedResponses["*"].Keys);
                                    //String randomValueKey = botLearnedKeyValueList[randomNumber.Next(botLearnedResponses[randomKey].Count)];

                                    bool checkForBest = true;
                                    foreach (string keyItem in botLearnedKeyValueList)
                                    {
                                        var weight = botLearnedResponses["*"][keyItem];
                                        Console.WriteLine("checking weight response..." + weight + " / " + keyItem);

                                        // if weight is high enough, return found response as bot response
                                        if (Convert.ToDouble(weight) >= 0.6 && checkForBest)
                                        {
                                            Console.WriteLine("found a good weight response");
                                            botResponse = keyItem;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= 0.5 && Convert.ToDouble(weight) < 1.0)
                                        {
                                            if (checkForBest)
                                            {
                                                Console.WriteLine("found an okay weight response");
                                                botResponse = keyItem;
                                            }
                                            else
                                            {
                                                Console.WriteLine("found an okay weight response");
                                                botResponse = keyItem;
                                                break;
                                            }
                                        }
                                        // no responses available
                                        else
                                        {
                                            Random rnd = new Random();
                                            // TODO: add the random part (below) back in later...
                                            // int choice = rnd.Next(1, 2);
                                            int choice = 1;

                                            switch (choice)
                                            {
                                                // randomly guess from learned words
                                                case 1:
                                                    // refresh bot learned key list to get recent changes
                                                    botLearnedKeyList = new List<string>(botLearnedResponses.Keys);
                                                    String randomKey = botLearnedKeyList[randomNumber.Next(botLearnedResponses.Count)];

                                                    botLearnedKeyValueList = new List<string>(botLearnedResponses[randomKey].Keys);
                                                    String randomValueKey = botLearnedKeyValueList[randomNumber.Next(botLearnedResponses[randomKey].Count)];

                                                    if (botLearnedResponses.ContainsKey(randomKey) && botLearnedResponses[randomKey].Count > 0)
                                                    {
                                                        var foundResponse = botLearnedResponses[randomKey][randomValueKey];
                                                        botResponse = foundResponse;
                                                    }
                                                    else
                                                    {
                                                        // repeat what teacher said
                                                        botResponse = teacherResponse;
                                                    }
                                                    break;
                                                default:
                                                    // repeat what teacher said
                                                    botResponse = teacherResponse;
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // repeat what teacher said
                                    botResponse = teacherResponse;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Console.Write("bot      > " + botResponse + "\n");

                // teacher tells bot 1 (yes) 0.5 (maybe) 0 (no)
                Console.Write("teacher  > 1:yes, 2:maybe, 3:no > ");
                teacherDecision = Console.ReadKey().KeyChar.ToString();
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
                    //currentValuesList = botLearnedResponses[teacherResponse];
                    currentValuesList = new List<string>(botLearnedResponses[teacherResponse].Keys);

                    Console.WriteLine(string.Join(":", currentValuesList));
                    Console.WriteLine(botResponse);
                    Console.WriteLine(teacherResponse);

                    if (currentValuesList.Count > 0)
                    {
                        bool foundResponseInValuesList = false;

                        List<string> botLearnedKeyValueList = new List<string>(botLearnedResponses[teacherResponse].Keys);
                        //String randomValueKey = botLearnedKeyValueList[randomNumber.Next(botLearnedResponses[randomKey].Count)];

                        // TODO: if teacher says "no", save and try another response?

                        foreach (string keyItem in botLearnedKeyValueList)
                        {
                            //var foundResponse = botLearnedResponses[teacherResponse][keyItem];
                            if (botResponse.Equals(keyItem))
                            {
                                botLearnedResponses[teacherResponse][botResponse] = teacherDecision;
                                Console.WriteLine("bot already knows this response... but let's update info");
                                foundResponseInValuesList = true;
                                break;
                            }
                        }

                        if (!foundResponseInValuesList)
                        {
                            botLearnedResponses[teacherResponse][botResponse] = teacherDecision;
                            //currentValuesList.Add(teacherDecision + "/" + botResponse);
                            //botLearnedResponses[teacherResponse] = currentValuesList;
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
                    Console.WriteLine("<> adding new bot response key: " + botResponse);
                    botLearnedResponses[teacherResponse] = new Dictionary<string, string> {
                        {botResponse, teacherDecision}
                    };
                    firstTimeForThisTopic = false;
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
                Console.WriteLine("{0}", key);
                var value = botLearnedResponses[key];

                foreach (var innerKey in value)
                {
                    Console.WriteLine("  {0}", innerKey);
                }
            }

            Console.ReadLine();
        }
    }
}