// some conversation data comes from: http://en.wikibooks.org/wiki/English_in_Use/Conversation_Pieces

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace chortle
{
    class MainClass
    {
        public static void botAsk()
        {
            Dictionary<string, string> questionData = new Dictionary<string, string>();
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            Dictionary<string, string> vocabularyData = new Dictionary<string, string>();
            Dictionary<string, string> phraseData = new Dictionary<string, string>();

            // init questionData
            // this data represents what questions the chatbot has previously "learned" how to ask from a teacher
            //questionData.Add("your name", "What is your name?");
            questionData.Add("way to go", "what is the way to go? (multiple verb test)");
            //questionData.Add("your favorite color", "What is your favorite color?");
            //questionData.Add("your favorite food", "What is your favorite food?");
            //questionData.Add("you like {{your favorite color}} {{your favorite food}}", "Do you like {{your favorite color}} {{your favorite food}}?");

            // init responseData
            // the keys in this data represent concepts that the chatbot has previously "learned" from a teacher
            //responseData.Add("your name", "");
            responseData.Add("way to go", "");
            //responseData.Add("your favorite color", "");
            //responseData.Add("your favorite food", "");
            //responseData.Add("you like {{your favorite color}} {{your favorite food}}", "");

            // init vocabularyData
            // this data represents what vocabulary the chatbot has previously "learned" from a teacher
            vocabularyData.Add("yes", "UH");
            vocabularyData.Add("no", "DT");

            vocabularyData.Add("i", "PRP");
            vocabularyData.Add("you", "PRP");
            vocabularyData.Add("he", "PRP");
            vocabularyData.Add("she", "PRP");
            vocabularyData.Add("it", "PRP");
            vocabularyData.Add("we", "PRP");
            vocabularyData.Add("they", "PRP");
            vocabularyData.Add("me", "PRP");
            vocabularyData.Add("him", "PRP");
            vocabularyData.Add("her", "PRP");
            vocabularyData.Add("us", "PRP");
            vocabularyData.Add("them", "PRP");
            vocabularyData.Add("my", "PRP");
            vocabularyData.Add("your", "PRP");
 
            vocabularyData.Add("because", "IN");
  
            vocabularyData.Add("eat", "VBP");
            vocabularyData.Add("eats", "VBZ");
            vocabularyData.Add("go", "VB");
            vocabularyData.Add("is", "VBZ");
            vocabularyData.Add("like", "VBP");

            // init phraseData
            // this data represents what phrases the chatbot has previously "learned" from a teacher
            phraseData.Add("response", "I see");

            string[] posVerbTypes = new string[] { "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };

            string response;
            bool firstTime = true;
            bool doneChatting = false;
            List<string> questionKeyList = new List<string>(questionData.Keys);
            List<string> phraseKeyList = new List<string>(phraseData.Keys);

            int numBotQuestionsAsked = 0;
            int numTotalBotQuestions = questionKeyList.Count;

            while (!doneChatting && (numBotQuestionsAsked < numTotalBotQuestions))
            {
                Random randomNumber = new Random();
                string randomKey = questionKeyList[randomNumber.Next(questionKeyList.Count)];
                bool validQuestion = true;
                bool questionNeedsInterpolation = false;

                // make sure to ask name at start
                if (firstTime)
                {
                    randomKey = "your name";
                    firstTime = false;
                }

                // check if we've already asked question
                if (responseData.ContainsKey(randomKey) && responseData[randomKey] == "")
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

                        if (response.Equals("goodbye"))
                            doneChatting = true;

                        Random randomPhraseNumber = new Random();
                        string randomPhraseKey = phraseKeyList[randomPhraseNumber.Next(phraseKeyList.Count)];
                        Console.WriteLine("bot    > " + phraseData[randomPhraseKey]);

                        // originally from chortlejs parsing
                        string[] responsePieces = response.Split(' ');
                        List<string> responsePiecesAsPOS = new List<string>();

                        foreach (string word in responsePieces)
                        {
                            if (vocabularyData.ContainsKey(word))
                                responsePiecesAsPOS.Add(vocabularyData[word]);
                            else
                                responsePiecesAsPOS.Add("UNKNOWN");
                        }

                        string responsePOS = string.Join(",", responsePiecesAsPOS);

                        // TODO: match actual last/final verb

                        Console.WriteLine("responsePOS: " + responsePOS);

                        int finalVerbPosition = 0;

                        // loop through POS and find last VBZ index and value
                        for (int posIndex = 0; posIndex < responsePiecesAsPOS.Count; posIndex++)
                        {
                            if (posVerbTypes.Contains(responsePiecesAsPOS[posIndex]))
                            {
                                finalVerbPosition = posIndex;
                            }
                        }

                        // general match with final verb at end (if verb exists)
                        Match matchPOS = Regex.Match(responsePOS, @"(.*)VBZ", RegexOptions.IgnoreCase);
                        Console.WriteLine(response);
                        List<string> generatedKeyList = new List<string>();
                        List<string> generatedValueList = new List<string>();
                        List<string> generatedValuePatternList = new List<string>();
                        if (matchPOS.Success)
                        {
                            Console.WriteLine("> found match!");

                            bool pastFinalVerb = false;

                            // divide up how we learn this data (key:value)
                            for (int userResponseIndex = 0; userResponseIndex < responsePiecesAsPOS.Count; userResponseIndex++)
                            {
                                // if after final verb (goes into value)
                                if (pastFinalVerb)
                                {
                                    if (vocabularyData.ContainsKey(responsePieces[userResponseIndex]))
                                    {
                                        generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                        generatedValuePatternList.Add(responsePiecesAsPOS[userResponseIndex]);
                                    }
                                    else
                                    {
                                        generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                        generatedValuePatternList.Add("UNKNOWN");
                                    }
                                }
                                // if before or equal to final verb (goes into key)
                                else
                                {
                                    generatedKeyList.Add(responsePiecesAsPOS[userResponseIndex]);
                                }

                                if (userResponseIndex == finalVerbPosition)
                                {
                                    // TODO: check for actual final (last in order) verb
                                    Console.WriteLine("found final verb: " + responsePiecesAsPOS[userResponseIndex]);
                                    pastFinalVerb = true;
                                }
                            }
                            
                            Console.WriteLine("generated key/value lists joined individually");
                            Console.WriteLine(string.Join(",", generatedKeyList));
                            Console.WriteLine(string.Join(",", generatedValueList));
                            Console.WriteLine(string.Join(",", generatedValuePatternList));
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



                        Console.WriteLine(">>> result: " + string.Join(" ", generatedValueList));
                        // save response data to dictionary
                        responseData[randomKey] = string.Join(" ", generatedValueList);
                        numBotQuestionsAsked++;
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

        public static void teacherMode()
        {
            Dictionary<string, Dictionary<string, double>> botLearnedResponses = new Dictionary<string, Dictionary<string, double>>();
            List<string> botLearnedKeyList = new List<string>(botLearnedResponses.Keys);

            Random randomNumber = new Random();
            String teacherResponse;
            String teacherDecision;
            String botResponse;

            const double maxWeight      = 1.0;
            const double midWeight      = 0.5;
            const double minWeight      = 0.0;
            const double incDecWeight   = 0.1;

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
                        if (Convert.ToDouble(weight) >= (midWeight+incDecWeight) && checkForBest)
                        {
                            if (debugMode)
                                Console.WriteLine("> found a good weight response");
                            botResponse = keyItem;
                            break;
                        }
                        else if (Convert.ToDouble(weight) >= midWeight && Convert.ToDouble(weight) < maxWeight)
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
                                        if (Convert.ToDouble(weight) >= (midWeight+incDecWeight) && checkForBest)
                                        {
                                            if (debugMode)
                                                Console.WriteLine("> found a good weight response");
                                            //Console.WriteLine("should be exiting here...");
                                            botResponse = keyItem;
                                            foundAResponse = true;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= midWeight && Convert.ToDouble(weight) < maxWeight)
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
                                        if (Convert.ToDouble(weight) >= (midWeight+incDecWeight) && checkForBest)
                                        {
                                            if (debugMode)
                                                Console.WriteLine("> found a good weight response");
                                            botResponse = keyItem;
                                            break;
                                        }
                                        else if (Convert.ToDouble(weight) >= midWeight && Convert.ToDouble(weight) < maxWeight)
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
                            if ((midWeight + teacherDecisionValue) > maxWeight)
                                teacherDecisionValue = maxWeight;
                            else if ((midWeight + teacherDecisionValue) < minWeight)
                                teacherDecisionValue = minWeight;

                            if (debugMode)
                                Console.WriteLine("> adding response to learned key ...");
                            botLearnedResponses[teacherResponse].Add(botResponse, (midWeight + teacherDecisionValue));
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
                    if ((midWeight + teacherDecisionValue) > maxWeight)
                        teacherDecisionValue = maxWeight;
                    else if ((midWeight + teacherDecisionValue) < minWeight)
                        teacherDecisionValue = minWeight;

                    botLearnedResponses[teacherResponse] = new Dictionary<string, double> {
                        {botResponse, (midWeight + teacherDecisionValue)}
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
            botAsk();

            Console.ReadLine();
        }
    }
}