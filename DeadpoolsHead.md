I wanted to update my demos when speaking about Blazm.Bluetooth.
I found Deadpools head from Hasbro and I got really excited.
Sadly there are no public APIs and no documentation, this opened a whole new rabbit hole for me.
I was able to get a Bluetooth connection to a device, but I couldn't get it to work.
But after testing and debugging Bluetooth commands I figured a bunch of things out, In this document I will document my finds.

## The device
The device is a model of Deadpools head, everything from the box it comes in to the device itself is really well thought out.Evertything is in a deadpool theme (quirky).
Eyes and mouth moves as well as the whole head, super well done robotics.

The name of the device is **WilsonBLE**.

## Services & Characteristics

It has 2 services with a couple of characteristics each  
**7cd80aa0-b9ec-42cb-9ace-10389f7e2576**  
7cd801b0-b9ec-42cb-9ace-10389f7e2576 Read,WriteWithoutResponse  
7cd802b0-b9ec-42cb-9ace-10389f7e2576 Read,Notify   
**00001800-0000-1000-8000-00805f9b34fb**  
00002a00-0000-1000-8000-00805f9b34fb Read  
00002a01-0000-1000-8000-00805f9b34fb Read  
00002a04-0000-1000-8000-00805f9b34fb Read  

When I saw this I assumed that the app is able to send commands to the head, and the head probably can send commands (notifications) back to the app.
The two characteristics that we are interested in is **01b0** and **02b0** and since both of them ar located in the same Service we are only interested in that service.

Next, I rigged my Mac and Iphone to capture Bluetooth (see blog post) and opened the log in Wireshark.
The app seems to have an authentication step, when the user should press a button on the bottom of the head.
Looking at the data coming back an forth from the head I didn't see any difference between the commands befor and after so I guessed that the authentication part just might be in the app only.

There are 3 segments that I have found when it comes to phrases.
These segments are controlled by the last byte. The phrases has different categories and looking in the app I have tried to categorize them as they are beeing used in the Hasbro app.
All of the starts with byte 0x17 followed be 0x00, changing these doesn't make any difference (except nothing happens).  
0x17,0x00, code, segment
 
### Segment 0x00
Code | Category | Phrase
--- | --- |--- |

### Segment 0x01
Code | Category | Phrase
--- | --- |--- |

