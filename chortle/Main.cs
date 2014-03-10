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
            public static bool debugMode    = false;
            public static bool firstTime    = true;
            public static bool roundWeight  = true;

            public static List<string> humanResponseConversationData = new List<string>();

            // init dictionary data
            public static string taughtResponsesDataPath    = @"../../Data/bot-taught-responses.json";

            private static string questionDataSrc           = File.ReadAllText("../../Data/bot-questions.json");
            private static string vocabularyDataSrc         = File.ReadAllText("../../Data/vocabulary.json");
            private static string taughtResponsesDataSrc    = File.ReadAllText("../../Data/bot-taught-responses.json");
            private static string botFavoritesDataSrc       = File.ReadAllText("../../Data/bot-favorites.json");

            public static Dictionary<string, string> questionData   = JsonConvert.DeserializeObject<Dictionary<string, string>>(questionDataSrc);
            public static Dictionary<string, string> responseData = new Dictionary<string, string>();
            public static Dictionary<string, string> vocabularyData = JsonConvert.DeserializeObject<Dictionary<string, string>>(vocabularyDataSrc);
            public static Dictionary<string, Dictionary<string, double>> taughtResponseData = JsonConvert.DeserializeObject <Dictionary<string, Dictionary<string, double>>>(taughtResponsesDataSrc);
            public static Dictionary<string, string> phraseData = new Dictionary<string, string>();
            
            // meaning data
            // subject(verb, object)
            // you(like, green pears)
            public static Dictionary<string, Dictionary<string, List<string>>> relationalData = new Dictionary<string, Dictionary<string, List<string>>>();
            public static Dictionary<string, Dictionary<string, List<string>>> botRelationalData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(botFavoritesDataSrc);

            public static Dictionary<string, string> talkedAbout = new Dictionary<string, string>();

            public static string[] posSubjectTypes  = new string[] { "PRP", "WP", "WRB", "UNKNOWN", "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };
            public static string[] posObjectTypes   = new string[] { "NN", "UNKNOWN", "WP", "WRB", "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };
            public static string[] posVerbTypes     = new string[] { "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };
            public static string[] posDetVerbTypes  = new string[] { "DT,VB", "DT,VBD", "DT,VBG", "DT,VBN", "DT,VBP", "DT,VBZ" };

            // teacher decision weights
            public const double maxWeight       = 1.0;
            public const double midWeight       = 0.5;
            public const double minWeight       = 0.0;
            public const double incDecWeight    = 0.1;

            // bot states
            public const int BOT_NOP        = 0;
            public const int BOT_ASK        = 1;
            public const int BOT_RESPOND    = 2;
            public const int BOT_FOLLOW_UP  = 3;

            public static int botState = ChortleSettings.BOT_ASK;

            public static double pruneWeightsLowerLimit = 0.3;
        }

        public static Dictionary<string, string> botAsk(string questionKey, bool followUp=false)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string response     = "";
            result["question"]  = questionKey;
            result["answer"]    = "";
            bool validQuestion  = true;
            bool questionNeedsInterpolation = false;

            // make sure to ask name at start
            if (ChortleSettings.firstTime)
            {
                if (ChortleSettings.questionData.ContainsKey("your name"))
                    questionKey = "your name";
                ChortleSettings.firstTime = false;

                // add an initial response (e.g. "hello!")
                Dictionary<string, string> previousQA = new Dictionary<string, string>();
                previousQA["answer"] = "hello";
                botRespond(previousQA);
            }

            // add response key to response dictionary if it doesn't exist
            if (!ChortleSettings.responseData.ContainsKey(questionKey))
            {
                ChortleSettings.responseData.Add(questionKey, "");

                // add keys of interpolation data too...
                String interpolatedKey = questionKey;
                string patternInterpolations = @"({{[\w\s]+}})";
                foreach (Match match in Regex.Matches(questionKey, patternInterpolations, RegexOptions.IgnoreCase))
                {
                    String itemKey = match.Groups[0].ToString();
                    itemKey = itemKey.Replace("{", "").Replace("}", "");
                    if (!ChortleSettings.responseData.ContainsKey(itemKey))
                        ChortleSettings.responseData.Add(itemKey, "");
                }
            }

            // check if we've already asked question
            if ((ChortleSettings.responseData.ContainsKey(questionKey) && ChortleSettings.responseData[questionKey] == "") || followUp)
            {
                // check for "dynamic" patterns in question
                // requires that the bot has already asked about related patterns
                string pattern = @"({{[\w\s]+}})";
                foreach (Match match in Regex.Matches(ChortleSettings.questionData[questionKey], pattern, RegexOptions.IgnoreCase))
                {
                    //Console.WriteLine ("found tags to replace... {0}", match.Groups[0].Value);
                    //Console.WriteLine ("passing through...");
                    String item = match.Groups[0].ToString();
                    item = item.Replace("{", "").Replace("}", "");

                    // check if responseData contains required, previously-asked information
                    if (ChortleSettings.responseData.ContainsKey(item) && ChortleSettings.responseData[item] == "")
                    {
                        // sorry, still have to ask about this...
                        validQuestion = false;
                    }
                    else
                    {
                        questionNeedsInterpolation = true;
                    }
                }

                if (validQuestion)
                {
                    String questionText = ChortleSettings.questionData[questionKey];

                    if (questionNeedsInterpolation)
                    {
                        // interpolate "dynamic" patterns in question
                        String interpolatedString = ChortleSettings.questionData[questionKey];
                        string patternItem = @"({{[\w\s]+}})";
                        foreach (Match match in Regex.Matches(ChortleSettings.questionData[questionKey], patternItem, RegexOptions.IgnoreCase))
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
                    result["question"] = questionText;
                    Console.Write("human  > ");
                    response = Console.ReadLine();

                    // save human response to conversation data
                    ChortleSettings.humanResponseConversationData.Add(response);

                    //if (response.Equals("goodbye"))
                        //doneChatting = true;

                    // originally from chortlejs parsing
                    string[] questionPieces = questionKey.Split(' ');
                    List<string> questionPiecesAsPOS = new List<string>();

                    foreach (string word in questionPieces)
                    {
                        if (ChortleSettings.vocabularyData.ContainsKey(word))
                            questionPiecesAsPOS.Add(ChortleSettings.vocabularyData[word]);
                        else
                            questionPiecesAsPOS.Add("UNKNOWN");
                    }

                    string questionPOS = string.Join(",", questionPiecesAsPOS);

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

                    // TODO: find "target verb" from question and use as root verb?

                    if (ChortleSettings.debugMode)
                        Console.WriteLine("responsePOS: " + responsePOS);

                    string resultKeyValueDirection = "ltr";
                    string sentenceBeginning = "";
                    
                    int rootVerbPosition = 0;


                    // search for root verb
                    string questionRootVerb = "";
                    string questionRootVerbPOS = "";
                    int questionRootVerbIndex = 0;
                    for (int itemIndex = 0; itemIndex < questionPiecesAsPOS.Count; itemIndex++)
                    {
                        if (ChortleSettings.posVerbTypes.Contains(questionPiecesAsPOS[itemIndex]))
                        {
                            questionRootVerb = questionPieces[itemIndex];
                            questionRootVerbPOS = questionPiecesAsPOS[itemIndex];
                            questionRootVerbIndex = itemIndex;
                        }
                    }

                    // match: DT *** VB (that *** is ...)
                    Match matchDT_X_VB = Regex.Match(responsePOS, @"DT(.*)(VB*)", RegexOptions.IgnoreCase);
                    if (matchDT_X_VB.Success)
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> found DT_X_VB!");
                        // split match and find root verb (last verb in match)
                        string matchedPOSString = matchDT_X_VB.Groups[0].ToString();
                        string[] matchedPOSItems = matchedPOSString.Split(',');

                        resultKeyValueDirection = "rtl";

                        int leftSideMatchCount = 0;
                        for (int leftSidePieceIndex = 0; leftSidePieceIndex < questionPieces.Length; leftSidePieceIndex++)
                        {
                            if (responsePieces[leftSidePieceIndex].Equals(questionRootVerb))
                            {
                                break;
                            }
                            else
                            {
                                if (questionPieces.Contains(responsePieces[leftSidePieceIndex]))
                                    leftSideMatchCount++;
                            }
                        }

                        //Console.WriteLine();

                        int rightSideMatchCount = 0;

                        // make sure we have a right side to work with...
                        if ((questionRootVerbIndex + 1) < responsePieces.Length)
                        {
                            for (int rightSidePieceIndex = questionRootVerbIndex + 1; rightSidePieceIndex < questionPieces.Length; rightSidePieceIndex++)
                            {
                                //Console.WriteLine(" checking... " + responsePieces[rightSidePieceIndex]);
                                if (responsePieces[rightSidePieceIndex].Length > 0)
                                {
                                    if (!responsePieces[rightSidePieceIndex].Equals(questionRootVerb))
                                    {
                                        if (questionPieces.Contains(responsePieces[rightSidePieceIndex]))
                                            rightSideMatchCount++;
                                    }
                                }
                            }
                        }

                        //Console.WriteLine("@@@@ left count: " + leftSideMatchCount + " ... right count: " + rightSideMatchCount);
                        
                        if (leftSideMatchCount > rightSideMatchCount)
                            resultKeyValueDirection = "ltr";  
                    }

                    // check for WP/ VB*/ (e.g. "that is") statements
                    // check for DT,Verb type sentence openings
                    if (responsePiecesAsPOS.Count >= 2)
                        sentenceBeginning = responsePiecesAsPOS[0] + "," + responsePiecesAsPOS[1];

                    bool followUpFlag = false;
                    bool verbFound = true;

                    if (ChortleSettings.posDetVerbTypes.Contains(sentenceBeginning))
                    {
                        rootVerbPosition = 1;
                        resultKeyValueDirection = "rtl";
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> switching to RTL for verb key value parsing...");

                        // human response is unclear... ask a follow up question
                        followUpFlag = true;
                    }
                    // else this is a "common" verb statement
                    else
                    {
                        // loop through POS and find root VB* index and value
                        for (int posIndex = 0; posIndex < responsePiecesAsPOS.Count; posIndex++)
                        {
                            if (ChortleSettings.posVerbTypes.Contains(responsePiecesAsPOS[posIndex]))
                            {
                                rootVerbPosition = posIndex;
                            }
                        }
                    }

                    // general match with root verb (if verb exists)
                    Match matchPOS = Regex.Match(responsePOS, @"(.*)VB*", RegexOptions.IgnoreCase);
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
                                        Console.WriteLine("------ " + responsePieces[userResponseIndex] + " " + responsePieces[rootVerbPosition]);

                                    if (!responsePieces[userResponseIndex].Equals(questionRootVerb))
                                    {
                                        if (ChortleSettings.debugMode)
                                            Console.WriteLine("right to left verb direction!");

                                        if (!ChortleSettings.posVerbTypes.Contains(responsePiecesAsPOS[userResponseIndex]))
                                        {
                                            generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                                            generatedValuePatternList.Add(responsePiecesAsPOS[userResponseIndex]);
                                        }
                                    }
                                    else
                                    {
                                        pastRootVerb = true;
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
                        verbFound = false;
                        matchPOS = Regex.Match(responsePOS, @"(.*)", RegexOptions.IgnoreCase);
                        if (matchPOS.Success)
                        {
                            for (int userResponseIndex = 0; userResponseIndex < responsePiecesAsPOS.Count; userResponseIndex++)
                            {
                                generatedValueList.Add(responsePieces[userResponseIndex].ToLower());
                            }
                        }
                    }


                    if (followUpFlag)
                    {
                        result["answer"] = botFollowUp(questionKey);
                    }
                    else
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("verb found: " + verbFound.ToString());

                        // if no (obvious) verb is found, use question as part of key for bot response search
                        if (!verbFound)
                            result["combinedKey"] = result["question"].ToLower() + " " + string.Join(" ", generatedValueList);

                        result["answer"] = string.Join(" ", generatedValueList);
                        ChortleSettings.botState = ChortleSettings.BOT_RESPOND;
                    }

                    if (ChortleSettings.debugMode)
                        Console.WriteLine(">>> result: " + result["answer"]);

                    // save response data to dictionary
                    ChortleSettings.responseData[questionKey] = result["answer"];

                    // save relation data
                    // break down question into POS
                    if (ChortleSettings.debugMode)
                        Console.WriteLine(">>>>> Question POS: " + string.Join(",", questionPiecesAsPOS));

                    List<string> questionSubjects = new List<string>();
                    List<string> questionObjects = new List<string>();

                    // search for root verb
                    questionRootVerb = "";
                    questionRootVerbIndex = 0;
                    for (int itemIndex = 0; itemIndex < questionPiecesAsPOS.Count; itemIndex++)
                    {
                        if (ChortleSettings.posVerbTypes.Contains(questionPiecesAsPOS[itemIndex]))
                        {
                            questionRootVerb = questionPieces[itemIndex];
                            questionRootVerbIndex = itemIndex;
                        }
                    }


                    // no root verb found (implies "x is what")
                    if (string.IsNullOrWhiteSpace(questionRootVerb))
                    {
                        questionRootVerb = "is";

                        foreach (string item in questionPieces)
                        {
                            questionSubjects.Add(item);
                        }

                        questionObjects.Add("what");
                    }
                    else
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("current root verb: " + questionRootVerb);

                        // search for subject                    
                        for (int itemIndex = 0; itemIndex < questionRootVerbIndex; itemIndex++)
                        {
                            if (ChortleSettings.posSubjectTypes.Contains(questionPiecesAsPOS[itemIndex]))
                            {
                                questionSubjects.Add(questionPieces[itemIndex]);
                            }
                        }

                        // search for object
                        if ((questionRootVerbIndex + 1) <= questionPiecesAsPOS.Count)
                        {
                            for (int itemIndex = questionRootVerbIndex+1; itemIndex < questionPiecesAsPOS.Count; itemIndex++)
                            {
                                if (ChortleSettings.posObjectTypes.Contains(questionPiecesAsPOS[itemIndex]))
                                {
                                    questionObjects.Add(questionPieces[itemIndex]);
                                }
                            }
                        }
                    }

                    string questionSubject = string.Join(" ", questionSubjects);
                    string questionObject = string.Join(" ", questionObjects);


                    // interpolate "dynamic" patterns in question
                    String interpolatedStringTwo = questionObject;
                    string patternItemTwo = @"({{[\w\s]+}})";
                    foreach (Match match in Regex.Matches(questionObject, patternItemTwo, RegexOptions.IgnoreCase))
                    {
                        String itemKey = match.Groups[0].ToString();
                        itemKey = itemKey.Replace("{", "").Replace("}", "");
                        interpolatedStringTwo = interpolatedStringTwo.Replace(match.Groups[0].ToString(), ChortleSettings.responseData[itemKey]);
                    }

                    questionObject = interpolatedStringTwo;


                    if (ChortleSettings.debugMode) {
                        Console.WriteLine("> found question subject: " + questionSubject);
                        Console.WriteLine("> found question (root) verb: " + questionRootVerb);
                        Console.WriteLine("> found question object: " + questionObject);
                    }

                    if (!string.IsNullOrWhiteSpace(questionSubject))
                    {
                        List<string> values = new List<string>();

                        // key exists
                        if (ChortleSettings.relationalData.ContainsKey(questionSubject))
                        {
                            //ChortleSettings.relationalData[questionSubject] = new Dictionary<string, List<string>>();
                            values = ChortleSettings.relationalData[questionSubject][questionRootVerb];
                        }
                        // create new key
                        else
                        {
                            ChortleSettings.relationalData[questionSubject] = new Dictionary<string, List<string>>();
                            values = new List<string> { questionObject };
                        }
                            
                        if (questionRootVerb == "is")
                        {
                            values = new List<string> { result["answer"] };
                        }
                        else
                        {
                            // TODO: add better check for negation
                            Match answerMatch = Regex.Match(result["answer"], @"no", RegexOptions.IgnoreCase);

                            if (answerMatch.Success)
                            {
                                // TODO: add better list of inflections for negation
                                questionRootVerb = "don't " + questionRootVerb;
                            }
                        }

                        ChortleSettings.relationalData[questionSubject][questionRootVerb] = values;
                    }
                }
            }
            return result;
        }

        public static string botFollowUp(string keyInQuestion)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (ChortleSettings.debugMode)
                Console.WriteLine("follow up question about: " + keyInQuestion);
            result = botAsk(keyInQuestion, true);
            return result["answer"];
        }

        public static string botRespond(Dictionary<string, string> phraseData)
        {
            string result = "";

            // formulate a response from learned responses
            result = botFormulateResponse(phraseData);

            // respond
            Console.WriteLine("bot    > " + result);

            ChortleSettings.botState = ChortleSettings.BOT_ASK;

            return result;
        }

        public static void chatMode()
        {
            bool doneChatting = false;
            List<string> questionKeyList = new List<string>(ChortleSettings.questionData.Keys);
            List<string> phraseKeyList = new List<string>(ChortleSettings.phraseData.Keys);

            int numBotQuestionsAsked = 0;
            int numTotalBotQuestions = questionKeyList.Count;

            Dictionary<string, string> previousQA = new Dictionary<string, string>();

            while (!doneChatting && (numBotQuestionsAsked < numTotalBotQuestions))
            {
                switch (ChortleSettings.botState)
                {
                    case ChortleSettings.BOT_NOP:
                        break;
                    case ChortleSettings.BOT_ASK:
                        Random randomNumber = new Random();
                        string randomKey = questionKeyList[randomNumber.Next(questionKeyList.Count)];
                        previousQA = botAsk(randomKey);
                        if (!string.IsNullOrEmpty(previousQA["answer"]))
                        {
                            numBotQuestionsAsked++;

                            // bot trying really hard to feel accepted...
                            // check if a relation can help with this question

                            // TODO: use variables here

                            if (!ChortleSettings.talkedAbout.ContainsKey("i like fruit"))
                            {
                                ChortleSettings.talkedAbout.Add("i like fruit", "");
                            }

                            if (ChortleSettings.talkedAbout.ContainsKey("i like fruit") && !ChortleSettings.talkedAbout["i like fruit"].Equals("yes"))
                            {
                                if (ChortleSettings.relationalData.ContainsKey("you"))
                                {
                                    if (ChortleSettings.relationalData["you"].ContainsKey("like"))
                                    {
                                        if (ChortleSettings.relationalData["you"]["like"].Contains("fruit"))
                                        {

                                            if (ChortleSettings.botRelationalData.ContainsKey("i"))
                                            {
                                                if (ChortleSettings.botRelationalData["i"].ContainsKey("like"))
                                                {
                                                    if (ChortleSettings.botRelationalData["i"]["like"].Contains("fruit"))
                                                    {
                                                        Console.WriteLine("bot    > Oh, I like fruit too, lol");
                                                        ChortleSettings.talkedAbout["i like fruit"] = "yes";
                                                    }
                                                }
                                            }
                                            // skip extra response and go directly back to asking a question
                                            ChortleSettings.botState = ChortleSettings.BOT_ASK;
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    case ChortleSettings.BOT_FOLLOW_UP:
                        break;
                    case ChortleSettings.BOT_RESPOND:
                        botRespond(previousQA);
                        break;
                }
            }

            Console.WriteLine("bot    > Well, I need to be going!");

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

            string showRelationalValues = "no";
            if (!ChortleSettings.debugMode)
            {
                Console.WriteLine("\nWant to see what I learned about you?");
                showRelationalValues = Console.ReadLine();
            }

            // print out relational values
            if (ChortleSettings.debugMode || (!ChortleSettings.debugMode && !showRelationalValues.Equals("no")))
            {
                Console.WriteLine("\n\nlearned relational data:");

                foreach (KeyValuePair<string, Dictionary<string, List<string>>> item in ChortleSettings.relationalData)
                {
                    Console.WriteLine(item.Key);

                    foreach (KeyValuePair<string, List<string>> innerItem in item.Value)
                    {
                        Console.WriteLine(innerItem.Key);
                        Console.WriteLine(string.Join(",", innerItem.Value));
                    }
                }
            }

        }

        public static string botFormulateResponse(Dictionary<string, string> topicPhraseData)
        {
            string botResponse = "";
            List<string> botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
            Random randomNumber = new Random();
            string searchKey = "";

            if (topicPhraseData.ContainsKey("answer"))
                searchKey = topicPhraseData["answer"];
            if (topicPhraseData.ContainsKey("combinedKey"))
                searchKey = topicPhraseData["combinedKey"];

            if (ChortleSettings.debugMode)
                Console.WriteLine("]] searching for: " + searchKey);

            // bot tries out a response
            if (ChortleSettings.taughtResponseData.ContainsKey(searchKey))
            {
                if (ChortleSettings.debugMode)
                    Console.WriteLine("> found key: " + searchKey);

                bool checkForBest = true;

                // Add some randomness in how responses are initially found (ordered)
                Random rndNumber = new Random();
                var randomizedItems = from pair in ChortleSettings.taughtResponseData[searchKey]
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
                    var weight = ChortleSettings.taughtResponseData[searchKey][keyItem];
                    if (ChortleSettings.debugMode)
                        Console.WriteLine("> checking weight response..." + weight + " / " + keyItem);

                    // if weight is high enough, return found response as bot response
                    if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> found a good weight response");
                        botResponse = keyItem;
                        break;
                    }
                    else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                    {
                        if (checkForBest)
                        {
                            if (ChortleSettings.debugMode)
                                Console.WriteLine("> found an okay weight response");
                            botResponse = keyItem;
                        }
                        else
                        {
                            if (ChortleSettings.debugMode)
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
                                botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
                                String randomKey = botLearnedKeyList[randomNumber.Next(ChortleSettings.taughtResponseData.Count)];

                                if (ChortleSettings.taughtResponseData[randomKey].Count > 0)
                                {
                                    // Add some randomness in how guesses happen
                                    // (when bot doesn't have a response... try guessing at another topic response)
                                    Random rndNumberForGuess = new Random();
                                    var randomizedTopicsForGuess = from pair in ChortleSettings.taughtResponseData[randomKey]
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
                                        if (ChortleSettings.taughtResponseData.ContainsKey(randomKey) && ChortleSettings.taughtResponseData[randomKey].Count > 0)
                                        {
                                            var foundResponse = keyItemForGuess;
                                            if (ChortleSettings.debugMode)
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
                                            botResponse = searchKey;
                                        }
                                    }
                                }
                                else
                                {
                                    // repeat what teacher said
                                    botResponse = searchKey;
                                }
                                break;
                            default:
                                // repeat what teacher said
                                botResponse = searchKey;
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
                string[] teacherResponsePieces = searchKey.Split(' ');
                string[] teacherResponsePiecesWorkingCopy = searchKey.Split(' ');
                string shorterKey = "";
                bool foundAResponse = false;

                for (int teacherResponsePieceIndex = teacherResponsePieces.Length; teacherResponsePieceIndex >= 0; --teacherResponsePieceIndex)
                {
                    if (!foundAResponse)
                    {
                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> finding response...");
                        shorterKey = "";

                        // build shorterKey from longest to shortest key possible
                        for (int buildKeyIndex = 0; buildKeyIndex < teacherResponsePieceIndex; buildKeyIndex++)
                        {
                            shorterKey += teacherResponsePieces[buildKeyIndex] + " ";
                        }
                        shorterKey = shorterKey.TrimEnd(' ');

                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> trying shorter key: " + shorterKey);

                        if (ChortleSettings.taughtResponseData.ContainsKey(shorterKey))
                        {
                            if (ChortleSettings.debugMode)
                                Console.WriteLine("> found this shorter key in learned responses: " + shorterKey);

                            // bot tries out a response
                            if (ChortleSettings.taughtResponseData.ContainsKey(shorterKey))
                            {
                                List<string> botLearnedKeyValueList = new List<string>(ChortleSettings.taughtResponseData[shorterKey].Keys);

                                bool checkForBest = true;
                                foreach (string keyItem in botLearnedKeyValueList)
                                {
                                    var weight = ChortleSettings.taughtResponseData[shorterKey][keyItem];
                                    if (ChortleSettings.debugMode)
                                        Console.WriteLine("> checking weight response..." + weight + " / " + ChortleSettings.taughtResponseData[shorterKey][keyItem]);

                                    // if weight is high enough, return found response as bot response
                                    if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                                    {
                                        if (ChortleSettings.debugMode)
                                            Console.WriteLine("> found a good weight response");
                                        //Console.WriteLine("should be exiting here...");
                                        botResponse = keyItem;
                                        foundAResponse = true;
                                        break;
                                    }
                                    else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                                    {
                                        if (ChortleSettings.debugMode)
                                            Console.WriteLine("> checking an 'okay' response");
                                        if (checkForBest)
                                        {
                                            if (ChortleSettings.debugMode)
                                                Console.WriteLine("> found an okay weight response");
                                            botResponse = keyItem;
                                            foundAResponse = true;
                                            //break;
                                        }
                                        else
                                        {
                                            if (ChortleSettings.debugMode)
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
                                                botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
                                                String randomKey = botLearnedKeyList[randomNumber.Next(ChortleSettings.taughtResponseData.Count)];

                                                botLearnedKeyValueList = new List<string>(ChortleSettings.taughtResponseData[randomKey].Keys);
                                                String randomValueKey = botLearnedKeyValueList[randomNumber.Next(ChortleSettings.taughtResponseData[randomKey].Count)];

                                                if (ChortleSettings.taughtResponseData.ContainsKey(randomKey) && ChortleSettings.taughtResponseData[randomKey].Count > 0)
                                                {
                                                    //var foundResponse = taughtResponseData[randomKey][randomValueKey];
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
                                if (ChortleSettings.debugMode)
                                    Console.WriteLine("> botResponse: " + botResponse);
                            }
                        }

                        if (ChortleSettings.debugMode)
                            Console.WriteLine("> found a response: " + foundAResponse);

                        if (!foundAResponse)
                        {
                            if (ChortleSettings.debugMode)
                                Console.WriteLine("> picking from grab bag...");

                            // save new key first
                            ChortleSettings.taughtResponseData[searchKey] = new Dictionary<string, double>();

                            // otherwise... pick from the * (grab bag) responses
                            if (ChortleSettings.taughtResponseData.ContainsKey("*"))
                            {
                                //teacherResponse = "*";

                                List<string> botLearnedKeyValueList = new List<string>(ChortleSettings.taughtResponseData["*"].Keys);
                                //String randomValueKey = botLearnedKeyValueList[randomNumber.Next(taughtResponseData[randomKey].Count)];

                                bool checkForBest = true;
                                foreach (string keyItem in botLearnedKeyValueList)
                                {
                                    var weight = ChortleSettings.taughtResponseData["*"][keyItem];
                                    if (ChortleSettings.debugMode)
                                        Console.WriteLine("> checking weight response..." + weight + " / " + keyItem);

                                    // if weight is high enough, return found response as bot response
                                    if (Convert.ToDouble(weight) >= (ChortleSettings.midWeight + ChortleSettings.incDecWeight) && checkForBest)
                                    {
                                        if (ChortleSettings.debugMode)
                                            Console.WriteLine("> found a good weight response");
                                        botResponse = keyItem;
                                        break;
                                    }
                                    else if (Convert.ToDouble(weight) >= ChortleSettings.midWeight && Convert.ToDouble(weight) < ChortleSettings.maxWeight)
                                    {
                                        if (checkForBest)
                                        {
                                            if (ChortleSettings.debugMode)
                                                Console.WriteLine("> found an okay weight response");
                                            botResponse = keyItem;
                                        }
                                        else
                                        {
                                            if (ChortleSettings.debugMode)
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
                                                botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
                                                String randomKey = botLearnedKeyList[randomNumber.Next(ChortleSettings.taughtResponseData.Count)];

                                                botLearnedKeyValueList = new List<string>(ChortleSettings.taughtResponseData[randomKey].Keys);
                                                String randomValueKey = botLearnedKeyValueList[randomNumber.Next(ChortleSettings.taughtResponseData[randomKey].Count)];

                                                if (ChortleSettings.taughtResponseData.ContainsKey(randomKey) && ChortleSettings.taughtResponseData[randomKey].Count > 0)
                                                {
                                                    //var foundResponse = taughtResponseData[randomKey][randomValueKey];
                                                    var foundResponse = randomValueKey;
                                                    botResponse = foundResponse;
                                                }
                                                else
                                                {
                                                    // repeat what teacher said
                                                    botResponse = searchKey;
                                                }
                                                break;
                                            default:
                                                // repeat what teacher said
                                                botResponse = searchKey;
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // repeat what teacher said
                                botResponse = searchKey;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return botResponse;
        }

        public static void teacherMode()
        {
            List<string> botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
            
            String teacherResponse;
            String teacherDecision;
            String botResponse;

            // topic phrase format:
            // phrase, weight
            //
            // weight values:
            // good = increment weight by incDecWeight
            // bad  = decrement weight by incDecWeight

            // init taughtResponseData
            // init topics

            int loopLimit = 5;

            for (int i = 0; i < loopLimit; i++)
            {
                botResponse = "";
                botLearnedKeyList = new List<string>(ChortleSettings.taughtResponseData.Keys);
                // teacher says something
                Console.Write("teacher  > ");
                teacherResponse = Console.ReadLine();

                Dictionary<string, string> combinedResponse = new Dictionary<string, string>();
                combinedResponse["question"] = teacherResponse;
                combinedResponse["answer"] = teacherResponse;
                combinedResponse["combined"] = teacherResponse;

                // bot formulates a response
                botResponse = botFormulateResponse(combinedResponse);

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
                if (ChortleSettings.taughtResponseData.ContainsKey(teacherResponse))
                {
                    firstTimeForThisTopic = false;
                    //currentValuesList = taughtResponseData[teacherResponse];
                    currentValuesList = new List<string>(ChortleSettings.taughtResponseData[teacherResponse].Keys);

                    if (currentValuesList.Count > 0)
                    {
                        bool foundResponseInValuesList = false;

                        List<string> botLearnedKeyValueList = new List<string>(ChortleSettings.taughtResponseData[teacherResponse].Keys);
                        //String randomValueKey = botLearnedKeyValueList[randomNumber.Next(taughtResponseData[randomKey].Count)];

                        // TODO: if teacher says "no", save and try another response?

                        foreach (string keyItem in botLearnedKeyValueList)
                        {
                            //var foundResponse = taughtResponseData[teacherResponse][keyItem];
                            if (botResponse.Equals(keyItem))
                            {
                                // make sure we are within 0.0 and 1.0 limits for response weight values
                                if ((ChortleSettings.taughtResponseData[teacherResponse][botResponse] + teacherDecisionValue <= 1.0) &&
                                (ChortleSettings.taughtResponseData[teacherResponse][botResponse] + teacherDecisionValue >= 0.0))
                                {
                                    double weightTotal = ChortleSettings.taughtResponseData[teacherResponse][botResponse] + teacherDecisionValue;

                                    if (ChortleSettings.roundWeight)
                                        weightTotal = Math.Round(weightTotal, 2);

                                    ChortleSettings.taughtResponseData[teacherResponse][botResponse] = weightTotal;

                                    // prune entry if weight falls below lower limit
                                    if (weightTotal < ChortleSettings.pruneWeightsLowerLimit)
                                    {
                                        if (ChortleSettings.debugMode)
                                            Console.WriteLine("> pruning \"" + botResponse + "\" from \"" + teacherResponse + "\"");
                                        ChortleSettings.taughtResponseData[teacherResponse].Remove(botResponse);
                                    }
                                }
                                if (ChortleSettings.debugMode)
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

                            if (ChortleSettings.debugMode)
                                Console.WriteLine("> adding response to learned key ...");
                            ChortleSettings.taughtResponseData[teacherResponse].Add(botResponse, (ChortleSettings.midWeight + teacherDecisionValue));
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
                    if (ChortleSettings.debugMode)
                        Console.WriteLine("> adding new bot response key: " + botResponse);

                    // ensure that weight values are not above or below limits (minWeight and maxWeight)
                    if ((ChortleSettings.midWeight + teacherDecisionValue) > ChortleSettings.maxWeight)
                        teacherDecisionValue = ChortleSettings.maxWeight;
                    else if ((ChortleSettings.midWeight + teacherDecisionValue) < ChortleSettings.minWeight)
                        teacherDecisionValue = ChortleSettings.minWeight;

                    double weightTotal = ChortleSettings.midWeight + teacherDecisionValue;

                    if (ChortleSettings.roundWeight)
                        weightTotal = Math.Round(weightTotal, 2);

                    ChortleSettings.taughtResponseData[teacherResponse] = new Dictionary<string, double> {
                        {botResponse, weightTotal}
                    };

                    firstTimeForThisTopic = false;
                }
            }

            // save changes back to data file
            string json = JsonConvert.SerializeObject(ChortleSettings.taughtResponseData, Formatting.Indented);
            File.WriteAllText(ChortleSettings.taughtResponsesDataPath, json);

            Console.WriteLine("\n\nlearned responses:");
            foreach (var key in ChortleSettings.taughtResponseData.Keys)
            {
                Console.WriteLine("{0}", key);
                var value = ChortleSettings.taughtResponseData[key];

                foreach (var innerKey in value)
                {
                    Console.WriteLine("  {0}", innerKey);
                }
            }

            Console.WriteLine();
        }

        public static void Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("Menu");
                Console.WriteLine("1) chat mode");
                Console.WriteLine("2) teacher mode");
                Console.WriteLine("3) quit");
                string choice = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine("\n");

                switch (choice)
                {
                    case "1":
                        chatMode();
                        break;
                    case "2":
                        teacherMode();
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        chatMode();
                        break;
                }

                Console.WriteLine("push enter to continue");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}