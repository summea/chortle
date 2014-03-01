chortle
=======

a really simple chat bot

Teacher Example Output:
-----------------------

    teacher  > i like red
    finding response...
    trying shorter key: i like red
    finding response...
    trying shorter key: i like
    found this shorter key in learned responses: i like
    checking weight response...1 / Good for you!
    found a good weight response
    bot      > Good for you!
    teacher  > 1:yes, 2:maybe, 3:no > 3
    teacher  > i like red
    found key: i like red
    checking weight response...0 / Good for you!
    bot      > Good for you!
    teacher  > 1:yes, 2:maybe, 3:no > 3
    bot already knows this response... but let's update info
    teacher  > i like
    found key: i like
    checking weight response...1 / Good for you!
    found a good weight response
    bot      > Good for you!
    teacher  > 1:yes, 2:maybe, 3:no > 3
    bot already knows this response... but let's update info
    teacher  > i like
    found key: i like
    checking weight response...0 / Good for you!
    checking weight response...1 / I'm sorry to hear that.
    found a good weight response
    bot      > I'm sorry to hear that.
    teacher  > 1:yes, 2:maybe, 3:no > 1
    bot already knows this response... but let's update info


    learned information:
    your name -
    your favorite color -
    your favorite food -
    you like {{your favorite color}} {{your favorite food}} -


    learned responses:
    hello - 1/oh hey there!,1/lovely day, isn't it?
    goodbye - 1/be seeing you!,1/see ya?
    i like - 0/Good for you!,1/I'm sorry to hear that.,1/Oh, how lovely!,1/Sounds gr
    eat.,1/Yes, I suppose you must be.,1/Wow! That sounds exciting.
    i like red - 0/Good for you!


Learning Example Output:
------------------------

    bot    > What is your name?
    human  > My name is Jim.
    bot    > I see
    bot    > What is your favorite food?
    human  > My favorite food is pasta.
    bot    > I see
    bot    > What is your favorite color?
    human  > My favorite color is orange.
    bot    > I see
    bot    > Do you like orange pasta?
    human  > Yes
    bot    > I see


    learned information:
    your name - jim
    your favorite color - orange
    your favorite food - pasta
    you like {{your favorite color}} {{your favorite food}} - yes
