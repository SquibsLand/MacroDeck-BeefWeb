# MacroDeck BeefWeb TODO

## Player (8/12)
Extension of IMusicPlayer
- [X] GetArtworkAsync 
	- Caching might need work
- [X] GetStateAsync
- [X] NextAsync
- [X] PauseAsync
- [X] PlayAsync
- [ ] PlayItemAsync
- [X] PreviousAsync
- [X] SeekAsync
- [!] SetRepeatModeAsync
	- Current attempts have failed, it is unknown how to do this with the current API
- [!] SetShuffleAsync
	- See Above	
- [X] SetVolumeAsync
- [X] TogglePlayPauseAsync

## Custom Actions (3/7)
- [ ] Refresh
- [ ] Add to Playback Queue
- [ ] Stop after next track
- [X] Seek Relative
- [X] Volume Relative
	- Due to the conversion from DB to Percent, volume relative is calculated locally and not by beefweb.
- [ ] Get Custom Value
- [X] Change active playlist

## Varriables (12/14)
- [X] Title
- [X] Album
- [X] Artist
- [X] State
- [X] Playing
- [X] Volume
- [X] Duration
- [X] Position
- [X] Progress Percentage
- [ ] Current Playlist
- [X] Shuffled
- [X] Repeat Mode
- [X] Connected
- [ ] Rating

## Events (0/2)
- [ ] Track Changed
- [ ] Playlist Changed

## Ideas
- Output/Speaker Control