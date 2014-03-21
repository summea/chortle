chortle
=======
a really simple chat bot

## Overview
Chortle is still very new to the human world and will take a while to get used to human conversation. Chortle will (often) listen to what a human says in conversation, but will not always know the best way to respond. For some words or phrases, however, Chortle has been taught common, contextual responses _by a teacher_ in an attempt to help Chortle along the path of becoming a better conversationalist.

In conversation _(chat mode)_, Chortle:

- knows when a human is making a statement
- knows when a human is asking a question
- will respond with learned phrases
- will respond with new response guesses
- has some questions to ask human
- will remember human answers to questions (relational data)
- can recall human answers to questions (relational data)
- has its own favorites (relational data)
- can respond in kind if human relational data matches Chortle relational data

In learning _(teacher mode)_, Chortle:

- waits for teacher to say a phrase
- listens to phrase and responds with a related phrase guess (if available)
- will take a guess if no previously-learned response is found
- waits for teacher response (yes: good response ... no: bad response)
- increases weight (usefulness) of response phrase, depending on teacher's response decision
- decreases weight (usefulness) of response phrase, depending on teacher's response decision
- will forget responses that are no longer useful (weight pruning)

## Phrases to Try with Chortle

- "What is your name?"
- "What are your hobbies?"
- ...and whatever else comes to mind! _(check out the [chortle-conversations](https://github.com/datanets/chortle/blob/master/chortle-conversations.md) archive for past gems...)_

## Chat Mode Example Output
### Overview
        __  __ __   ___   ____  ______  _        ___
       /  ]|  |  | /   \ |    \|      || |      /  _]
      /  / |  |  ||     ||  D  )      || |     /  [_
     /  /  |  _  ||  O  ||    /|_|  |_|| |___ |    _]
    /   \_ |  |  ||     ||    \  |  |  |     ||   [_
    \     ||  |  ||     ||  .  \ |  |  |     ||     |
     \____||__|__| \___/ |__|\_| |__|  |_____||_____|

    1) chat mode
    2) teacher mode
    3) quit
    1

    bot    > Lovely day, isn't it?
    bot    > What is your name?
    human  > andy
    bot    > Nice to meet you!
    human  > what is your name?
    bot    > chortle
    human  > nice to meet you
    human  > what are your hobbies?
    bot    > chatting
    human  > oh really
    human  > yes
    bot    > Oh, how lovely!
    human  > haha
    bot    > Do you like fruit?
    human  > yes
    bot    > Ah, I see.
    human  > that's cool
    human  > do you like food?
    bot    > What is your favorite food?
    human  > i like pizza
    bot    > What do you mean?
    human  > oh, a food that is bad for you
    bot    > Ah, I see.
    human  > yeah
    bot    > Be seeing you!
    human  > oh, okay
    bot    > Ah, I see.
    human  > bye
    bot    > Well, I need to be going!

    Want to see what I learned about you?
    yes


    learned relational data:
    your name
      is
        andy
    you
      like
        fruit
    your favorite food
      is
        pizza
    push enter to continue


### Learning About Human

    bot    > What is your name?
    human  > my name is andy bean
    bot    > I see
    my name is andy bean
    found final verb
    generated key/value lists joined individually
    PRP,UNKNOWN,VBZ
    andy,bean
    UNKNOWN,UNKNOWN
    >>> result: andy bean
    bot    > What is your favorite food?
    human  > green jelly
    bot    > I see
    green jelly
    >>> result: green jelly
    bot    > What is your favorite color?
    human  > red ruby
    bot    > I see
    red ruby
    >>> result: red ruby
    bot    > Do you like red ruby green jelly?
    human  > yes
    bot    > I see
    yes
    >>> result: yes


    learned information:
    your name - andy bean
    your favorite color - red ruby
    your favorite food - green jelly
    you like red ruby green jelly - yes


