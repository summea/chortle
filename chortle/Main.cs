// some conversation data comes from: http://en.wikibooks.org/wiki/English_in_Use/Conversation_Pieces

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace chortle
{
    class MainClass
    {
        public static class ChortleSettings
        {
            public static bool debugMode = true;
            public static bool firstTime = true;

            public static List<string> humanResponseConversationData = new List<string>();

            // init dictionary data
            private static string questionDataSrc = File.ReadAllText("../../Data/bot-questions.json");
            private static string responseDataSrc = File.ReadAllText("../../Data/bot-responses.json");
            private static string vocabularyDataSrc = File.ReadAllText("../../Data/vocabulary.json");

            public static Dictionary<string, string> questionData = JsonConvert.DeserializeObject<Dictionary<string, string>>(questionDataSrc);
            public static Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseDataSrc);
            public static Dictionary<string, string> vocabularyData = JsonConvert.DeserializeObject<Dictionary<string, string>>(vocabularyDataSrc);
            public static Dictionary<string, string> phraseData = new Dictionary<string, string>();

            public static string[] posVerbTypes = new string[] { "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };
            public static string[] posDetVerbTypes = new string[] { "DT,VB", "DT,VBD", "DT,VBG", "DT,VBN", "DT,VBP", "DT,VBZ" };

            // teacher decision weights
            public const double maxWeight = 1.0;
            public const double midWeight = 0.5;
            public const double minWeight = 0.0;
            public const double incDecWeight = 0.1;
        }

        public static bool botAsk()
        {
            List<string> questionKeyList = new List<string>(ChortleSettings.questionData.Keys);
            List<string> phraseKeyList = new List<string>(ChortleSettings.phraseData.Keys);
            string response;
            Random randomNumber = new Random();
            string randomKey = questionKeyList[randomNumber.Next(questionKeyList.Count)];
            bool validQuestion = true;
            bool questionNeedsInterpolation = false;
            bool success = false;

            // make sure to ask name at start
            if (ChortleSettings.firstTime)
            {
                randomKey = "your name";
                ChortleSettings.firstTime = false;
            }

            // check if we've already asked question
            if (ChortleSettings.responseData.ContainsKey(randomKey) && ChortleSettings.responseData[randomKey] == "")
            {
                // check for "dynamic" patterns in question
                // requires that the bot has already asked about related patterns
                string pattern = @"({{[\w\s]+}})";
                foreach (Match match in Regex.Matches(ChortleSettings.questionData[randomKey], pattern, RegexOptions.IgnoreCase))
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
                    if (ChortleSettings.responseData.ContainsKey(item) && ChortleSettings.responseData[item] == "")
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
                    String questionText = ChortleSettings.questionData[randomKey];

                    if (questionNeedsInterpolation)
                    {
                        // interpolate "dynamic" patterns in question
                        String interpolatedString = ChortleSettings.questionData[randomKey];
                        string patternItem = @"({{[\w\s]+}})";
                        foreach (Match match in Regex.Matches(ChortleSettings.questionData[randomKey], patternItem, RegexOptions.IgnoreCase))
                        {
                            String itemKey = match.Groups[0].ToString();
                            itemKey = itemKey.Replace("{", "").Replace("}", "");
                            interpolatedString = interpolatedString.Replace(match.Groups[0].ToString(), ChortleSettings.responseData[itemKey]);
                        }

                        questionText = interpolatedString;
                        questionNeedsInterpolation = false;

                    }

                    // bot asks question and gets human response
                    Console.WriteLine("bot    > " + questionText);
                    Console.Write("human  > ");
                    response = Console.ReadLine();

                    // save human response to conversation data
                    ChortleSettings.humanResponseConversationData.Add(response);

                    //if (response.Equals("goodbye"))
                        //doneChatting = true;

                    Random randomPhraseNumber = new Random();
                    string randomPhraseKey = phraseKeyList[randomPhraseNumber.Next(phraseKeyList.Count)];
                    Console.WriteLine("bot    > " + ChortleSettings.phraseData[randomPhraseKey]);

                    // originally from chortlejs parsing
                    string[] responsePieces = response.Split(' ');
                    List<string> responsePiecesAsPOS = new List<string>();

                    foreach (string word in responsePieces)
                    {
                        if (ChortleSettings.vocabularyData.ContainsKey(word))
                            responsePiecesAsPOS.Add(ChortleSettings.vocabularyData[word]);
                        else
                            responsePiecesAsPOS.Add("UNKNOWN");
                    }

                    string responsePOS = string.Join(",", responsePiecesAsPOS);

                    // TODO: find "target verb" from question and use as root verb

                    // TODO: "what is _that_?" (clarify)
                    // TODO: save clarification to previous answer key...

                    if (ChortleSettings.debugMode)
                        Console.WriteLine("responsePOS: " + responsePOS);

                    string resultKeyValueDirection = "ltr";
                    string sentenceBeginning = "";
                    if (responsePiecesAsPOS.Count >= 2)
                        sentenceBeginning = responsePiecesAsPOS[0] + "," + responsePiecesAsPOS[1];
                    int rootVerbPosition = 0;

                    // check for WP/ VBZ/ (e.g. "that is") statements

                    // check for DT,Verb type sentence openings
                    if (ChortleSettings.posDetVerbTypes.Contains(sentenceBeginning))
                    {
                        rootVerbPosition = 1;
                        resultKeyValueDirection = "rtl";
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> switching to RTL for verb key value parsing...");
                    }
                    // else this is a "common" verb statement
                    else
                    {
                        // loop through POS and find root VBZ index and value
                        for (int posIndex = 0; posIndex < responsePiecesAsPOS.Count; posIndex++)
                        {
                            if (ChortleSettings.posVerbTypes.Contains(responsePiecesAsPOS[posIndex]))
                            {
                                rootVerbPosition = posIndex;
                            }
                        }
                    }

                    // general match with root verb (if verb exists)
                    Match matchPOS = Regex.Match(responsePOS, @"(.*)VBZ", RegexOptions.IgnoreCase);
                    if (ChortleSettings.debugMode)
                        Console.WriteLine(response);
                    List<string> generatedKeyList = new List<string>();
                    List<string> generatedValueList = new List<string>();
                    List<string> generatedValuePatternList = new List<string>();
                    if (matchPOS.Success)
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> found match!");

                        bool pastRootVerb = false;

                        // divide up how we learn this data (key:value)
                        for (int userResponseIndex = 0; userResponseIndex < responsePiecesAsPOS.Count; userResponseIndex++)
                        {
                            // if we are past root verb (goes into value)
                            if (pastRootVerb)
                            {
                                if (ChortleSettings.vocabularyData.ContainsKey(responsePieces[userResponseIndex]))
                                {
                                    if (resultKeyValueDirection.Equals("ltr"))
                                    {
                                        generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                        generatedValuePatternList.Add(responsePiecesAsPOS[userResponseIndex]);
                                    }
                                    else
                                    {
                                        generatedKeyList.Add(responsePieces[userResponseIndex].ToLower());
                                    }
                                }
                                else
                                {
                                    if (resultKeyValueDirection.Equals("ltr"))
                                    {
                                        generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                        generatedValuePatternList.Add("UNKNOWN");
                                    }
                                    else
                                    {
                                        generatedKeyList.Add(responsePieces[userResponseIndex].ToLower());
                                    }
                                }
                            }
                            // if we are in front of or looking at root verb (goes into key)
                            else
                            {
                                if (resultKeyValueDirection.Equals("ltr"))
                                {
                                    generatedKeyList.Add(responsePiecesAsPOS[userResponseIndex]);
                                }
                                else
                                {
                                    if (ChortleSettings.debugMode)
                                        Console.WriteLine("right to left verb direction!");

                                    if (!ChortleSettings.posVerbTypes.Contains(responsePiecesAsPOS[userResponseIndex]))
                                    {
                                        generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                        generatedValuePatternList.Add(responsePiecesAsPOS[userResponseIndex]);
                                    }
                                }
                            }

                            if (userResponseIndex == rootVerbPosition)
                            {
                                if (ChortleSettings.debugMode)
                                    Console.WriteLine("found root verb: " + responsePiecesAsPOS[userResponseIndex]);
                                pastRootVerb = true;
                            }
                        }

                        if (ChortleSettings.debugMode)
                        {
                            Console.WriteLine("generated key/value lists joined individually");
                            Console.WriteLine("generated key list: " + string.Join(",", generatedKeyList));
                            Console.WriteLine("generated value list: " + string.Join(",", generatedValueList));
                            Console.WriteLine("generated value pattern list: " + string.Join(",", generatedValuePatternList));
                        }
                    }
                    else
                    {
                        // check for just answers (no verb necessarily)
                        matchPOS = Regex.Match(responsePOS, @"(.*)", RegexOptions.IgnoreCase);
                        if (matchPOS.Success)
                        {
                            for (int userResponseIndex = 0; userResponseIndex < responsePiecesAsPOS.Count; userResponseIndex++)
                            {
                                generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                            }
                        }
                    }


                    if (ChortleSettings.debugMode)
                        Console.WriteLine(">>> result: " + string.Join(" ", generatedValueList));

                    // save response data to dictionary
                    ChortleSettings.responseData[randomKey] = string.Join(" ", generatedValueList);
                    //numBotQuestionsAsked++;
                    success = true;
                }
            }
            return success;
        }

        public static void chatMode()
        {
            // TODO: connected learned phrases (teacher to bot) to this...

            // init phraseData
            // this data represents what phrases the chatbot has previously "learned" from a teacher
            ChortleSettings.phraseData.Add("response", "I see");

            bool doneChatting = false;
            List<string> questionKeyList = new List<string>(ChortleSettings.questionData.Keys);
            List<string> phraseKeyList = new List<string>(ChortleSettings.phraseData.Keys);

            int numBotQuestionsAsked = 0;
            int numTotalBotQuestions = questionKeyList.Count;            
            
            // bot states:
            // 0 = do nothing
            // 1 = ask
            // 2 = respond
            // 3 = follow-up
            int botState = 1;

            while (!doneChatting && (numBotQuestionsAsked < numTotalBotQuestions))
            {
                switch (botState)
                {
                    // bot ask
                    case 1:
                        if (botAsk())
                        {
                            numBotQuestionsAsked++;
                        }
                        break;
                }
            }

            // print out human response conversation data
            if (ChortleSettings.debugMode)
            {
                Console.WriteLine("\n\nhuman response conversation data:");
                foreach (var line in ChortleSettings.humanResponseConversationData)
                {
                    Console.WriteLine("{0}", line);
                }
            }

            // print out learned values
            if (ChortleSettings.debugMode)
            {
                Console.WriteLine("\n\nlearned information:");
                foreach (var key in ChortleSettings.responseData.Keys)
                {
                    Console.WriteLine("{0} - {1}", key, ChortleSettings.responseData[key]);
                }
            }
        }

        public static void teacherMode()
        {
            Dictionary<string, Dictionary<string, double>> botLearnedResponses = new Dictionary<string, Dictionary<string, double>>();
            List<string> botLearnedKeyList = new List<string>(botLearnedResponses.Keys);

            Random randomNumber = new Random();
            String teacherResponse;
            String teacherDecision;
            String botResponse;

            bool debugMode = true;

            // topic phrase format:
            // phrase, weight
            //
            // weight values:
            // good = increment weight by incDecWeight
            // bad  = decrement weight by incDecWeight

            // init botLearnedResponses
            // init topics
            Dictionary<string, double> topicMeetPeople = new Dictionary<string, double>
            {
                {"Oh, hey there!", 0.5},
                {"Lovely day, isn't it?", 0.5},
                {"Be seeing you!", 0.5},
                {"See ya!", 0.5},
                {"Nice to meet you!", 0.5},
            };

            Dictionary<string, double> topicRespondToInformation = new Dictionary<string, double>
            {
                {"Good for you!",0.5},
                {"I’m sorry to hear that.", 0.5},
                {"Oh, how lovely!", 0.5},
                {"Sounds great.", 0.5},
                {"Yes, I suppose you must be.", 0.5},
                {"Wow! That sounds exciting.", 0.5},
            };

            Dictionary<string, double> topicRespondToThanks = new Dictionary<string, double>
            {
                {"You are welcome!", 0.5},
                {"Don't mention it.", 0.5},
                {"No problem.", 0.5},
                {"It's okay", 0.5}
            };

            Dictionary<string, double> topicAskAgain = new Dictionary<string, double>
            {
                {"What do you mean?", 0.5},
                {"Hmm?", 0.5},
            };
            

            // init learned keyword response keys
            botLearnedResponses["*"] = new Dictionary<string, double>(topicAskAgain);
            botLearnedResponses["hello"] = new Dictionary<string, double>(topicMeetPeople);
            botLearnedResponses["goodbye"] = new Dictionary<string, double>(topicMeetPeople);
            botLearnedResponses["i like"] = new Dictionary<string, double>(topicRespondToInformation);
            botLearnedResponses["my name is"] = new Dictionary<string, double>(topicMeetPeople);
            botLearnedResponses["thanks"] = new Dictionary<string, double>(topicRespondToThanks);

            int loopLimit = 5;

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
                    if (debugMode)
                        Console.WriteLine("> found key: " + teacherResponse);

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
                    foreach (KeyValuePair<string, double> pair in randomizedItems)
                    {
                        orderedKeys.Add(pair.Key);
                    }

                    List<string> botLearnedKeyValueList = new List<string>(orderedKeys);

                    foreach (string keyItem in botLearnedKeyValueList)
                    {
                        var weight = botLearnedResponses[teacherResponse][keyItem];
                        if (debugMode)
                            Console.WriteLine("> checking weight response..." + weight + " / " + keyItem);

                        // if weight is high enough, return found response as bot response
                        if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                        {
                            if (debugMode)
                                Console.WriteLine("> found a good weight response");
                            botResponse = keyItem;
                            break;
                        }
                        else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                        {
                            if (checkForBest)
                            {
                                if (debugMode)
                                    Console.WriteLine("> found an okay weight response");
                                botResponse = keyItem;
                            }
                            else
                            {
                                if (debugMode)
                                    Console.WriteLine("> found an okay weight response");
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
                                        foreach (KeyValuePair<string, double> pair in randomizedTopicsForGuess)
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
                                                if (debugMode)
                                                    Console.WriteLine("> found response: " + foundResponse);
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
                            if (debugMode)
                                Console.WriteLine("> finding response...");
                            shorterKey = "";

                            // build shorterKey from longest to shortest key possible
                            for (int buildKeyIndex = 0; buildKeyIndex < teacherResponsePieceIndex; buildKeyIndex++)
                            {
                                shorterKey += teacherResponsePieces[buildKeyIndex] + " ";
                            }
                            shorterKey = shorterKey.TrimEnd(' ');

                            if (debugMode)
                                Console.WriteLine("> trying shorter key: " + shorterKey);

                            if (botLearnedResponses.ContainsKey(shorterKey))
                            {
                                if (debugMode)
                                    Console.WriteLine("> found this shorter key in learned responses: " + shorterKey);

                                // bot tries out a response
                                if (botLearnedResponses.ContainsKey(shorterKey))
                                {
                                    List<string> botLearnedKeyValueList = new List<string>(botLearnedResponses[shorterKey].Keys);

                                    bool checkForBest = true;
                                    foreach (string keyItem in botLearnedKeyValueList)
                                    {
                                        var weight = botLearnedResponses[shorterKey][keyItem];
                                        if (debugMode)
                                            Console.WriteLine("> checking weight response..." + weight + " / " + botLearnedResponses[shorterKey][keyItem]);

                                        // if weight is high enough, return found response as bot response
                                        if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                                        {
                                            if (debugMode)
                                                Console.WriteLine("> found a good weight response");
                                            //Console.WriteLine("should be exiting here...");
                                            botResponse = keyItem;
                                            foundAResponse = true;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                                        {
                                            if (debugMode)
                                                Console.WriteLine("> checking an 'okay' response");
                                            if (checkForBest)
                                            {
                                                if (debugMode)
                                                    Console.WriteLine("> found an okay weight response");
                                                botResponse = keyItem;
                                                //foundAResponse = true;
                                                //break;
                                            }
                                            else
                                            {
                                                if (debugMode)
                                                    Console.WriteLine("> found an okay weight response");
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
                                                        //var foundResponse = botLearnedResponses[randomKey][randomValueKey];
                                                        var foundResponse = randomValueKey;
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
                                    if (debugMode)
                                        Console.WriteLine("> botResponse: " + botResponse);
                                }
                            }

                            if (debugMode)
                                Console.WriteLine("> found a response: " + foundAResponse);

                            if (!foundAResponse)
                            {
                                if (debugMode)
                                    Console.WriteLine("> picking from grab bag...");

                                // save new key first
                                botLearnedResponses[teacherResponse] = new Dictionary<string, double>();

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
                                        if (debugMode)
                                            Console.WriteLine("> checking weight response..." + weight + " / " + keyItem);

                                        // if weight is high enough, return found response as bot response
                                        if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                                        {
                                            if (debugMode)
                                                Console.WriteLine("> found a good weight response");
                                            botResponse = keyItem;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                                        {
                                            if (checkForBest)
                                            {
                                                if (debugMode)
                                                    Console.WriteLine("> found an okay weight response");
                                                botResponse = keyItem;
                                            }
                                            else
                                            {
                                                if (debugMode)
                                                    Console.WriteLine("> found an okay weight response");
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
                                                        //var foundResponse = botLearnedResponses[randomKey][randomValueKey];
                                                        var foundResponse = randomValueKey;
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

                // teacher tells bot (yes = +.1) (no = -.1)
                Console.Write("teacher  > 1:yes, 2:no > ");
                teacherDecision = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine();
                double teacherDecisionValue = 0.0;

                switch (teacherDecision)
                {
                    case "1":
                        teacherDecisionValue = 0.1;
                        break;
                    case "2":
                        teacherDecisionValue = -0.1;
                        break;
                    default:
                        teacherDecisionValue = 0.0;
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
                                // make sure we are within 0.0 and 1.0 limits for response weight values
                                if ((botLearnedResponses[teacherResponse][botResponse] + teacherDecisionValue <= 1.0) &&
                                (botLearnedResponses[teacherResponse][botResponse] + teacherDecisionValue >= 0.0))
                                {
                                    botLearnedResponses[teacherResponse][botResponse] += teacherDecisionValue;
                                }
                                if (debugMode)
                                    Console.WriteLine("> bot already knows this response... but let's update info");
                                foundResponseInValuesList = true;
                                break;
                            }
                        }

                        if (!foundResponseInValuesList)
                        {
                            // ensure that weight values are not above or below limits (minWeight and maxWeight)
                            if ((ChortleSettings.midWeight + teacherDecisionValue) > ChortleSettings.maxWeight)
                                teacherDecisionValue = ChortleSettings.maxWeight;
                            else if ((ChortleSettings.midWeight + teacherDecisionValue) < ChortleSettings.minWeight)
                                teacherDecisionValue = ChortleSettings.minWeight;

                            if (debugMode)
                                Console.WriteLine("> adding response to learned key ...");
                            botLearnedResponses[teacherResponse].Add(botResponse, (ChortleSettings.midWeight + teacherDecisionValue));
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
                    if (debugMode)
                        Console.WriteLine("> adding new bot response key: " + botResponse);

                    // ensure that weight values are not above or below limits (minWeight and maxWeight)
                    if ((ChortleSettings.midWeight + teacherDecisionValue) > ChortleSettings.maxWeight)
                        teacherDecisionValue = ChortleSettings.maxWeight;
                    else if ((ChortleSettings.midWeight + teacherDecisionValue) < ChortleSettings.minWeight)
                        teacherDecisionValue = ChortleSettings.minWeight;

                    botLearnedResponses[teacherResponse] = new Dictionary<string, double> {
                        {botResponse, (ChortleSettings.midWeight + teacherDecisionValue)}
                    };
                    firstTimeForThisTopic = false;
                }
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
        }

        public static void Main(string[] args)
        {
            //teacherMode();
            chatMode();

            Console.ReadLine();
        }
    }
}