### Segment 0x02
Code | Category | Phrase
--- | --- |--- |
00 |  |zzZZ o Logan I love it when you call me bub
01 |  |psst how about we make out way down to the danger room?
02 | |  I'll be the captain of the football team and you'll be wolverine
03 |  Food |  Hey can I get a double paddy wolverine style
04 |  Food | Is that gluten-free? Gluten makes me gassy
05 |  Food | Cough garson if this isn't as hot as???????
06 |  Food | Hey bob can you read the specials in your best wolverine voice?
07 |  Food | Cough I need it extra spicy
?08 | Food | 
09 | Food | Hahaha do you sell hot-dog water?
10 | Food | Okey, would you please pass the, you know, everything I have no arms
11 | Food | hahaha quick I need some beefy chilly blasten straight into my face hole aaaaaah
12 | Food | ooh Got any ranch dressing? I need a whole lo of ranchdressing, give med some D, D is for ranch dressing.
13 | Food | ooh Got any ranch dressing? I need a whole lo of ranchdressing, give med some D, D is for diabeties
14 | |Oh I forgot I gotta take squirellpool out of the blender
15 | |I gotta go, I gotta take sabertooth for a walk
16 | |Peace out you mother father, yep this is how we swear on basic cable
17 | |Bye bye, see you after your 10year stay in the vault, tell juggernauth I said hi
18 | |I gotta go!, Baby seal in the owen
19 | |Bye bye, gotta go big comic book cross over and you were invited
20 | |Dos verdanja ????? (Russian?)
21 | |Mike drop deadpool out
22 | |bye bye, see you next tuesday
23 | |adious el banjo
24 | |
25 | |Hi i'm deadpools head, not to be confused with headpool, different guy
26 | |Name's pool, dead pool nice to meet you
27 | |Wow, Hi there my name is wade but you can cal me renaldo
28 | |Greating from you uncanny pool guy
29 | |Oh hello, I am the grously unpaid voice in deadpools head
30 | |Hiiii yiiiiii
31 | |hi am Wade but you can call me the nail in the coffin of you marrige bye bye 
32 | |Cough Hi i'm deadpool but n my currentstate more like deadpuddle
33 | |Cough Hi Deadpool here, mecenery for hire, I charge by the limb hahah
34 | |Ooh Hi am deadpool or as corporate calls me of screen demice pool
35 | |Hi I'm marvel x-men's deadpool presented by Hasbro
36 | |ooh hey there handsome, have you seen my abs, no really where are they?
37 | |You are super attractive, you can't tell bu I am using air quotes ha ha ha
38 | |Eue You smell lik beatst lab after taco night 
?39 || Egh you smell like a second batch og egg nog had a secondery mutation
?40 || You so ugly you
41 | |Wohoo wow that's an ugmug
42 | |
43 | |I know colosis since he was a weee anvill
44 | |Music I like my x-men with big guns an full pouches
45 | |psst, wolverine is actually just a pig, talking, wolverine
46 | |Truth is I lost my body trying to teleport with nightcrawler into a taxi
47 | |
48 | |
49 | |*music* I mean face it cable you need to update you name, how about captain streaming?
50 | |*music* Magneto is so old *How old id he* that he call his turkey neck sagneto
51 | |*music* Hey cable, more like basic cable. Hashtag dadpool joke
52 | |*music* Why did cable pay so much almoney? His ex forced him to, no he is really distrot I am francly worried about him
53 | |Who is the least popular x-men at a poolparty? Canonball! I hate myself
54 | |*music* What do you call a cevered head floating in a kids pool? waaade
55 | |*music* I just flow in from New York and boy are my stumps tired. I'll be here all night, try the veil.
56 | News | Breaking news man sixe humoculus plays with superhero toys   
57 | News | After the break unicorns are real and boy are they hot
58 | News | This just in Twerking help lowe colesterole
59 | News | This just in I decided not to remove my trampstamp
60 | News | 
61 | News | Bob gets a reservation for one at the mall food court
62 | News |  Jean grey has been resurrected, oh wait I'm beeing told she is dead again
63 | |Bleep bleep you
64 | |bleep censored again
65 | |Chimmy changaaaaa
66 | |Chimmy changa
67 | |shape shifters?
68 | |K-pop is not for sissies
69 | |Put me in a professor x magneto sanwich
70 | |I feel like a british girl
71 | |Can you belive that a person was PAID to write this
72 | |Professor X I don't wanna go for a hover chair ride
73 | |Chimmy changaaaa, who the fuck gave away my chimy changa
74 | |what x-men universe is this? Continuity people!
75 | |I can't feel my legs!
76 | |I can't feel my stump
77 | |Cheeese
78 | |Selfie
79 | |Selfie2
80 | |Selfie3
81 | |You gotta pose me I don't have a booty
82 | |Let's get one for grandma
83 | |hahahah ok let's get one for granmama
84 | |Let's get one for mema
85 | |woh woh lets get on for peppa and gigi
86 | |Duck faces OBOY
87 | |How's my hair look?
88 | |Hiyeeeh, how my nub look?
89 | |Hey's how my mass  ?? look
90 | |Makup!
91 | |Make sure you get my good angle, who am I kidding they ar all good
92 | |say chimiy chunga
93 | |Strike a pode girls
94 | |So help me bob, I will bleep in you litter box
95 | |HA, so you think just because I don't have a body I can't kick your ass? Let's dance
96 | |Listen ....?? teeth
97 | |Hey bub I'm the second best at whar wolvie does so don't mess with the head
98 | |Iam here to chew gum and to chew gum, do you have any gum?
99 | |You talking to me? You talking to me? Oh you ARE talking to me... hi!

 ## Sensors
0x21,0x0B Touch mouth
0x20,0x0E,0x01 Carry
0x20,0x0C,0x01 OnTable
0x20,0x4C,0x00 Button press
0x20,0x4C,0x00 Button releas

