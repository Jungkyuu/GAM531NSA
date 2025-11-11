## Sprite Animation Game with Jumping and Sprinting

This project extends the original SpriteGameOpenTk sample by adding new movement mechanics and a proper animation controller. It is built using C#, OpenTK (OpenGL 3.3), and ImageSharp for texture management. The focus was to create smoother and more interactive character movement with consistent frame handling and readable code structure.

## New Movements Implemented

Two new player mechanics were introduced:
	1.	Jumping — The player can press the Space bar to perform a jump. Vertical motion is controlled using a velocity variable and gravity simulation. The jump animation is shown while the player is airborne and transitions smoothly back to idle or running upon landing.
	2.	Running / Sprinting — Holding the Shift key while moving increases the player’s speed and switches to a faster animation playback rate. This makes the character movement feel more dynamic and responsive.

Both mechanics integrate seamlessly with the animation controller and preserve the “last frame hold” rule — meaning that when input stops, the sprite remains on its final frame rather than snapping back to idle immediately.

## Animation Controller and Logic

A small finite state machine (FSM) manages the animation states: Idle, Walk, Run, and Jump.
Each state defines:
	•	The sprite row and number of frames.
	•	The animation playback speed (frames per second).
	•	Whether the last frame should remain visible when input stops.

The FSM ensures logical transitions — for example, the player cannot jump while already in the air, and running transitions back to walking when Shift is released. The system separates physics (Movement class) and visuals (Animator class) for clear design and easier debugging.

## Development Challenges

One challenge was synchronizing animation updates with physical motion. This was solved by advancing animation frames independently of physics updates, keeping frame timing consistent. Another issue was avoiding texture bleeding between sprite frames, which was handled by using Nearest filtering and ClampToEdge wrapping in OpenGL. The result is a clean, stable 2D animation system that responds correctly to player input and maintains high visual quality.


## License
This project is based on open-source code by Leonardo Moura,  
used and modified under the terms of the MIT License.