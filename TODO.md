# MacroDeck BeefWeb TODO

## Player (10/12)
Extension of IMusicPlayer
- [X] GetArtworkAsync 
	- Caching might need work
- [X] GetStateAsync
- [X] NextAsync
- [X] PauseAsync
- [X] PlayAsync
- [X] PlayItemAsync
- [X] PreviousAsync
- [X] SeekAsync
- [!] SetRepeatModeAsync
	- Current attempts have failed, it is unknown how to do this with the current API
- [!] SetShuffleAsync
	- See Above	
- [X] SetVolumeAsync
- [X] TogglePlayPauseAsync

## Custom Actions (3/6)
- [ ] Refresh
- [ ] Add to Playback Queue
- [ ] Stop after next track
- [X] Seek Relative
- [X] Volume Relative
	- Due to the conversion from DB to Percent, volume relative is calculated locally and not by beefweb.	
- [X] Change active playlist

## Varriables (15/15)
- [X] Title
- [X] Album
- [X] Artist
- [X] State
- [X] Playing
- [X] Volume
- [X] Duration
- [X] Position
- [X] Progress Percentage
- [X] Current Playlist
- [X] Shuffled
- [X] Repeat Mode
- [X] Connected
- [X] Play Queue Size
- [X] Rating

## Events (0/2)
- [X] Track Changed
- [ ] Playlist Changed

## Ideas
- Output/Speaker Control