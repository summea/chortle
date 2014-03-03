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