### Learning About Human (final verb check)
The *final verb check* is used to test if Chortle can find the intended _target verb_ in a given phrase (where there are two actual verbs provided, and the intended _target verb_ is the final verb in given phrase.) Once Chortle decides which verb to use, Chortle is able to parse out the intended subject, verb, and object of the given phrase.

    bot    > what is the way to go? (multiple verb test)
    human  > my way to go is over there
    bot    > I see
    responsePOS: PRP,UNKNOWN,UNKNOWN,VBZ,VBZ,UNKNOWN,UNKNOWN
    my way to go is over there
    > found match!
    found final verb: VBZ
    generated key/value lists joined individually
    PRP,UNKNOWN,UNKNOWN,VBZ,VBZ
    over,there
    UNKNOWN,UNKNOWN
    >>> result: over there


    learned information:
    way to go - over there
    
    
### Learning About Human (DT_X_VB check)
The *DT_X_VB check* is used to test if Chortle can find the correct object when a determiner is being used near the beginning of a phrase. For example, the initial question given by bot could be in a pattern such as: O-V-S (object-verb-subject). So the human response may be given in a similar O-V-S pattern... and if so, Chortle needs to be able to understand what the human is using for the subject and object of the overall phrase.

    bot    > What is the way to go? (multiple verb test)
    human  > that way is the way
    bot    > I see
    responsePOS: DT,UNKNOWN,VBZ,DT,UNKNOWN
    > found DT_X_VB!
    that way is the way
    > found match!
    right to left verb direction!
    found root verb: VBZ
    generated key/value lists joined individually
    generated key list: the,way
    generated value list: that,way
    generated value pattern list: DT,UNKNOWN
    >>> result: that way


    learned information:
    way to go - that way


