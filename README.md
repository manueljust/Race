# Race
A round-based 2D racing game.
Work in progress.

### ToDo's
* rework win condition
* rework crash penalty
  - velocity stop at crash point and permanent speed penalty
* svg implement \<use /\> tag
* fix transform of image import
* fix pattern import (unintended padding)
* fix powershape area
* p2p
* add networking error correction (request re-send of corrupted frames)

### Networking
* Frame definition:<br/>
<code>| start | length | type | payload | stop |</code>
  - start: 0xC9
  - length: 32bit signed integer (little endian) designating the length of the entire frame (including start and stop bytes)
  - type: byte indicating the data type of the payload (see PayloadType enum)
  - palyoad: variable length payload buffer
  - stop:  0x63
