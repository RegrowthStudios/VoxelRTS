~~~+=======+
   |INSTALL|
   +=======+

-   Download And Install The XNA 4.0 Redistributable From This Location:
    http://www.microsoft.com/en-us/download/details.aspx?id=20914
-   Run "RTS.exe"
-   Select "Play Game" To Play The Game
-   "Army Painter" Is Not Developed Enough To Be Toyed With (Too Much Explaining To Do)
-   Selecting "Options" Will Exit The Game And Toggle Fullscreen Mode. It Must Be Manually Relaunched.
-   "Exit"... Your Last Resort

~~~+========+
   |CONTROLS|
   +========+

-   Move Mouse To Edge Of Screen
    =>  Move Camera Around The Map

-   Hold Down "q" And Move Mouse To Edge Of Screen
    =>  Orbit Camera

-   Scroll Mouse Wheel
    =>  Zoom Camera

-   Hold Down "e" And Right Click On The Map
    =>  Spawn Unit (Default Is Team 1 Type 1

-   Press "1"
    =>  Units Will Be Spawned To Team 1

-   Press "2"
    =>  Units Will Be Spawned To Team 2

-   Press "8"
    =>  Unit Type 1 Will Be Spawned

-   Press "9"
    =>  Unit Type 2 Will Be Spawned

-   Press "0"
    =>  Unit Type 3 Will Be Spawned

-   Use Left Mouse Button To Make A Selection Rectangle
    =>  Units On Team 1 In That Rectangle Will Be Selected (But They Don't Show Up Differently)

-   Right Click On Map With Units Selected
    =>  Selected Units Will Move To That Location

-   Right Click On A Unit With Units Selected
    =>  Selected Units Will Attack That Unit (Friendly Fire Is On And They Can Commit Suicide)

-   Press "~" Key
    =>  Toggle The Dev Console Showing Up

~~~+============+
   |DEV COMMANDS|
   +============+

-   Press Ctrl-V To Paste Whatever Is Currently In Your Clipboard
-   Press Ctrl-C To Copy The Text Currently Being Written In The Console
-   Press Enter To Send The Command

-   spawn [{0},{1},{2},{3},{4}]
    =>  {0} = Team With Zero-Based Indexing (Team 1 = 0, Team 2 = 1)
    =>  {1} = Unit Type With Zero-Based Indexing (Type 1 = 0, Type 2 = 1, Type 3 = 2)
    =>  {2} = Number Of Units To Spawns (Be Careful, This Can Crash The Game)
    =>  {3} = X Location
    =>  {4} = Z Location
    ~>  Ex. "spawn [0,0,400,100,100]" = Spawn 400 Of Unit Type 1 On Team 1 At Location (100,100)
-   avada kedavra
    =>  The Killing Curse (It Kills Everything On The Map By Dealing 9001 Damage ("It's Over 9000"))
    ~>  Ex. "avada kedavra" = I'm Voldemort And I Have No Nose

~~~+=====+
   |NOTES|
   +=====+

-   A Maximum Of 1000 Units Per Type Per Team Can Exist (Total Of 6000)
-   The Map Size Is 200x200 Units (Meters)

