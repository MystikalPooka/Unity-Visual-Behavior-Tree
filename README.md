<!--- This Readme was made with the help of Dillinger online markdown editor https://dillinger.io/--->
[![Build Status](https://travis-ci.org/MystikalPooka/Unity-Visual-Behavior-Tree.svg?branch=master)](https://travis-ci.org/MystikalPooka/Unity-Visual-Behavior-Tree) [![Join the chat at https://gitter.im/MystikalPooka/Unity-Visual-Behavior-Tree](https://badges.gitter.im/MystikalPooka/Unity-Visual-Behavior-Tree.svg)](https://gitter.im/MystikalPooka/Unity-Visual-Behavior-Tree?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
# Unity-Visual-Behavior-Tree (Name pending....)
Visual Scripting Behavior Tree Tool for Unity 2017.2.x+

### Requires [uniRX](https://github.com/neuecc/UniRx) - reactive extensions for Unity by nuecc

# **How To Behavior Tree**
[GREAT Introduction to RX here](http://introtorx.com/)! (introtorx)
[Another one (with shiny marble diagrams!)](http://reactivex.io/)! (ReactiveX.io)
- ### **Step 1:** 
   ##### Inherit from your choice of abstract behavior. Behavior type descriptions are below.
  
- ### **Step 2:** 
  ##### Override the Tick() method as if it were Unity's update() method. 
  - ###### This method will receive a call to tick from its parent and will run as a coroutine until finished.
  
- ### **Step 3:** 
  ##### This newly-created class will show up in the tree view window editor when opening a Behavior Tree Manager Asset either by double clicking it in unity or in the behavior manager.
  
  ###### The treeview editor window is where these classes can be created, dragged and dropped, and rearranged visually. The Behavior Manager (explained below) will be responsible for instantiating instances of these behavior trees.
 **This is all done via reflection, so they automatically show up!**
![Imgur](https://i.imgur.com/o9bywOx.jpg)
![Imgur](https://i.imgur.com/WQ8ftpv.jpg)
![Imgur](https://i.imgur.com/rTboxF0.jpg)
- ### **Step 4:**
  ##### Any monobehavior that you wish to have a behavior from these trees must have the Behavior Tree Manager component added to it.
  The Behavior Tree Manager component has the behavior tree asset field which is the tree that will be run when the game is started.
  It uses a ParallelRunner to run all base behaviors in the tree at the same time.
  
# **Types Of Behaviors**

- ### **Behavior Components:**

  ##### **Components have *multiple* children behaviors.**
 
  - ###  **Selector:**
    ####    A Selector will run through its children behaviors ***one at a time***.
    #####  If the child | ***fails*** | the selector will choose the next behavior and run it.
    - ##### The selector returns  | ***succeed*** | when one of its children returns | ***succeed*** |.
      
  - ###  **Sequencer:**
    ####    A Sequencer will run through its children behaviors ***one at a time***.
    #####   If the child | ***succeeds*** | the sequencer will run the next child in sequence.
    - ##### The sequencer will | ***succeed*** | when a child | ***fails***  |.
      
  - ###  **ParallelRunner:**
      ####  A ParallelRunner runs all of its children ***at the same time***. 
      #####  The ParallelRunner keeps track of how many children *fail* or *succeed*. 
    - ###### If the NumberOfSucceed states is hit, the ParallelRunner returns | ***succeed***   | to its parent.
     - ###### If the NumberOfFail states is hit, the ParallelRunner returns | ***Fail***  |to its parent.
     - ###### If NumberOfSucceed ***AND*** the NumberOfFail ints are set to **0** the ParallelRunner will run | ***indefinitely*** |. 

- ### **Behavior Decorator**
   ##### **The Decorator has only ***one*** child. Useful for specialized conditions.**

    - ### **Inverter:**
        #### *Inverts* the state of its child.
        - ###### If the child returns | ***succeed*** | the inverter returns | ***fail***  |
        - ###### If the child returns | ***fail***  | the inverter returns | ***succeed***  |
        
- ### **Behavior Node**
   ##### **The Meat and Potatoes of every behavior!**

    - ### **This is where new behaviors are typically added.**
    - ##### Ignores any children it might (accidentally?) have. *(Nodes are selfish like that)*

## Behavior Manager 
#### The Component to make your monobehaviours BEHAVE.

##### The Behavior Manager is responsible for actually *running* your behavior trees.
###### It has a few parts:
- ###### **Name**:  The name of the file and the name to save the tree as *(will be replaced by something better later)*
- ###### **Behavior Tree File**: The behavior tree to run for whatever this component is attached to
- ###### **Seconds Between Ticks**: The number of seconds between each tick. ***0** is the same as a Unity Update() loop*
- ###### **Times To Tick**: Number of times to tick. Used for debugging mostly. *-1 for infinitely running*
- ###### **Splice Into Tree**: Should the list of trees be enabled to splice other trees into this current tree?
- ###### **List**: The list of trees to splice into the current tree. *These **will** be saved with the current tree when saving!*
  
