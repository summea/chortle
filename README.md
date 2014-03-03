chortle
=======

a really simple chat bot

Teacher Example Output:
-----------------------

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


      
Teacher Example Output (debug mode):
------------------------------------

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

      

Learning Example Output:
------------------------

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
    you like {{your favorite color}} {{your favorite food}} - yes


    
Learning Example Output (final verb check)
------------------------------------------

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
