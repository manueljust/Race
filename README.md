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
* implement connection broker
* implement locked-out notification (event?)

### Networking
* Frame definition:<br/>
<code>| start | length | type | payload | stop |</code>
  - start: 0xC9
  - length: 32bit signed integer (little endian) designating the length of the entire frame (including start and stop bytes)
  - type: byte indicating the data type of the payload (see PayloadType enum)
  - palyoad: variable length payload buffer
  - stop:  0x63

### New Game Protocol
 * Host uses "create online game" dialog
   - has a tcpserver listening for connections
   - negotiates connections between peers
   - connecting peer receives current selected track
   - connected peer may change track or accept
   - if a change received, accept state is reset
   - if all players accept, track change is locked and car selection enabled
   - when all players have chosen car and clicked start, game is started
   - host determines order randomly