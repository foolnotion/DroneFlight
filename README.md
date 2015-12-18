# DroneFlight

This is a pet project implemented for a programming contest.

The challenge is to program a drone AI which can navigate a grid map with moving obstacles towards a predefined target.

Since the drone is equipped with a very basic CPU (two register, branch-if-not-negative machine), the navigation code needs to be compiled into assembly instructions.

Thefore, we have three main components:
1) A register machine for the drone CPU which can interpret sequences of assembly instructions
2) An ast implementation for assembly generation
3) A map class for simulating the scenario, coupled with a graphical interface
