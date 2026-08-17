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
	- This appears to be unused, but I plan to support it
- [X] PreviousAsync
- [X] SeekAsync
- [!] SetRepeatModeAsync
	- Current attempts have failed, it is unknown how to do this with the current API
- [!] SetShuffleAsync
	- See Above	
- [X] SetVolumeAsync
- [X] TogglePlayPauseAsync

## Custom Actions (0/5)
- [ ] Refresh
- [ ] Add to Playback Queue
- [ ] Stop after next track
- [X] Seek Relative
- [X] Volume Relative
	- Due to the conversion from DB to Percent, volume relative is calculated locally and not by beefweb.
- [ ] Get Custom Value

## Varriables
- [ ] Title
- [ ] Artist
- [ ] Is Playing
- [ ] Volume

## Events
- [ ] Track Changed

## Ideas
- Output/Speaker Control
- 