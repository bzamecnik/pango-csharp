# Pango

Author: [Bohumír Zámečník](http://zamecnik.me)

The Pango game is a 2D maze with some creatures. The goal is to kill all the
enemies, gather bonuses and survive.

It is a remake of the [Pango](http://dosgamer.com/pango/) for MS-DOS from 1983, which was a clone of an arcade game [Pengo](http://en.wikipedia.org/wiki/Pengo). Take a look at a [screencast of the original Pango at YouTube](http://www.youtube.com/watch?v=3wBFqKZwJCs).

We played this game with my [brother](http://pango.cz) about 20 years ago as children (around 1993). "Pango" sticked as his nickname until today. About 5 years ago at the university I made a remake of this game. It was a semester project at the Programming II course.

The game is rewritten from scratch in C#/.NET. The artwork is from the original Pango.

## Controls
* arrows - move the player
* space - run the game, attack enemies (by hitting the wall)
* P - pause
* Esc - quit the game

## Command-line arguments

```
pango.exe [MAP_FILE]
```

You can specify the file with the map definition. When nothing is given the `maps.txt` is used by default.

Enjoy.
