TODO
====
- add more vocabulary
- add more responses for the * taught response?
- fix: take out times where human writes something and bot doesn't respond? (this is natural, though...)


Done
====
- fixed: bot says "i like * too" after starting program over (in-app) (was an init issue... talkedAbout)
- fixed: learned relational data is not picking up new objects (your name is _what_) (it was the order of the questionRootVerb part of the algorithm...)
+ fixed: "don't like" relational data not saving
+ fixed: multiple "like" relational data not saving
+ fixed: "i like fruit" strings and keys with variables
+ added: times where human can ask a question instead of responding with a statement (though this is really simple for now...)
+ added: matched goodbye phrases end conversation
+ added: times where bot just says something and human can respond (naive implementation)
+ fixed: learned responses are accidentally being added to taught responses... (after doing: taught -> chat modes) (using init() to reset some settings)
+ fixed: going back to chat mode after one run
+ added: bot-favorites data file (json)
+ bot only respond once (for now) about a particular similar like
+ teacher: prune responses that fall below a certain weight? (0.3)
+ find a way to not need to match question/responses in two separate data files...
+ fixed: relational data for "you like {{ x }} {{ y }}" (object should pick up both x and y...)
  + be able to parse answer that contains "no" (basic) and save results... like/don't like
  + interpolate question for relational data part...
  + know when to use question object "green beans" vs answer
- fixed: "that is" (way to go question) (right side check)
+ fixed: "that way is the way to go" (make sure to stop collecting values after root verb...)
+ fixed: "the way to go is over there" (this is finding "go" before "is"... maybe time to go back to target verb idea... take verb from question and match?)
+ fixed: "the way to go is over there" OK
+ fixed: "that is the way to go" OK  <-- this will re-ask question to try to get a more definite answer...
+ fixed: "that way is the way to go" OK
+ fix rounding (lack of) bug for double values for bot-taught-response data save
+ fix weight boundary checks (for some reason I'm getting larger than 1.0 weights...)
+ when UNKNOWN POS is found for a one-word answer (or something without verbs) use question text as key for bot response formulation
+ teacher: save teacher mode data back to taught-responses json
+ added bot respond state: using previously-taught expressions (try to find closest-matching key...)
+ break up main.cs into smaller functions
+ menu for chatMode/teacherMode
+ convert learnedResponses (taught responses) to json
+ botFollowUp: follow-up (repeat question until satisfactory answer is provided)
+ catch multiple words before "is" (verb) in this example: "that way is the way"
+ grouped settings under ChortleSettings class
+ create some sort of pooled dictionary location (learned responses + questions)
+ keep track of previous human response conversation data in some sort of stack
+ botAsk: added "right to left" verb pattern (i.e. "what is ...? _that is_ ...) ("_that is_ the way to go" (key=the way to go, value=that))
+ botAsk: find actual final verb in POS parsing
+ botAsk: bring over parser from chortlejs to match parts of speech (POS)
+ teacher: instead of having yes/maybe/no, what about having just yes/no (and increment/decrement accordingly until 0 or 1)?
+ updated question process:
  + what is {{your name}}?
  + get response
  + save response to newly-added key in responseData
+ fixed: when adding a new phrase key, I end up getting weights as "text" keys because there are no other choices
+ guess at another response topic when no clear, existing response is found
+ abstract out responses into groups (reduce the amount of duplicate data lines...)
+ randomize the responses (so that the first one doesn't always get picked first...)
+ pick response from grab bag for newly-created phrase keys
+ convert 0/word to dictionary (for efficiency...) word:0
+ store newly-learned keys
+ use short string if key is found (first try)
+ if learnedResponse key is not found, try looking for a slightly smaller part of the key (i like green things -> i like green -> i like)


Future
======
- teacher: ultimately, try mixing learned responses and get teacher feedback
- teacher: merge patterns from previous work into the new teacher project so that bot can start to build phrase keys like: learned["NN ..."] instead of a hard coded phrase
- teacher: go back to possible response list more often and get new response possibility if available (especially if the response is a 1)?
- support for: "it's" and "that's" (it is, that is)
- punctuation check: ? is a question
- punctuation check: . (or nothing) is a statement
- break out interpolation code into a function
- questions: somehow abstract out the questions list into something based on POS?
- use saved relational data in other questions... (maybe do a check to see if relational data key exists relating to answer)
- fix: don't add extra bot response when saying the "i like fruit" thing


Extra
=====
- add a way to look for related target verb (from the question)
- maybe check how many words are in front of the verbs and after... and determine which one is more likely the more important verb? (the side with the least amount of words might be the target...)
- (I think this was added... but just in case)
  - determine how to know when answer is on left or right of root verb
    - one approach: use determiner+something to show that answer is on opposite side of root verb
    - problem: what if the question has determiner+something?
    - two approach: check which side contains 50% or more of the original question pieces?
    - problem: partial phrase will throw this off...