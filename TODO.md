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

## Varriables (0/5)
- [ ] Title
- [ ] Artist
- [ ] Is Playing
- [ ] Volume
- [ ] Current Playlist
- [ ] Rating

## Events (0/1)
- [ ] Track Changed

## Ideas
- Output/Speaker Control