## Teacher Mode Example Output
### Overview

    teacher  > hello
    bot      > See ya!
    teacher  > 1:yes, 2:no > 2
    teacher  > hello
    bot      > Lovely day, isn't it?
    teacher  > 1:yes, 2:no > 1
    teacher  > goodbye
    bot      > Oh, hey there!
    teacher  > 1:yes, 2:no > 2
    teacher  > goodbye
    bot      > Be seeing you!
    teacher  > 1:yes, 2:no > 1
    teacher  > goodbye
    bot      > Be seeing you!
    teacher  > 1:yes, 2:no > 1


    learned responses:
    *
      [What do you mean?, 0.5]
      [Hmm?, 0.5]
    hello
      [Oh, hey there!, 0.5]
      [Lovely day, isn't it?, 0.6]
      [Be seeing you!, 0.5]
      [See ya!, 0.4]
      [Nice to meet you!, 0.5]
    goodbye
      [Oh, hey there!, 0.4]
      [Lovely day, isn't it?, 0.5]
      [Be seeing you!, 0.7]
      [See ya!, 0.5]
      [Nice to meet you!, 0.5]
    i like
      [Good for you!, 0.5]
      [I'm sorry to hear that., 0.5]
      [Oh, how lovely!, 0.5]
      [Sounds great., 0.5]
      [Yes, I suppose you must be., 0.5]
      [Wow! That sounds exciting., 0.5]
    my name is
      [Oh, hey there!, 0.5]
      [Lovely day, isn't it?, 0.5]
      [Be seeing you!, 0.5]
      [See ya!, 0.5]
      [Nice to meet you!, 0.5]
    thanks
      [You are welcome!, 0.5]
      [Don't mention it., 0.5]
      [No problem., 0.5]
      [It's okay, 0.5]

      
### Debug Mode

    teacher  > hello there
    > finding response...
    > trying shorter key: hello there
    > found a response: False
    > picking from grab bag...
    > checking weight response...0.5 / What do you mean?
    > found an okay weight response
    > checking weight response...0.5 / Hmm?
    > found an okay weight response
    > finding response...
    > trying shorter key: hello
    > found this shorter key in learned responses: hello
    > checking weight response...0.5 / 0.5
    > checking an 'okay' response
    > found an okay weight response
    > checking weight response...0.5 / 0.5
    > checking an 'okay' response
    > found an okay weight response
    > checking weight response...0.5 / 0.5
    > checking an 'okay' response
    > found an okay weight response
    > checking weight response...0.5 / 0.5
    > checking an 'okay' response
    > found an okay weight response
    > checking weight response...0.5 / 0.5
    > checking an 'okay' response
    > found an okay weight response
    > botResponse: Nice to meet you!
    > found a response: False
    > picking from grab bag...
    > checking weight response...0.5 / What do you mean?
    > found an okay weight response
    > checking weight response...0.5 / Hmm?
    > found an okay weight response
    > finding response...
    > trying shorter key:
    > found a response: False
    > picking from grab bag...
    > checking weight response...0.5 / What do you mean?
    > found an okay weight response
    > checking weight response...0.5 / Hmm?
    > found an okay weight response
    bot      > Hmm?
    teacher  > 1:yes, 2:no > 2
    > adding new bot response key: Hmm?
    teacher  > hello there
    > found key: hello there
    > checking weight response...0.4 / Hmm?
    > found response: Nice to meet you!
    bot      > Nice to meet you!
    teacher  > 1:yes, 2:no > 1
    > adding response to learned key ...
    teacher  > hello there
    > found key: hello there
    > checking weight response...0.4 / Hmm?
    > found response: Wow! That sounds exciting.
    > checking weight response...0.6 / Nice to meet you!
    > found a good weight response
    bot      > Nice to meet you!
    teacher  > 1:yes, 2:no > 2
    > bot already knows this response... but let's update info
    teacher  > hello there
    > found key: hello there
    > checking weight response...0.5 / Nice to meet you!
    > found an okay weight response
    > checking weight response...0.4 / Hmm?
    > found response: Nice to meet you!
    bot      > Nice to meet you!
    teacher  > 1:yes, 2:no > 2
    > bot already knows this response... but let's update info
    teacher  > hello there
    > found key: hello there
    > checking weight response...0.4 / Hmm?
    > found response: Nice to meet you!
    > checking weight response...0.4 / Nice to meet you!
    > found response: Oh, hey there!
    bot      > Oh, hey there!
    teacher  > 1:yes, 2:no > 1
    > adding response to learned key ...


    learned responses:
    *
      [What do you mean?, 0.5]
      [Hmm?, 0.5]
    hello
      [Oh, hey there!, 0.5]
      [Lovely day, isn't it?, 0.5]
      [Be seeing you!, 0.5]
      [See ya!, 0.5]
      [Nice to meet you!, 0.5]
    goodbye
      [Oh, hey there!, 0.5]
      [Lovely day, isn't it?, 0.5]
      [Be seeing you!, 0.5]
      [See ya!, 0.5]
      [Nice to meet you!, 0.5]
    i like
      [Good for you!, 0.5]
      [I'm sorry to hear that., 0.5]
      [Oh, how lovely!, 0.5]
      [Sounds great., 0.5]
      [Yes, I suppose you must be., 0.5]
      [Wow! That sounds exciting., 0.5]
    my name is
      [Oh, hey there!, 0.5]
      [Lovely day, isn't it?, 0.5]
      [Be seeing you!, 0.5]
      [See ya!, 0.5]
      [Nice to meet you!, 0.5]
    thanks
      [You are welcome!, 0.5]
      [Don't mention it., 0.5]
      [No problem., 0.5]
      [It's okay, 0.5]
    hello there
      [Hmm?, 0.4]
      [Nice to meet you!, 0.4]
      [Oh, hey there!, 0.6]

      
## Notes
- some conversation response data comes from: http://en.wikibooks.org/wiki/English_in_Use/Conversation_Pieces
- parts of speech (POS) tags adhere to Illinois POS: http://cogcomp.cs.illinois.edu/demo/pos/?id=4
- POS key is located in [pos-key.txt](https://github.com/datanets/chortle/blob/master/pos-key.txt) for general reference