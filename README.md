# THIS REPO IS IN WORK IN PROGRESS
## CURRENT SCRIPTS HAS COMPILATION ERRORS DUE DEPENDENCIES WITH:
## "Odin Inspector", "UniTasks Extensions" and "HybridEZS"


## I Will be working on simplifying these scripts and creating the documentation for their use

### the purpose of this repo is just giving a layer of compatibility between Gameobjets and Dots triggers
in a way that "Gameobjects can trigger dots triggers" (is not so easy but a way to express it)

This enables the use of physics cathegories from dots, using special attached colliders on gameobjects

this is great for sparating the phyisics and the "Phyisic triggers logic" in your game, this increases the physics performance in areas where your game needs lots of triggers, or dynamic ones

Even you can take advantage of the "Incompatibility" between dots physics and gameobjects physics



when you have lots of colliders in a gameobject for giving them complex shapes made with primitives, this comes with a cost, trigger events may be triggered more than once per object and even if the layers can not mutually touch, there still having a little check to see if these can collide
also the current physics layering scheme that unity offers, has a problem... **its is directly coupled with the rendering layers!!**,
and also, each item can just have a single layer set, wich gives problems when you want some objects with the same layers to arbitrary not detect other layers
just giving headaches for organizing your layers

Dots physics cathegories are like layers
but just for physics and can be combined in single colliders
for example you can have a single collider to have "item" layer , and another to have "player" layer
and then you can also have a trigger which just has "Trigger" layer
in Dots physics you can also set with what collides each collider, for example if i want 
a trigger that only detects items, i can set a trigger to have " I am from Trigger layer but only Collide with item"
and also can have other trigger with " I am from Trigger layer but only Collide with player"
even have other trigger with " I am from Trigger layer but only Collide with player and item"
the objects with player and item layers, can also set what layers they want to collide with, you can set multiple, in this case you can set
"I m player but i dont collide with Triggers" then triggers will not collide with player even if they have it set to do so
so this offers a lot of flexibility with the physics

in the other hand, Unity's default physics layers dont let you do this by default
so if you want to have a trigger that only detects items, and other trigger that only detects players
you will have to create 2 layers (triggerForItems and TriggerForPlayers) increasing the layers you need
 *Yes you can just use a "Trigger" layer and do a Tag check, but even doing so, the physics TriggerEnterExit phase occurs, and the Event OnTriggerEnter/Exit/Stay still getting called from c++ wich is not fast*
 
 //Documentation and Code are in progress


# DOTS-Triggers-For-GameObjects
